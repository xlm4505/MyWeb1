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
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("OnHand Shortage");

                // =========================
                // ヘッダ作成
                // =========================
                ws.Cell(1, 1).Value = "ItemCode";
                ws.Cell(1, 2).Value = "ItemDesc";
                ws.Cell(1, 3).Value = "Category";
                ws.Cell(1, 4).Value = "OnHand(Reg)";
                ws.Cell(1, 5).Value = "OpenPO(Reg)";
                ws.Cell(1, 6).Value = "OpenSO(Reg)";
                ws.Cell(1, 7).Value = "Available(Reg)";
                ws.Cell(1, 8).Value = "OnHand(Ex)";
                ws.Cell(1, 9).Value = "OpenPO(Ex)";
                ws.Cell(1, 10).Value = "OpenSO(Ex)";
                ws.Cell(1, 11).Value = "Available(Ex)";
                ws.Cell(1, 12).Value = "OnHand(Total)";
                ws.Cell(1, 13).Value = "OpenPO(Total)";
                ws.Cell(1, 14).Value = "OpenSO(Total)";
                ws.Cell(1, 15).Value = "Available(Total)";

                int lastCol = 15;

                // =========================
                // データ出力
                // =========================
                int row = 2;
                foreach (var item in data)
                {
                    ws.Cell(row, 1).Value = item.ItemCode;
                    ws.Cell(row, 2).Value = item.ItemDesc;
                    ws.Cell(row, 3).Value = item.Category;

                    ws.Cell(row, 4).Value = item.OnHand_Reg;
                    ws.Cell(row, 5).Value = item.OpenPO_Reg;
                    ws.Cell(row, 6).Value = item.OpenSO_Reg;
                    ws.Cell(row, 7).Value = item.Available_Reg;

                    ws.Cell(row, 8).Value = item.OnHand_Ex;
                    ws.Cell(row, 9).Value = item.OpenPO_Ex;
                    ws.Cell(row, 10).Value = item.OpenSO_Ex;
                    ws.Cell(row, 11).Value = item.Available_Ex;

                    ws.Cell(row, 12).Value = item.OnHand_Total;
                    ws.Cell(row, 13).Value = item.OpenPO_Total;
                    ws.Cell(row, 14).Value = item.OpenSO_Total;
                    ws.Cell(row, 15).Value = item.Available_Total;

                    row++;
                }

                int lastRow = row - 1;

                // =========================
                // ヘッダ書式（VBA再現）
                // =========================
                var header = ws.Range(1, 1, 1, lastCol);
                header.Style.Font.Bold = true;
                header.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                header.Style.Fill.BackgroundColor = XLColor.FromArgb(217, 217, 217); // ColorIndex 15
                header.Style.Border.BottomBorder = XLBorderStyleValues.Medium;
                header.Style.Border.BottomBorderColor = XLColor.Black;

                // =========================
                // 全体罫線（薄グレー）
                // =========================
                var body = ws.Range(1, 1, lastRow, lastCol);
                body.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                body.Style.Border.InsideBorder = XLBorderStyleValues.Thin;
                body.Style.Border.OutsideBorderColor = XLColor.FromArgb(208, 215, 229);
                body.Style.Border.InsideBorderColor = XLColor.FromArgb(208, 215, 229);

                // =========================
                // 太罫線（VBAの A,C,G,K,O 列）
                // =========================
                SetMediumBorder(ws, "A", lastRow);
                SetMediumBorder(ws, "C", lastRow);
                SetMediumBorder(ws, "G", lastRow);
                SetMediumBorder(ws, "K", lastRow);
                SetMediumBorder(ws, "O", lastRow);

                // =========================
                // Freeze Pane（2行目固定）
                // =========================
                ws.SheetView.FreezeRows(1);

                // =========================
                // 数値フォーマット
                // =========================
                ws.Range(2, 4, lastRow, 15)
                  .Style.NumberFormat.Format = "#,###_);[Red]-#,##0;";

                // =========================
                // フォント
                // =========================
                body.Style.Font.FontName = "Calibri";
                body.Style.Font.FontSize = 10;

                // =========================
                // 列幅
                // =========================
                ws.Columns().AdjustToContents();
                ws.Column(3).Width = 14;
                ws.Columns(4, 15).Width = 12;

                // =========================
                // 保存
                // =========================
                return SaveExcel(wb, reportName);
            }
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
            using (var wb = new XLWorkbook())
            {
                var ws = wb.Worksheets.Add("SQL-EXEC");

                int col = 1;
                ws.Range("E1:H1").Merge();
                ws.Cell(1, 5).Value = "Total";
                ws.Range("I1:L1").Merge();
                ws.Cell(1, 9).Value = "On Hold";


                ws.Cell(2, col++).Value = "ItemCode";
                ws.Cell(2, col++).Value = "ItemCodeDesc";
                ws.Cell(2, col++).Value = "VendorName";
                ws.Cell(2, col++).Value = "UnitCost";
                ws.Cell(2, col++).Value = "OnHand";
                ws.Cell(2, col++).Value = "OpenPO";
                ws.Cell(2, col++).Value = "OpenSO";
                ws.Cell(2, col++).Value = "Surplus";
                ws.Cell(2, col++).Value = "OnHand";
                ws.Cell(2, col++).Value = "OpenPO";
                ws.Cell(2, col++).Value = "OpenSO";
                ws.Cell(2, col++).Value = "Surplus";
                ws.Cell(2, col++).Value = "Available";

                int monthStart = col;
                for (int i = -4; i <= 7; i++)
                {
                    ws.Cell(2, col++).Value =
                        DateTime.Today.AddMonths(i).ToString("yyyy-MM");
                }
                ws.Cell(2, col++).Value = "Total";

                ws.Range(1, 1, 2, col - 1).Style
                    .Font.SetBold()
                    .Fill.SetBackgroundColor(XLColor.FromArgb(242, 242, 242))
                    .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

                int row = 3;
                foreach (var d in data)
                {
                    col = 1;
                    ws.Cell(row, col++).Value = d.ItemCode;
                    ws.Cell(row, col++).Value = d.ItemCodeDesc;
                    ws.Cell(row, col++).Value = d.VendorName;
                    ws.Cell(row, col++).Value = d.UnitCost;
                    ws.Cell(row, col++).Value = d.OnHand1;
                    ws.Cell(row, col++).Value = d.OpenPO1;
                    ws.Cell(row, col++).Value = d.OpenSO1;
                    ws.Cell(row, col++).Value = d.Surplus1;
                    ws.Cell(row, col++).Value = d.OnHand2;
                    ws.Cell(row, col++).Value = d.OpenPO2;
                    ws.Cell(row, col++).Value = d.OpenSO2;
                    ws.Cell(row, col++).Value = d.Surplus2;
                    ws.Cell(row, col++).Value = d.Available;

                    for (int i = 0; i < 12; i++)
                        ws.Cell(row, col++).Value = d.MonthlyQty[i];

                    ws.Cell(row, col++).Value = d.Total;

                    row++;
                }

                ws.SheetView.FreezeRows(2);
                ws.RangeUsed().Style.NumberFormat.Format = "#,##0;[Red]-#,##0";
                ws.Columns().AdjustToContents();

                return SaveExcel(wb, reportName);
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
                    $"{reportName}_{DateTime.Now:yyMMdd_HHmmss}.xlsx"
                );
            }
        }
    }
}
