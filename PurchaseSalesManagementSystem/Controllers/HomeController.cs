using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Models;
using System.Diagnostics;
using System.Drawing;
using ClosedXML.Excel;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Repository;
namespace PurchaseSalesManagementSystem.Controllers
{
    public class HomeController : Controller
    {

        private readonly Repository_Menu _repo;
        public HomeController(Repository_Menu repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Menu()
        {
            // ログインしていない場合はログイン画面へ
            var user = HttpContext.Session.GetString("LoginUser");
            if (string.IsNullOrEmpty(user))
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult ExportToExcel(string reportName)
        {

            switch (reportName)
            {
                case "InventoryForecastingReport":
                case "InventoryForecastingReportWithoutPO":
                    var data = _repo.GetInventoryForecasting(reportName).ToList();
                    return ExportInventoryForecast(data, reportName);

                case "InventoryForecastingReportByMonthforFujikin":
                    var byMonth = _repo.GetInventoryForecastingByMonth().ToList();
                    return ExportInventoryForecastByMonth(byMonth, reportName);

                case "OnHandShortageCheckList":
                    var onHandShortage = _repo.GetOnHandShortage().ToList();
                    return ExportOnHandShortage(onHandShortage, reportName);

                case "ProjectPartOpenOrderVolume":
                    var projectPartOpenOrderVolume = _repo.GetProjectPartOpenOrderVolume().ToList();
                    return ExportProjectPartOpenOrderVolume(projectPartOpenOrderVolume, reportName);

                default:
                    return StatusCode(
                        400,
                        $"Invalid report name: {reportName}"
                    );
            }
        }

        // =========================
        // 11-1 / 11-2
        // =========================
        private ActionResult ExportInventoryForecast(
            List<Model_InventoryForecast> data,
            string reportName)
        {
            var yyyymm = new List<string>();
            for (int i = -1; i <= 7; i++)
            {
                yyyymm.Add(DateTime.Today.AddMonths(i).ToString("yyyy-MM"));
            }

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Inventory Forecast");

                int col = 1;

                ws.Cell(1, col++).Value = "ItemCode";
                ws.Cell(1, col++).Value = "ItemCodeDesc";
                ws.Cell(1, col++).Value = "ItemNo";
                ws.Cell(1, col++).Value = "Category1";
                ws.Cell(1, col++).Value = "VendorName";
                ws.Cell(1, col++).Value = "UnitCost";
                ws.Cell(1, col++).Value = "OnHand";
                ws.Cell(1, col++).Value = "PurchaseOrder";
                ws.Cell(1, col++).Value = "SalesOrder";
                ws.Cell(1, col++).Value = "Surplus";
                ws.Cell(1, col++).Value = "Data Type";

                int monthStartCol = col;

                foreach (var ym in yyyymm)
                {
                    ws.Cell(1, col++).Value = ym;
                }

                int totalCol = col;
                ws.Cell(1, col).Value = "Total";

                ws.Range(1, 1, 1, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Range(1, 1, 1, col).Style.Font.Bold = true;

                int row = 2;
                foreach (var item in data)
                {
                    col = 1;

                    ws.Cell(row, col++).Value = item.ItemCode;
                    ws.Cell(row, col++).Value = item.ItemCodeDesc;
                    ws.Cell(row, col++).Value = item.ItemNo;
                    ws.Cell(row, col++).Value = item.Category1;
                    ws.Cell(row, col++).Value = item.VendorName;
                    ws.Cell(row, col++).Value = item.UnitCost;
                    ws.Cell(row, col++).Value = item.OnHand;
                    ws.Cell(row, col++).Value = item.PurchaseOrder;
                    ws.Cell(row, col++).Value = item.SalesOrder;
                    ws.Cell(row, col++).Value = item.Surplus;
                    ws.Cell(row, col++).Value = item.DataType;

                    for (int i = 0; i < yyyymm.Count; i++)
                    {
                        ws.Cell(row, col++).Value = item.MonthlyQty?[i];
                    }

                    // Total は Excel で計算
                    string startAddr = ws.Cell(row, monthStartCol).Address.ToStringRelative();
                    string endAddr = ws.Cell(row, monthStartCol + yyyymm.Count - 1).Address.ToStringRelative();
                    ws.Cell(row, totalCol).FormulaA1 = $"SUM({startAddr}:{endAddr})";

                    row++;
                }

                ws.Range(1, 1, row - 1, totalCol).SetAutoFilter();
                ws.Columns().AdjustToContents();

                return SaveExcel(workbook, reportName);
            }
        }

        // =========================
        // 11-7 Inventory Forecast By Month
        // =========================
        private ActionResult ExportInventoryForecastByMonth(
            List<Model_InventoryForecastByMonth> data,
            string reportName)
        {
            // YM0 ～ YM8（表示用）
            var yyyymm = new List<string>();
            for (int i = -1; i <= 7; i++)
            {
                yyyymm.Add(DateTime.Today.AddMonths(i).ToString("yyyy-MM"));
            }

            using (var workbook = new XLWorkbook())
            {
                var ws = workbook.Worksheets.Add("Inventory Forecast By Month");

                int col = 1;

                ws.Cell(1, col++).Value = "ItemCode";
                ws.Cell(1, col++).Value = "ItemCodeDesc";
                ws.Cell(1, col++).Value = "ItemNo";
                ws.Cell(1, col++).Value = "Category1";
                ws.Cell(1, col++).Value = "VendorName";
                ws.Cell(1, col++).Value = "UnitCost";
                ws.Cell(1, col++).Value = "OnHand";
                ws.Cell(1, col++).Value = "PurchaseOrder";
                ws.Cell(1, col++).Value = "SalesOrder";
                ws.Cell(1, col++).Value = "Surplus";
                ws.Cell(1, col++).Value = "Data Type";

                int monthStartCol = col;

                foreach (var ym in yyyymm)
                {
                    ws.Cell(1, col++).Value = ym;
                }

                int totalCol = col;
                ws.Cell(1, col).Value = "Total";

                // Header style
                ws.Range(1, 1, 1, col).Style.Fill.BackgroundColor = XLColor.LightGray;
                ws.Range(1, 1, 1, col).Style.Font.Bold = true;

                int row = 2;
                foreach (var item in data)
                {
                    col = 1;

                    ws.Cell(row, col++).Value = item.ItemCode;
                    ws.Cell(row, col++).Value = item.ItemCodeDesc;
                    ws.Cell(row, col++).Value = item.ItemNo;
                    ws.Cell(row, col++).Value = item.Category1;
                    ws.Cell(row, col++).Value = item.VendorName;
                    ws.Cell(row, col++).Value = item.UnitCost;
                    ws.Cell(row, col++).Value = item.OnHand;
                    ws.Cell(row, col++).Value = item.PurchaseOrder;
                    ws.Cell(row, col++).Value = item.SalesOrder;
                    ws.Cell(row, col++).Value = item.Surplus;
                    ws.Cell(row, col++).Value = item.DataType;

                    // M0 ～ M8
                    for (int i = 0; i < 9; i++)
                    {
                        ws.Cell(row, col++).Value = item.MonthlyQty?[i];
                    }

                    // Total（Excel計算）
                    string startAddr = ws.Cell(row, monthStartCol).Address.ToStringRelative();
                    string endAddr = ws.Cell(row, monthStartCol + 8).Address.ToStringRelative();
                    ws.Cell(row, totalCol).FormulaA1 = $"SUM({startAddr}:{endAddr})";

                    row++;
                }

                ws.Range(1, 1, row - 1, totalCol).SetAutoFilter();
                ws.Columns().AdjustToContents();

                return SaveExcel(workbook, reportName);
            }
        }

        // =========================
        // 2 OnHandShortage
        // =========================
        private ActionResult ExportOnHandShortage(
            List<Model_OnHandShortageCheckList> data,
            string reportName)
        {
            var exportToExcel = new FormattedDataTableExcelExporter();
            var dt = exportToExcel.ConvertToDataTableFast(data);
            var workbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(dt, "SQL-EXEC");
            var worksheet = workbook.Worksheet("SQL-EXEC");

            int lastRow = worksheet.LastRowUsed()?.RowNumber() ?? 1;
            int endColumn = Math.Min(15, dt.Columns.Count);
            if (lastRow >= 2 && endColumn >= 4)
            {
                worksheet.Range(2, 4, lastRow, endColumn).Style.NumberFormat.Format = "#,###_);[Red]-#,##0;";
            }

            return SaveExcel(workbook, reportName);
        }

        private void SetMediumBorder(IXLWorksheet ws, string columnLetter, int lastRow)
        {
            var col = ws.Range($"{columnLetter}1:{columnLetter}{lastRow}");
            col.Style.Border.LeftBorder = XLBorderStyleValues.Medium;
            col.Style.Border.RightBorder = XLBorderStyleValues.Medium;
            col.Style.Border.LeftBorderColor = XLColor.Black;
            col.Style.Border.RightBorderColor = XLColor.Black;
        }


        // =========================
        // 2 ProjectPartOpenOrderVolume
        // =========================
        private ActionResult ExportProjectPartOpenOrderVolume(
            List<Model_ProjectPartOpenOrderVolume> data,
            string reportName)
        {
            var exportToExcel = new FormattedDataTableExcelExporter();
            var dt = exportToExcel.ConvertToDataTableFast(data);
            var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt, "SQL-EXEC");

            return File(
                excelBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{reportName}_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
            );
        }


        // =========================
        // 共通保存処理
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
}
