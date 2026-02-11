using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class MF_SalesPersonController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public MF_SalesPersonController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult MF_SalesPerson()
    {
        return View();
    }


}
