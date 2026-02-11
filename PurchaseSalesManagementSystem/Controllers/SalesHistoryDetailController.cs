using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class SalesHistoryDetailController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public SalesHistoryDetailController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult SalesHistoryDetail()
    {
        return View();
    }


}
