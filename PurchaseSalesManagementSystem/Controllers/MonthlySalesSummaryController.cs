using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Repository;
using System.Data;
using System.Globalization;

public class MonthlySalesSummaryController : Controller
{
    private readonly Repository_MonthlySalesSummary _repo;

    public MonthlySalesSummaryController(Repository_MonthlySalesSummary repo)
    {
        _repo = repo;
    }

    public IActionResult MonthlySalesSummary()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetTargetYears()
    {
        var years = _repo.GetTargetYears();
        return Json(years);
    }

    [HttpGet]
    public IActionResult ExportToExcel(string exportTarget, int targetYear, string? targetData)
    {
        DataTable dt;
        var isSummary = string.Equals(exportTarget, "summary", StringComparison.OrdinalIgnoreCase);
        var isSummaryAll = isSummary && string.Equals(targetData, "ALL", StringComparison.OrdinalIgnoreCase);
        var isSummaryFastSelling = isSummary && !isSummaryAll;
        if (isSummary)
        {
            if (isSummaryAll)
            {
                dt = _repo.GetMonthlySalesSummaryAll(targetYear);
                ApplyMonthlyHeaderNames(dt, targetYear, includeItemNo: false);
            }
            else
            {
                dt = _repo.GetMonthlySalesSummary(targetYear);
                ApplyMonthlyHeaderNames(dt, targetYear, includeItemNo: true);
            }
        }
        else
        {
            dt = _repo.GetMonthlySalesAndPurchasesReport(targetYear);
            ApplyMonthlySalesAndPurchasesReportHeaderNames(dt, targetYear);
        }

        var exportToExcel = new FormattedDataTableExcelExporter();
        using var workbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(dt, "Export", "PO");
        if (!isSummary)
        {
            AddMonthlySalesAndPurchasesTotals(workbook);
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var timestamp = DateTime.Now.ToString("yyMMdd_HHmmss");
        var fileName = BuildExportFileName(exportTarget, targetData, timestamp);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName
        );
    }
    private static string BuildExportFileName(string exportTarget, string? targetData, string timestamp)
    {
        if (string.Equals(exportTarget, "summary", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(targetData, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                return $"Monthly Monthly Sales Summary Report_All_{timestamp}.xlsx";
            }

            return $"Monthly Sales Summary Report_{timestamp}.xlsx";
        }

        return $"Monthly Sales and Purchases Report_{timestamp}.xlsx";
    }
    private static void ApplyMonthlyHeaderNames(DataTable dt, int targetYear, bool includeItemNo)
    {
        if (dt.Columns.Count < 15)
        {
            return;
        }

        if (includeItemNo && !dt.Columns.Contains("ItemNo"))
        {
            dt.Columns.Add("ItemNo", typeof(string)).SetOrdinal(0);
        }

        if (includeItemNo)
        {
            dt.Columns[0].ColumnName = "ItemNo";
            dt.Columns[1].ColumnName = "ItemCode";
            dt.Columns[2].ColumnName = "ItemCodeDesc";
        }
        else
        {
            dt.Columns[0].ColumnName = "ItemCode";
            dt.Columns[1].ColumnName = "ItemCodeDesc";
        }

        var qtyStartIndex = includeItemNo ? 3 : 2;
        var monthHeaders = Enumerable.Range(1, 12)
            .Select(m =>
            {
                var monthText = FormatMonthHeader(targetYear, m);
                return $"Qty({monthText})";
            })
            .ToList();

        for (var i = 0; i < 12; i++)
        {
            dt.Columns[qtyStartIndex + i].ColumnName = monthHeaders[i];
        }

        dt.Columns[qtyStartIndex + 12].ColumnName = "OnHand(EOM)";
        dt.Columns[qtyStartIndex + 13].ColumnName = "OnHand(Current)";
        dt.Columns[qtyStartIndex + 14].ColumnName = "JFI(Rinku)";
        dt.Columns[qtyStartIndex + 15].ColumnName = "SO(Current)";
        dt.Columns[qtyStartIndex + 16].ColumnName = "PO (Current)";
    }

    private static string FormatMonthHeader(int targetYear, int month)
    {
        return new DateTime(targetYear, month, 1).ToString("MMM\\'yy", CultureInfo.InvariantCulture);
    }
    private static void ApplyMonthlySalesAndPurchasesReportHeaderNames(DataTable dt, int targetYear)
    {
        const int fixedColumnCount = 8;
        const int monthCount = 12;
        const int columnsPerMonth = 2;
        var requiredColumnCount = fixedColumnCount + (monthCount * columnsPerMonth);

        if (dt.Columns.Count < requiredColumnCount)
        {
            return;
        }

        for (var month = 1; month <= monthCount; month++)
        {
            var monthText = FormatMonthHeader(targetYear, month);
            var qtyColumnIndex = fixedColumnCount + ((month - 1) * columnsPerMonth);
            var amtColumnIndex = qtyColumnIndex + 1;

            dt.Columns[qtyColumnIndex].ColumnName = $"Qty({monthText})";
            dt.Columns[amtColumnIndex].ColumnName = $"Amt({monthText})";
        }
    }
    private static void AddMonthlySalesAndPurchasesTotals(XLWorkbook workbook)
    {
        var ws = workbook.Worksheet("Export");
        var lastDataRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        if (lastDataRow < 2)
        {
            return;
        }

        var totalDefinitions = new (string Label, string Criteria)[]
        {
            ("Total Shipped:", "1.Shipped"),
            ("Total Received:", "2.Received"),
            ("Total Transferred:", "3.Transfer"),
            ("Total Inventory:", "4.Inventory")
        };

        var labelColumn = XLHelper.GetColumnNumberFromLetter("H");
        var firstFormulaColumn = XLHelper.GetColumnNumberFromLetter("I");
        var lastFormulaColumn = XLHelper.GetColumnNumberFromLetter("AF");

        for (var i = 0; i < totalDefinitions.Length; i++)
        {
            var totalRow = lastDataRow + i + 1;
            var (label, criteria) = totalDefinitions[i];
            ws.Cell(totalRow, labelColumn).Value = label;

            for (var col = firstFormulaColumn; col <= lastFormulaColumn; col++)
            {
                var colLetter = XLHelper.GetColumnLetterFromNumber(col);
                ws.Cell(totalRow, col).FormulaA1 =
                    $"SUMIF($H$2:$H${lastDataRow},\"{criteria}\",{colLetter}2:{colLetter}{lastDataRow})";
            }
        }
    }
}