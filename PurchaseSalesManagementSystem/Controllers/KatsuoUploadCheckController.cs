using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class KatsuoUploadCheckController : Controller
{
    private readonly Repository_KatsuoUploadCheck _repo;

    public KatsuoUploadCheckController(Repository_KatsuoUploadCheck repo)
    {
        _repo = repo;
    }

    public IActionResult KatsuoUploadCheck()
    {
        return View();
    }

    [HttpPost]
    public IActionResult RunChecklist([FromForm] string selectAction)
    {
        try
        {
            bool isValves = selectAction == "Valves";
            var dt = _repo.GetCheckList(isValves);

            var workbook = new XLWorkbook();
            var exporter = new FormattedDataTableExcelExporter();
            workbook = exporter.ExportDataTableWithFormattingForWorkbook(workbook, dt, "CheckList", "PO");

            using var ms = new MemoryStream();
            workbook.SaveAs(ms);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var fileName = $"Open PO Check List ({selectAction})_{timestamp}.xlsx";
            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            return File(ms.ToArray(), contentType, fileName);
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    [HttpPost]
    public IActionResult UploadExcel(IFormFile excelFile)
    {
        try
        {
            var userName = HttpContext.Session.GetString("LoginUser") ?? "";

            // ファイル名から日付を抽出: YYMMDD...（失敗時は null → DB に NULL を更新）
            var fileName = Path.GetFileName(excelFile.FileName);
            DateTime? issueDate = null;
            try
            {
                string month = fileName.Substring(2, 2);
                string day   = fileName.Substring(4, 2);
                string year  = "20" + fileName.Substring(0, 2);
                issueDate = DateTime.Parse($"{year}-{month}-{day}");
            }
            catch { }

            // Katsuo_Issue_Date テーブルを更新
            _repo.UpdateIssueDate(userName, issueDate);

            // Excel を読み込み
            var katsuoOriginal = new List<Model_KatsuoUploadCheck>();

            using (var stream = new MemoryStream())
            {
                excelFile.CopyTo(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var ws = workbook.Worksheets.First();
                    var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                    for (int r = 3; r <= lastRow; r++)
                    {
                        var office        = ws.Cell(r, 1).GetValue<string>();   // A
                        var partNumber    = ws.Cell(r, 5).GetValue<string>();   // E
                        var backlog       = ws.Cell(r, 8).GetValue<string>();   // H
                        var poNumber      = ws.Cell(r, 11).GetValue<string>();  // K
                        var shippingDate  = ws.Cell(r, 14).GetValue<string>(); // N
                        var itemCode      = ws.Cell(r, 24).GetValue<string>(); // X

                        // 全列空の行はスキップ
                        if (string.IsNullOrWhiteSpace(office) &&
                            string.IsNullOrWhiteSpace(partNumber) &&
                            string.IsNullOrWhiteSpace(backlog) &&
                            string.IsNullOrWhiteSpace(poNumber) &&
                            string.IsNullOrWhiteSpace(shippingDate) &&
                            string.IsNullOrWhiteSpace(itemCode))
                        {
                            continue;
                        }

                        katsuoOriginal.Add(new Model_KatsuoUploadCheck
                        {
                            Office         = office,
                            PartNumber     = partNumber,
                            BacklogOfOrders = backlog,
                            PONumber       = poNumber,
                            ShippingDate   = shippingDate,
                            ItemCode       = itemCode,
                        });
                    }
                }
            }

            FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
            DataTable dt_katsuoOriginal = new DataTable();

            dt_katsuoOriginal = exportToExcel.ConvertToDataTableFast(katsuoOriginal);

            // dt_Katsuo_Work を作成
            DataTable dt_Katsuo_Work = new DataTable();
            dt_Katsuo_Work.Columns.Add("PurchaseOrderNo", typeof(string));
            dt_Katsuo_Work.Columns.Add("PromiseDate",      typeof(DateTime));
            dt_Katsuo_Work.Columns.Add("ItemCode",         typeof(string));
            dt_Katsuo_Work.Columns.Add("OpenQty",          typeof(decimal));
            dt_Katsuo_Work.Columns.Add("DummyFlag",        typeof(string));
            dt_Katsuo_Work.Columns.Add("IssueDate",        typeof(DateTime));
            dt_Katsuo_Work.Columns.Add("CreateDate",       typeof(DateTime));

            string[] validOffices = { "ＦＯＡ", "ＦＯＡ※", "りんくう" };

            var grouped = dt_katsuoOriginal.AsEnumerable()
                .Where(row =>
                {
                    string office   = row["Office"]          as string ?? "";
                    string backlog  = row["BacklogOfOrders"] as string ?? "";
                    string itemCode = row["ItemCode"]        as string ?? "";
                    string poNumber = row["PONumber"]        as string ?? "";

                    if (!validOffices.Contains(office))                                    return false;
                    if (!decimal.TryParse(backlog, out decimal bVal) || bVal <= 0)         return false;
                    if (itemCode == "")                                                    return false;
                    if (poNumber == "")                                                    return false;
                    if (poNumber.Length < 8)                                               return false;
                    if (!long.TryParse(poNumber.Substring(0, 7), out _))                   return false;
                    if (poNumber[7] != '-')                                                return false;
                    return true;
                })
                .GroupBy(row => new
                {
                    PONumber     = row["PONumber"]     as string ?? "",
                    ItemCode     = row["ItemCode"]     as string ?? "",
                    PartNumber   = row["PartNumber"]   as string ?? "",
                    ShippingDate = row["ShippingDate"] as string ?? "",
                });

            foreach (var grp in grouped)
            {
                decimal openQty = grp.Sum(row =>
                    decimal.TryParse(row["BacklogOfOrders"] as string ?? "", out decimal v) ? v : 0m);

                DateTime promiseDate = string.IsNullOrWhiteSpace(grp.Key.ShippingDate)
                    ? new DateTime(1900, 1, 1)
                    : (DateTime.TryParse(grp.Key.ShippingDate, out DateTime pd)
                        ? pd
                        : new DateTime(1900, 1, 1));

                dt_Katsuo_Work.Rows.Add(
                    grp.Key.PONumber,
                    promiseDate,
                    grp.Key.ItemCode,
                    openQty,
                    "",
                    (object?)issueDate ?? DBNull.Value,
                    DateTime.Today
                );
            }

            // dt_katsuoDummy を作成
            DataTable dt_katsuoDummy = new DataTable();
            dt_katsuoDummy.Columns.Add("PurchaseOrderNo", typeof(string));
            dt_katsuoDummy.Columns.Add("PromiseDate",     typeof(DateTime));
            dt_katsuoDummy.Columns.Add("ItemCode",        typeof(string));
            dt_katsuoDummy.Columns.Add("OpenQty",         typeof(decimal));
            dt_katsuoDummy.Columns.Add("DummyFlag",       typeof(string));
            dt_katsuoDummy.Columns.Add("IssueDate",       typeof(DateTime));
            dt_katsuoDummy.Columns.Add("CreateDate",      typeof(DateTime));

            var dummyGrouped = dt_Katsuo_Work.AsEnumerable()
                .GroupBy(row => new
                {
                    PurchaseOrderNo = row["PurchaseOrderNo"] as string ?? "",
                    ItemCode        = row["ItemCode"]        as string ?? "",
                    IssueDate       = row["IssueDate"] == DBNull.Value ? (DateTime?)null : (DateTime?)row.Field<DateTime>("IssueDate"),
                });

            foreach (var grp in dummyGrouped)
            {
                DateTime minPromiseDate = grp
                    .Where(row => row["PromiseDate"] != DBNull.Value)
                    .Select(row => row.Field<DateTime>("PromiseDate"))
                    .Min();

                dt_katsuoDummy.Rows.Add(
                    grp.Key.PurchaseOrderNo,
                    minPromiseDate,
                    grp.Key.ItemCode,
                    0m,
                    "1",
                    grp.Key.IssueDate.HasValue ? (object)grp.Key.IssueDate.Value : DBNull.Value,
                    DateTime.Today
                );
            }

            // U_Katsuo_Dummy テーブルの全件削除
            _repo.DeleteUKatsuoDummy();

            // U_Katsuo_Dummy へ INSERT
            foreach (DataRow row in dt_katsuoDummy.Rows)
            {
                _repo.InsertUKatsuoDummy(row["PurchaseOrderNo"] as string ?? "");
            }

            // U_Katsuo から dt_katsuoDummy の PurchaseOrderNo に一致するレコードを削除
            var poNos = dt_katsuoDummy.AsEnumerable()
                .Select(row => row["PurchaseOrderNo"] as string ?? "")
                .Distinct()
                .ToList();

            _repo.DeleteUKatsuoByPurchaseOrderNos(poNos);

            // dt_KatsuoUplod を作成（dt_Katsuo_Work UNION ALL dt_katsuoDummy、PurchaseOrderNo / PromiseDate 順）
            DataTable dt_KatsuoUplod = dt_Katsuo_Work.Clone();

            foreach (DataRow row in dt_Katsuo_Work.Rows)
                dt_KatsuoUplod.ImportRow(row);

            foreach (DataRow row in dt_katsuoDummy.Rows)
            {
                dt_KatsuoUplod.Rows.Add(
                    row["PurchaseOrderNo"],
                    row["PromiseDate"],
                    row["ItemCode"],
                    row["OpenQty"],
                    row["DummyFlag"],
                    row["IssueDate"],
                    row["CreateDate"]
                );
            }

            var sortedRows = dt_KatsuoUplod.AsEnumerable()
                .OrderBy(row => row["PurchaseOrderNo"] as string ?? "")
                .ThenBy(row => row["PromiseDate"] == DBNull.Value
                    ? DateTime.MinValue
                    : row.Field<DateTime>("PromiseDate"))
                .ToList();

            dt_KatsuoUplod = sortedRows.CopyToDataTable();

            // U_Katsuo へ INSERT
            foreach (DataRow row in dt_KatsuoUplod.Rows)
            {
                _repo.InsertUKatsuo(row);
            }

            return Json(new { message = "OK" });
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
}
