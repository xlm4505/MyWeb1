using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;

public class ReceivingActiveController : Controller
{
    public IActionResult ReceivingActive()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UploadExcel(IFormFile[] excelFiles)
    {
        try
        {
            string[] allowedPrefixes = ["Receiving_Active", "CI_Summary"];
            var invalidFile = excelFiles
                .FirstOrDefault(f => !allowedPrefixes.Any(p => f.FileName.StartsWith(p, StringComparison.OrdinalIgnoreCase)));

            if (invalidFile != null)
                return Json(new { error_msg = $"Invalid file name: \"{invalidFile.FileName}\". File name must start with \"Receiving_Active\" or \"CI_Summary\"." });

            var targetFile = excelFiles
                .FirstOrDefault(f => f.FileName.StartsWith("Receiving_Active", StringComparison.OrdinalIgnoreCase));

            if (targetFile == null)
                return Json(new { error_msg = "Receiving_Active file not found." });

            ExcelPackage.License.SetNonCommercialOrganization("Fujikin of America, Inc.");

            using var inputStream = new MemoryStream();
            await targetFile.CopyToAsync(inputStream);
            inputStream.Position = 0;

            using var package = new ExcelPackage(inputStream);
            var ws = package.Workbook.Worksheets[0];

            // 4行目: K列(11)以降で "Bal" を含む列 → 最終列
            int totalCols = ws.Dimension?.End.Column ?? 0;
            int balCol = -1;
            for (int col = 11; col <= totalCols; col++)
            {
                if ((ws.Cells[4, col].Text ?? "").Contains("Bal"))
                {
                    balCol = col;
                    break;
                }
            }
            if (balCol == -1)
                return Json(new { error_msg = "Row 4: \"Bal\" column not found." });

            // 4行目: K列から最終列(Bal列)の間で入力ありの最終列を取得
            int lastFilledColRow4 = -1;
            for (int col = balCol-1; col >= 11; col--)
            {
                if (!string.IsNullOrWhiteSpace(ws.Cells[4, col].Text))
                {
                    lastFilledColRow4 = col;
                    break;
                }
            }

            //// 5行目: K列から最終列(Bal列)の間で入力ありの最終セル値を取得
            //object? lastFilledValueRow5 = null;
            //for (int col = balCol-1; col >= 11; col--)
            //{
            //    var cell = ws.Cells[5, col];
            //    if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Text))
            //    {
            //        lastFilledValueRow5 = cell.Value;
            //        break;
            //    }
            //}

            // lastFilledColRow4 と Bal列の間の空白列数
            int emptyColsBetween = (lastFilledColRow4 != -1) ? balCol - lastFilledColRow4 - 1 : -1;

            // CI_Summary ファイルの RA_Input シートから最終行A列の値を取得
            var ciFile = excelFiles
                .FirstOrDefault(f => f.FileName.StartsWith("CI_Summary", StringComparison.OrdinalIgnoreCase));

            if (ciFile == null)
                return Json(new { error_msg = "CI_Summary file not found." });

            using var ciStream = new MemoryStream();
            await ciFile.CopyToAsync(ciStream);
            ciStream.Position = 0;

            using var ciPackage = new ExcelPackage(ciStream);
            var raInputSheet = ciPackage.Workbook.Worksheets["RA_Input"];

            if (raInputSheet == null)
                return Json(new { error_msg = "RA_Input sheet not found in CI_Summary file." });

            int ciLastRow = raInputSheet.Dimension?.End.Row ?? 0;
            if (ciLastRow == 0)
                return Json(new { error_msg = "RA_Input sheet is empty." });

            // C列 (FileName) のユニーク値を順序付きで収集
            var fileNameColOffset = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            int offsetCounter = 0;
            for (int raRow = 2; raRow <= ciLastRow; raRow++)
            {
                string fn = raInputSheet.Cells[raRow, 3].Text ?? "";
                if (!fileNameColOffset.ContainsKey(fn))
                    fileNameColOffset[fn] = ++offsetCounter;
            }
            int uniqueFileNameCount = offsetCounter;

            // インサート後のBal列位置（インサートなし時は元のbalColのまま）
            int currentBalCol = balCol;

            // ユニークFileName数 > 空白列数 の場合、差分列をインサート
            if (uniqueFileNameCount > emptyColsBetween)
            {
                int diff = uniqueFileNameCount - emptyColsBetween;
                int sourceCol = balCol - 1;
                int totalRows = ws.Dimension?.End.Row ?? 0;

                // Bal列の前に diff + 1 列を挿入（Bal列以降が右にシフト）
                ws.InsertColumn(balCol, diff + 1);

                // Bal列の前列（sourceCol）を挿入した各列にコピー（式・スタイル含む）
                for (int newCol = balCol; newCol < balCol + diff + 1; newCol++)
                {
                    ws.Cells[1, sourceCol, totalRows, sourceCol]
                        .Copy(ws.Cells[1, newCol, totalRows, newCol]);
                }

                // インサート後の新Bal列のSUM計算式の終了列をBal前列に更新
                int newBalCol = balCol + diff + 1;
                currentBalCol = newBalCol;
                string newEndColLetter = ToColumnLetter(newBalCol - 1);

                for (int row = 1; row <= totalRows; row++)
                {
                    string formula = ws.Cells[row, newBalCol].Formula ?? "";
                    if (Regex.IsMatch(formula, @"\$?[A-Za-z]+\$?\d+-SUM\(", RegexOptions.IgnoreCase))
                    {
                        ws.Cells[row, newBalCol].Formula = Regex.Replace(formula,
                            @"\$?([A-Za-z]+)\$?(\d+):\$?([A-Za-z]+)\$?(\d+)",
                            m => $"{m.Groups[1].Value}{m.Groups[2].Value}:{newEndColLetter}{m.Groups[4].Value}");
                    }
                }
            }

            int raLastRow = ws.Dimension?.End.Row ?? 0;
            //double row5Counter = Convert.ToDouble(lastFilledValueRow5 ?? 0);

            // Bal式をパース: jCol=J, sumStartCol=L
            // $絶対参照を含む場合も対応
            int jCol = -1;
            int sumStartCol = 11;
            for (int row = 8; row <= raLastRow; row++)
            {
                string formula = ws.Cells[row, currentBalCol].Formula ?? "";
                var m = Regex.Match(formula,
                    @"\$?([A-Za-z]+)\$?\d+-SUM\(\$?([A-Za-z]+)\$?\d+", RegexOptions.IgnoreCase);
                if (m.Success)
                {
                    jCol = FromColumnLetter(m.Groups[1].Value);
                    sumStartCol = FromColumnLetter(m.Groups[2].Value);
                    break;
                }
                var m2 = Regex.Match(formula, @"SUM\(\$?([A-Za-z]+)\$?\d+", RegexOptions.IgnoreCase);
                if (m2.Success) { sumStartCol = FromColumnLetter(m2.Groups[1].Value); break; }
            }

            // Bal列を数値化: 式を保存して Bal = J値 - SUM(割り当て済み) で上書き
            var balFormulas = new Dictionary<int, string>();
            for (int row = 8; row <= raLastRow; row++)
            {
                string formula = ws.Cells[row, currentBalCol].Formula ?? "";
                if (formula.IndexOf("SUM", StringComparison.OrdinalIgnoreCase) < 0) continue;

                double allocatedSum = 0;
                for (int c = sumStartCol; c < currentBalCol; c++)
                    allocatedSum += ws.Cells[row, c].Value is double d ? d : 0;

                double balValue = jCol > 0
                    ? (ws.Cells[row, jCol].Value is double jv ? jv : 0) - allocatedSum
                    : allocatedSum;

                balFormulas[row] = formula;
                ws.Cells[row, currentBalCol].Value = balValue;
            }

            // I列（col 9）を事前インデックス化してO(m×n)スキャンをO(1)ルックアップに置換
            var iColIndex = new Dictionary<string, List<int>>(StringComparer.OrdinalIgnoreCase);
            for (int row = 8; row <= raLastRow; row++)
            {
                string iVal = ws.Cells[row, 9].Text ?? "";
                if (!iColIndex.TryGetValue(iVal, out var rowList))
                    iColIndex[iVal] = rowList = new List<int>();
                rowList.Add(row);
            }

            var headerWritten = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            for (int raRow = 2; raRow <= ciLastRow; raRow++)
            {
                string fileName = raInputSheet.Cells[raRow, 3].Text ?? ""; // C列 = FileName
                int col = lastFilledColRow4 + fileNameColOffset[fileName];

                // ヘッダ行は各FileName初回のみ書き込み
                if (headerWritten.Add(fileName))
                {
                    // 3行目: RA_Input D列
                    ws.Cells[3, col].Value = raInputSheet.Cells[raRow, 4].Value;

                    // 4行目: システム日付
                    ws.Cells[4, col].Value = DateTime.Now.Date;

                    // 6行目: RA_Input C列 (FileName)
                    ws.Cells[6, col].Value = raInputSheet.Cells[raRow, 3].Value;

                    // 7行目: RA_Input E列（折り返し全体を表示）
                    ws.Cells[7, col].Value = raInputSheet.Cells[raRow, 5].Value;
                    ws.Cells[7, col].Style.WrapText = true;
                    ws.Cells[7, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    ws.Cells[7, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                }

                // 8行目以降: RA_Input H列とReceiving_Active I列が一致し、Bal列が0以外の行に割り当て
                string hMatchKey = raInputSheet.Cells[raRow, 8].Text ?? "";
                double hRemaining = Convert.ToDouble(raInputSheet.Cells[raRow, 9].Value ?? 0);

                if (!iColIndex.TryGetValue(hMatchKey, out var candidates)) continue;

                foreach (int row in candidates)
                {
                    double bal = Convert.ToDouble(ws.Cells[row, currentBalCol].Value ?? 0);
                    if (bal == 0) continue;

                    if (hRemaining <= bal)
                    {
                        ws.Cells[row, col].Value = hRemaining;
                        ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        ws.Cells[row, currentBalCol].Value = bal - hRemaining;
                        break;
                    }
                    else
                    {
                        ws.Cells[row, col].Value = bal;
                        ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        ws.Cells[row, currentBalCol].Value = 0;
                        hRemaining -= bal;
                    }
                }
            }

            // Bal列に式を復元
            foreach (var (row, formula) in balFormulas)
                ws.Cells[row, currentBalCol].Formula = formula;

            var outputStream = new MemoryStream();
            await package.SaveAsAsync(outputStream);

            return File(outputStream.ToArray(),
                "application/vnd.ms-excel.sheet.macroEnabled.12",
                targetFile.FileName);
        }
        catch (Exception ex)
        {
            return Json(new { error_msg = $"server error: {ex.Message}" });
        }
    }

    private static int FromColumnLetter(string col)
    {
        int result = 0;
        foreach (char c in col.ToUpper())
            result = result * 26 + (c - 'A' + 1);
        return result;
    }

    private static string ToColumnLetter(int col)
    {
        string letter = "";
        while (col > 0)
        {
            col--;
            letter = (char)('A' + col % 26) + letter;
            col /= 26;
        }
        return letter;
    }
}
