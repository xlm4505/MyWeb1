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
        var exportToExcel = new FormattedDataTableExcelExporter();
        var excelBytes = exportToExcel.ExportDataTableWithFormatting(dataTable, "ItemCodeMaster", "PO");

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ItemCodeMaster_{DateTime.Now:yyMMdd_HHmmss}.xlsx"
        );
    }
}