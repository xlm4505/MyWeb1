using ClosedXML.Excel;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using System.Data;

namespace PurchaseSalesManagementSystem.Repository;

public class Repository_InvConv
{
    private readonly CreateConnection _connectionFactory;
    private readonly IWebHostEnvironment _env;

    public Repository_InvConv(CreateConnection connectionFactory, IWebHostEnvironment env)
    {
        _connectionFactory = connectionFactory;
        _env = env;
    }

    public async Task<Model_InvConv> ConvertInvoicesAsync(IReadOnlyCollection<IFormFile> files)
    {
        var result = new Model_InvConv();
        result.Logs.Add("Invoice conversion process started.");

        var poDetails = await LoadOpenPoDetailsAsync();
        result.Logs.Add($"Open PO data retrieved. ({poDetails.Count} rows)");

        var categorized = new Dictionary<SummaryType, List<InvoiceSheetData>>
        {
            [SummaryType.Con] = [],
            [SummaryType.Rinku] = [],
            [SummaryType.ConFlow] = [],
            [SummaryType.RinkuFlow] = []
        };

        var processedDate = DateTime.Today;

        foreach (var file in files)
        {
            if (!file.FileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                result.Warnings.Add($"Skipped unsupported file format: {file.FileName} (.xlsx only)");
                continue;
            }

            try
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);
                var invSheet = workbook.Worksheets.FirstOrDefault(x => x.Name.Equals("INV", StringComparison.OrdinalIgnoreCase));
                if (invSheet is null)
                {
                    result.Warnings.Add($"Warning: Irregular format (INV sheet missing). File = {file.FileName}");
                    continue;
                }

                var forRow = FindForRow(invSheet);
                if (forRow is null)
                {
                    result.Warnings.Add($"Warning: Irregular format (FOR row missing). File = {file.FileName}");
                    continue;
                }

                var siteCode = GetSiteCode(invSheet.Cell(forRow.Value, 2).GetString());
                if (string.IsNullOrWhiteSpace(siteCode))
                {
                    result.Warnings.Add($"Warning: Irregular format (site code missing). File = {file.FileName}");
                    continue;
                }

                var isFlow = IsFlowInvoice(invSheet.Cell(24, 2).GetString());
                var summaryType = ResolveType(siteCode, isFlow);
                var invoiceDate = GetDate(invSheet.Cell("G9"));
                processedDate = invoiceDate;

                var data = BuildInvoiceSheetData(file.FileName, invSheet, siteCode, invoiceDate, poDetails);
                data.Note = summaryType is SummaryType.Rinku or SummaryType.RinkuFlow
                    ? $"{invoiceDate:MM/dd/yyyy} RINKU"
                    : $"{GetConsolidationDate(invoiceDate):MM/dd/yyyy} Consolidation";

                categorized[summaryType].Add(data);

                result.TotalInvoices++;
                switch (summaryType)
                {
                    case SummaryType.Con: result.ConCount++; break;
                    case SummaryType.Rinku: result.RinkuCount++; break;
                    case SummaryType.ConFlow: result.ConFlowCount++; break;
                    case SummaryType.RinkuFlow: result.RinkuFlowCount++; break;
                }
            }
            catch (Exception ex)
            {
                result.Warnings.Add($"Warning: File skipped due to processing error. File = {file.FileName}, Reason = {ex.Message}");
            }
        }

        CreateSummaryFile(result, categorized[SummaryType.Con], "Summary CON", processedDate, poDetails);
        CreateSummaryFile(result, categorized[SummaryType.Rinku], "Summary RINKU", processedDate, poDetails);
        CreateSummaryFile(result, categorized[SummaryType.ConFlow], "Summary CON (Flow)", processedDate, poDetails);
        CreateSummaryFile(result, categorized[SummaryType.RinkuFlow], "Summary RINKU (Flow)", processedDate, poDetails);

        if (result.TotalInvoices == 0)
        {
            result.Errors.Add("No valid invoice file was processed.");
        }

        result.Logs.Add("Process complete.");
        return result;
    }

    private async Task<List<OpenPoRow>> LoadOpenPoDetailsAsync()
    {
        var list = new List<OpenPoRow>();
        var sqlPath = Path.Combine(_env.ContentRootPath, "SQL", "InvConv", "InvConv.sql");
        var sql = await File.ReadAllTextAsync(sqlPath);

        await using var conn = _connectionFactory.GetConnection();
        await conn.OpenAsync();

        await using var cmd = new SqlCommand(sql, conn)
        {
            CommandType = CommandType.Text,
            CommandTimeout = 900
        };

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var poNo = reader["PoNo"]?.ToString() ?? string.Empty;
            var lnKey = Convert.ToDecimal(reader["LnKey"]).ToString("00");
            list.Add(new OpenPoRow
            {
                PoLn = $"{poNo}-{lnKey}",
                PoNo = poNo,
                LnKey = lnKey,
                ItemCode = reader["ItemCode"]?.ToString() ?? string.Empty,
                Whse = reader["Whse"]?.ToString() ?? string.Empty,
                QtyOrdered = ToDecimal(reader["QtyOrdered"]),
                QtyBalance = ToDecimal(reader["QtyBalance"]),
                UnitCost = ToDecimal(reader["UnitCost"]),
                LastTotalUnitCost = ToDecimal(reader["LastTotalUnitCost"]),
                StandardUnitCost = ToDecimal(reader["StandardUnitCost"]),
                QtyDiscCost = ToDecimal(reader["QtyDiscCost"]),
                PromiseDate = reader["PromiseDate"] as DateTime?
            });
        }

        return list;
    }

    private static InvoiceSheetData BuildInvoiceSheetData(
        string fileName,
        IXLWorksheet sheet,
        string siteCode,
        DateTime invoiceDate,
        List<OpenPoRow> poRows)
    {
        var map = poRows.ToDictionary(x => x.PoLn, x => x, StringComparer.OrdinalIgnoreCase);
        var data = new InvoiceSheetData
        {
            InvoiceName = Path.GetFileNameWithoutExtension(fileName),
            SiteCode = siteCode,
            InvoiceDate = invoiceDate
        };

        var lastRow = sheet.LastRowUsed()?.RowNumber() ?? 21;
        for (var row = 22; row <= lastRow; row++)
        {
            var poLn = sheet.Cell(row, 1).GetString().Trim();
            if (!poLn.StartsWith("00", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            map.TryGetValue(poLn, out var po);
            data.Lines.Add(new InvoiceLine
            {
                PoLn = poLn,
                ItemCode = po?.ItemCode ?? string.Empty,
                InvoiceWh = sheet.Cell(row, 6).GetString(),
                PoWh = po?.Whse ?? string.Empty,
                InvoiceQty = sheet.Cell(row, 7).TryGetValue<decimal>(out var qty) ? qty : 0m,
                PoQty = po?.QtyBalance ?? 0m,
                InvoiceUnitCost = sheet.Cell(row, 9).TryGetValue<decimal>(out var uc) ? uc : 0m,
                PoUnitCost = po?.UnitCost ?? 0m,
                DiscountCost = po?.QtyDiscCost ?? 0m,
                PromiseDate = po?.PromiseDate
            });
        }

        return data;
    }

    private static void CreateSummaryFile(
        Model_InvConv result,
        List<InvoiceSheetData> invoices,
        string title,
        DateTime processedDate,
        List<OpenPoRow> poRows)
    {
        if (invoices.Count == 0)
        {
            return;
        }

        using var wb = new XLWorkbook();
        var poSheet = wb.Worksheets.Add("PODetail");
        WritePoSheet(poSheet, poRows);

        foreach (var inv in invoices)
        {
            var name = inv.InvoiceName.Length > 31 ? inv.InvoiceName[..31] : inv.InvoiceName;
            var ws = wb.Worksheets.Add(name);
            ws.Cell(1, 1).Value = "Note";
            ws.Cell(1, 2).Value = inv.Note;

            ws.Cell(3, 1).Value = "PO-Ln";
            ws.Cell(3, 2).Value = "ItemCode";
            ws.Cell(3, 3).Value = "Invoice WH";
            ws.Cell(3, 4).Value = "PO WH";
            ws.Cell(3, 5).Value = "Invoice Qty";
            ws.Cell(3, 6).Value = "PO QtyBalance";
            ws.Cell(3, 7).Value = "Invoice UnitCost";
            ws.Cell(3, 8).Value = "PO UnitCost";
            ws.Cell(3, 9).Value = "Discount Cost";
            ws.Cell(3, 10).Value = "PromiseDate";
            ws.Range(3, 1, 3, 10).Style.Font.Bold = true;

            var row = 4;
            foreach (var line in inv.Lines)
            {
                ws.Cell(row, 1).Value = line.PoLn;
                ws.Cell(row, 2).Value = line.ItemCode;
                ws.Cell(row, 3).Value = line.InvoiceWh;
                ws.Cell(row, 4).Value = line.PoWh;
                ws.Cell(row, 5).Value = line.InvoiceQty;
                ws.Cell(row, 6).Value = line.PoQty;
                ws.Cell(row, 7).Value = line.InvoiceUnitCost;
                ws.Cell(row, 8).Value = line.PoUnitCost;
                ws.Cell(row, 9).Value = line.DiscountCost;
                ws.Cell(row, 10).Value = line.PromiseDate;
                row++;
            }

            ws.Columns().AdjustToContents();
        }

        using var stream = new MemoryStream();
        wb.SaveAs(stream);
        result.Files.Add(new GeneratedSummaryFile
        {
            FileName = $"{title} {processedDate:yyMMdd}WI.xlsx",
            Content = stream.ToArray()
        });

        result.Logs.Add($"{title} file was created.");
    }

    private static void WritePoSheet(IXLWorksheet ws, List<OpenPoRow> poRows)
    {
        var headers = new[]
        {
            "PO-Ln", "PoNo", "LnKey", "ItemCode", "Whse", "QtyOrdered",
            "QtyBalance", "UnitCost", "LastTotalUnitCost", "StandardUnitCost", "QtyDiscCost", "PromiseDate"
        };

        for (var i = 0; i < headers.Length; i++)
        {
            ws.Cell(1, i + 1).Value = headers[i];
        }

        ws.Range(1, 1, 1, headers.Length).Style.Font.Bold = true;

        var row = 2;
        foreach (var po in poRows)
        {
            ws.Cell(row, 1).Value = po.PoLn;
            ws.Cell(row, 2).Value = po.PoNo;
            ws.Cell(row, 3).Value = po.LnKey;
            ws.Cell(row, 4).Value = po.ItemCode;
            ws.Cell(row, 5).Value = po.Whse;
            ws.Cell(row, 6).Value = po.QtyOrdered;
            ws.Cell(row, 7).Value = po.QtyBalance;
            ws.Cell(row, 8).Value = po.UnitCost;
            ws.Cell(row, 9).Value = po.LastTotalUnitCost;
            ws.Cell(row, 10).Value = po.StandardUnitCost;
            ws.Cell(row, 11).Value = po.QtyDiscCost;
            ws.Cell(row, 12).Value = po.PromiseDate;
            row++;
        }

        ws.Columns().AdjustToContents();
    }

    private static DateTime GetConsolidationDate(DateTime dateTime) => dateTime.DayOfWeek switch
    {
        DayOfWeek.Sunday => dateTime.AddDays(2),
        DayOfWeek.Monday => dateTime.AddDays(1),
        DayOfWeek.Tuesday => dateTime.AddDays(2),
        DayOfWeek.Wednesday => dateTime.AddDays(1),
        DayOfWeek.Thursday => dateTime.AddDays(2),
        DayOfWeek.Friday => dateTime.AddDays(1),
        DayOfWeek.Saturday => dateTime.AddDays(3),
        _ => dateTime
    };

    private static SummaryType ResolveType(string siteCode, bool isFlow)
    {
        var isRinku = siteCode is "FCH" or "FTP";
        return (isRinku, isFlow) switch
        {
            (false, false) => SummaryType.Con,
            (true, false) => SummaryType.Rinku,
            (false, true) => SummaryType.ConFlow,
            _ => SummaryType.RinkuFlow
        };
    }

    private static bool IsFlowInvoice(string value)
    {
        var upper = value.ToUpperInvariant();
        return upper.StartsWith("MASS F")
               || upper.StartsWith("LIQUID")
               || upper.StartsWith("LIOUID")
               || upper.StartsWith("CONCEN")
               || upper.StartsWith("VAPORI");
    }

    private static string GetSiteCode(string text)
    {
        var val = text.Trim();
        if (!val.StartsWith("FOR ", StringComparison.OrdinalIgnoreCase) || val.Length < 7)
        {
            return string.Empty;
        }

        return val.Substring(4, 3).ToUpperInvariant();
    }

    private static int? FindForRow(IXLWorksheet ws)
    {
        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 19;
        for (var row = lastRow; row >= 20; row--)
        {
            if (ws.Cell(row, 2).GetString().StartsWith("FOR ", StringComparison.OrdinalIgnoreCase))
            {
                return row;
            }
        }

        return null;
    }

    private static DateTime GetDate(IXLCell cell)
    {
        if (cell.TryGetValue<DateTime>(out var dt))
        {
            return dt;
        }

        return DateTime.Today;
    }

    private static decimal ToDecimal(object? value)
    {
        if (value is null || value == DBNull.Value)
        {
            return 0m;
        }

        return Convert.ToDecimal(value);
    }

    private enum SummaryType
    {
        Con,
        Rinku,
        ConFlow,
        RinkuFlow
    }

    private sealed class OpenPoRow
    {
        public required string PoLn { get; init; }
        public required string PoNo { get; init; }
        public required string LnKey { get; init; }
        public required string ItemCode { get; init; }
        public required string Whse { get; init; }
        public decimal QtyOrdered { get; init; }
        public decimal QtyBalance { get; init; }
        public decimal UnitCost { get; init; }
        public decimal LastTotalUnitCost { get; init; }
        public decimal StandardUnitCost { get; init; }
        public decimal QtyDiscCost { get; init; }
        public DateTime? PromiseDate { get; init; }
    }

    private sealed class InvoiceSheetData
    {
        public required string InvoiceName { get; init; }
        public required string SiteCode { get; init; }
        public required DateTime InvoiceDate { get; init; }
        public string Note { get; set; } = string.Empty;
        public List<InvoiceLine> Lines { get; } = [];
    }

    private sealed class InvoiceLine
    {
        public required string PoLn { get; init; }
        public required string ItemCode { get; init; }
        public required string InvoiceWh { get; init; }
        public required string PoWh { get; init; }
        public decimal InvoiceQty { get; init; }
        public decimal PoQty { get; init; }
        public decimal InvoiceUnitCost { get; init; }
        public decimal PoUnitCost { get; init; }
        public decimal DiscountCost { get; init; }
        public DateTime? PromiseDate { get; init; }
    }
}