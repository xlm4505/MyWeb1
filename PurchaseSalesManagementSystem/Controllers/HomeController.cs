using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

namespace PurchaseSalesManagementSystem.Controllers
{
    public class HomeController : Controller
    {

        private readonly Repository_Menu _repo;
		private readonly IWebHostEnvironment _env;
		public HomeController(Repository_Menu repo, IWebHostEnvironment env)
        {
            _repo = repo;
			_env = env;
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

                case "OnHand Shortage Check List":
                    var onHandShortage = _repo.GetOnHandShortage().ToList();
                    return ExportOnHandShortage(onHandShortage, reportName);

                case "Open Order Volume by Month":
                    var projectPartOpenOrderVolume = _repo.GetProjectPartOpenOrderVolume().ToList();
                    return ExportProjectPartOpenOrderVolume(projectPartOpenOrderVolume, reportName);

                case "Combine shipping list":
					var combineShippingList = _repo.GetCombineShippingList().ToList();
					return ExportCombineShippingList(combineShippingList);

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
            if (dt.Columns.Contains("OnHand_Reg"))
            {
                dt.Columns["OnHand_Reg"]!.ColumnName = "OnHand(Reg)";
            }
            if (dt.Columns.Contains("OpenPO_Reg"))
            {
                dt.Columns["OpenPO_Reg"]!.ColumnName = "OpenPO(Reg)";
            }
            if (dt.Columns.Contains("OpenSO_Reg"))
            {
                dt.Columns["OpenSO_Reg"]!.ColumnName = "OpenSO(Reg)";
            }
            if (dt.Columns.Contains("Available_Reg"))
            {
                dt.Columns["Available_Reg"]!.ColumnName = "Available(Reg)";
            }
            if (dt.Columns.Contains("OnHand_Ex"))
            {
                dt.Columns["OnHand_Ex"]!.ColumnName = "OnHand(Ex)";
            }
            if (dt.Columns.Contains("OpenPO_Ex"))
            {
                dt.Columns["OpenPO_Ex"]!.ColumnName = "OpenPO(Ex)";
            }
            if (dt.Columns.Contains("OpenSO_Ex"))
            {
                dt.Columns["OpenSO_Ex"]!.ColumnName = "OpenSO(Ex)";
            }
            if (dt.Columns.Contains("Available_Ex"))
            {
                dt.Columns["Available_Ex"]!.ColumnName = "Available(Ex)";
            }
            if (dt.Columns.Contains("OnHand_Total"))
            {
                dt.Columns["OnHand_Total"]!.ColumnName = "OnHand(Total)";
            }
            if (dt.Columns.Contains("OpenPO_Total"))
            {
                dt.Columns["OpenPO_Total"]!.ColumnName = "OpenPO(Total)";
            }
            if (dt.Columns.Contains("OpenSO_Total"))
            {
                dt.Columns["OpenSO_Total"]!.ColumnName = "OpenSO(Total)";
            }
            if (dt.Columns.Contains("Available_Total"))
            {
                dt.Columns["Available_Total"]!.ColumnName = "Available(Total)";
            }
            var workbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(dt, "SQL-EXEC","PO");
 
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

            if (dt.Columns.Contains("MonthlyQty0"))
            {
                dt.Columns["MonthlyQty0"]!.ColumnName = DateTime.Today.AddMonths(-4).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty1"))
            {
                dt.Columns["MonthlyQty1"]!.ColumnName = DateTime.Today.AddMonths(-3).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty2"))
            {
                dt.Columns["MonthlyQty2"]!.ColumnName = DateTime.Today.AddMonths(-2).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty3"))
            {
                dt.Columns["MonthlyQty3"]!.ColumnName = DateTime.Today.AddMonths(-1).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty4"))
            {
                dt.Columns["MonthlyQty4"]!.ColumnName = DateTime.Today.ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty5"))
            {
                dt.Columns["MonthlyQty5"]!.ColumnName = DateTime.Today.AddMonths(1).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty6"))
            {
                dt.Columns["MonthlyQty6"]!.ColumnName = DateTime.Today.AddMonths(2).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty7"))
            {
                dt.Columns["MonthlyQty7"]!.ColumnName = DateTime.Today.AddMonths(3).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty8"))
            {
                dt.Columns["MonthlyQty8"]!.ColumnName = DateTime.Today.AddMonths(4).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty9"))
            {
                dt.Columns["MonthlyQty9"]!.ColumnName = DateTime.Today.AddMonths(5).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty10"))
            {
                dt.Columns["MonthlyQty10"]!.ColumnName = DateTime.Today.AddMonths(6).ToString("yyyy-MM");
            }
            if (dt.Columns.Contains("MonthlyQty11"))
            {
                dt.Columns["MonthlyQty11"]!.ColumnName = DateTime.Today.AddMonths(7).ToString("yyyy-MM");
            }
            var workbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(dt, "SQL-EXEC","PO");
            var worksheet = workbook.Worksheet("SQL-EXEC");

            worksheet.Row(1).InsertRowsAbove(1);

            var totalHeaderRange = worksheet.Range("E1:H1");
            totalHeaderRange.Merge();
            totalHeaderRange.Value = "Total";

            var onHoldHeaderRange = worksheet.Range("I1:L1");
            onHoldHeaderRange.Merge();
            onHoldHeaderRange.Value = "On Hold";

            worksheet.Range("E1:L1").Style.Font.Bold = true;
            worksheet.Range("E1:L1").Style.Fill.BackgroundColor = XLColor.FromArgb(255, 192, 0);
            worksheet.Range("E1:L1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            worksheet.Range("E1:L1").Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            worksheet.SheetView.FreezeRows(2);
            if (dt.Columns.Contains("OnHand1"))
            {
                worksheet.Cell(2, dt.Columns["OnHand1"]!.Ordinal + 1).Value = "OnHand";
            }

            if (dt.Columns.Contains("OnHand2"))
            {
                worksheet.Cell(2, dt.Columns["OnHand2"]!.Ordinal + 1).Value = "OnHand ";
            }

            if (dt.Columns.Contains("OpenPO1"))
            {
                worksheet.Cell(2, dt.Columns["OpenPO1"]!.Ordinal + 1).Value = "OpenPO";
            }

            if (dt.Columns.Contains("OpenPO2"))
            {
                worksheet.Cell(2, dt.Columns["OpenPO2"]!.Ordinal + 1).Value = "OpenPO ";
            }
            if (dt.Columns.Contains("OpenSO1"))
            {
                worksheet.Cell(2, dt.Columns["OpenSO1"]!.Ordinal + 1).Value = "OpenSO";
            }

            if (dt.Columns.Contains("OpenSO2"))
            {
                worksheet.Cell(2, dt.Columns["OpenSO2"]!.Ordinal + 1).Value = "OpenSO ";
            }
            if (dt.Columns.Contains("Surplus1"))
            {
                worksheet.Cell(2, dt.Columns["Surplus1"]!.Ordinal + 1).Value = "Surplus";
            }

            if (dt.Columns.Contains("Surplus2"))
            {
                worksheet.Cell(2, dt.Columns["Surplus2"]!.Ordinal + 1).Value = "Surplus ";
            }
            return SaveExcel(workbook, reportName);
        }
		private ActionResult ExportCombineShippingList(List<Model_CombineShippingList> data)
		{
			var exportToExcel = new FormattedDataTableExcelExporter();
			var dt = exportToExcel.ConvertToDataTableFast(data);

			string templatePath = Path.Combine(
				_env.ContentRootPath,
				"ExcelTemplate",
				"CombineShippingList",
				"Combine_ShippingList (for Rinku)_V1.2.xlsm");

			ExcelPackage.License.SetNonCommercialOrganization("Fujikin of America, Inc.");
			using (var package = new ExcelPackage(new FileInfo(templatePath)))
			{
				var workbook = package.Workbook;

				workbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(workbook, dt, "PODetail", "PO");

				var stream = new MemoryStream();
				package.SaveAs(stream);
				stream.Position = 0;

				return File(stream.ToArray(),
					"application/vnd.ms-excel.sheet.macroEnabled.12",
                    $"Combine_ShippingList (for Rinku)_V1.2- {DateTime.Now:MMddyy}.xlsm");
			}
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
