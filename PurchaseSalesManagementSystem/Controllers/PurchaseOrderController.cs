using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Data;

public class PurchaseOrderController : Controller
{

    private readonly Repository_PurchaseOrder _repo;
    public PurchaseOrderController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }


    public IActionResult PurchaseOrderExport()
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
    public IActionResult ExportToExcel(string reportName, string vendor, string productType, string vendorName)
    {


        // Mass Flow のときだけ
        Dictionary<string, string> salesPersonMap = new();

        if (productType != "Valves")
        {
            salesPersonMap = _repo
            .GetAllSalesPersons()
            .OrderBy(x => x.CustomerNo)     
            .ThenBy(x => x.SalesPerson)      
            .GroupBy(x => x.CustomerNo)
            .ToDictionary(
                g => g.Key,
                g => g.First().SalesPerson
            );
        }


        // SQL 実行
        var vendorParam = string.IsNullOrEmpty(vendor) ? "00-0000000" : vendor;

        var purchaseOrder = _repo.GetPurchaseOrder(vendorParam, productType);

        // ★ Mass Flow の場合は 50 分割
        if (productType != "Valves")
        {
            purchaseOrder = SplitQtyForMassFlow(purchaseOrder);
        }

        FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
        DataTable dt = new DataTable();

        dt = exportToExcel.ConvertToDataTableFast(purchaseOrder);

        var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt, "PO");

        //using (var wb = new XLWorkbook())
        //{
            //var ws = wb.Worksheets.Add();

            //int col = 1;

            //// ヘッダー
            //ws.Cell(1, col++).Value = "Seq";
            //ws.Cell(1, col++).Value = "ConfirmTo";
            //ws.Cell(1, col++).Value = "SalesPerson";
            //ws.Cell(1, col++).Value = "ItemCode";
            //ws.Cell(1, col++).Value = "ItemCodeDesc";
            //ws.Cell(1, col++).Value = "CustReqDate";
            //ws.Cell(1, col++).Value = "POReqDate";
            //ws.Cell(1, col++).Value = "PurchaseOrderQty";
            //ws.Cell(1, col++).Value = "WarehouseCode";
            //ws.Cell(1, col++).Value = "CustomerNo";
            //ws.Cell(1, col++).Value = "BillToName";
            //ws.Cell(1, col++).Value = "SalesOrderNo";
            //ws.Cell(1, col++).Value = "SalesOrderEntryDate";
            //ws.Cell(1, col++).Value = "VendorNo";
            //ws.Cell(1, col++).Value = "Message";
            //ws.Cell(1, col++).Value = "AliasItemNo";
            //ws.Cell(1, col++).Value = "CustomerPONo";

            ////// ヘッダー
            ////ws.Range(1, 1, 1, col - 1).Style
            ////    .Font.SetBold()
            ////    .Fill.SetBackgroundColor(XLColor.FromArgb(242, 242, 242))
            ////    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            //int row = 2;

            //// データ行
            //foreach (var d in purchaseOrder)
            //{
            //    col = 1;

            //    ws.Cell(row, col++).Value = 1;
            //    ws.Cell(row, col++).Value = d.ConfirmTo;
            //    ws.Cell(row, col++).Value = d.SalesPerson;
            //    ws.Cell(row, col++).Value = d.ItemCode;
            //    ws.Cell(row, col++).Value = d.ItemCodeDesc;

            //    if (d.CustReqDate.HasValue)
            //    {
            //        ws.Cell(row, col).Value = d.CustReqDate.Value;
            //        //ws.Cell(row, col).Style.DateFormat.Format = "yyyy/M/d";
            //        //ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //    }
            //    col++;

            //    if (d.POReqDate.HasValue)
            //    {
            //        ws.Cell(row, col).Value = d.POReqDate.Value;
            //        //ws.Cell(row, col).Style.DateFormat.Format = "yyyy/M/d";
            //        //ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //    }
            //    col++;


            //    ws.Cell(row, col++).Value = d.PurchaseOrderQty ?? 0;

            //    ws.Cell(row, col++).Value = d.WarehouseCode;
            //    ws.Cell(row, col++).Value = d.CustomerNo;
            //    ws.Cell(row, col++).Value = d.BillToName;
            //    ws.Cell(row, col++).Value = d.SalesOrderNo;


            //    if (d.SalesOrderEntryDate.HasValue)
            //    {
            //        ws.Cell(row, col).Value = d.SalesOrderEntryDate.Value;
            //        //ws.Cell(row, col).Style.DateFormat.Format = "yyyy/M/d";
            //        //ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            //    }
            //    col++;

            //    ws.Cell(row, col++).Value = d.VendorNo;
            //    //ws.Cell(row, col++).Value = d.Message;
            //    ws.Cell(row, col++).Value = d.Message?.Trim() ?? "";
            //    ws.Cell(row, col++).Value = d.AliasItemNo;
            //    ws.Cell(row, col++).Value = d.CustomerPONo;

            //    row++;
            //}

            //var lastRowUsed = ws.LastRowUsed();
            //if (lastRowUsed != null && lastRowUsed.RowNumber() >= 2)
            //{
            //    int lastRow = lastRowUsed.RowNumber();

            //    int seq = 1;
            //    ws.Cell(2, 1).Value = seq;

            //    for (int r = 3; r <= lastRow; r++)
            //    {
            //        bool isNewGroup = productType == "Valves"
            //            ? ws.Cell(r, 14).GetString() != ws.Cell(r - 1, 14).GetString()   // VendorNo
            //            : ws.Cell(r, 10).GetString() != ws.Cell(r - 1, 10).GetString(); // CustomerNo

            //        if (isNewGroup)
            //        {
            //            seq++;
            //        }

            //        ws.Cell(r, 1).Value = seq;
            //    }

            //    if (productType != "Valves")
            //    {
            //        for (int r = 2; r <= lastRow; r++)
            //        {
            //            var customerNo = ws.Cell(r, 10).GetString(); // CustomerNo

            //            if (salesPersonMap.TryGetValue(customerNo, out var sp))
            //            {
            //                ws.Cell(r, 3).Value = sp; // SalesPerson
            //            }
            //        }
            //    }
            //}

            //ws.ShowGridLines = false;
            //ws.Name = "PO";

            //ws.Columns("F:G").Style.DateFormat.Format = "M/d/yyyy";
            //ws.Column("M").Style.DateFormat.Format = "M/d/yyyy";

            //ws.Column("I").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //ws.Column("D").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            //var header = ws.Range("A1:Q1");
            //header.Style.Font.Bold = true;
            //header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            //header.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            //header.Style.Border.BottomBorderColor = XLColor.Black;
            //header.Style.Fill.BackgroundColor = XLColor.FromArgb(242, 242, 242);

            //int lRow = ws.LastRowUsed().RowNumber();

            //var dataRange = ws.Range($"A2:Q{lRow}");
            //var borderColor = XLColor.FromArgb(208, 215, 229);

            //dataRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            //dataRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            //dataRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            //dataRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            //dataRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            //dataRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            //dataRange.Style.Border.LeftBorderColor = borderColor;
            //dataRange.Style.Border.RightBorderColor = borderColor;
            //dataRange.Style.Border.TopBorderColor = borderColor;
            //dataRange.Style.Border.BottomBorderColor = borderColor;

            //ws.Style.Font.FontName = "Calibri";
            //ws.Style.Font.FontSize = 10;
            //ws.Style.Alignment.WrapText = false;

            //ws.Columns().AdjustToContents();
            //ws.Rows().AdjustToContents();

            //ws.Column("P").Width = 20;


            //ws.SheetView.FreezeRows(1);
            //ws.Columns().AdjustToContents();



            string fillName = "";
            if ("Valves".Equals(productType))
            {
                fillName = "PO Upload Data (Valves-";
            }
            else {
                fillName = "PO Upload Data (Mass Flow-";
            }
            return File(excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{fillName}{vendorName})_{DateTime.Now:yyMMdd_HHmmss}.xlsx");

            //using (var stream = new MemoryStream())
            //    {
            //        wb.SaveAs(stream);
            //        return File(
            //            stream.ToArray(),
            //            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            //            $"{fillName}{vendorName})_{DateTime.Now:yyMMdd_HHmmss}.xlsx"
            //        );
            //    }
        //}
    }

    private List<Model_PurchaseOrder> SplitQtyForMassFlow(IEnumerable<Model_PurchaseOrder> source)
    {
        var result = new List<Model_PurchaseOrder>();

        foreach (var d in source)
        {
            int qty = d.PurchaseOrderQty ?? 0;

            if (qty <= 50)
            {
                result.Add(d);
                continue;
            }

            int remain = qty;

            while (remain > 0)
            {
                int take = Math.Min(50, remain);

                result.Add(new Model_PurchaseOrder
                {
                    ConfirmTo = d.ConfirmTo,
                    SalesPerson = d.SalesPerson,
                    ItemCode = d.ItemCode,
                    ItemCodeDesc = d.ItemCodeDesc,
                    CustReqDate = d.CustReqDate,
                    POReqDate = d.POReqDate,
                    PurchaseOrderQty = take,
                    WarehouseCode = d.WarehouseCode,
                    CustomerNo = d.CustomerNo,
                    BillToName = d.BillToName,
                    SalesOrderNo = d.SalesOrderNo,
                    SalesOrderEntryDate = d.SalesOrderEntryDate,
                    VendorNo = d.VendorNo,
                    Message = d.Message,
                    AliasItemNo = d.AliasItemNo,
                    CustomerPONo = d.CustomerPONo
                });

                remain -= take;
            }
        }

        return result;
    }


}
