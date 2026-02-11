using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class RAUploadController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public RAUploadController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult RAUpload()
    {
        return View();
    }


}
