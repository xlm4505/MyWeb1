using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
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
        SummaryType? targetSummaryType = null;
        var categorized = new Dictionary<SummaryType, SummaryWorkbook>
        {
            [SummaryType.Con] = new SummaryWorkbook("Summary CON"),
            [SummaryType.Rinku] = new SummaryWorkbook("Summary RINKU"),
            [SummaryType.ConFlow] = new SummaryWorkbook("Summary CON (Flow)"),
            [SummaryType.RinkuFlow] = new SummaryWorkbook("Summary RINKU (Flow)")
        };
        foreach (var summary in categorized.Values)
        {
            WritePoSheet(summary.Workbook.Worksheets.Add("PODetail"), poDetails);
        }

        var poMap = poDetails.ToDictionary(x => x.PoLn, x => x, StringComparer.OrdinalIgnoreCase);
        var processedDate = DateTime.Today;

        foreach (var file in files)
        {
            if (!IsSupportedInvoiceFile(file.FileName))
            {
                result.Warnings.Add($"Skipped unsupported file: {file.FileName} (I*.xlsx only)");
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
                if (targetSummaryType is null)
                {
                    targetSummaryType = summaryType;
                }
                else if (targetSummaryType.Value != summaryType)
                {
                    result.Warnings.Add($"Warning: Skipped due to mixed condition. File = {file.FileName}");
                    continue;
                }
                var invoiceDate = GetDate(invSheet.Cell("G9"));
                processedDate = invoiceDate;

                var sheetInfo = new InvoiceSheetData
                {
                    InvoiceName = BuildLegacySheetName(file.FileName),
                    SiteCode = siteCode,
                    InvoiceDate = invoiceDate,
                    Note = summaryType is SummaryType.Rinku or SummaryType.RinkuFlow
                        ? $"{invoiceDate:MM/dd/yyyy} RINKU"
                        : $"{GetConsolidationDate(invoiceDate):MM/dd/yyyy} Consolidation"
                };

                CreateInvoiceSheet(categorized[summaryType].Workbook, invSheet, sheetInfo, forRow.Value, poMap);
                categorized[summaryType].Count++;

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

        if (targetSummaryType is not null)
        {
            CreateSummaryFile(result, categorized[targetSummaryType.Value], processedDate);
        }

        if (result.TotalInvoices == 0)
        {
            result.Errors.Add("No valid invoice file was processed.");
        }

        result.Logs.Add("Process complete.");
        return result;
    }
    private static bool IsSupportedInvoiceFile(string fileName)
    {
        if (!fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        var name = Path.GetFileNameWithoutExtension(fileName);
        return name.Length >= 6 && name.StartsWith("I", StringComparison.OrdinalIgnoreCase);
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
            var lineKeyNumber = Convert.ToDecimal(reader["LnKey"]);
            var lnKeyForPoLn = lineKeyNumber.ToString("00");
            var lnKey = lineKeyNumber.ToString("000000");
            list.Add(new OpenPoRow
            {
                PoLn = $"{poNo}-{lnKeyForPoLn}",
                PoNo = poNo,
                LnKey = lnKey,
                PoDate = reader["PODate"] as DateTime?,
                Status = reader["Status"]?.ToString() ?? string.Empty,
                ItemCode = reader["ItemCode"]?.ToString() ?? string.Empty,
                ItemDesc = reader["UDF_ITEMDESC"]?.ToString() ?? string.Empty,
                Whse = reader["Whse"]?.ToString() ?? string.Empty,
                QtyOrdered = ToDecimal(reader["QtyOrdered"]),
                QtyRcpt = ToDecimal(reader["QtyRcpt"]),
                QtyBalance = ToDecimal(reader["QtyBalance"]),
                QtyInvoiced = ToDecimal(reader["QtyInvoiced"]),
                UnitCost = ToDecimal(reader["UnitCost"]),
                LastTotalUnitCost = ToDecimal(reader["LastTotalUnitCost"]),
                StandardUnitCost = ToDecimal(reader["StandardUnitCost"]),
                QtyDiscCost = ToDecimal(reader["QtyDiscCost"]),
                RequiredDate = reader["RequiredDate"] as DateTime?,
                PromiseDate = reader["PromiseDate"] as DateTime?
            });
        }

        return list;
    }

    private static void CreateSummaryFile(Model_InvConv result, SummaryWorkbook summary, DateTime processedDate)
    {
        if (summary.Count == 0)
        {
            return;
        }

        using var stream = new MemoryStream();
        summary.Workbook.SaveAs(stream);
        result.Files.Add(new GeneratedSummaryFile
        {
            FileName = $"{summary.Title} {processedDate:yyMMdd}WI.xlsx",
            Content = stream.ToArray()
        });

        result.Logs.Add($"{summary.Title} file was created.");
    }

    private static void WritePoSheet(IXLWorksheet ws, List<OpenPoRow> poRows)
    {
        var headers = new[]
        {
            "PO-Ln", "PoNo", "LnKey", "PODate", "Status", "ItemCode", "UDF_ITEMDESC", "Whse", "QtyOrdered",
            "QtyRcpt", "QtyBalance", "QtyInvoiced", "UnitCost", "LastTotalUnitCost", "StandardUnitCost", "QtyDiscCost", "RequiredDate", "PromiseDate"
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
            ws.Cell(row, 4).Value = po.PoDate;
            ws.Cell(row, 5).Value = po.Status;
            ws.Cell(row, 6).Value = po.ItemCode;
            ws.Cell(row, 7).Value = po.ItemDesc;
            ws.Cell(row, 8).Value = po.Whse;
            ws.Cell(row, 9).Value = po.QtyOrdered;
            ws.Cell(row, 10).Value = po.QtyRcpt;
            ws.Cell(row, 11).Value = po.QtyBalance;
            ws.Cell(row, 12).Value = po.QtyInvoiced;
            ws.Cell(row, 13).Value = po.UnitCost;
            ws.Cell(row, 14).Value = po.LastTotalUnitCost;
            ws.Cell(row, 15).Value = po.StandardUnitCost;
            ws.Cell(row, 16).Value = po.QtyDiscCost;
            ws.Cell(row, 17).Value = po.RequiredDate;
            ws.Cell(row, 18).Value = po.PromiseDate;

            row++;
        }

        ws.Columns().AdjustToContents();
    }
    private static void CreateInvoiceSheet(
            XLWorkbook summaryWorkbook,
            IXLWorksheet sourceSheet,
            InvoiceSheetData inv,
            int forRow,
            Dictionary<string, OpenPoRow> poMap)
    {
        var sheetName = GetUniqueSheetName(summaryWorkbook, inv.InvoiceName);
        var ws = sourceSheet.CopyTo(summaryWorkbook, sheetName);

        ws.Cell("G20").Value = inv.Note;
        ws.Cell("G21").Value = ws.Cell(forRow, 2).GetString();

        ws.Column(5).InsertColumnsBefore(2);
        ws.Cell("E24").Value = "ITEM CODE";
        ws.Cell("F24").Value = "WH";
        ws.Range("E24:F24").Style.Font.Bold = true;
        ws.Range("E24:F24").Style.Font.Underline = XLFontUnderlineValues.Single;
        ws.Column("E").AdjustToContents();
        ws.Column("F").Width = 8;

        var lastRow = ws.LastRowUsed()?.RowNumber() ?? 22;
        for (var row = 22; row <= lastRow; row++)
        {
            var poLn = ws.Cell(row, 1).GetString().Trim();
            if (!poLn.StartsWith("00", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            poMap.TryGetValue(poLn, out var po);
            ws.Cell(row, 6).Value = po?.Whse ?? string.Empty;
            ws.Cell(row, 16).Value = po?.ItemCode ?? string.Empty;
            ws.Cell(row, 17).Value = po is null ? string.Empty : po.QtyInvoiced;
            ws.Cell(row, 18).Value = po is null ? string.Empty : po.LastTotalUnitCost;
            ws.Cell(row, 19).Value = po is null ? string.Empty : po.StandardUnitCost;
            ws.Cell(row, 20).Value = po?.Whse ?? string.Empty;
            ws.Cell(row, 21).Value = po is null ? string.Empty : po.QtyDiscCost;
            ws.Cell(row, 22).Value = po?.PromiseDate;
            ws.Cell(row, 5).Value = ws.Cell(row, 12).GetString();
        }

        ws.PageSetup.PagesWide = 1;
    }

    private static string GetUniqueSheetName(XLWorkbook wb, string preferredName)
    {
        var baseName = preferredName[..Math.Min(preferredName.Length, 31)];
        var sheetName = baseName;
        var suffix = 1;
        while (wb.Worksheets.Contains(sheetName))
        {
            var prefix = baseName[..Math.Min(baseName.Length, 28)];
            sheetName = $"{prefix}_{suffix++}";
        }

        return sheetName;
    }

    private static string BuildLegacySheetName(string fileName)
    {
        var name = Path.GetFileNameWithoutExtension(fileName);
        if (name.Length >= 6 && name.StartsWith("I", StringComparison.OrdinalIgnoreCase))
        {
            return name.Substring(1, 5);
        }

        return name[..Math.Min(name.Length, 31)];
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
        public DateTime? PoDate { get; init; }
        public required string Status { get; init; }
        public required string ItemCode { get; init; }
        public required string ItemDesc { get; init; }
        public required string Whse { get; init; }
        public decimal QtyOrdered { get; init; }
        public decimal QtyRcpt { get; init; }   
        public decimal QtyBalance { get; init; }
        public decimal QtyInvoiced { get; init; }
        public decimal UnitCost { get; init; }
        public decimal LastTotalUnitCost { get; init; }
        public decimal StandardUnitCost { get; init; }
        public decimal QtyDiscCost { get; init; }
        public DateTime? RequiredDate { get; init; }
        public DateTime? PromiseDate { get; init; }
    }

    private sealed class InvoiceSheetData
    {
        public required string InvoiceName { get; init; }
        public required string SiteCode { get; init; }
        public required DateTime InvoiceDate { get; init; }
        public string Note { get; set; } = string.Empty;
    }

    private sealed class SummaryWorkbook
    {
        public SummaryWorkbook(string title)
        {
            Title = title;
            Workbook = new XLWorkbook();
        }

        public string Title { get; }
        public XLWorkbook Workbook { get; }
        public int Count { get; set; }
    }
}