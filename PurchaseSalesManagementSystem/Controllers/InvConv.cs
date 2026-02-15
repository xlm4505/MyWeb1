using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Repository;

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

        var summaryFile = conversion.Files.FirstOrDefault();
        if (summaryFile is null)
        {
            return BadRequest("No summary file was generated.");
        }

        return File(
            summaryFile.Content,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            summaryFile.FileName);
    }
}