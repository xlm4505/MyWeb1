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
			// U_RAInventoryデータ削除処理
			_repo.DeleteRAInventory();

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
						// "Bal" 列検索（4行目 K～ZZ）
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
							// 空白チェック
							if (string.IsNullOrWhiteSpace(itemCode)) continue;
							// 0チェック
							if (!qtyCell.TryGetValue(out decimal qty)) continue;
							if (qty == 0) continue;

							var rAUpload_insert =  new RAUpload_Insert
							{
								EntryDate = DateTime.Now,
								WarehouseCode = ws.Name,
								ItemCode = itemCode,
								Description = ws.Cell(r, 7).GetString(),
								OriginalQty = ws.Cell(r, 10).GetValue<decimal>(),
								Qty = ws.Cell(r, balColumn).GetValue<decimal>(),
								InvoiceNo = ws.Cell(r, 1).GetString(),
								Box = ws.Cell(r, 2).GetString(),
								Weight = decimal.Zero,
								DateReceived = ws.Cell(r, 4).GetValue<DateTime>(),
								From = ws.Cell(r, 5).GetString(),
								VantecRef =	ws.Cell(r, 6).GetString(),
								UnitPrice = ws.Cell(r, 8).GetValue<decimal>(),
								ShipMark = ws.Cell(r, 3).GetString(),
								Comment = "",
							};

							_repo.InsertRAInventory(rAUpload_insert);
						}
					}
				}
			}

			// ダウンロードデータを抽出する
			var downloadData = _repo.GetDownloadData();

			// JFIの合計
			var jfiSum = downloadData.Sum(x => x.JFI);
			// NALの合計
			var nalsum = downloadData.Sum(x => x.NAL);
			// NCAの合計
			var ncaSum = downloadData.Sum(x => x.NCA);
			// NTXの合計
			var ntxSum = downloadData.Sum(x => x.NTX);
			// UTXの合計
			var utxSum = downloadData.Sum(x => x.UTX);
			// UGPの合計
			var ugpSum = downloadData.Sum(x => x.UGP);
			// IFSの合計
			var ifsSum = downloadData.Sum(x => x.IFS);
			// NNJの合計
			var nnjSum = downloadData.Sum(x => x.NNJ);
			// XITの合計
			var xitSum = downloadData.Sum(x => x.XIT);
			// Totalの合計
			var totalSum = downloadData.Sum(x => x.Total);

			var list = downloadData.ToList();

			list.Add(new RAUpload_ExportToExcel
			{
				ItemCode = "",
				ItemDesc = "",
				JFI = jfiSum,
				NAL = nalsum,
				NCA = ncaSum,
				NTX = ntxSum,
				UTX = utxSum,
				UGP = ugpSum,
				IFS = ifsSum,
				NNJ = nnjSum,
				XIT = xitSum,
				Total = totalSum,
			});

			IEnumerable<RAUpload_ExportToExcel> result = list;

			FormattedDataTableExcelExporter exportToExcel = new FormattedDataTableExcelExporter();
			DataTable dt = new DataTable();

			dt = exportToExcel.ConvertToDataTableFast(result);

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
