using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Repository;
using System.Data;

public class FOAInventoryController : Controller
{
    private readonly Repository_FOAInventory _repo;

    public FOAInventoryController(Repository_FOAInventory repo)
    {
        _repo = repo;
    }

    public IActionResult FOAInventory()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetWareHouseList()
    {
        var list = _repo.GetWareHouseList();
        return Json(list);
    }

    [HttpGet]
    public IActionResult Search(string? itemCode, string? wareHouse, bool minusOnly)
    {
        var items = _repo.GetFOAInventory(itemCode, wareHouse, minusOnly);
        return Json(items);
    }

    [HttpGet]
    public IActionResult ExportToExcel(string? itemCode, string? wareHouse, bool minusOnly)
    {
        var items = _repo.GetFOAInventory(itemCode, wareHouse, minusOnly);

        var dataForExcel = items.Select(m => new
        {
            m.ProdLn,
            m.ItemCode,
            m.ItemCodeDesc,
            m.WHSE,
            m.OnHand,
            QtyPO      = m.QtyPO,
            m.StandardUnitCost,
            m.LastSoldDate,
            m.LastReceiptDate,
            m.LastTotalUnitCost,
        });

        var exporter = new FormattedDataTableExcelExporter();
        DataTable dt = exporter.ConvertToDataTableFast(dataForExcel);

        if (dt.Columns.Contains("QtyPO"))
            dt.Columns["QtyPO"]!.ColumnName = "Qty PO";

		var workbook = new XLWorkbook();

		workbook = exporter.ExportDataTableWithFormattingForWorkbook(workbook, dt, "InvData", "PO");

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return File(
            stream.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"FOA Inventory_{DateTime.Now:yyMMdd_HHmmss}.xlsx");

    }
}
