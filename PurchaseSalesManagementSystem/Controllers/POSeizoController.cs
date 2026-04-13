using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Data;
using System.Diagnostics;

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
        var vendorParam = string.IsNullOrEmpty(vendor) ? "00-0000000" : vendor;

        var list = _repo.GetPOSeizo_TKF(
            vendorParam,
            userName,
            orderStatus,
            poEntryDate
        ).ToList();

        var exportToExcel = new FormattedDataTableExcelExporter();
        var dt = exportToExcel.ConvertToDataTableFast(list);
        if (dt.Columns.Contains("ItemCode"))
        {
            dt.Columns["ItemCode"]!.ColumnName = "Item Code";
        }
        if (dt.Columns.Contains("CustomerPartNumber"))
        {
            dt.Columns["CustomerPartNumber"]!.ColumnName = "Customer Part Number";
        }
        if (dt.Columns.Contains("WHCode"))
        {
            dt.Columns["WHCode"]!.ColumnName = "WH Code";
        }
        if (dt.Columns.Contains("RequiredDeliveryDate"))
        {
            dt.Columns["RequiredDeliveryDate"]!.ColumnName = "Required Delivery Date";
        }
        if (dt.Columns.Contains("OrderedQty"))
        {
            dt.Columns["OrderedQty"]!.ColumnName = "Ordered Q'ty";
        }
        if (dt.Columns.Contains("UnitPrice"))
        {
            dt.Columns["UnitPrice"]!.ColumnName = "Unit Price";
        }
        var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt, "PO Seizo", "PO");

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"{"PO_SeizoExport ["}{vendorName}{"]"}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );
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
        // Vendor Ç™ãÛ Å® ALL Vendors
        var vendorParam = string.IsNullOrEmpty(vendor)
            ? "00-0000000"
            : vendor;

        // SQL é¿çs
        var list = _repo.GetPOSeizo_ALL(
            vendorParam,
            userName,
            orderStatus,
            poEntryDate
        ).ToList();

        var exportToExcel = new FormattedDataTableExcelExporter();
        var dt = exportToExcel.ConvertToDataTableFast(list);
        if (dt.Columns.Contains("ProductionControlNotice"))
        {
            dt.Columns["ProductionControlNotice"]!.ColumnName = "Production ControlNotice";
        }

        var workbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(dt, "PO Seizo", "PO");
        var worksheet = workbook.Worksheet("PO Seizo");
        worksheet.Column(1).Style.NumberFormat.Format = "#,##0";
        worksheet.Column(8).Style.NumberFormat.Format = "#,##0";
        reportName = "PO_SeizoExport [" + vendorName + "]";
        return SaveExcel(workbook, reportName);

    }

    [HttpPost]
    public IActionResult RunPrint([FromBody] Model_POSeizo_Check model)
    {
        if (!DateTime.TryParse(model.poEntryDate, out DateTime entryDate))

        {
            return BadRequest(new
            {
                success = false,
                message = "Invalid PO Entry Date."
            });
        }

        if (model.orderStatus == "New")
        {
            try
            {
                _repo.UpdatePurchaseOrderStatusToOpen(model.vendor, model.userName, entryDate);
            }
            catch (SqlException)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to update PO status."
                });
            }
        }

        string vendorDisplay = string.IsNullOrWhiteSpace(model.vendorName) ? model.vendor : model.vendorName;
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string outputFileName = $"PurchaseOrder [{vendorDisplay}]_{timestamp}.pdf";
        string outputPath = Path.Combine(Path.GetTempPath(), outputFileName);

        try
        {
            string crystalReportNinja = @"P:\\IT\\Tools\\CrystalReportsNinja";
            string reportPath = @"P:\\IT\\Crystal\\PO_PurchaseOrder3_Auto.rpt";
            string newOrder = model.orderStatus == "New" ? "True" : "False";

            string arguments =
                $"-S VMP-10 " +
                $"-D MAS_FOA " +
                $"-U MAS_REPORTS " +
                $"-P Reporting1 " +
                $"-F \"{reportPath}\" " +
                $"-O \"{outputPath}\" " +
                $"-E pdf " +
                $"-a \"Entry Date:{model.poEntryDate}\" " +
                $"-a \"User Name:{model.userName}\" " +
                $"-a \"New Order:{newOrder}\" " +
                $"-a \"Vendor Code:{model.vendor}\"";

            var startInfo = new ProcessStartInfo
            {
                FileName = crystalReportNinja,
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Failed to start Crystal Reports process."
                });
            }

            process.WaitForExit();

            if (process.ExitCode != 0 || !System.IO.File.Exists(outputPath))
            {
                string error = process.StandardError.ReadToEnd();
                return StatusCode(500, new
                {
                    success = false,
                    message = $"PDF creation failed. {error}".Trim()
                });
            }

            byte[] pdfBytes = System.IO.File.ReadAllBytes(outputPath);
            System.IO.File.Delete(outputPath);

            return File(pdfBytes, "application/pdf", outputFileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                success = false,
                message = $"Print: {ex.Message}"
            });
        }
    }

    [HttpPost]
    public JsonResult CheckData_ALL([FromBody] Model_POSeizo_Check model)
    {
        // Vendor Ç™ãÛ Å® ALL Vendors
        var vendorParam = string.IsNullOrEmpty(model.vendor)
            ? "00-0000000"
            : model.vendor;

        // SQL é¿çs
        var list = _repo.GetPOSeizo_ALL(
            vendorParam,
            model.userName,
            model.orderStatus,
            model.poEntryDate
        ).ToList();

        return Json(new
        {
            success = true
        });
    }
    [HttpPost]
    public JsonResult CheckData_TKF([FromBody] Model_POSeizo_Check model)
    {
        // Vendor Ç™ãÛ Å® ALL Vendors
        var vendorParam = string.IsNullOrEmpty(model.vendor)
            ? "00-0000000"
            : model.vendor;

        // SQL é¿çs
        var list = _repo.GetPOSeizo_TKF(
            vendorParam,
            model.userName,
            model.orderStatus,
            model.poEntryDate
        ).ToList();

        return Json(new
        {
            success = true
        });
    }
    // =========================
    // ã§í ï€ë∂èàóù
    // =========================
    private ActionResult SaveExcel(XLWorkbook workbook, string reportName)
    {
        using (var stream = new MemoryStream())
        {
            workbook.SaveAs(stream);
            return File(
                stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            );
        }
    }
}
