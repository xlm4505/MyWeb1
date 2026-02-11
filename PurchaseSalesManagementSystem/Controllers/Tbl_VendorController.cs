using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class Tbl_VendorController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public Tbl_VendorController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult Tbl_Vendor()
    {
        return View();
    }


}
