using System.Data;
using System.IO.Compression;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class CISummaryController : Controller
{

    private readonly Repository_CISummary _repo;
	private int excel_num = 0;
    private readonly IWebHostEnvironment _env;
    public CISummaryController(Repository_CISummary repo , IWebHostEnvironment env)
    {
        _repo = repo;
        _env = env;
    }

    public IActionResult CISummary()
    {
        return View();
    }

	[HttpPost]
	public async Task<IActionResult> UploadFiles(List<IFormFile> pdfFiles, List<IFormFile> excelFiles)
	{
		var workDir = Path.Combine(
            _env.ContentRootPath,
            "AppDataWork",
            "CISummary",
            Guid.NewGuid().ToString());
		string? zipPath = null;
		try
		{
            var fileExcelName = "";
            var originalExcelPath = "";

            if (!Directory.Exists(workDir))
                Directory.CreateDirectory(workDir);

            var deleteFiles = new List<string>();

            // delete from U_CIDetailData
            _repo.DeleteUCIDetailData();

			foreach (var excelFile in excelFiles.OrderBy(f => f.FileName))
			{
                var newExcelFileName = "";
                fileExcelName = excelFile.FileName;
                originalExcelPath = Path.Combine(workDir, fileExcelName);

                using (var stream = new FileStream(originalExcelPath, FileMode.Create))
                {
                    await excelFile.CopyToAsync(stream);
                }

                // .xls → .xlsx NPOI 変換（データ読み取り用一時ファイル）
                var workExcelPath = originalExcelPath;
                string? tempConvertedPath = null;
                if (Path.GetExtension(fileExcelName).Equals(".xls", StringComparison.OrdinalIgnoreCase))
                {
                    workExcelPath = ConvertXlsToXlsx(originalExcelPath);
                    tempConvertedPath = workExcelPath;
                    // originalExcelPath は削除しない（ZIP 出力用に保持）
                }

                using (var stream = new FileStream(workExcelPath, FileMode.Open, FileAccess.Read))
                using (var workbook = new XLWorkbook(stream))
                {
                    foreach (var ws in workbook.Worksheets)
                        TryProcessWorksheet(ws, fileExcelName, ref newExcelFileName, deleteFiles, originalExcelPath);
                }

                // 一時変換ファイルを削除
                if (tempConvertedPath != null)
                    System.IO.File.Delete(tempConvertedPath);

                if(newExcelFileName != "")
                {
                    // .xls の場合は ZIP 内のファイル名も .xls に変更
                    if (Path.GetExtension(fileExcelName).Equals(".xls", StringComparison.OrdinalIgnoreCase))
                        newExcelFileName = Path.ChangeExtension(newExcelFileName, ".xls");

                    var renamedExcelPath = Path.Combine(workDir, newExcelFileName);
                    System.IO.File.Move(originalExcelPath, renamedExcelPath);

                    string excelBaseName = Path.GetFileNameWithoutExtension(fileExcelName);
                    string newBaseName = Path.GetFileNameWithoutExtension(newExcelFileName);

                    foreach (var pdfFile in pdfFiles)
                    {
                        string pdfBaseName = Path.GetFileNameWithoutExtension(pdfFile.FileName);
                        if (pdfBaseName == excelBaseName)
                        {
                            string newPdfFileName = newBaseName + ".pdf";
                            var pdfSavePath = Path.Combine(workDir, newPdfFileName);
                            using (var pdfStream = new FileStream(pdfSavePath, FileMode.Create))
                            {
                                await pdfFile.CopyToAsync(pdfStream);
                            }
                        }
                    }
                }
			}

            foreach (var deleteFile in deleteFiles)
            {
                System.IO.File.Delete(deleteFile);
            }

            FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
            var outputWorkbook = new XLWorkbook();
            DataTable dt = new DataTable();

            var ciDetailList = _repo.GetCIDetailData();
            dt = exportToExcel.ConvertToDataTableFast(ciDetailList);
            outputWorkbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(outputWorkbook, dt, "Detail", "SO");
            var detailtWorksheet = outputWorkbook.Worksheet("Detail");
            detailtWorksheet.Column(24).Style.NumberFormat.Format = "#,##0.00";
            detailtWorksheet.Column(25).Style.NumberFormat.Format = "#,##0.00";
            detailtWorksheet.Cell(1, 6).Value = "FOA_CI#";
            detailtWorksheet.Cell(1, 14).Value = "Whse (Out)";
            detailtWorksheet.Cell(1, 15).Value = "Whse (In)";

            var raInputList = _repo.GetRAInputData();
            dt = exportToExcel.ConvertToDataTableFast(raInputList);
            outputWorkbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(outputWorkbook, dt, "RA_Input", "SO");
			var raInputWorksheet = outputWorkbook.Worksheet("RA_Input");
			raInputWorksheet.Cell(1, 5).Value = "ShipTo - ShipVia";
			raInputWorksheet.Cell(1, 6).Value = "Whse (Out)";
			raInputWorksheet.Cell(1, 7).Value = "Whse (In)";

			var stockCheckList = _repo.GetStockCheckData();
            dt = exportToExcel.ConvertToDataTableFast(stockCheckList);
            outputWorkbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(outputWorkbook, dt, "StockCheck", "SO");
			var stockCheckWorksheet = outputWorkbook.Worksheet("StockCheck");
			stockCheckWorksheet.Cell(1, 4).Value = "Qty (Current)";
			stockCheckWorksheet.Cell(1, 6).Value = "IT (Out)";
			stockCheckWorksheet.Cell(1, 7).Value = "IT (In)";
			stockCheckWorksheet.Cell(1, 8).Value = "Qty (After)";

            var stockLastRow = stockCheckWorksheet.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= stockLastRow; row++)
            {
                var qtyAfterCell = stockCheckWorksheet.Cell(row, 8);
                if (qtyAfterCell.TryGetValue(out decimal qtyVal) && qtyVal < 0)
                    qtyAfterCell.Style.Font.FontColor = XLColor.Red;
            }

			var rinkuList = _repo.GetRINKUData();
            dt = exportToExcel.ConvertToDataTableFast(rinkuList);
            outputWorkbook = exportToExcel.ExportDataTableWithFormattingForWorkbook(outputWorkbook, dt, "RINKU", "SO");
            var rinkuWorksheet = outputWorkbook.Worksheet("RINKU");
            rinkuWorksheet.Column(14).Style.NumberFormat.Format = "#,##0.00";
			rinkuWorksheet.Cell(1, 11).Value = "Requested By";

			var rinkuLastRow = rinkuWorksheet.LastRowUsed()?.RowNumber() ?? 1;
            for (int row = 2; row <= rinkuLastRow; row++)
            {
                var paceCell = rinkuWorksheet.Cell(row, 12);
                if (!string.IsNullOrEmpty(paceCell.GetString()))
                {
                    paceCell.Style.Fill.BackgroundColor = XLColor.Yellow;
                    rinkuWorksheet.Cell(row, 8).Style.Fill.BackgroundColor = XLColor.Yellow;
                }
            }

            var summaryExcelFileName = "CI_Summary_" + DateTime.Now.ToString("MMddyyyy") + ".xlsx";
            var summaryExcelPath = Path.Combine(workDir, summaryExcelFileName);
            outputWorkbook.SaveAs(summaryExcelPath);

            var zipFileName = "CISummary-" + DateTime.Now.ToString("MMddyyyy") + ".zip";
            zipPath = Path.Combine(Path.GetDirectoryName(workDir)!, zipFileName);
            System.IO.Compression.ZipFile.CreateFromDirectory(workDir, zipPath);

            var zipBytes = await System.IO.File.ReadAllBytesAsync(zipPath);
            Directory.Delete(workDir, true);
            System.IO.File.Delete(zipPath);

            return File(zipBytes, "application/zip", zipFileName);

        }
		catch (Exception ex)
		{
			if (Directory.Exists(workDir))
				Directory.Delete(workDir, true);
			if (zipPath != null && System.IO.File.Exists(zipPath))
				System.IO.File.Delete(zipPath);
			var errorMsg = !string.IsNullOrEmpty(ex.Message) ? ex.Message : ex.GetType().Name;
			return new JsonResult(new { error_msg = $"server error: {errorMsg}" }) { StatusCode = 500 };
		}

		bool TryProcessWorksheet(IXLWorksheet ws, string excelName, ref string newFileName, List<string> delFiles, string origPath)
		{
			var a1 = ws.Cell("A1");
			var a2 = ws.Cell("A2");
			var j5 = ws.Cell("J5");
			if (j5.GetString() == "Packing List / Commercial Invoice")
			{
				newFileName = ProcessPackingWorksheet(ws, excelName);
				return true;
			}
			else if (a1.GetString() == "Rinku to Rinku" ||
					 a2.GetString() == "IT from Rinku to SFO" ||
					 a1.GetString() == "IT from RINKU to SFO Consolidation")
			{
				ProcessSfoWorksheet(ws, excelName);
				delFiles.Add(origPath);
				return true;
			}
			return false;
		}
	}

	private string ProcessPackingWorksheet(IXLWorksheet worksheet, string fileName)
	{
		int lastRow = worksheet.Column("B").LastCellUsed()?.Address.RowNumber ?? 5;
		if (lastRow < 5) lastRow = 5;

		var title = worksheet.Cell("J5");

		string meisaiFlg = "";
		string shipTo1 = "";
		string attn = "";
		string shipTo2 = "";
		string shipVia = "";
		string account = "";
		string instrucstions = "";
		string foa_ci = "";
		string requestor = "";
        DateTime shipDate = DateTime.MinValue;
        string foa_so_col = "";
		string qty_col = "";
		string po_col = "";
		string po_ln_col = "";
		string itemCode_col = "";
		string part_col = "";
		string tariff_col = "";
		string unitPrice_col = "";
		string ext_col = "";

		string yymmdd = DateTime.Now.ToString("yyMMdd");
		
		string newFileName = "D" + yymmdd + "-" + (excel_num + 1).ToString("00");
        excel_num += 1;

        for (int j = 5; j <= lastRow; j++)
		{
			var currentRow = worksheet.Row(j);
			var shipToCell = FindCellInRow(currentRow, "Ship To:");
			if (shipToCell != null)
			{
				string colLetter = shipToCell.Address.ColumnLetter;
				var addressCell = worksheet.Cell(j + 1, colLetter);
				string addressText = addressCell.GetString();
				string[] lines = addressText.Split(new[] { '\n' }, StringSplitOptions.None);

				shipTo1 = lines.Length > 0 ? lines[0] : "";

				int nextLineIndex;
				if (lines.Length > 1 && lines[1].StartsWith("Attn: ", StringComparison.OrdinalIgnoreCase))
				{
					attn = lines[1].Substring(6);
					nextLineIndex = 2;
				}
				else
				{
					nextLineIndex = 1;
				}

				if (lines.Length > nextLineIndex)
				{
					shipTo2 = lines[nextLineIndex];
					for (int k = nextLineIndex + 1; k < lines.Length; k++)
					{
						shipTo2 += " " + lines[k];
					}
				}
				meisaiFlg = "";
			}

			// ---- "Ship VIA" のチェック ----
			var shipViaCell = worksheet.Cell(j, "A");
			if (shipViaCell.GetString() == "Ship VIA")
			{
				var shipViaValueCell = worksheet.Cell(j + 1, "A");
				shipVia = shipViaValueCell.GetString();

				var nextLineCell = worksheet.Cell(j + 2, "A");
				string[] lines = nextLineCell.GetString().Split(new[] { '\n' }, StringSplitOptions.None);

				if (lines.Length > 0)
				{
					for (int l = 0; l < lines.Length; l++)
					{
						string line = lines[l];
						int posAcct = line.IndexOf("Acct", StringComparison.OrdinalIgnoreCase) + 1; 
						int posInstr = line.IndexOf("Instructions:", StringComparison.OrdinalIgnoreCase) + 1;

						if (posAcct > 0)
						{
							string temp = line
								.Replace("ACCT", "Acct")
								.Replace("Acct #: ", "Acct#")
								.Replace("Acct #:", "Acct#")
								.Replace("Acct #", "Acct#")
								.Replace("Acct# ", "Acct#")
								.Replace("Acct#:", "Acct#");

							int posAcctSharp = temp.IndexOf("Acct#", StringComparison.OrdinalIgnoreCase) + 1;
							if (posAcctSharp > 0)
							{
								if (posInstr > 0 && posInstr - 8 >= posAcctSharp)
								{
									account = temp.Substring(posAcctSharp + 4, posInstr - posAcctSharp - 8); 
								}
								else
								{
									account = temp.Substring(posAcctSharp + 4, temp.Length - (posAcctSharp + 4));
								}
							}
							else
							{
								account = temp;
							}
						}

						if (posInstr > 0)
						{
							string temp = line.Replace("Instructions: ", "Instructions:");
							instrucstions = temp.Substring(posInstr + 12, temp.Length - (posInstr + 12));
						}

						if (posAcct == 0 && posInstr == 0)
						{
							instrucstions += " " + line;
						}
					}
				}
			}
			
			// "FOA CI #:" 
			var foaCICell = FindCellInRow(currentRow, "FOA CI #:");
			if (foaCICell != null)
			{
				string colLetter = foaCICell.Address.ColumnLetter;
				var ciValueCell = worksheet.Cell(j + 1, colLetter);
				if (!ciValueCell.IsEmpty())
					foa_ci = ciValueCell.GetString();
			}


			// "Requested By:"
			var requestedByCell = FindCellInRow(currentRow, "Requested By:");
			if (requestedByCell != null)
			{
				string colLetter = requestedByCell.Address.ColumnLetter;
				var reqValueCell = worksheet.Cell(j + 1, colLetter);
				requestor = reqValueCell.GetString();
			}

			// "Ship Date:"
			var shipDateCell = FindCellInRow(currentRow, "Ship Date:");
			if (shipDateCell != null)
			{
				string colLetter = shipDateCell.Address.ColumnLetter;
				var dateCell = worksheet.Cell(j + 1, colLetter);
				if (DateTime.TryParse(dateCell.GetString(), out DateTime shipDateFormat))
				{
					shipDate = shipDateFormat;
				}
			}

			// ---- "Whse" ----
			var whseCell = worksheet.Cell(j, "A");

			if (whseCell.GetString() == "Whse")
			{
				meisaiFlg = "1";
				foa_so_col = GetColumnLetter(FindCellInRow(currentRow, "FOA SO#"));
				qty_col = GetColumnLetter(FindCellInRow(currentRow, "Qty"));
				po_col = GetColumnLetter(FindCellInRow(currentRow, "PO #"));
				po_ln_col = GetColumnLetter(FindCellInRow(currentRow, "PO Ln"));
				itemCode_col = GetColumnLetter(FindCellInRow(currentRow, "ItemCode"));
				part_col = GetColumnLetter(FindCellInRow(currentRow, "Fujikin Part # / [Customer Part#]"));
				tariff_col = GetColumnLetter(FindCellInRow(currentRow, "Desc. / [Tariff]"));
				unitPrice_col = GetColumnLetter(FindCellInRow(currentRow, "Unit Price"));
				ext_col = GetColumnLetter(FindCellInRow(currentRow, "Ext."));
			}

			if (meisaiFlg == "1" && IsNumeric(worksheet.Cell(j, qty_col).GetString()) && worksheet.Cell(j, "A").GetString().Length == 3)
			{
				string whse1 = worksheet.Cell(j, "A").GetString();
				string foa_so = worksheet.Cell(j, foa_so_col).GetString();
				decimal qty = string.IsNullOrEmpty(worksheet.Cell(j, qty_col).GetString()) ? 0 : decimal.Parse(worksheet.Cell(j, qty_col).GetString());
				string po = worksheet.Cell(j, po_col).GetString();
				string po_line = worksheet.Cell(j, po_ln_col).GetString();
				string ItemCode = worksheet.Cell(j, itemCode_col).GetString();
				if (double.TryParse(ItemCode, out _) && ItemCode.Length <= 6)
				{
					ItemCode = ItemCode.PadLeft(6, '0');
				}
				string partNo = worksheet.Cell(j, part_col).GetString();
				string customerNo = worksheet.Cell(j+1, part_col).GetString();
				string category = worksheet.Cell(j, tariff_col).GetString();
				string tariff = worksheet.Cell(j+1, tariff_col).GetString();
				decimal unitPrice = string.IsNullOrEmpty(worksheet.Cell(j, unitPrice_col).GetString()) ? 0 : decimal.Parse(worksheet.Cell(j, unitPrice_col).GetString());
				decimal ext = string.IsNullOrEmpty(worksheet.Cell(j, ext_col).GetString()) ? 0 : decimal.Parse(worksheet.Cell(j, ext_col).GetString());

				var uCIDetailData_insert = new Model_CISummary
				{
					DocType = "CI",
					EntryDate = DateTime.Now,
					ShipDate = shipDate,
					NewFileName = newFileName,
					OriginalFileName = fileName,
					FOA_CI = foa_ci,
					ShipTo1 = shipTo1,
					ShipTo2 = shipTo2,
					Attn = attn,
					ShipVia = shipVia,
					Account = account,
					RequestedBy = requestor,
					ItemCode = ItemCode,
					Whse1 = whse1,
					Whse2 = "",
					TranQty = qty,
					SalesOrderNo = foa_so,
					CustomerPONo = po,
					LineNo = po_line,
					FujikinPartNo = partNo,
					CustomerPartNo = customerNo,
					Category = category,
					Tariff = tariff,
					UnitPrice = unitPrice,
					TotalPrice = ext,
					Instrucstions = instrucstions,
				};

				_repo.InsertUCIDetailData(uCIDetailData_insert);
			}
		}

        return newFileName + "_CI.xlsx";
    }

    private void ProcessSfoWorksheet(IXLWorksheet worksheet, string fileName)
    {
        var a1Cell = worksheet.Cell("A1");
        string shipTo1;
        string docType;

        if (a1Cell.GetString() == "Rinku to Rinku")
        {
            shipTo1 = "棚移動";
            docType = "IT2";
        }
        else
        {
            shipTo1 = "SF混載便";
            docType = "IT1";
        }

        int lastRow = worksheet.LastRowUsed().RowNumber();

        for (int m = 2; m <= lastRow; m++)
        {
            var cellA = worksheet.Cell(m, 1);

            // 日付判定
            if (cellA.TryGetValue<DateTime>(out DateTime shipDate))
            {
                decimal qty = decimal.Zero;

                // K(Qty)
                if (decimal.TryParse(worksheet.Cell(m, 11).GetString(), out decimal valQty))
                    qty = valQty;

                // J（ItemCode）
                string itemCode = worksheet.Cell(m, 10).GetString();

                if (int.TryParse(itemCode, out _) && itemCode.Length <= 6)
                    itemCode = itemCode.PadLeft(6, '0');

                var uCIDetailData_insert = new Model_CISummary
                {
                    DocType = docType,
                    EntryDate = DateTime.Now,
                    ShipDate = shipDate,
                    NewFileName = "",
                    OriginalFileName = fileName,
                    FOA_CI = "",
                    ShipTo1 = shipTo1,
                    ShipTo2 = "",
                    Attn = "",
                    ShipVia = "",
                    Account = "",
                    RequestedBy = worksheet.Cell(m, 14).GetString(),
                    ItemCode = itemCode,
                    Whse1 = worksheet.Cell(m, 12).GetString(),
                    Whse2 = worksheet.Cell(m, 13).GetString(),
                    TranQty = qty,
                    SalesOrderNo = "",
                    CustomerPONo = "",
                    LineNo = "",
                    FujikinPartNo = "",
                    CustomerPartNo = "",
                    Category = "",
                    Tariff = "",
                    UnitPrice = 0,
                    TotalPrice = 0,
                    Instrucstions = "",
                };

                _repo.InsertUCIDetailData(uCIDetailData_insert);

            }
        }
    }

    private IXLCell FindCellInRow(IXLRow row, string searchText)
	{
		return row.Cells().FirstOrDefault(c => c.GetString().Equals(searchText, StringComparison.OrdinalIgnoreCase));
	}

	private string GetColumnLetter(IXLCell cell)
	{
		return cell?.Address.ColumnLetter ?? "";
	}

	private object GetCellValue(IXLWorksheet ws, string colLetter, int row)
	{
		if (string.IsNullOrEmpty(colLetter)) return DBNull.Value;
		var cell = ws.Cell(row, colLetter);
		return cell.IsEmpty() ? DBNull.Value : (object)cell.GetString();
	}

	private bool IsNumeric(string s)
	{
		return double.TryParse(s, out _);
	}

	private string ConvertXlsToXlsx(string xlsPath)
	{
		var xlsxPath = Path.ChangeExtension(xlsPath, ".xlsx");
		using var fs = new FileStream(xlsPath, FileMode.Open, FileAccess.Read);
		var hssf = new HSSFWorkbook(fs);
		var xlWorkbook = new XLWorkbook();

		for (int i = 0; i < hssf.NumberOfSheets; i++)
		{
			var srcSheet = hssf.GetSheetAt(i);
			var dstSheet = xlWorkbook.Worksheets.Add(srcSheet.SheetName);

			for (int rowIdx = srcSheet.FirstRowNum; rowIdx <= srcSheet.LastRowNum; rowIdx++)
			{
				var srcRow = srcSheet.GetRow(rowIdx);
				if (srcRow == null) continue;

				for (int colIdx = 0; colIdx < srcRow.LastCellNum; colIdx++)
				{
					var srcCell = srcRow.GetCell(colIdx);
					if (srcCell == null) continue;

					// ClosedXML は 1 始まりインデックス
					var dstCell = dstSheet.Cell(rowIdx + 1, colIdx + 1);

					var cellType = srcCell.CellType == NPOI.SS.UserModel.CellType.Formula
						? srcCell.CachedFormulaResultType
						: srcCell.CellType;

					switch (cellType)
					{
						case NPOI.SS.UserModel.CellType.String:
							dstCell.Value = srcCell.StringCellValue;
							break;
						case NPOI.SS.UserModel.CellType.Numeric:
							if (DateUtil.IsCellDateFormatted(srcCell))
							{
								var dateVal = srcCell.DateCellValue;
								if (dateVal.HasValue)
								{
									dstCell.Value = dateVal.Value;
									dstCell.Style.NumberFormat.Format = "yyyy/mm/dd";
								}
								// null の場合はセル空白のまま（DateTime.MinValue を渡さない）
							}
							else
								dstCell.Value = srcCell.NumericCellValue;
							break;
						case NPOI.SS.UserModel.CellType.Boolean:
							dstCell.Value = srcCell.BooleanCellValue;
							break;
					}
				}
			}
		}

		xlWorkbook.SaveAs(xlsxPath);
		return xlsxPath;
	}

}
