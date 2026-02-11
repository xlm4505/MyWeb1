using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class PurchaseReceiptController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public PurchaseReceiptController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult PurchaseReceipt()
    {
        return View();
    }


}
