using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Repository;
using System.IO.Compression;
using System.Text;

public class InvConvController : Controller
{
    private readonly Repository_InvConv _repo;

    public InvConvController(Repository_InvConv repo)
    {
        _repo = repo;
    }

    public IActionResult InvConv()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Convert(List<IFormFile> files)
    {
        if (files.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var conversion = await _repo.ConvertInvoicesAsync(files);
        if (conversion.HasError)
        {
            return BadRequest(string.Join(Environment.NewLine, conversion.Errors));
        }

        await using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
        {
            foreach (var file in conversion.Files)
            {
                var entry = archive.CreateEntry(file.FileName, CompressionLevel.Fastest);
                await using var entryStream = entry.Open();
                await entryStream.WriteAsync(file.Content);
            }

            var logEntry = archive.CreateEntry("conversion-log.txt", CompressionLevel.Fastest);
            await using var logStream = logEntry.Open();
            var logs = new StringBuilder();
            foreach (var log in conversion.Logs)
            {
                logs.AppendLine(log);
            }

            foreach (var warning in conversion.Warnings)
            {
                logs.AppendLine(warning);
            }

            logs.AppendLine($"Total invoice: {conversion.TotalInvoices}");
            logs.AppendLine($"CON: {conversion.ConCount}");
            logs.AppendLine($"RINKU: {conversion.RinkuCount}");
            logs.AppendLine($"CON (Flow): {conversion.ConFlowCount}");
            logs.AppendLine($"RINKU (Flow): {conversion.RinkuFlowCount}");

            var bytes = Encoding.UTF8.GetBytes(logs.ToString());
            await logStream.WriteAsync(bytes);
        }

        return File(
            zipStream.ToArray(),
            "application/zip",
            $"CI-Summary-{DateTime.Now:yyyyMMddHHmmss}.zip");
    }
}