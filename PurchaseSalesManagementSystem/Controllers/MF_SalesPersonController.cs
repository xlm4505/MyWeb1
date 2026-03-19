using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class MF_SalesPersonController : Controller
{
    private readonly Repository_MF_SalesPerson _repo;

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