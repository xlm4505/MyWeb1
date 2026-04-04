using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Text.RegularExpressions;

public class MF_SalesPersonController : Controller
{
    private readonly Repository_MF_SalesPerson _repo;
    private static readonly Regex SalesPersonRegex = new("^[A-Za-z0-9]{0,50}$", RegexOptions.Compiled);
    public MF_SalesPersonController(Repository_MF_SalesPerson repo)
    {
        _repo = repo;
    }

    public IActionResult MF_SalesPerson()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Search(string? customerCode)
    {
        var items = _repo.GetSalesPersons(customerCode);
        return Json(items);
    }

    [HttpPost]
    public IActionResult Update([FromBody] List<Model_SalesPerson>? items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = true, updatedCount = 0, message = "No rows selected." });
        }
        if (items.Any(x => !SalesPersonRegex.IsMatch(x.SalesPerson ?? string.Empty)))
        {
            return BadRequest(new { success = false, message = "SalesPerson must be up to 50 half-width alphanumeric characters." });
        }

        var updatedCount = _repo.UpdateSalesPersons(items);
        return Json(new { success = true, updatedCount });
    }

    [HttpPost]
    public IActionResult Delete([FromBody] List<Model_SalesPerson>? items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = true, deletedCount = 0, message = "No rows selected." });
        }

        var deletedCount = _repo.DeleteSalesPersons(items);
        return Json(new { success = true, deletedCount });
    }
}