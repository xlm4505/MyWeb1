using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Text.RegularExpressions;

public class Tbl_VendorController : Controller
{
    private readonly Repository_Tbl_VendorMaintenance _repo;

    public Tbl_VendorController(Repository_Tbl_VendorMaintenance repo)
    {
        _repo = repo;
    }

    public IActionResult Tbl_Vendor()
    {
        return View();
    }

    [HttpGet]
    public IActionResult Search(string? id)
    {
        var items = _repo.GetVendors(id);
        return Json(items);
    }

    [HttpPost]
    public IActionResult Update([FromBody] List<Model_Tbl_VendorMaintenance>? items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = true, updatedCount = 0, message = "No rows selected." });
        }

        var updatedCount = _repo.UpdateVendors(items);
        return Json(new { success = true, updatedCount });
    }

    [HttpPost]
    public IActionResult Delete([FromBody] List<Model_Tbl_VendorMaintenance>? items)
    {
        if (items == null || !items.Any())
        {
            return Json(new { success = true, deletedCount = 0, message = "No rows selected." });
        }

        var deletedCount = _repo.DeleteVendors(items);
        return Json(new { success = true, deletedCount });
    }
    [HttpPost]
    public IActionResult Add([FromBody] Model_Tbl_VendorMaintenance? item)
    {
        if (item == null)
        {
            return BadRequest(new { success = false, message = "No data to register." });
        }

        if (string.IsNullOrWhiteSpace(item.ID))
        {
            return BadRequest(new { success = false, message = "ID is required." });
        }

        if (!Regex.IsMatch(item.ID, @"^\d{1,30}$"))
        {
            return BadRequest(new { success = false, message = "ID must be numeric and up to 30 digits." });
        }

        if (!IsNumericOptional(item.APDivisionNo, 50) || !IsNumericOptional(item.VendorNo, 50))
        {
            return BadRequest(new { success = false, message = "APDivisionNo and VendorNo must be numeric and up to 50 digits." });
        }

        if (_repo.ExistsVendorById(item.ID))
        {
            return BadRequest(new { success = false, message = "The entered ID already exists." });
        }

        _repo.InsertVendor(item);
        return Json(new { success = true });
    }

    [HttpGet]
    public IActionResult ExportToExcel(string id)
    {
        var list = _repo.GetVendors(id).ToList();
        var exportToExcel = new FormattedDataTableExcelExporter();
        var dt = exportToExcel.ConvertToDataTableFast(list);
        var workbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(dt, "Tbl_Vendor");
        return SaveExcel(workbook, "Tbl_Vendor");
    }

    private static bool IsNumericOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        return Regex.IsMatch(value, $@"^\d{{1,{maxLength}}}$");
    }

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