using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Repository;
using System.Data;

public class POListController : Controller
{
    private readonly Repository_POList _repo;

    public POListController(Repository_POList repo)
    {
        _repo = repo;
    }

    public IActionResult POListExport()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetPOListData(string purchaseOrderNo, string exportTarget)
    {
        var poList = _repo.GetPOListData(purchaseOrderNo, exportTarget);
        return Json(poList);
    }

    [HttpGet]
    public IActionResult ExportToExcel(string purchaseOrderNo, string exportTarget)
    {
        var poList = _repo.GetPOListData(purchaseOrderNo, exportTarget);

        FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
        DataTable dt = exportToExcel.ConvertToDataTableFast(poList);
        var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt, "Report");

        return File(excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"PO List ({exportTarget})_{DateTime.Now:yyMMdd_HHmmss}.xlsx");
    }
}