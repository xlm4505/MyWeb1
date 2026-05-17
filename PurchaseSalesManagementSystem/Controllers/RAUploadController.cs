using System.Data;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class RAUploadController : Controller
{

    private readonly Repository_RAUpload _repo;
	private readonly List<string> _warehouses = new() { "NAL", "NCA", "NTX", "UTX", "UGP", "IFS", "NNJ", "XIT", "JFI" };

	public RAUploadController(Repository_RAUpload repo)
    {
        _repo = repo;
    }


    public IActionResult RAUpload()
    {
        return View();
    }

	[HttpPost]
	public async Task<IActionResult> UploadExcel(IFormFile excelFile)
	{
		try
		{
			// U_RAInventoryデータ削除する
			_repo.DeleteRAInventory();

			var rows = new List<RAUpload_Insert>();
			var errors = new List<string>();

			using (var stream = new MemoryStream())
			{
				await excelFile.CopyToAsync(stream);
				stream.Position = 0;

				using (var workbook = new XLWorkbook(stream))
				{
					foreach (var ws in workbook.Worksheets)
					{
						// 指定のシートのみ処理
						if (!_warehouses.Contains(ws.Name)) continue;
						// セルA1の値をチェック
						if (ws.Cell("A1").GetString() != "RECEIVING NOTE") continue;
						int lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;
						// 最終行をチェック
						if (lastRow <= 5) continue;
						// "Bal" 列検索（4行目 K列以降）
						var headerRow = ws.Row(4);
						int balColumn = -1;
						int lastColumn = ws.LastColumnUsed().ColumnNumber();

						for (int col = 11; col <= lastColumn; col++)
						{
							if (headerRow.Cell(col).GetString().Contains("Bal"))
							{
								balColumn = col;
								break;
							}
						}
						if (balColumn == -1) continue;

						for (int r = 5; r <= lastRow; r++)
						{
							var itemCode = ws.Cell(r, 9).GetString();
							// Bal列
							var qtyCell = ws.Cell(r, balColumn);

							// 0チェック
							if (!qtyCell.TryGetValue(out decimal qty)) continue;
							if (qty == 0) continue;

							// PO空白チェック
							var poNo = ws.Cell(r, 1).GetString();
							if (poNo == string.Empty) continue;

							// G/H/I 列の空白チェック
							var emptyColumns = new List<string>();
							if (string.IsNullOrWhiteSpace(ws.Cell(r, 7).GetString())) emptyColumns.Add("DESCRIPTION OF GOODS");
							if (string.IsNullOrWhiteSpace(ws.Cell(r, 8).GetString())) emptyColumns.Add("UNIT PRICE");
							if (string.IsNullOrWhiteSpace(itemCode)) emptyColumns.Add("Item#");
							if (emptyColumns.Count > 0)
							{
								errors.Add($"Sheet: {ws.Name}, Row: {r} - Column {string.Join(", ", emptyColumns)} is empty");
							}

							if (errors.Count > 0)
							{
								continue;
							}

							rows.Add(new RAUpload_Insert
							{
								EntryDate = DateTime.Now,
								WarehouseCode = ws.Name,
								ItemCode = itemCode,
								Description = ws.Cell(r, 7).GetString(),
								OriginalQty = ws.Cell(r, 10).GetValue<decimal>(),
								Qty = qtyCell.GetValue<decimal>(),
								InvoiceNo = ws.Cell(r, 1).GetString(),
								Box = ws.Cell(r, 2).GetString(),
								Weight = decimal.Zero,
								DateReceived = ws.Cell(r, 4).GetValue<DateTime>(),
								From = ws.Cell(r, 5).GetString(),
								VantecRef = ws.Cell(r, 6).GetString(),
								UnitPrice = ws.Cell(r, 8).GetValue<decimal>(),
								ShipMark = ws.Cell(r, 3).GetString(),
								Comment = "",
							});
						}
					}
				}
			}

			if (errors.Count > 0)
			{
				return Json(new { error_msg = string.Join("\n", errors) });
			}

			if (rows.Count > 0)
			{
				_repo.InsertRAInventoryBulk(rows);
			}

			// ダウンロードデータを抽出する
			var downloadData = _repo.GetDownloadData().ToList();

			// 合計行を追加
			downloadData.Add(new RAUpload_ExportToExcel
			{
				ItemCode = "",
				ItemDesc = "",
				JFI = downloadData.Sum(x => x.JFI),
				NAL = downloadData.Sum(x => x.NAL),
				NCA = downloadData.Sum(x => x.NCA),
				NTX = downloadData.Sum(x => x.NTX),
				UTX = downloadData.Sum(x => x.UTX),
				UGP = downloadData.Sum(x => x.UGP),
				IFS = downloadData.Sum(x => x.IFS),
				NNJ = downloadData.Sum(x => x.NNJ),
				XIT = downloadData.Sum(x => x.XIT),
				Total = downloadData.Sum(x => x.Total),
			});

			FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
			DataTable dt = exportToExcel.ConvertToDataTableFast(downloadData);

			var excelBytes = exportToExcel.ExportDataTableWithFormatting(dt, "StockList", "SO");

			return File(excelBytes,
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
				$"RA_StockList_{DateTime.Now:MMddyyyy}.xlsx");

		}
		catch (Exception ex)
		{
			return Json(new { error_msg = $"server error: {ex.Message}" });
		}
	}

}
