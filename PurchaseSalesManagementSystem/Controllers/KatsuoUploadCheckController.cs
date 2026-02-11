using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class KatsuoUploadCheckController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public KatsuoUploadCheckController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult KatsuoUploadCheck()
    {
        return View();
    }


}
