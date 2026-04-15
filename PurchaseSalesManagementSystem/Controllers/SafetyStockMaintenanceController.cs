using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Text.RegularExpressions;

public class SafetyStockMaintenanceController : Controller
{
    private const decimal QuantityMinValue = -9999999;
    private const decimal QuantityMaxValue = 9999999;
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
        var hasInvalidItem = items.Any(item =>
            !IsQuantityInRange(item.Quantity)
            || !IsHalfWidthAlphaNumericOptional(item.CustomerNo, 20)
            || !IsHalfWidthAlphaNumericOptional(item.WarehouseCode, 3)
            || !IsHalfWidthOptional(item.Comment, 30));

        if (hasInvalidItem)
        {
            return BadRequest(new
            {
                success = false,
                message = "CustomerNo/WarehouseCode must be half-width alphanumeric and Comment must be half-width within allowed length. Quantity must be between -9999999 and 9999999."
            });
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
            || !IsQuantityInRange(item.Quantity))
        {
            return BadRequest(new { success = false, message = "ItemCode and ProcType are required. Quantity must be between -9999999 and 9999999." });
        }
        if (!IsHalfWidthAlphaNumeric(item.ItemCode, 1, 30)
            || !IsHalfWidthAlphaNumeric(item.ProcType, 1, 1)
            || !IsHalfWidthAlphaNumericOptional(item.ARDivisionNo, 2)
            || !IsHalfWidthAlphaNumericOptional(item.CustomerNo, 20)
            || !IsHalfWidthAlphaNumericOptional(item.WarehouseCode, 3)
            || !IsHalfWidthOptional(item.Comment, 30))
        {
            return BadRequest(new { success = false, message = "Alphanumeric field format is invalid. Comment must be half-width within 30 characters." });
        }
        if (_repo.ExistsForecastItemByItemCode(item.ItemCode))
        {
            return BadRequest(new { success = false, message = "The entered ItemCode already exists." });
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

    private static bool IsHalfWidthOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        var pattern = $@"^[\x20-\x7E]{{1,{maxLength}}}$";
        return Regex.IsMatch(value, pattern);
    }

    private static bool IsQuantityInRange(decimal quantity)
    {
        return quantity >= QuantityMinValue && quantity <= QuantityMaxValue;
    }
}