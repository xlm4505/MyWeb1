using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class OrderForExportController : Controller
{

    private readonly Repository_OrderForExport _repo;
    public OrderForExportController(Repository_OrderForExport repo)
    {
        _repo = repo;
    }

	public IActionResult OrderForExport()
	{
		return View();
	}

	[HttpGet]
	public IActionResult GetOrderData(String salesOrderNo)
	{
		var orderData = _repo.GetOrderData(salesOrderNo);
		return Json(orderData);
	}

	[HttpGet]
	public IActionResult ExportToExcel(string salesOrderNo)
	{
		// 出力データ取得
		var orderData = _repo.GetOrderData(salesOrderNo);

		FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
		DataTable dt = new DataTable();

		dt = exportToExcel.ConvertToDataTableFast(orderData);

		var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt,"Report");

		return File(excelBytes,
			"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
			$"Order for export to Excel_{DateTime.Now:yyMMdd_HHmmss}.xlsx");

	}


}
