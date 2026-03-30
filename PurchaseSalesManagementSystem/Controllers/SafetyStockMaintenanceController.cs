using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Text.RegularExpressions;

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

    [HttpPost]
    public IActionResult Add([FromBody] Model_SafetyStockMaintenance? item)
    {
        if (item == null)
        {
            return BadRequest(new { success = false, message = "No data to register." });
        }

        if (string.IsNullOrWhiteSpace(item.ItemCode)
            || string.IsNullOrWhiteSpace(item.ProcType)
            || item.Quantity < 0)
        {
            return BadRequest(new { success = false, message = "ItemCode, ProcType and Quantity are required." });
        }
        if (!IsHalfWidthAlphaNumeric(item.ItemCode, 1, 30)
            || !IsHalfWidthAlphaNumeric(item.ProcType, 1, 1)
            || !IsHalfWidthAlphaNumericOptional(item.ARDivisionNo, 2)
            || !IsHalfWidthAlphaNumericOptional(item.CustomerNo, 20)
            || !IsHalfWidthAlphaNumericOptional(item.WarehouseCode, 3)
            || !IsHalfWidthAlphaNumericOptional(item.Comment, 30))
        {
            return BadRequest(new { success = false, message = "Alphanumeric field format is invalid." });
        }
        _repo.InsertForecastItem(item);
        return Json(new { success = true });
    }
    private static bool IsHalfWidthAlphaNumeric(string value, int minLength, int maxLength)
    {
        var pattern = $@"^[A-Za-z0-9]{{{minLength},{maxLength}}}$";
        return Regex.IsMatch(value ?? string.Empty, pattern);
    }

    private static bool IsHalfWidthAlphaNumericOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return IsHalfWidthAlphaNumeric(value, 1, maxLength);
    }
}