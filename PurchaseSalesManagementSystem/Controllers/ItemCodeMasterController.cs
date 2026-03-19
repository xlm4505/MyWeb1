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
    public IActionResult GetItemCodeMasterData()
    {
        var itemCodeMaster = _repo.GetItemCodeMasterData();
        return Json(itemCodeMaster);
    }

    [HttpGet]
    public IActionResult ExportToExcel()
    {
        var dataTable = _repo.GetItemCodeMasterDataTable();
        var exportToExcel = new FormattedDataTableExcelExporter();
        var excelBytes = exportToExcel.ExportDataTableWithFormatting(dataTable, "ItemCodeMaster", "PO");

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ItemCodeMaster_{DateTime.Now:yyMMdd_HHmmss}.xlsx"
        );
    }
}