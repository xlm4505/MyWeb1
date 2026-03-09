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
        if ("Misc".Equals(exportTarget, StringComparison.OrdinalIgnoreCase))
        {
            var poList = _repo.GetPOListDataMisc(purchaseOrderNo, exportTarget);

            return Json(poList);
        }
        else
        {
            var poList = _repo.GetPOListDataAllVendors(purchaseOrderNo, exportTarget);
            return Json(poList);
        }
    }

    [HttpGet]
    public IActionResult ExportToExcel(string purchaseOrderNo, string exportTarget)
    {
        DataTable dt;
        if ("Misc".Equals(exportTarget, StringComparison.OrdinalIgnoreCase))
        {
            var poList = _repo.GetPOListDataMisc(purchaseOrderNo, exportTarget).ToList();
            FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
            dt = exportToExcel.ConvertToDataTableFast(poList);
        }
        else
        {
            var poList = _repo.GetPOListDataAllVendors(purchaseOrderNo, exportTarget).ToList();
            FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
            dt = exportToExcel.ConvertToDataTableFast(poList);

        }
        FormattedDataTableExcelExporter exportToExcelFinal = new FormattedDataTableExcelExporter();
        var excelBytes = exportToExcelFinal.ExportDataTableWithFormatting(dt, "Report");

        return File(excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"PO List ({exportTarget})_{DateTime.Now:yyMMdd_HHmmss}.xlsx");
    }
}