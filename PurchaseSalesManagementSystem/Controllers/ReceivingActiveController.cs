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

            // 5行目: K列から最終列(Bal列)の間で入力ありの最終セル値を取得
            object? lastFilledValueRow5 = null;
            for (int col = balCol-1; col >= 11; col--)
            {
                var cell = ws.Cells[5, col];
                if (cell.Value != null && !string.IsNullOrWhiteSpace(cell.Text))
                {
                    lastFilledValueRow5 = cell.Value;
                    break;
                }
            }

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

            if (!int.TryParse(raInputSheet.Cells[ciLastRow, 1].Value?.ToString(), out int ciLastRowA))
                return Json(new { error_msg = "RA_Input: last row column A is not a valid number." });

            // インサート後のBal列位置（インサートなし時は元のbalColのまま）
            int currentBalCol = balCol;

            // 最終行A列の値 > 空白列数 の場合、差分列をインサート
            if (ciLastRowA > emptyColsBetween)
            {
                int diff = ciLastRowA - emptyColsBetween;
                int sourceCol = balCol - 1;
                int totalRows = ws.Dimension?.End.Row ?? 0;

                // Bal列の前に diff + 1 列を挿入（Bal列以降が右にシフト）
                ws.InsertColumn(balCol, diff + 1);

                // Bal列の前列（sourceCol）を挿入した各列にコピー
                for (int newCol = balCol; newCol < balCol + diff + 1; newCol++)
                {
                    for (int row = 1; row <= totalRows; row++)
                    {
                        var src = ws.Cells[row, sourceCol];
                        var dst = ws.Cells[row, newCol];
                        dst.Value = src.Value;
                        dst.StyleID = src.StyleID;
                    }
                }

                // インサート後の新Bal列のSUM計算式の終了列をBal前列に更新
                int newBalCol = balCol + diff + 1;
                currentBalCol = newBalCol;
                string newEndColLetter = ToColumnLetter(newBalCol - 1);

                for (int row = 1; row <= totalRows; row++)
                {
                    string formula = ws.Cells[row, newBalCol].Formula ?? "";
                    if (formula.IndexOf("SUM", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        ws.Cells[row, newBalCol].Formula = Regex.Replace(formula,
                            @"([A-Za-z]+)(\d+):([A-Za-z]+)(\d+)",
                            m => $"{m.Groups[1].Value}{m.Groups[2].Value}:{newEndColLetter}{m.Groups[4].Value}");
                    }
                }
            }

            // 計算式セルの値を確定（Bal列等のSUM式をValueとして読み取れるようにする）
            ws.Calculate();

            // RA_Input 2行目からのデータを lastFilledColRow4+1 列から繰り返し書き込む
            int raLastRow = ws.Dimension?.End.Row ?? 0;
            double row5Counter = Convert.ToDouble(lastFilledValueRow5 ?? 0);

            for (int raRow = 2; raRow <= ciLastRow; raRow++)
            {
                int col = lastFilledColRow4 + (raRow - 1); // 1件目 → lastFilledColRow4+1
                 
                // 3行目: RA_Input D列
                ws.Cells[3, col].Value = raInputSheet.Cells[raRow, 4].Value;

                // 4行目: システム日付
                ws.Cells[4, col].Value = DateTime.Now.Date;

                // 5行目: B列が CI の場合のみ row5Counter を +1 して書き込む
                string bValue = raInputSheet.Cells[raRow, 2].Text ?? "";
                if (bValue.Equals("CI", StringComparison.OrdinalIgnoreCase))
                {
                    row5Counter += 1;
                    ws.Cells[5, col].Value = row5Counter;
                }

                // 6行目: RA_Input C列
                ws.Cells[6, col].Value = raInputSheet.Cells[raRow, 3].Value;

                // 7行目: RA_Input E列（折り返し全体を表示）
                ws.Cells[7, col].Value = raInputSheet.Cells[raRow, 5].Value;
                ws.Cells[7, col].Style.WrapText = true;

                // 8行目以降: RA_Input H列とReceiving_Active I列が一致し、Bal列が0以外の行に割り当て
                string hMatchKey = raInputSheet.Cells[raRow, 8].Text ?? "";
                double hRemaining = Convert.ToDouble(raInputSheet.Cells[raRow, 9].Value ?? 0);

                for (int row = 8; row <= raLastRow; row++)
                {
                    // I列(9)とRA_Input H列が一致するか確認
                    if (!(ws.Cells[row, 9].Text ?? "").Equals(hMatchKey, StringComparison.OrdinalIgnoreCase))
                        continue;

                    // Bal列が0の行はスキップ
                    double bal = Convert.ToDouble(ws.Cells[row, currentBalCol].Value ?? 0);
                    if (bal == 0) continue;

                    if (hRemaining <= bal)
                    {
                        // H列 <= Bal列: H残分をセットして次のRA_Input行へ
                        ws.Cells[row, col].Value = hRemaining;
                        ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        break;
                    }
                    else
                    {
                        // H列 > Bal列: Bal値をセットしてH残分を次行へ繰り越し
                        ws.Cells[row, col].Value = bal;
                        ws.Cells[row, col].Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        ws.Cells[row, col].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.Yellow);
                        hRemaining -= bal;
                    }
                }
            }

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
