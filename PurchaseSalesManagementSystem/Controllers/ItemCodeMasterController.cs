using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Repository;

public class ItemCodeMasterController : Controller
{
    private readonly Repository_ItemCodeMaster _repo;

    public ItemCodeMasterController(Repository_ItemCodeMaster repo)
    {
        _repo = repo;
    }

    public IActionResult ItemCodeMaster()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetItemCodeMasterData(string? itemCode, bool excludeInactiveItems = false)
    {
        var itemCodeMaster = _repo.GetItemCodeMasterData(itemCode, excludeInactiveItems);
        return Json(itemCodeMaster);
    }

    [HttpGet]
    public IActionResult ExportToExcel(string? itemCode, bool excludeInactiveItems = false)
    {
        var dataTable = _repo.GetItemCodeMasterDataTable(itemCode, excludeInactiveItems);
        var excelBytes = ExportItemCodeMasterExcel(dataTable);

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ItemCodeMaster_{DateTime.Now:yyMMdd_HHmmss}.xlsx"
        );
    }

    private static byte[] ExportItemCodeMasterExcel(DataTable dataTable)
    {
        var exporter = new FormattedDataTableExcelExporter();
        using var workbook = exporter.ExportDataTableWithFormattingForWorkbook(dataTable, "ItemCode", "PO");
        var worksheet = workbook.Worksheet("ItemCode");

        worksheet.Row(1).InsertRowsAbove(1);
        worksheet.SheetView.FreezeRows(2);

        var groupHeaderRanges = new (string Range, string Title)[]
        {
            ("B1:K1", "Product Information"),
            ("L1:Q1", "Unit Price / Cost"),
            ("R1:V1", "Inventory (Regular Items)"),
            ("W1:AA1", "Inventory (Excluded Items)"),
            ("AB1:AC1", "Last Transaction Date"),
            ("AE1:AH1", "Database Access Information"),
            ("AI1:AO1", "Master Price List")
        };

        var groupedHeaderStyle = worksheet.Row(2).Style;
        var topHeaderRow = worksheet.Row(1);
        topHeaderRow.Style = groupedHeaderStyle;
        topHeaderRow.Style.Font.Bold = true;
        topHeaderRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        topHeaderRow.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

        foreach (var (rangeAddress, title) in groupHeaderRanges)
        {
            var range = worksheet.Range(rangeAddress);
            range.Merge();
            range.Value = title;
            range.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            range.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            range.Style.Font.Bold = true;
        }

        worksheet.Cell("A1").Value = string.Empty;
        worksheet.Cell("AD1").Value = string.Empty;
        worksheet.Row(1).Height = 22;
        worksheet.Row(2).Height = 22;
        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}