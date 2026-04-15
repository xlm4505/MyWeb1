using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;
using System.Diagnostics;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

public class PurchaseReceiptController : Controller
{

    private readonly Repository_PurchaseReceiptTK _repo;
    private readonly Repository_PurchaseReceiptFJKCheck _repo_FJCcheck;
    private readonly Repository_PurchaseReceiptCCL _repoCCL;
    public PurchaseReceiptController(
        Repository_PurchaseReceiptTK repo,
        Repository_PurchaseReceiptFJKCheck repoFJKCheck,
        Repository_PurchaseReceiptCCL repoCCL)
    {
        _repo = repo;
        _repo_FJCcheck = repoFJKCheck;
        _repoCCL = repoCCL;
    }

    public IActionResult PurchaseReceipt()
    {
        return View();
    }

    //TK用のアクション
    [HttpPost]
    public IActionResult ProcessTK(List<IFormFile> files)
    {
        try
        {
            //var ip = HttpContext.Connection.RemoteIpAddress?
            //    .MapToIPv4()
            //    .ToString();
            var ip = GetClientIp();

            var userName = HttpContext.Session.GetString("LoginUser");

            //string _sageClientHost = "10.32.75.126"; // used when launching pvxwin32 (from original)
            //string _sagePvXPath = @"C:\Sage\Sage 100 Workstation\MAS90\Home\pvxwin32.EXE";
            //string _sageHomeCd = @"C:\Sage\Sage 100 Workstation\MAS90\Home";
            //string _sageLauncherArgTemplate = "\"{0}\" ../launcher/sota.ini *Client -ARG \"{1}\" \"9921\" \"Import\" -ARG=DIRECT UIOFF {2} {3} FOA VIWI1C AUTO";
            //string Quote(string s) => $"\"{s}\"";

            Model_InvoiceHeader_PurchaseReceiptTK lastHeader = null;
            var lastDetails = new List<Model_InvoiceDetail_PurchaseReceiptTK>();

            foreach (var file in files)
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);

                //CI sheet 存在しない場合
                if (!workbook.Worksheets.Contains("CI"))
                    throw new Exception($"No CI sheet in {file.FileName}");

                var ws = workbook.Worksheet("CI");

                //INVOICE文言が存在しない場合
                if (ws.Cell(2, 2).GetString() != "INVOICE")
                    throw new Exception("Error!: There's no invoice file");

                //HS Codeのような文言が存在しない場合
                var value = ws.Cell(13, 2).GetString();  
                if (!value.StartsWith("HS Code", StringComparison.Ordinal))
                {
                    throw new Exception("Wrong format (HS Code)");
                }

                var header = ReadHeader(ws);

                if (_repo.InvoiceExists(header.InvoiceNo))
                    throw new Exception($"Invoice exists: {header.InvoiceNo}");

                var details = ReadDetails(ws);

                ValidateTotals(ws, details);

                foreach (var d in details)
                {
                    var poNo7 = d.PoNo.PadLeft(7, '0');

                    // PO取得
                    var poList = _repo
                        .GetPoDetails(poNo7, d.PartNo)
                        .OrderBy(x => x.LineKey)
                        .ToList();

                    if (!poList.Any())
                        throw new Exception("Error!: PO missing");

                    // Unit Priceチェック
                    if (!poList.Any(x => x.UnitCost == d.UP))
                        throw new Exception("Wrong unit price.");

                    // Open Qty合計
                    var totalOpenQty = poList.Sum(x => x.QuantityOrdered - x.QuantityReceived);

                    if (d.Quantity > totalOpenQty)
                        throw new Exception(
                        $"Quantity received is more than open quantity.<br>" +
                        $"PO#: {poNo7}<br>" +
                        $"Item: {d.PartNo}"
                    );

                    decimal remainQty = d.Quantity ?? 0;

                    // Qty分割
                    foreach (var po in poList)
                    {
                        if (remainQty <= 0)
                            break;

                        decimal openQty =  (po.QuantityOrdered ?? 0) - (po.QuantityReceived ?? 0);

                        if (openQty <= 0)
                            continue;

                        decimal allocated;

                        if (openQty >= remainQty)
                        {
                            allocated = remainQty;
                            remainQty = 0;
                        }
                        else
                        {
                            allocated = openQty;
                            remainQty -= openQty;
                        }

                        var newDetail = new Model_InvoiceDetail_PurchaseReceiptTK
                        {
                            HSCode = d.HSCode,
                            HSName = d.HSName,
                            PoNo = poNo7,
                            PartNo = d.PartNo,
                            Description = d.Description,
                            Quantity = d.Quantity,
                            UP = d.UP,
                            Amount = d.Amount,

                            LineKey = po.LineKey,
                            ItemCode = po.ItemCode,
                            WarehouseCode = po.WarehouseCode,
                            UnitCost = po.UnitCost,
                            QuantityOrdered = po.QuantityOrdered,
                            QuantityReceived = po.QuantityReceived,
                            OrderStatus = po.OrderStatus,

                            //AllocatedQty = allocated
                        };

                        _repo.InsertUploadData(header, newDetail, ip, userName);

                        lastDetails.Add(newDetail);
                    }
                }

                _repo.UpdateStatus(ip);

                //string saltValue = (userName + "*******").Substring(0, Math.Min(8, userName.Length + 7));
                //string password = _repo.GetPassword(saltValue);
                //string userPassword = "";

                //if (string.IsNullOrEmpty(password))
                //{
                //    throw new ApplicationException("SAGE password for user not found in U_User. Please supply password to continue.");
                //}
                //else
                //{
                //    userPassword = Decrypt(password, saltValue);
                //}


                //// Trigger SAGE100 import via pvxwin32.EXE
                //string cmdLine = $"cmd /S /C {Quote(_sageHomeCd)} && {string.Format(_sageLauncherArgTemplate, _sagePvXPath, _sageClientHost, userName, userPassword)}";
                //// 
                //ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/S /C " + $"cd /d \"{_sageHomeCd}\" && \"{_sagePvXPath}\" ../launcher/sota.ini *Client -ARG \"{_sageClientHost}\" \"9921\" \"Import\" -ARG=DIRECT UIOFF {userName} {userPassword} FOA VIWI1C AUTO")
                //{
                //    CreateNoWindow = true,
                //    UseShellExecute = false
                //};
                //using (Process p = Process.Start(psi))
                //{
                //    p.WaitForExit();
                //}

                lastHeader = header;
            }

            // Detail.xlsx生成
            using var exportBook = new XLWorkbook();
            CreateDetailSheet(exportBook, lastHeader, lastDetails);

            using var ms = new MemoryStream();
            exportBook.SaveAs(ms);
            ms.Position = 0;



            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Check List_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }
    [HttpPost]
    public async Task<IActionResult> CheckFJK(List<IFormFile> files)
    {
        if (files.Count == 0)
        {
            return BadRequest("No files uploaded.");
        }

        var conversion = await _repo_FJCcheck.ConvertInvoicesAsync(files);
        if (conversion.HasError)
        {
            return BadRequest(string.Join(Environment.NewLine, conversion.Errors));
        }

        var summaryFile = conversion.Files.FirstOrDefault();
        if (summaryFile is null)
        {
            return BadRequest("No summary file was generated.");
        }

        return Json(new
        {
            success = true,
            message = "Completed",
            files = conversion.Files.Select(x => new
            {
                fileName = x.FileName,
                contentBase64 = Convert.ToBase64String(x.Content)
            })
        });
    }


    private Model_InvoiceHeader_PurchaseReceiptTK ReadHeader(IXLWorksheet ws)
    {
        // No.& Date of Invoice
        var text = ws.Cell("H4").GetString();

        var parsed = ParseInvoice(text);

        return new Model_InvoiceHeader_PurchaseReceiptTK
        {
            InvoiceNo = parsed.invoiceNo,
            InvoiceDate = parsed.invoiceDate,
            EntryDate = DateTime.Now
        };
    }

    private List<Model_InvoiceDetail_PurchaseReceiptTK> ReadDetails(IXLWorksheet ws)
    {
        var list = new List<Model_InvoiceDetail_PurchaseReceiptTK>();

        int row = 15; // ← 明細の1行目

        while (true)
        {
            // 1行目側（HS Code + Po No）の F列 を見て "Total" なら終了
            var totalMark = ws.Cell(row, 6).GetString();   // F列
            if (totalMark == "Total")
                break;

            var HsCode = ws.Cell(row, 2).GetString();        // B列
            var HsName = ws.Cell(row + 1, 2).GetString();

            var poNo = ws.Cell(row, 3).GetString();        //C列
            var partNo = ws.Cell(row + 1, 3).GetString();

            var Description = ws.Cell(row, 6).GetString();        // F列

            // Po No が空なら明細終わりとみなす
            if (string.IsNullOrWhiteSpace(poNo))
                break;

            // Part No が空ならスキップ
            if (string.IsNullOrWhiteSpace(partNo))
            {
                row += 2;
                continue;
            }

            list.Add(new Model_InvoiceDetail_PurchaseReceiptTK
            {
                HSCode = HsCode.Trim(),
                HSName = HsName.Trim(),
                PoNo = poNo.Trim(),
                PartNo = partNo.Trim(),

                Description = Description.Trim(),
                Quantity = GetDecimalSafe(ws.Cell(row, 10)),        //J列
                UP = GetDecimalSafe(ws.Cell(row, 13)),        //M列
                Amount = GetDecimalSafe(ws.Cell(row, 15))         //O列

            });

            // ★ 2行で1明細なので +2
            row += 2;
        }

        return list;
    }


    private void ValidateTotals(
    IXLWorksheet ws,
    List<Model_InvoiceDetail_PurchaseReceiptTK> details)
    {
        //Quantityの合計値を計算
        decimal detailQuantityTotal = details.Sum(x => x.Quantity ?? 0);
        //Amountの合計値を計算
        decimal detailAmountTotal = details.Sum(x => x.Amount ?? 0);

        int row = 15;

        while (true)
        {
            if (ws.Cell(row, 6).GetString() == "Total")  // F列
                break;

            row += 2;
        }

        //Quantity列からTotalを取得
        decimal excelQuantityTotal = GetDecimalSafe(ws.Cell(row, 10)); // J列
        //Amount列からTotalを取得
        decimal excelAmountTotal = GetDecimalSafe(ws.Cell(row, 15)); // O列

        //Quantity列から取得したTotal≠Quantityの合計値の場合、エラーとし、終了
        if (detailQuantityTotal != excelQuantityTotal)
            throw new Exception(
                $"Error!: Total quantity not matched. Excel:{excelQuantityTotal} System:{detailQuantityTotal}"
            );
        //Amount列から取得したTotal≠Amountの合計値の場合、エラーとし、終了
        if (detailAmountTotal != excelAmountTotal)
            throw new Exception(
                $"Error!: Total amount not matched. Excel:{detailAmountTotal} System:{excelAmountTotal}"
            );
    }

    private decimal GetDecimalSafe(IXLCell cell)
    {
        if (cell == null)
            return 0;

        // 空白なら0
        if (cell.IsEmpty())
            return 0;

        // 文字列として取得してパース
        decimal.TryParse(cell.GetString(), out var result);
        return result;
    }

    private (string invoiceNo, DateTime invoiceDate) ParseInvoice(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new Exception("Invoice No/Date is missing");

        string[] parts = text.Split('/');

        if (parts.Length != 2)
            throw new Exception("Invoice No/Date format invalid");

        var invoiceNo = parts[0].Trim();
        var datePart = parts[1].Trim();

        if (!DateTime.TryParseExact(
                datePart,
                "yy.MM.dd",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out var invoiceDate))
        {
            throw new Exception("Invoice Date format invalid");
        }

        return (invoiceNo, invoiceDate);
    }

    private string GetClientIp()
    {
        string targetPrefix = "10.32.58.";
        string? selectedIp = null;

        var host = Dns.GetHostEntry(Dns.GetHostName());

        foreach (var ip in host.AddressList)
        {
            if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) // IPv4
            {
                string ipStr = ip.ToString();

                if (ipStr.StartsWith(targetPrefix, StringComparison.Ordinal))
                {
                    selectedIp = ipStr;
                    break;
                }
            }
        }

        if (string.IsNullOrEmpty(selectedIp))
        {
            selectedIp = Dns.GetHostEntry(Dns.GetHostName())
            .AddressList
            .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)?
            .ToString() ?? "0.0.0.0";
        }

        return selectedIp;
    }

    //public string Decrypt(string cipherText, string saltValue)
    //{
    //    string password = "12345678";
    //    int iterations = 2;
    //    string ivString = "@1B2c3D4e5F6g7H8"; // 16 chars
    //    int keySize = 256;
    //    byte[] iv = Encoding.ASCII.GetBytes(ivString);
    //    byte[] saltBytes = Encoding.ASCII.GetBytes(saltValue);
    //    byte[] cipherBytes = Convert.FromBase64String(cipherText);
    //    var key = new Rfc2898DeriveBytes(password, saltBytes, iterations).GetBytes(keySize / 8);
    //    using (var aes = new RijndaelManaged { Mode = CipherMode.CBC })
    //    using (var ms = new MemoryStream(cipherBytes))
    //    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(key, iv), CryptoStreamMode.Read))
    //    {
    //        byte[] plain = new byte[cipherBytes.Length];
    //        int read = cs.Read(plain, 0, plain.Length);
    //        return Encoding.UTF8.GetString(plain, 0, read);
    //    }
    //}

    private IXLWorksheet CreateDetailSheet(
    XLWorkbook workbook,
    Model_InvoiceHeader_PurchaseReceiptTK header,
    List<Model_InvoiceDetail_PurchaseReceiptTK> details)
    {
        var ws = workbook.Worksheets.Add("Detail");

        // ヘッダー行
        ws.Cell(1, 1).Value = "Invoice No.";
        ws.Cell(1, 2).Value = "Invoice Date";
        ws.Cell(1, 3).Value = "Entry Date";
        ws.Cell(1, 4).Value = "Status";
        ws.Cell(1, 5).Value = "PO No.";
        ws.Cell(1, 6).Value = "Ln";
        ws.Cell(1, 7).Value = "Part Number (TK)";
        ws.Cell(1, 8).Value = "Description (TK)";
        ws.Cell(1, 9).Value = "Item Code";
        ws.Cell(1, 10).Value = "WH";
        ws.Cell(1, 11).Value = "UnitCost";
        ws.Cell(1, 12).Value = "OrderQty";
        ws.Cell(1, 13).Value = "ShippedQty";
        ws.Cell(1, 14).Value = "AllocatedQty";
        ws.Cell(1, 15).Value = "Purchase Price";
        ws.Cell(1, 16).Value = "Amount";
        ws.Cell(1, 17).Value = "Batch";

        ws.Range(1, 1, 1, 17).Style.Font.Bold = true;

        int row = 2;

        foreach (var d in details)
        {
            ws.Cell(row, 1).Value = header.InvoiceNo;
            ws.Cell(row, 2).Value = header.InvoiceDate.ToString("M/dd/yyyy");
            ws.Cell(row, 3).Value = header.EntryDate.ToString("M/dd/yyyy");
            ws.Cell(row, 4).Value = d.OrderStatus;
            ws.Cell(row, 5).Value = d.PoNo.PadLeft(7, '0');
            ws.Cell(row, 6).Value = int.Parse(d.LineKey);
            ws.Cell(row, 7).Value = d.PartNo;
            ws.Cell(row, 8).Value = "";
            ws.Cell(row, 9).Value = d.ItemCode;
            ws.Cell(row, 10).Value = d.WarehouseCode;
            ws.Cell(row, 11).Value = d.UnitCost;
            ws.Cell(row, 12).Value = d.QuantityOrdered;
            ws.Cell(row, 13).Value = d.QuantityReceived;
            ws.Cell(row, 14).Value = d.Quantity;
            ws.Cell(row, 15).Value = d.UP;
            ws.Cell(row, 16).Value = d.UP*d.Quantity;
            ws.Cell(row, 17).Value = header.InvoiceNo.Substring(header.InvoiceNo.Length - 5);

            if (row % 2 == 0)
            {
                ws.Range(row, 1, row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 230, 153);
            }


            row++;
        }

        // 数値列フォーマット
        ws.Column(6).Style.NumberFormat.Format = "0";
        //ws.Column(8).Style.NumberFormat.Format = "0.00";
        //ws.Column(12).Style.NumberFormat.Format = "0.00";

        ws.Column(11).Style.NumberFormat.Format = "0.00";
        ws.Column(15).Style.NumberFormat.Format = "0.00";
        ws.Column(16).Style.NumberFormat.Format = "0.00";

        ws.Cell(row, 14).FormulaA1 = $"=SUM(N2:N{row - 1})";
        ws.Cell(row, 14).Style.Font.Bold = true;

        ws.Cell(row, 16).FormulaA1 = $"=SUM(P2:P{row - 1})";
        ws.Cell(row, 16).Style.Font.Bold = true;

        ws.Range(1, 1, 1, 17).Style.Font.Bold = true;
        ws.Range(1, 1, 1, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 192, 0);

        ws.Columns().AdjustToContents();

        return ws;

    }

    //CCL用のアクション
    [HttpPost]
    public IActionResult ProcessCCL(List<IFormFile> files)
    {
        try
        {
            //var ip = HttpContext.Connection.RemoteIpAddress?
            //    .MapToIPv4()
            //    .ToString();
            var ip = GetClientIp();

            var userName = HttpContext.Session.GetString("LoginUser");

            //string _sageClientHost = "10.32.75.126"; // used when launching pvxwin32 (from original)
            //string _sagePvXPath = @"C:\Sage\Sage 100 Workstation\MAS90\Home\pvxwin32.EXE";
            //string _sageHomeCd = @"C:\Sage\Sage 100 Workstation\MAS90\Home";
            //string _sageLauncherArgTemplate = "\"{0}\" ../launcher/sota.ini *Client -ARG \"{1}\" \"9921\" \"Import\" -ARG=DIRECT UIOFF {2} {3} FOA VIWI1C AUTO";
            //string Quote(string s) => $"\"{s}\"";

            Model_InvoiceHeader_PurchaseReceiptCCL lastHeader = null;
            var allDetails = new List<Model_InvoiceDetail_PurchaseReceiptCCL>();

            foreach (var file in files)
            {
                using var stream = file.OpenReadStream();
                using var workbook = new XLWorkbook(stream);


                // ARForm から始まるシートをすべて取得
                var arSheets = workbook.Worksheets
                    .Where(ws => ws.Name.StartsWith("ARForm", StringComparison.Ordinal))
                    .ToList();

                //ARFormから始まる sheet 存在しない場合
                if (!arSheets.Any())
                    throw new Exception($"No ARForm sheet in {file.FileName}");

                foreach (var ws in arSheets)
                {
                    //var ws = workbook.Worksheet("CI");

                    //INVOICE文言が存在しない場合
                    if (ws.Cell(10, 34).GetString() != "INVOICE")
                        throw new Exception("Error!: There's no invoice file");

                    //Invoice Dateが存在しない場合
                    // セルの値を取得
                    string cellValue = ws.Cell(57, 32).GetString();
                    // 右10文字を取得
                    string right10 = cellValue.Length >= 10
                        ? cellValue.Substring(cellValue.Length - 10)
                        : cellValue;
                    // dd/MM/yyyy の形式かどうか判定
                    bool isDateFormat = Regex.IsMatch(right10, @"^\d{2}/\d{2}/\d{4}$");
                    if (!isDateFormat)
                    {
                        throw new Exception("Error!: Invoice Date is missing");
                    }

                    var header = ReadHeaderCCL(ws);

                    //invoice no すでに存在する場合
                    if (_repoCCL.InvoiceExists(header.InvoiceNo))
                        throw new Exception($"Invoice exists: {header.InvoiceNo}");


                    var details = ReadDetailsCCL(ws);

                    ValidateTotalsCCL(ws, details);

                    foreach (var d in details)
                    {

                        // PO取得
                        var poList = _repoCCL
                            .GetPoDetails(header.PoNo, d.PartNo)
                            .OrderBy(x => x.LineKey)
                            .ToList();

                        if (!poList.Any())
                            throw new Exception("Error!: PO missing");

                        // Unit Priceチェック
                        if (!poList.Any(x => x.UnitCost == d.UP))
                            throw new Exception("Wrong unit price.");

                        // Open Qty合計
                        var totalOpenQty = poList.Sum(x => x.QuantityOrdered - x.QuantityReceived);

                        if (d.Quantity > totalOpenQty)
                            throw new Exception(
                            $"Quantity received is more than open quantity.<br>" +
                            $"PO#: {header.PoNo}<br>" +
                            $"Item: {d.PartNo}"
                        );

                        decimal remainQty = d.Quantity ?? 0;

                        // Qty分割
                        foreach (var po in poList)
                        {
                            if (remainQty <= 0)
                                break;

                            decimal openQty = (po.QuantityOrdered ?? 0) - (po.QuantityReceived ?? 0);

                            if (openQty <= 0)
                                continue;

                            decimal allocated;

                            if (openQty >= remainQty)
                            {
                                allocated = remainQty;
                                remainQty = 0;
                            }
                            else
                            {
                                allocated = openQty;
                                remainQty -= openQty;
                            }

                            var newDetail = new Model_InvoiceDetail_PurchaseReceiptCCL
                            {
                                HSCode = d.HSCode,
                                HSName = d.HSName,
                                PoNo = header.PoNo,
                                PartNo = d.PartNo,
                                Description = d.Description,
                                Quantity = d.Quantity,
                                UP = d.UP,
                                Amount = d.Amount,

                                LineKey = po.LineKey,
                                ItemCode = po.ItemCode,
                                WarehouseCode = po.WarehouseCode,
                                UnitCost = po.UnitCost,
                                QuantityOrdered = po.QuantityOrdered,
                                QuantityReceived = po.QuantityReceived,
                                OrderStatus = po.OrderStatus,

                                //AllocatedQty = allocated
                            };

                            _repoCCL.InsertUploadData(header, newDetail, ip, userName);

                            allDetails.Add(newDetail);
                        }
                    }


                    lastHeader = header;

                    _repoCCL.UpdateStatus(ip);
                }

                //string saltValue = (userName + "*******").Substring(0, Math.Min(8, userName.Length + 7));
                //string password = _repo.GetPassword(saltValue);
                //string userPassword = "";

                //if (string.IsNullOrEmpty(password))
                //{
                //    throw new ApplicationException("SAGE password for user not found in U_User. Please supply password to continue.");
                //}
                //else
                //{
                //    userPassword = Decrypt(password, saltValue);
                //}


                //// Trigger SAGE100 import via pvxwin32.EXE
                //string cmdLine = $"cmd /S /C {Quote(_sageHomeCd)} && {string.Format(_sageLauncherArgTemplate, _sagePvXPath, _sageClientHost, userName, userPassword)}";
                //// 
                //ProcessStartInfo psi = new ProcessStartInfo("cmd.exe", "/S /C " + $"cd /d \"{_sageHomeCd}\" && \"{_sagePvXPath}\" ../launcher/sota.ini *Client -ARG \"{_sageClientHost}\" \"9921\" \"Import\" -ARG=DIRECT UIOFF {userName} {userPassword} FOA VIWI1C AUTO")
                //{
                //    CreateNoWindow = true,
                //    UseShellExecute = false
                //};
                //using (Process p = Process.Start(psi))
                //{
                //    p.WaitForExit();
                //}


            }

            // Detail.xlsx生成
            using var exportBook = new XLWorkbook();
            CreateDetailSheetCCL(exportBook, lastHeader, allDetails);

            using var ms = new MemoryStream();
            exportBook.SaveAs(ms);
            ms.Position = 0;



            return File(
                ms.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Check List_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
            );
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    private Model_InvoiceHeader_PurchaseReceiptCCL ReadHeaderCCL(IXLWorksheet ws)
    {
        // InvoiceDateの値を取得
        string cellValue = ws.Cell(57, 32).GetString();
        // 右10文字を取得
        string invoiceDateStr = cellValue.Length >= 10
            ? cellValue.Substring(cellValue.Length - 10)
            : cellValue;

        DateTime invoiceDate;
        if (!DateTime.TryParseExact(
                invoiceDateStr,
                "dd/MM/yyyy",
                System.Globalization.CultureInfo.InvariantCulture,
                System.Globalization.DateTimeStyles.None,
                out invoiceDate))
        {
            throw new Exception("Invoice Date format is invalid.");
        }

        string cellValue2 = ws.Cell(15, 35).GetString();

        string invoiceNo = cellValue2.Length >= 6
            ? cellValue2.Substring(cellValue2.Length - 6)
            : cellValue2;

        string cellValue3 = ws.Cell(57, 7).GetString();
        int newlineIndex = cellValue3.IndexOf('\n');
        var poNo = cellValue3.Substring(newlineIndex + 1).Trim();


        return new Model_InvoiceHeader_PurchaseReceiptCCL
        {
            InvoiceNo = invoiceNo,
            InvoiceDate = invoiceDate,
            EntryDate = DateTime.Now,
            PoNo = poNo
        };
    }

    private List<Model_InvoiceDetail_PurchaseReceiptCCL> ReadDetailsCCL(IXLWorksheet ws)
    {
        var list = new List<Model_InvoiceDetail_PurchaseReceiptCCL>();

        int startRow; // ← 明細の1行目
        if (ws.Cell(62, 1).GetString() == "Item")
        {
            startRow = 63;
        }
        else if (ws.Cell(63, 1).GetString() == "Item")
        {
            startRow = 64;
        }
        else
        {
            throw new Exception("Item header not found");
        }

        // ■ 最終行取得（A列ベース）
        int lastRow = ws.LastRowUsed().RowNumber();
        // 下から上に向かって「A列が 'Terms and Conditions' で始まらない行」を探す
        while (lastRow >= 1)
        {
            string aValue = ws.Cell(lastRow, 1).GetString().Trim();

            // ★ 先頭一致で判定（StartsWith）
            if (!aValue.StartsWith("Terms and Conditions", StringComparison.OrdinalIgnoreCase)
                && !string.IsNullOrWhiteSpace(aValue))
            {
                break; // ← ここが実際の最終行
            }

            lastRow--;
        }
        int row2 = startRow;


        while (row2 <= lastRow)
        {
            // ■ A列の連番チェック
            var noStr = ws.Cell(row2, 1).GetString();

            if (!int.TryParse(noStr, out int no))
                break;

            if (no != row2 - startRow + 1)
                break;

            var raw = ws.Cell(row2, 10).GetString();

            int hyphenIndex = raw.IndexOf('-');
            int newlineIndex = raw.IndexOf('\n');

            if (hyphenIndex < 0 || newlineIndex < 0)
            {
                throw new Exception("Error!: ItemCode is missing is missing");
            }

            // Part No（ItemCode）
            var itemCode = raw.Substring(0, newlineIndex).Trim();

            // Description
            var description = raw.Substring(newlineIndex + 1).Trim();


            list.Add(new Model_InvoiceDetail_PurchaseReceiptCCL
            {
                //PoNo = poNo.Trim(),
                PartNo = itemCode.Trim(),

                Description = description.Trim(),
                Quantity = GetDecimalSafe(ws.Cell(row2, 63)),
                UP = GetDecimalSafe(ws.Cell(row2, 57)),
                Amount = GetDecimalSafe(ws.Cell(row2, 72))

            });

            row2++;

        }

        return list;
    }

    private void ValidateTotalsCCL(
    IXLWorksheet ws,
    List<Model_InvoiceDetail_PurchaseReceiptCCL> details)
    {
        // 1. 明細行ごとの計算チェック
        foreach (var d in details)
        {
            decimal qty = d.Quantity ?? 0;
            decimal up = d.UP ?? 0;
            decimal amt = d.Amount ?? 0;

            decimal calc = qty * up;

            // 小数誤差対策で丸める
            if (Math.Round(calc, 2) != Math.Round(amt, 2))
            {
                throw new Exception(
                    $"Error!: Amount mismatch. PartNo:{d.PartNo}  Qty:{qty}  UP:{up}  ExcelAmount:{amt}  Calc:{calc}"
                );
            }
        }
    }

    private IXLWorksheet CreateDetailSheetCCL(
    XLWorkbook workbook,
    Model_InvoiceHeader_PurchaseReceiptCCL header,
    List<Model_InvoiceDetail_PurchaseReceiptCCL> details)
    {
        var ws = workbook.Worksheets.Add("Detail");

        // ヘッダー行
        ws.Cell(1, 1).Value = "Invoice No.";
        ws.Cell(1, 2).Value = "Invoice Date";
        ws.Cell(1, 3).Value = "Entry Date";
        ws.Cell(1, 4).Value = "Status";
        ws.Cell(1, 5).Value = "PO No.";
        ws.Cell(1, 6).Value = "Ln";
        ws.Cell(1, 7).Value = "Part Number (TK)";
        ws.Cell(1, 8).Value = "Description (TK)";
        ws.Cell(1, 9).Value = "Item Code";
        ws.Cell(1, 10).Value = "WH";
        ws.Cell(1, 11).Value = "UnitCost";
        ws.Cell(1, 12).Value = "OrderQty";
        ws.Cell(1, 13).Value = "ShippedQty";
        ws.Cell(1, 14).Value = "AllocatedQty";
        ws.Cell(1, 15).Value = "Purchase Price";
        ws.Cell(1, 16).Value = "Amount";
        ws.Cell(1, 17).Value = "Batch";

        ws.Range(1, 1, 1, 17).Style.Font.Bold = true;

        int row = 2;

        foreach (var d in details)
        {
            ws.Cell(row, 1).Value = header.InvoiceNo;
            ws.Cell(row, 2).Value = header.InvoiceDate.ToString("M/dd/yyyy");
            ws.Cell(row, 3).Value = header.EntryDate.ToString("M/dd/yyyy");
            ws.Cell(row, 4).Value = d.OrderStatus;
            ws.Cell(row, 5).Value = d.PoNo.PadLeft(7, '0');
            ws.Cell(row, 6).Value = int.Parse(d.LineKey);
            ws.Cell(row, 7).Value = d.PartNo;
            ws.Cell(row, 8).Value = "";
            ws.Cell(row, 9).Value = d.ItemCode;
            ws.Cell(row, 10).Value = d.WarehouseCode;
            ws.Cell(row, 11).Value = d.UnitCost;
            ws.Cell(row, 12).Value = d.QuantityOrdered;
            ws.Cell(row, 13).Value = d.QuantityReceived;
            ws.Cell(row, 14).Value = d.Quantity;
            ws.Cell(row, 15).Value = d.UP;
            ws.Cell(row, 16).Value = d.UP * d.Quantity;
            ws.Cell(row, 17).Value = string.Concat("B", header.InvoiceNo.AsSpan(header.InvoiceNo.Length - 4));

            if (row % 2 == 0)
            {
                ws.Range(row, 1, row, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 230, 153);
            }


            row++;
        }

        // 数値列フォーマット
        ws.Column(6).Style.NumberFormat.Format = "0";
        //ws.Column(8).Style.NumberFormat.Format = "0.00";
        //ws.Column(12).Style.NumberFormat.Format = "0.00";

        ws.Column(11).Style.NumberFormat.Format = "0.00";
        ws.Column(15).Style.NumberFormat.Format = "0.00";
        ws.Column(16).Style.NumberFormat.Format = "0.00";

        ws.Cell(row, 14).FormulaA1 = $"=SUM(N2:N{row - 1})";
        ws.Cell(row, 14).Style.Font.Bold = true;

        ws.Cell(row, 16).FormulaA1 = $"=SUM(P2:P{row - 1})";
        ws.Cell(row, 16).Style.Font.Bold = true;

        ws.Range(1, 1, 1, 17).Style.Font.Bold = true;
        ws.Range(1, 1, 1, 17).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 192, 0);

        ws.Columns().AdjustToContents();

        return ws;

    }
}
