using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Repository;
using System.Globalization;

public class NittsuInventoryCheckController : Controller
{
    private static readonly HashSet<string> AllowedWhse = new(StringComparer.OrdinalIgnoreCase)
    {
        "AKR","BSR","FCA","FIA","FIS","FLC","FLJ","FLT","HCA","IBA","ITX","JIT",
        "LGR","MSN","RPC","SCP","STR","STX","TNJ","TTX","UCA","UOR","XUS"
    };

    private readonly Repository_PurchaseOrder _repo;
    public NittsuInventoryCheckController(Repository_PurchaseOrder repo)
    {
        _repo = repo;
    }

    public IActionResult NittsuInventoryCheck()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Compare(IFormFile masFile, IFormFile nittsuFile)
    {
        if (masFile == null || nittsuFile == null)
        {
            return BadRequest(new { error_msg = "Please select both Mas and Nittsu inventory data files." });
        }

        var masRows = new Dictionary<(string ItemCode, string Whse), decimal>();
        var nittsuRows = new Dictionary<(string ItemCode, string Whse), decimal>();

        using (var masStream = new MemoryStream())
        {
            await masFile.CopyToAsync(masStream);
            masStream.Position = 0;
            using var masWb = new XLWorkbook(masStream);
            var masSheet = masWb.Worksheet(1);

            var masHeader = BuildHeaderIndex(masSheet.Row(1));
            var masItemCol = GetColumnIndex(masHeader, "ItemCode");
            var masWhseCol = GetColumnIndex(masHeader, "WHSE");
            var masQtyCol = GetColumnIndex(masHeader, "OnHand");

            foreach (var row in masSheet.RowsUsed().Skip(1))
            {
                var itemCode = NormalizeItemCode(row.Cell(masItemCol).GetString());
                var whse = row.Cell(masWhseCol).GetString().Trim().ToUpperInvariant();
                var qty = ToDecimal(row.Cell(masQtyCol).Value);

                if (string.IsNullOrWhiteSpace(itemCode) || string.IsNullOrWhiteSpace(whse) || qty == 0 || !AllowedWhse.Contains(whse))
                {
                    continue;
                }

                AddQuantity(masRows, (itemCode, whse), qty);
            }
        }

        using (var nittsuStream = new MemoryStream())
        {
            await nittsuFile.CopyToAsync(nittsuStream);
            nittsuStream.Position = 0;
            using var nittsuWb = new XLWorkbook(nittsuStream);

            for (var sheetIndex = 1; sheetIndex <= Math.Min(2, nittsuWb.Worksheets.Count); sheetIndex++)
            {
                var sheet = nittsuWb.Worksheet(sheetIndex);
                var header = BuildHeaderIndex(sheet.Row(1));
                var itemCol = GetColumnIndex(header, "Item");
                var sfxCol = GetColumnIndex(header, "Sfx");
                var qtyCol = GetColumnIndex(header, "Quantity");

                foreach (var row in sheet.RowsUsed().Skip(1))
                {
                    var itemCode = NormalizeItemCode(row.Cell(itemCol).GetString());
                    var whseRaw = row.Cell(sfxCol).GetString().Trim();
                    var whse = (whseRaw.Length >= 3 ? whseRaw[..3] : whseRaw).ToUpperInvariant();
                    var qty = ToDecimal(row.Cell(qtyCol).Value);

                    if (string.IsNullOrWhiteSpace(itemCode) || string.IsNullOrWhiteSpace(whse) || qty == 0 || !AllowedWhse.Contains(whse))
                    {
                        continue;
                    }

                    AddQuantity(nittsuRows, (itemCode, whse), qty);
                }
            }
        }

        var allKeys = masRows.Keys.Union(nittsuRows.Keys)
            .OrderBy(k => k.ItemCode)
            .ThenBy(k => k.Whse);

        using var outWb = new XLWorkbook();
        var ws = outWb.Worksheets.Add("Compare");

        ws.Cell(1, 1).Value = "Item Code";
        ws.Cell(1, 2).Value = "Whse";
        ws.Cell(1, 3).Value = "Qty Mas200";
        ws.Cell(1, 4).Value = "Qty Nittsu";
        ws.Cell(1, 5).Value = "Difference";

        var rowNo = 2;
        foreach (var key in allKeys)
        {
            var masQty = masRows.GetValueOrDefault(key, 0);
            var nittsuQty = nittsuRows.GetValueOrDefault(key, 0);
            var diff = masQty - nittsuQty;
            if (diff == 0)
            {
                continue;
            }
            var cellItem = ws.Cell(rowNo, 1).SetValue(key.ItemCode);
            cellItem.Style.NumberFormat.Format = "@";

            var cellWhse = ws.Cell(rowNo, 2).SetValue(key.Whse);
            cellWhse.Style.NumberFormat.Format = "@";

            ws.Cell(rowNo, 3).Value = masQty;
            ws.Cell(rowNo, 4).Value = nittsuQty;
            ws.Cell(rowNo, 5).Value = diff;
            rowNo++;
        }

        ws.Range("A:E").Style.Font.FontName = "Calibri";
        ws.Range("A:E").Style.Font.FontSize = 10;
        ws.Range("A1:E1").Style.Font.Bold = true;
        ws.Range("A1:E1").Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Columns("A:E").AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using var output = new MemoryStream();
        outWb.SaveAs(output);

        return File(output.ToArray(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "Compare.xlsx");
    }

    private static Dictionary<string, int> BuildHeaderIndex(IXLRow headerRow)
    {
        return headerRow.CellsUsed()
            .ToDictionary(c => c.GetString().Trim(), c => c.Address.ColumnNumber, StringComparer.OrdinalIgnoreCase);
    }

    private static int GetColumnIndex(Dictionary<string, int> header, string columnName)
    {
        if (!header.TryGetValue(columnName, out var index))
        {
            throw new InvalidOperationException($"Required column '{columnName}' was not found.");
        }

        return index;
    }

    private static string NormalizeItemCode(string? input)
    {
        var trimmed = (input ?? string.Empty).Replace(" ", string.Empty).Trim();
        return string.IsNullOrEmpty(trimmed)
            ? string.Empty
            : trimmed.Length >= 6 ? trimmed[^6..] : trimmed.PadLeft(6, '0');
    }

    private static decimal ToDecimal(XLCellValue value)
    {
        if (value.TryConvert(out double dbl, CultureInfo.InvariantCulture))
        {
            return Convert.ToDecimal(dbl);
        }

        if (value.TryGetText(out var txt) && decimal.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out var dec))
        {
            return dec;
        }

        return 0;
    }

    private static void AddQuantity(Dictionary<(string ItemCode, string Whse), decimal> map, (string ItemCode, string Whse) key, decimal qty)
    {
        if (map.TryGetValue(key, out var current))
        {
            map[key] = current + qty;
            return;
        }

        map[key] = qty;
    }
}