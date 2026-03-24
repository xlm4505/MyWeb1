using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class Tbl_VendorController : Controller
{
    private readonly Repository_Tbl_VendorMaintenance _repo;

    public Tbl_VendorController(Repository_Tbl_VendorMaintenance repo)
    {
        _repo = repo;
    }

    public IActionResult Tbl_Vendor()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Search(string? id)
    {
        var items = _repo.GetVendors(id);
        return Json(items);
    }

    [HttpPost]
    public IActionResult Update([FromBody] List<Model_Tbl_VendorMaintenance>? items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = true, updatedCount = 0, message = "No rows selected." });
        }

        var updatedCount = _repo.UpdateVendors(items);
        return Json(new { success = true, updatedCount });
    }

    [HttpPost]
    public IActionResult Delete([FromBody] List<Model_Tbl_VendorMaintenance>? items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = true, deletedCount = 0, message = "No rows selected." });
        }

        var deletedCount = _repo.DeleteVendors(items);
        return Json(new { success = true, deletedCount });
    }
}