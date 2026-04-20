using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Repository;
using System.Data;

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

        if (string.Equals(exportTarget, "summary", StringComparison.OrdinalIgnoreCase))
        {
            var isAll = string.Equals(targetData, "ALL", StringComparison.OrdinalIgnoreCase);
            dt = _repo.GetMonthlySalesSummary(targetYear, isAll);
            ApplyMonthlyHeaderNames(dt, targetYear, includeItemNo: !isAll);
        }
        else
        {
            dt = _repo.GetMonthlySalesAndPurchasesReport(targetYear);
        }

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add(dt, "Export");
        worksheet.Columns().AdjustToContents();
        worksheet.SheetView.FreezeRows(1);

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);

        var fileLabel = string.Equals(exportTarget, "summary", StringComparison.OrdinalIgnoreCase)
            ? "Monthly Sales Summary"
            : "Monthly Sales and Purchases Report";

        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{fileLabel}_{targetYear}_{DateTime.Now:yyMMdd_HHmmss}.xlsx"
        );
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
            .Select(m => new DateTime(targetYear, m, 1).ToString("MMM'yy"))
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
}