using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class SafetyStockMaintenanceController : Controller
{

    private readonly Repository_SafetyStockMaintenance _repo;
    public SafetyStockMaintenanceController(Repository_SafetyStockMaintenance repo)
    {
        _repo = repo;
    }


    public IActionResult SafetyStockMaintenance()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Search(string? itemCode)
    {
        var items = _repo.GetForecastItems(itemCode);
        return Json(items);
    }

    [HttpPost]
    public IActionResult Update([FromBody] List<Model_SafetyStockMaintenance>? items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = true, updatedCount = 0, message = "No rows selected." });
        }

        var updatedCount = _repo.UpdateForecastItems(items);
        return Json(new { success = true, updatedCount });
    }

    [HttpPost]
    public IActionResult Delete([FromBody] List<Model_SafetyStockMaintenance>? items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = true, deletedCount = 0, message = "No rows selected." });
        }

        var deletedCount = _repo.DeleteForecastItems(items);
        return Json(new { success = true, deletedCount });
    }
}