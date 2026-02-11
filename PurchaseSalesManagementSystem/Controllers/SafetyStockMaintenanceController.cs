using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class SafetyStockMaintenanceController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public SafetyStockMaintenanceController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult SafetyStockMaintenance()
    {
        return View();
    }


}
