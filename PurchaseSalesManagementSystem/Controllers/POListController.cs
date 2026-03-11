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
        byte[] excelBytes;

        FormattedDataTableExcelExporter exportToExcelFinal = new FormattedDataTableExcelExporter();
        FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
        if ("Misc".Equals(exportTarget, StringComparison.OrdinalIgnoreCase))
        {
            var poList = _repo.GetPOListDataMisc(purchaseOrderNo, exportTarget).ToList();
            dt = exportToExcel.ConvertToDataTableFast(poList);
            excelBytes = exportToExcelFinal.ExportDataTableWithFormatting(dt, "CustItem", "PO");
        }
        else
        {
            var poList = _repo.GetPOListDataAllVendors(purchaseOrderNo, exportTarget).ToList();
            dt = exportToExcel.ConvertToDataTableFast(poList);
            if (dt.Columns.Contains("POLn"))
            {
                dt.Columns["POLn"].ColumnName = "PO-Ln";
            }

            if (dt.Columns.Contains("VenCostCM"))
            {
                dt.Columns["VenCostCM"].ColumnName = "VenCost(CM)";
            }
            excelBytes = exportToExcelFinal.ExportDataTableWithFormatting(dt, "OpenPOAll", "PO");
        }

        return File(excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"PO List ({exportTarget})_{DateTime.Now:yyMMdd_HHmmss}.xlsx");
    }
}