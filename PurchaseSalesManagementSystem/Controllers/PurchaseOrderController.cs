using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Data;

public class PurchaseOrderController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public PurchaseOrderController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult PurchaseOrderExport()
    {
        return View();
    }


    [HttpGet]
    public IActionResult GetVendors()
    {
        var vendors = _repo.GetVendors();
        return Json(vendors); 
    }


    [HttpGet]
    public IActionResult ExportToExcel(string reportName, string vendor, string productType, string vendorName)
    {


        // Mass Flow ÇÃÇ∆Ç´ÇæÇØ
        Dictionary<string, string> salesPersonMap = new();

        if (productType != "Valves")
        {
            salesPersonMap = _repo
            .GetAllSalesPersons()
            .OrderBy(x => x.CustomerNo)     
            .ThenBy(x => x.SalesPerson)      
            .GroupBy(x => x.CustomerNo)
            .ToDictionary(
                g => g.Key,
                g => g.First().SalesPerson
            );
        }


        // SQL é¿çs
        var vendorParam = string.IsNullOrEmpty(vendor) ? "00-0000000" : vendor;

        var purchaseOrder = _repo.GetPurchaseOrder(vendorParam, productType);

        // Åö Mass Flow ÇÃèÍçáÇÕ 50 ï™äÑ
        if (productType != "Valves")
        {
            purchaseOrder = SplitQtyForMassFlow(purchaseOrder);
        }

        FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
        DataTable dt = new DataTable();

        dt = exportToExcel.ConvertToDataTableFast(purchaseOrder);

        var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt, "PO");

        string fillName = "";
        if ("Valves".Equals(productType))
        {
            fillName = "PO Upload Data (Valves-";
        }
        else {
            fillName = "PO Upload Data (Mass Flow-";
        }
        return File(excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{fillName}{vendorName})_{DateTime.Now:yyMMdd_HHmmss}.xlsx");

    }

    private List<Model_PurchaseOrder> SplitQtyForMassFlow(IEnumerable<Model_PurchaseOrder> source)
    {
        var result = new List<Model_PurchaseOrder>();

        foreach (var d in source)
        {
            int qty = d.PurchaseOrderQty ?? 0;

            if (qty <= 50)
            {
                result.Add(d);
                continue;
            }

            int remain = qty;

            while (remain > 0)
            {
                int take = Math.Min(50, remain);

                result.Add(new Model_PurchaseOrder
                {
                    ConfirmTo = d.ConfirmTo,
                    SalesPerson = d.SalesPerson,
                    ItemCode = d.ItemCode,
                    ItemCodeDesc = d.ItemCodeDesc,
                    CustReqDate = d.CustReqDate,
                    POReqDate = d.POReqDate,
                    PurchaseOrderQty = take,
                    WarehouseCode = d.WarehouseCode,
                    CustomerNo = d.CustomerNo,
                    BillToName = d.BillToName,
                    SalesOrderNo = d.SalesOrderNo,
                    SalesOrderEntryDate = d.SalesOrderEntryDate,
                    VendorNo = d.VendorNo,
                    Message = d.Message,
                    AliasItemNo = d.AliasItemNo,
                    CustomerPONo = d.CustomerPONo
                });

                remain -= take;
            }
        }

        return result;
    }


}
