using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class FOAInventoryController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public FOAInventoryController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult FOAInventory()
    {
        return View();
    }


}
