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

		var orderDataForExcel = orderData.Select(m => new
		{
			m.SalesOffice,
			m.SalesOrderNo,
			m.OrderDate,
			m.OrderType,
			m.OrderStatus,
			m.CustomerPONo,
			m.CustomerNo,
			m.BillToName,
			m.ShipToCity,
			m.ShipVia,
			m.HeaderComment,
			m.CustPO_Ln,
			m.ItemCode,
			m.ItemDescription,
			m.AliasItemNo,
			m.Whs,
			m.Weight,
			m.Ordded,
			m.Shipped,
			m.BO,
			m.UnitPrice,
			m.ExtensionAmt,
			m.ReqDate,
			m.PushOut,
			m.PromiseDate,
			m.CommitDate,
			m.DeliveryDate,
			m.CommentText,
			m.UnitCost,
			m.PurchaseOrderNo,
			m.UDF_CUSTPONO,
			m.InternalNotes
		});

		FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
		DataTable dt = new DataTable();

		dt = exportToExcel.ConvertToDataTableFast(orderDataForExcel);

		var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt,"Report","SO");



		return File(excelBytes,
			"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
			$"Order for export to Excel_{DateTime.Now:yyMMdd_HHmmss}.xlsx");

	}


}
