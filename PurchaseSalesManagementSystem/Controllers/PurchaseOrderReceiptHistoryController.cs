using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class PurchaseOrderReceiptHistoryController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public PurchaseOrderReceiptHistoryController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult PurchaseOrderReceiptHistory()
    {
        return View();
    }


}
