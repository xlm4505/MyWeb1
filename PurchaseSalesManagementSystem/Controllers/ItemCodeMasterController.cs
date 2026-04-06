using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using PurchaseSalesManagementSystem.Repository;

public class ItemCodeMasterController : Controller
{

    private readonly Repository_ItemCodeMaster _repo;
    public ItemCodeMasterController(Repository_ItemCodeMaster repo)
    {
        _repo = repo;
    }


    public IActionResult ItemCodeMaster()
    {
        return View();
    }


    [HttpGet]
    public IActionResult GetItemCodeMaster(String itemNo, bool excludeInactive)
    {
        var orderData = _repo.GetItemCodeMaster(itemNo, excludeInactive);
        return Json(orderData);
    }

    [HttpGet]
    public IActionResult ExportToExcel(string itemNo,bool excludeInactive)
    {
        var items = _repo.GetItemCodeMaster(itemNo, excludeInactive).ToList();

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("ItemCode");

        int row = 1;

        // 色定義
        var headerColor = XLColor.FromArgb(255, 192, 0); // 1行目・2行目
        var evenRowColor = XLColor.FromArgb(255, 230, 153);  // 偶数データ行

        // ============================
        // 1. 結合ヘッダー行（1行目）
        // ============================
        ws.Cell(row, 2).Value = "Product Information";
        ws.Range(row, 2, row, 11).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(row, 12).Value = "Unit Price / Cost";
        ws.Range(row, 12, row, 17).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(row, 18).Value = "Inventory (Regular Items)";
        ws.Range(row, 18, row, 22).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(row, 23).Value = "Inventory (Excluded Items)";
        ws.Range(row, 23, row, 27).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(row, 28).Value = "Last Transaction Date";
        ws.Range(row, 28, row, 29).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(row, 31).Value = "Database Access Information";
        ws.Range(row, 31, row, 34).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        ws.Cell(row, 35).Value = "Master Price List";
        ws.Range(row, 35, row, 41).Merge().Style.Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // ★ 1行目の背景色
        ws.Range(1, 1, 1, 41).Style.Fill.BackgroundColor = headerColor;

        // ★ 1行目の文字色（白）
        ws.Range(1, 1, 1, 41).Style.Font.FontColor = XLColor.White;

        // ============================
        // 2. 通常ヘッダー行（2行目）
        // ============================
        row = 2;
        int col = 1;

        var headers = new[]
        {
        "ItemCode", "ItemDesc", "ItemDesc2", "Category", "ProductLineDesc", "ProductType",
        "Inactive", "Weight(lb)", "Whse", "PrimaryVendor", "QtyDisc", "StdSalesPrice", "StdUnitCost",
        "LastCost", "AvgCost", "VenCost(USD)", "VenCost(JPY)", "OnHand", "OpenSO",
        "Available", "OpenPO", "(InShip)", "OnHand ", "OpenSO ", "Available ", "OpenPO ", "(InShip) ",
        "LastSold", "LastReceipt", "ExtendedDescriptionText", "DateCreated", "UserCreated",
        "DateUpdated", "UserUpdated", "List COP", "Standard", "Discount",
        "Class 4", "Class 5", "Contract", "Class 6"
    };

        foreach (var h in headers)
        {
            ws.Cell(row, col).Value = h;
            ws.Cell(row, col).Style.Font.Bold = true;
            col++;
        }

        // ★ 2行目の背景色
        ws.Range(2, 1, 2, headers.Length).Style.Fill.BackgroundColor = headerColor;

        // ★ 2行目の文字色（白）
        ws.Range(2, 1, 2, headers.Length).Style.Font.FontColor = XLColor.White;

        // ============================
        // 3. データ行（3行目～）
        // ============================
        row = 3;

        foreach (var v in items)
        {
            col = 1;

            ws.Cell(row, col++).Value = v.ItemCode;
            ws.Cell(row, col++).Value = v.ItemDesc;
            ws.Cell(row, col++).Value = v.ItemDesc2;
            ws.Cell(row, col++).Value = v.Category;
            ws.Cell(row, col++).Value = v.ProductLineDesc;
            ws.Cell(row, col++).Value = v.ProductType;
            ws.Cell(row, col++).Value = v.Inactive;
            ws.Cell(row, col++).Value = v.Weight;
            ws.Cell(row, col++).Value = v.Whse;
            ws.Cell(row, col++).Value = v.PrimaryVendor;
            ws.Cell(row, col++).Value = v.QtyDisc;
            ws.Cell(row, col++).Value = v.StdSalesPrice;
            ws.Cell(row, col++).Value = v.StdUnitCost;
            ws.Cell(row, col++).Value = v.LastCost;
            ws.Cell(row, col++).Value = v.AvgCost;
            ws.Cell(row, col++).Value = v.VenCost_USD_;
            ws.Cell(row, col++).Value = v.VenCost_JPY;
            ws.Cell(row, col++).Value = v.OnHand;
            ws.Cell(row, col++).Value = v.OpenSO;
            ws.Cell(row, col++).Value = v.Available;
            ws.Cell(row, col++).Value = v.OpenPO;
            ws.Cell(row, col++).Value = v.InShip;
            ws.Cell(row, col++).Value = v.OnHand_;
            ws.Cell(row, col++).Value = v.OpenSO_;
            ws.Cell(row, col++).Value = v.Available_;
            ws.Cell(row, col++).Value = v.OpenPO_;
            ws.Cell(row, col++).Value = v.InShip_;

            ws.Cell(row, col++).Value = v.LastSold?.ToString("yyyy/M/d");
            ws.Cell(row, col++).Value = v.LastReceipt?.ToString("yyyy/M/d");

            ws.Cell(row, col++).Value = v.ExtendedDescriptionText;
            ws.Cell(row, col++).Value = v.DateCreated?.ToString("yyyy/M/d");
            ws.Cell(row, col++).Value = v.UserCreated;
            ws.Cell(row, col++).Value = v.DateUpdated?.ToString("yyyy/M/d");
            ws.Cell(row, col++).Value = v.UserUpdated;

            // ★ 0の場合空欄にする
            ws.Cell(row, col++).Value = v.ListCOP;
            ws.Cell(row, col++).Value = v.Standard;
            ws.Cell(row, col++).Value = v.Discount;
            ws.Cell(row, col++).Value = v.Class4;
            ws.Cell(row, col++).Value = v.Class5;
            ws.Cell(row, col++).Value = v.Contract;
            ws.Cell(row, col++).Value = v.Class6;

            // ★ 偶数行だけ背景色
            if ((row % 2) == 0)
            {
                ws.Range(row, 1, row, headers.Length).Style.Fill.BackgroundColor = evenRowColor;
            }

            row++;
        }

        int lastRow = row - 1;

        // ============================
        // 4. 列幅自動調整
        // ============================
        ws.Columns().AdjustToContents();

        // B列だけ 35.10
        ws.Column(2).Width = 35.10;

        // その他の列を 15.40 に設定
        for (int c = 1; c <= 41; c++)
        {
            if (c == 2) continue;
            ws.Column(c).Width = 15.40;
        }

        // 0 を非表示にする
        for (int c = 35; c <= 41; c++)
        {
            ws.Column(c).Style.NumberFormat.Format = "0;-0;;@";
        }

        // ============================
        // 5. ItemCode 列を固定
        // ============================
        ws.SheetView.FreezeRows(2);
        ws.SheetView.FreezeColumns(1);

        // ============================
        // 6. フィルター追加
        // ============================
        ws.Range(2, 1, 2, headers.Length).SetAutoFilter();

        // ============================
        // 7. 指定列の右側に罫線
        // ============================
        int[] borderCols = { 1, 11, 17, 22, 27, 29, 30, 34 };

        foreach (var c in borderCols)
        {
            ws.Range(1, c, lastRow, c)
                .Style.Border.RightBorder = XLBorderStyleValues.Thin;
        }

        // ============================
        // 8. Excel 出力
        // ============================
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        var excelBytes = stream.ToArray();

        return File(
            excelBytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"ItemMaster_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx"
        );
    }

}
