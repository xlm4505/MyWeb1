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
        using var workbook = exportToExcel.ExportDataTableWithFormattingForWorkbook_MoreHeards(dt, "SQL-EXEC", "PO");
        if (isSummaryAll)
        {
            AddMonthlySalesSummaryAllTotals(workbook);
            ApplyMonthlySalesSummaryAllLayout(workbook, targetYear);
        }
        else if (isSummaryFastSelling)
        {
            AddMonthlySalesSummaryFastSellingTotals(workbook);
            ApplyMonthlySalesSummaryFastSellingLayout(workbook, targetYear);
        }
        else if (!isSummary)
        {
            InsertBlankRowsAfterInventoryRecords(workbook);
            AddMonthlySalesAndPurchasesTotals(workbook);
            ApplyMonthlySalesAndPurchasesLayout(workbook, targetYear);
        }
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var fileName = BuildExportFileName(exportTarget, targetData, timestamp);

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            fileName
        );
    }
    private static void ApplyMonthlySalesSummaryFastSellingLayout(XLWorkbook workbook, int targetYear)
    {
        var ws = workbook.Worksheet("SQL-EXEC");
        ws.Row(1).InsertRowsAbove(1);
        ws.SheetView.FreezeRows(2);

        for (var month = 1; month <= 12; month++)
        {
            ws.Cell(1, 3 + month).Value = FormatMonthHeader(targetYear, month);
            ws.Cell(2, 3 + month).Value = "Qty";
        }
        NormalizeHeaderRows(ws, 1, 2, 1, 20);
        ApplyTopBorder(ws, 3, 1, 20);
        ApplyVerticalBorder(ws, "C", ws.LastRowUsed()?.RowNumber() ?? 3);
        ApplyVerticalBorder(ws, "O", ws.LastRowUsed()?.RowNumber() ?? 3);

    }

    private static void ApplyMonthlySalesSummaryAllLayout(XLWorkbook workbook, int targetYear)
    {
        var ws = workbook.Worksheet("SQL-EXEC");
        ws.Row(1).InsertRowsAbove(1);
        ws.SheetView.FreezeRows(2);

        for (var month = 1; month <= 12; month++)
        {
            ws.Cell(1, 2 + month).Value = FormatMonthHeader(targetYear, month);
            ws.Cell(2, 2 + month).Value = "Qty";
        }
        NormalizeHeaderRows(ws, 1, 2, 1, 19);
        ApplyTopBorder(ws, 3, 1, 19);
        ApplyVerticalBorder(ws, "B", ws.LastRowUsed()?.RowNumber() ?? 3);
        ApplyVerticalBorder(ws, "N", ws.LastRowUsed()?.RowNumber() ?? 3);
    }

    private static void ApplyMonthlySalesAndPurchasesLayout(XLWorkbook workbook, int targetYear)
    {
        var ws = workbook.Worksheet("SQL-EXEC");
        ws.Row(1).InsertRowsAbove(1);
        ws.SheetView.FreezeRows(2);

        for (var month = 1; month <= 12; month++)
        {
            var qtyCol = 8 + ((month - 1) * 2) + 1;
            var amtCol = qtyCol + 1;
            ws.Range(1, qtyCol, 1, amtCol).Merge();
            ws.Cell(1, qtyCol).Value = FormatMonthHeader(targetYear, month);
            ws.Cell(2, qtyCol).Value = "Qty";
            ws.Cell(2, amtCol).Value = "Amt";
        }
        NormalizeHeaderRows(ws, 1, 2,  1, 32);
        ApplyTopBorder(ws, 3, 1, 32);
        foreach (var col in new[] { "H", "J", "L", "N", "P", "R", "T", "V", "X", "Z", "AB", "AD" })
        {
            ApplyVerticalBorder(ws, col, ws.LastRowUsed()?.RowNumber() ?? 3);
        }
    }
    private static void NormalizeHeaderRows(
         IXLWorksheet ws,
         int firstHeaderRow,
         int sourceHeaderRow,
         int startCol,
         int endCol)
    {
        for (var col = startCol; col <= endCol; col++)
        {
            var firstHeaderCell = ws.Cell(firstHeaderRow, col);
            var sourceHeaderCell = ws.Cell(sourceHeaderRow, col);
            var secondRowFont = ws.Cell(sourceHeaderRow, col).Style.Font;

            var backgroundColor = sourceHeaderCell.Style.Fill.BackgroundColor;
            var patternType = sourceHeaderCell.Style.Fill.PatternType;
            firstHeaderCell.Style.Fill.PatternType = patternType;
            firstHeaderCell.Style.Fill.BackgroundColor = backgroundColor;
            firstHeaderCell.Style.Font = secondRowFont;
        }

    }
    private static void ApplyTopBorder(IXLWorksheet ws, int row, int startCol, int endCol)
    {
        ws.Range(row, startCol, row, endCol).Style.Border.TopBorder = XLBorderStyleValues.Medium;
    }

    private static void ApplyVerticalBorder(IXLWorksheet ws, string colLetter, int lastRow)
    {
        var col = XLHelper.GetColumnNumberFromLetter(colLetter);
        ws.Range(1, col, lastRow, col).Style.Border.RightBorder = XLBorderStyleValues.Medium;
    }

    private static string BuildExportFileName(string exportTarget, string? targetData, string timestamp)
    {
        if (string.Equals(exportTarget, "summary", StringComparison.OrdinalIgnoreCase))
        {
            if (string.Equals(targetData, "ALL", StringComparison.OrdinalIgnoreCase))
            {
                return $"Monthly Sales Summary Report_All_{timestamp}.xlsx";
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
    private static void AddMonthlySalesSummaryFastSellingTotals(XLWorkbook workbook)
    {
        var ws = workbook.Worksheet("SQL-EXEC");
        var lastDataRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        if (lastDataRow < 2)
        {
            return;
        }

        var totalRow = lastDataRow + 1;
        var labelColumn = XLHelper.GetColumnNumberFromLetter("C");
        var firstFormulaColumn = XLHelper.GetColumnNumberFromLetter("D");
        var lastFormulaColumn = XLHelper.GetColumnNumberFromLetter("T");

        ws.Cell(totalRow, labelColumn).Value = "Total Shipped:";
        ws.Cell(totalRow, labelColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
        for (var col = firstFormulaColumn; col <= lastFormulaColumn; col++)
        {
            var colLetter = XLHelper.GetColumnLetterFromNumber(col);
            ws.Cell(totalRow, col).FormulaA1 =
                $"SUM(${colLetter}$2:${colLetter}${lastDataRow})";
        }
        ws.Range(totalRow, 4, totalRow, 20).Style.NumberFormat.Format = "#,##0";
        ws.Range(3, 1, totalRow, 1).Style.NumberFormat.Format = "#,##0";
        ws.Range(totalRow, 1, totalRow, 20).Style.Font.Bold = true;
        ApplyTopBorder(ws, totalRow, 1, 20);
    }
    private static void AddMonthlySalesSummaryAllTotals(XLWorkbook workbook)
    {
        var ws = workbook.Worksheet("SQL-EXEC");
        var lastDataRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        if (lastDataRow < 2)
        {
            return;
        }

        var totalRow = lastDataRow + 1;
        var labelColumn = XLHelper.GetColumnNumberFromLetter("B");
        var firstFormulaColumn = XLHelper.GetColumnNumberFromLetter("C");
        var lastFormulaColumn = XLHelper.GetColumnNumberFromLetter("S");

        ws.Cell(totalRow, labelColumn).Value = "Total Shipped:";
        ws.Cell(totalRow, labelColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

        for (var col = firstFormulaColumn; col <= lastFormulaColumn; col++)
        {
            var colLetter = XLHelper.GetColumnLetterFromNumber(col);
            ws.Cell(totalRow, col).FormulaA1 =
                $"SUM(${colLetter}$2:${colLetter}${lastDataRow})";
        }

        ws.Range(totalRow, 3, totalRow, 19).Style.NumberFormat.Format = "#,##0";
        ws.Range(totalRow, 1, totalRow, 19).Style.Font.Bold = true;
        ApplyTopBorder(ws, totalRow, 1, 19);

    }

    private static void AddMonthlySalesAndPurchasesTotals(XLWorkbook workbook)
    {
        var ws = workbook.Worksheet("SQL-EXEC");
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
            var totalRow = lastDataRow + i + 2;
            var (label, criteria) = totalDefinitions[i];
            ws.Cell(totalRow, labelColumn).Value = label;
            ws.Cell(totalRow, labelColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

            for (var col = firstFormulaColumn; col <= lastFormulaColumn; col++)
            {
                var colLetter = XLHelper.GetColumnLetterFromNumber(col);
                ws.Cell(totalRow, col).FormulaA1 =
                    $"SUMIF($H$2:$H${lastDataRow},\"{criteria}\",{colLetter}2:{colLetter}{lastDataRow})";
            }
            ws.Range(totalRow, 9, totalRow, 32).Style.NumberFormat.Format = "#,##0";
            ws.Range(totalRow, 1, totalRow, 19).Style.Font.Bold = true;
        }

        ApplyTopBorder(ws, lastDataRow + 2, 1, 32);
    }
    private static void InsertBlankRowsAfterInventoryRecords(XLWorkbook workbook)
    {
        var ws = workbook.Worksheet("SQL-EXEC");
        var lastDataRow = ws.LastRowUsed()?.RowNumber() ?? 1;
        if (lastDataRow < 2)
        {
            return;
        }

        var targetColumn = XLHelper.GetColumnNumberFromLetter("H");
        for (var row = lastDataRow; row >= 2; row--)
        {
            var cellText = ws.Cell(row, targetColumn).GetString().Trim();
            if (!string.IsNullOrEmpty(cellText) && cellText.StartsWith("4", StringComparison.Ordinal))
            {
                ws.Row(row + 1).InsertRowsAbove(1);
            }
        }
    }
}