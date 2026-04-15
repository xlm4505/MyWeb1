using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Repository;

public class SalesHistoryDetailController : Controller
{
    private readonly Repository_SalesHistoryDetail _repo;

    public SalesHistoryDetailController(Repository_SalesHistoryDetail repo)
    {
        _repo = repo;
    }

    public IActionResult SalesHistoryDetail()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetCustomers()
    {
        var customers = _repo.GetCustomers();
        return Json(customers);
    }

    [HttpGet]
    public IActionResult GetItemCodes()
    {
        var items = _repo.GetItemCodes();
        return Json(items);
    }

    [HttpGet]
    public IActionResult GetItemDescs()
    {
        var items = _repo.GetItemDescs();
        return Json(items);
    }

    [HttpPost]
    public IActionResult Run(string customer, string itemCode, string itemDesc, string dateFrom, string dateTo)
    {
        var data = _repo.GetSalesHistoryDetail(
            customer ?? "",
            itemCode ?? "",
            itemDesc ?? "",
            dateFrom ?? "",
            dateTo ?? ""
        ).ToList();

        var exporter = new FormattedDataTableExcelExporter();
        var dt = exporter.ConvertToDataTableFast(data);
        var bytes = exporter.ExportDataTableWithFormatting(dt, "Sales History Detail", "SO");

        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        return File(
            bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Sales History Detail Report_{timestamp}.xlsx"
        );
    }
}
