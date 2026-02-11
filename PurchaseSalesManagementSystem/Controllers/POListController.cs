using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Repository;
using PurchaseSalesManagementSystem.Models;

public class POListController : Controller
{
    private readonly Repository_POSeizo _repo;

    public POListController(Repository_POSeizo repo)
    {
        _repo = repo;
    }

    public IActionResult POListExport()
    {
        return View();
    }

    //[HttpGet]
    //public IActionResult GetVendors()
    //{
    //    var vendors = _repo.GetVendors();
    //    return Json(vendors);
    //}

    //[HttpGet]
    //public IActionResult GetUser()
    //{
    //    var users = _repo.GetUser();
    //    return Json(users);
    //}


}
