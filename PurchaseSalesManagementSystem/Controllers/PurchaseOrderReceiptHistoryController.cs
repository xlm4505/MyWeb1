using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class PurchaseOrderReceiptHistoryController : Controller
{

    private readonly Repository_PurchaseOrderReceiptHistory _repo;
    public PurchaseOrderReceiptHistoryController(Repository_PurchaseOrderReceiptHistory repo)
    {
        _repo = repo;
    }


    public IActionResult PurchaseOrderReceiptHistory()
    {
        return View();
    }

    [HttpGet]
    public IActionResult GetVendors()
    {
        var vendors = _repo.GetVendors();
        return Json(vendors);
    }

    [HttpGet]
    public IActionResult GetItems()
    {
        var items = _repo.GetItems();
        return Json(items);
    }

    [HttpGet]
    public IActionResult GetUsers()
    {
        var users = _repo.GetUsers();
        return Json(users);
    }

    [HttpGet]
    public IActionResult ExportToExcel(string dateFrom, string dateTo, string vendorCode, string poNo, string invoiceNo, string receiptNo, string itemCode, string userName)
    {
        // 出力データ取得
        var list = _repo.GetPOReceiptHistory(dateFrom, dateTo, vendorCode, poNo, invoiceNo, receiptNo, itemCode, userName).ToList();

        var exportToExcel = new FormattedDataTableExcelExporter();
        var dt = exportToExcel.ConvertToDataTableFast(list);
        if (dt.Columns.Contains("PONo"))
        {
            dt.Columns["PONo"]!.ColumnName = "PO No";
        }
        if (dt.Columns.Contains("VendorNo"))
        {
            dt.Columns["VendorNo"]!.ColumnName = "Vendor No";
        }
        if (dt.Columns.Contains("VendorName"))
        {
            dt.Columns["VendorName"]!.ColumnName = "Vendor Name";
        }
        if (dt.Columns.Contains("PODate"))
        {
            dt.Columns["PODate"]!.ColumnName = "PO Date";
        }
        if (dt.Columns.Contains("ReceiptNo"))
        {
            dt.Columns["ReceiptNo"]!.ColumnName = "Receipt No";
        }
        if (dt.Columns.Contains("ReceiptDate"))
        {
            dt.Columns["ReceiptDate"]!.ColumnName = "Receipt Date";
        }
        if (dt.Columns.Contains("InvoiceNo"))
        {
            dt.Columns["InvoiceNo"]!.ColumnName = "Invoice No";
        }
        if (dt.Columns.Contains("InvoiceDate"))
        {
            dt.Columns["InvoiceDate"]!.ColumnName = "Invoice Date";
        }
        if (dt.Columns.Contains("InvoiceTotal"))
        {
            dt.Columns["InvoiceTotal"]!.ColumnName = "Invoice Total";
        }
        if (dt.Columns.Contains("LineKey"))
        {
            dt.Columns["LineKey"]!.ColumnName = "Line Key";
        }
        if (dt.Columns.Contains("ItemCode"))
        {
            dt.Columns["ItemCode"]!.ColumnName = "Item Code";
        }
        if (dt.Columns.Contains("ItemDescription"))
        {
            dt.Columns["ItemDescription"]!.ColumnName = "Item Description";
        }
        if (dt.Columns.Contains("QtyRcvd"))
        {
            dt.Columns["QtyRcvd"]!.ColumnName = "Qty Rcvd";
        }
        if (dt.Columns.Contains("UnitCost"))
        {
            dt.Columns["UnitCost"]!.ColumnName = "Unit Cost";
        }
        if (dt.Columns.Contains("ExtensionAmt"))
        {
            dt.Columns["ExtensionAmt"]!.ColumnName = "Extension Amt";
        }
        if (dt.Columns.Contains("SONo"))
        {
            dt.Columns["SONo"]!.ColumnName = "SO No";
        }
        if (dt.Columns.Contains("UserName"))
        {
            dt.Columns["UserName"]!.ColumnName = "User Name";
        }
        if (dt.Columns.Contains("PostingDate"))
        {
            dt.Columns["PostingDate"]!.ColumnName = "Posting Date";
        }
        if (dt.Columns.Contains("OperationDate"))
        {
            dt.Columns["OperationDate"]!.ColumnName = "Operation Date";
        }


        var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt, "SQL-EXEC", "PO");

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"Purchase Order Receipt History Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );

    }

}
