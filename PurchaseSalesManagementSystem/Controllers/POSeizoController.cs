using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class POSeizoController : Controller
{
    private readonly Repository_POSeizo _repo;

    public POSeizoController(Repository_POSeizo repo)
    {
        _repo = repo;
    }

    public IActionResult POSeizoExport()
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
    public IActionResult GetUser()
    {
        var users = _repo.GetUser();
        return Json(users);
    }

    [HttpGet]
    public IActionResult ExportToExcel_TKF(
        string reportName,
        string vendor,
        string userName,
        string orderStatus,
        string poEntryDate,
        string vendorName)
    {
        // Vendor が空 → ALL Vendors
        var vendorParam = string.IsNullOrEmpty(vendor) ? "00-0000000" : vendor;

        // SQL 実行
        var list = _repo.GetPOSeizo_TKF(
            vendorParam,
            userName,
            orderStatus,
            poEntryDate
        ).ToList();

        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("PO Seizo");

            int col = 1;

            // ヘッダー
            ws.Cell(1, col++).Value = "PO";
            ws.Cell(1, col++).Value = "Unit";
            ws.Cell(1, col++).Value = "Item Code";
            ws.Cell(1, col++).Value = "Description";
            ws.Cell(1, col++).Value = "Customer Part Number";
            ws.Cell(1, col++).Value = "Customer";
            ws.Cell(1, col++).Value = "WH Code";
            ws.Cell(1, col++).Value = "Required Delivery Date";
            ws.Cell(1, col++).Value = "Ordered Q'ty";
            ws.Cell(1, col++).Value = "Unit Price";
            ws.Cell(1, col++).Value = "Amount";

            ws.Range(1, 1, 1, col - 1).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromArgb(242, 242, 242))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            int row = 2;

            foreach (var d in list)
            {
                col = 1;

                ws.Cell(row, col++).Value = d.PO;
                ws.Cell(row, col++).Value = d.Unit;
                ws.Cell(row, col++).Value = d.ItemCode;
                ws.Cell(row, col++).Value = d.Description;
                ws.Cell(row, col++).Value = d.CustomerPartNumber;
                ws.Cell(row, col++).Value = d.Customer;
                ws.Cell(row, col++).Value = d.WHCode;

                ws.Cell(row, col++).Value = d.RequiredDeliveryDate;
                ws.Cell(row, col).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                ws.Cell(row, col++).Value = d.OrderedQty ?? 0;
                ws.Cell(row, col++).Value = d.UnitPrice ?? 0;
                ws.Cell(row, col++).Value = d.Amount ?? 0;

                row++;
            }

            ws.ShowGridLines = false;
            var header = ws.Range("A1:K1");
            header.Style.Font.Bold = true;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            header.Style.Border.BottomBorderColor = XLColor.Black;
            header.Style.Fill.BackgroundColor = XLColor.FromArgb(242, 242, 242);
            var lastRow = ws.LastRowUsed().RowNumber();
            var bodyRange = ws.Range($"A2:K{lastRow}");
            var borderColor = XLColor.FromArgb(208, 215, 229);

            bodyRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            bodyRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            bodyRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            bodyRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
            bodyRange.Style.Border.LeftBorderColor = borderColor;
            bodyRange.Style.Border.RightBorderColor = borderColor;
            bodyRange.Style.Border.InsideBorderColor = borderColor;
            ws.SheetView.FreezeRows(1);
            ws.Column("H").Style.DateFormat.Format = "MM/dd/yyyy";
            ws.Columns("J:K").Style.NumberFormat.Format = "#,##0.00";
            ws.Style.Font.FontName = "Calibri";
            ws.Style.Font.FontSize = 10;
            ws.Style.Font.FontName = "Calibri";
            ws.Style.Font.FontSize = 10;
            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                wb.SaveAs(stream);
                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{"PO_SeizoExport ["}{vendorName}{"]"}_{DateTime.Now:yyMMdd_HHmmss}.xlsx"
                );
            }
        }
    }


    [HttpGet]
    public IActionResult ExportToExcel_ALL(
    string reportName,
    string vendor,
    string userName,
    string orderStatus,
    string poEntryDate,
    string vendorName)
    {
        // Vendor が空 → ALL Vendors
        var vendorParam = string.IsNullOrEmpty(vendor)
            ? "00-0000000"
            : vendor;

        // SQL 実行
        var list = _repo.GetPOSeizo_ALL(
            vendorParam,
            userName,
            orderStatus,
            poEntryDate
        ).ToList();

        using (var wb = new XLWorkbook())
        {
            var ws = wb.Worksheets.Add("PO Seizo");

            int col = 1;

            // ============================
            // ヘッダー（通常版 全カラム）
            // ============================
            ws.Cell(1, col++).Value = "No";
            ws.Cell(1, col++).Value = "ItemCode";
            ws.Cell(1, col++).Value = "Desc";
            ws.Cell(1, col++).Value = "PartNumber";
            ws.Cell(1, col++).Value = "Unit";
            ws.Cell(1, col++).Value = "RequiredDate";
            ws.Cell(1, col++).Value = "EstDeliveryDate";
            ws.Cell(1, col++).Value = "QuantityOrdered";
            ws.Cell(1, col++).Value = "UnitCost";
            ws.Cell(1, col++).Value = "ExtensionAmt";
            ws.Cell(1, col++).Value = "SalesOffice";
            ws.Cell(1, col++).Value = "SalesClass";
            ws.Cell(1, col++).Value = "Customer";
            ws.Cell(1, col++).Value = "EndCustmer";
            ws.Cell(1, col++).Value = "ShipTo";
            ws.Cell(1, col++).Value = "PO";
            ws.Cell(1, col++).Value = "Line";
            ws.Cell(1, col++).Value = "Factory";
            ws.Cell(1, col++).Value = "Filled";
            ws.Cell(1, col++).Value = "Confirmed";
            ws.Cell(1, col++).Value = "Approved";
            ws.Cell(1, col++).Value = "DateApproved";
            ws.Cell(1, col++).Value = "Production ControlNotice";

            // ヘッダー
            ws.Range(1, 1, 1, col - 1).Style
                .Font.SetBold()
                .Fill.SetBackgroundColor(XLColor.FromArgb(242, 242, 242))
                .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

            // ============================
            // データ行
            // ============================
            int row = 2;

            foreach (var d in list)
            {
                col = 1;

                ws.Cell(row, col++).Value = d.No;

                ws.Cell(row, col++).Value = d.ItemCode;
                ws.Cell(row, col++).Value = d.Desc;
                ws.Cell(row, col++).Value = d.PartNumber;
                ws.Cell(row, col++).Value = d.Unit;

                ws.Cell(row, col++).Value = d.RequiredDate;

                ws.Cell(row, col++).Value = d.EstDeliveryDate;

                ws.Cell(row, col++).Value = d.QuantityOrdered ?? 0;
                ws.Cell(row, col++).Value = d.UnitCost ?? 0;
                ws.Cell(row, col++).Value = d.ExtensionAmt ?? 0;

                ws.Cell(row, col++).Value = d.SalesOffice;
                ws.Cell(row, col++).Value = d.SalesClass;

                ws.Cell(row, col++).Value = d.Customer;
                ws.Cell(row, col++).Value = d.EndCustmer;
                ws.Cell(row, col++).Value = d.ShipTo;

                ws.Cell(row, col++).Value = d.PO;
                ws.Cell(row, col++).Value = d.Line;

                ws.Cell(row, col++).Value = d.Factory;

                ws.Cell(row, col++).Value = d.Filled;
                ws.Cell(row, col++).Value = d.Confirmed;
                ws.Cell(row, col++).Value = d.Approved;

                ws.Cell(row, col++).Value = d.DateApproved;

                ws.Cell(row, col++).Value = d.ProductionControlNotice;

                row++;
            }

            ws.ShowGridLines = false;
            var header = ws.Range("A1:Y1");
            header.Style.Font.Bold = true;
            header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            header.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
            header.Style.Border.BottomBorderColor = XLColor.Black;
            header.Style.Fill.BackgroundColor = XLColor.FromArgb(242, 242, 242);
            var lastRow = ws.LastRowUsed().RowNumber();
            var bodyRange = ws.Range($"A2:Y{lastRow}");
            var borderColor = XLColor.FromArgb(208, 215, 229);

            bodyRange.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
            bodyRange.Style.Border.RightBorder = XLBorderStyleValues.Thin;
            bodyRange.Style.Border.TopBorder = XLBorderStyleValues.Thin;
            bodyRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

            bodyRange.Style.Border.LeftBorderColor = borderColor;
            bodyRange.Style.Border.RightBorderColor = borderColor;
            bodyRange.Style.Border.InsideBorderColor = borderColor;

            ws.SheetView.FreezeRows(1);
            ws.Column("F").Style.DateFormat.Format = "MM/dd/yyyy";
            ws.Column("V").Style.NumberFormat.Format = "[$-en-US]dd-MMM-yy";
            ws.Column("X").Style.DateFormat.Format = "MM/dd/yyyy";
            if (vendorParam == "06-0000200")
            {
                ws.Columns("I:J").Style.NumberFormat.Format = "#,##0";
            }
            else
            {
                ws.Columns("I:J").Style.NumberFormat.Format = "#,##0.00";
            }

            ws.Style.Font.FontName = "Calibri";
            ws.Style.Font.FontSize = 10;

            ws.Columns().AdjustToContents();
            ws.Rows().AdjustToContents();

            using (var stream = new MemoryStream())
            {
                wb.SaveAs(stream);

                return File(
                    stream.ToArray(),
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{"PO_SeizoExport ["}{vendorName}{"]"}_{DateTime.Now:yyMMdd_HHmmss}.xlsx"
                );
            }
        }
    }

    [HttpPost]
    public IActionResult RunPrint([FromBody] Model_POSeizo_Check model)
    {
        var vendorParam = string.IsNullOrEmpty(model.vendor)
            ? "00-0000000"
            : model.vendor;

        DateTime entryDate;
        if (!DateTime.TryParse(model.poEntryDate, out entryDate))
        {
            return Json(new
            {
                success = false,
                message = "Invalid PO Entry Date."
            });
        }

        var list = vendorParam == "08-0000250"
            ? _repo.GetPOSeizo_TKF(vendorParam, model.userName, model.orderStatus, model.poEntryDate).ToList()
            : _repo.GetPOSeizo_ALL(vendorParam, model.userName, model.orderStatus, model.poEntryDate).ToList();

        if (list == null || !list.Any())
        {
            return Json(new
            {
                success = false,
                message = "There's no target data.\nPlease check your parameters\nProcess terminated."
            });
        }

        if (model.orderStatus == "New")
        {
            try
            {
                _repo.UpdatePurchaseOrderStatusToOpen(vendorParam, model.userName, entryDate);
            }
            catch (SqlException)
            {
                return Json(new
                {
                    success = false,
                    message = "Failed to update PO status."
                });
            }
        }

        return Json(new
        {
            success = true,
            message = "Print process completed.",
            rows = list.Count
        });
    }

    [HttpPost]
    public JsonResult CheckData_ALL([FromBody] Model_POSeizo_Check model)
    {
        // Vendor が空 → ALL Vendors
        var vendorParam = string.IsNullOrEmpty(model.vendor)
            ? "00-0000000"
            : model.vendor;

        // SQL 実行
        var list = _repo.GetPOSeizo_ALL(
            vendorParam,
            model.userName,
            model.orderStatus,
            model.poEntryDate
        ).ToList();

        if (list == null || !list.Any())
        {
            return Json(new
            {
                success = false,
                message = "There's no target data.\nPlease check your parameters\nProcess terminated."
            });
        }

        return Json(new
        {
            success = true
        });
    }
    [HttpPost]
    public JsonResult CheckData_TKF([FromBody] Model_POSeizo_Check model)
    {
        // Vendor が空 → ALL Vendors
        var vendorParam = string.IsNullOrEmpty(model.vendor)
            ? "00-0000000"
            : model.vendor;

        // SQL 実行
        var list = _repo.GetPOSeizo_TKF(
            vendorParam,
            model.userName,
            model.orderStatus,
            model.poEntryDate
        ).ToList();

        if (list == null || !list.Any())
        {
            return Json(new
            {
                success = false,
                message = "There's no target data.\nPlease check your parameters\nProcess terminated."
            });
        }

        return Json(new
        {
            success = true
        });
    }
}
