using ClosedXML.Excel;
using DocumentFormat.OpenXml.Vml;
using Microsoft.AspNetCore.Mvc;
using PurchaseSalesManagementSystem.Repository;
using System.Globalization;
using System.Text;

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
            using var reader = new StreamReader(nittsuStream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);

            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrWhiteSpace(headerLine))
            {
                return BadRequest(new { error_msg = "Nittsu inventory data CSV is empty." });
            }

            var headerColumns = SplitCsvLine(headerLine);
            var header = BuildHeaderIndex(headerColumns);
            var productCol = GetColumnIndex(header, "Product");
            var qtyCol = GetColumnIndex(header, "Available QTY");

            string? line;
            while ((line = await reader.ReadLineAsync()) != null)
            {
                if (string.IsNullOrWhiteSpace(line))
                    {
                    continue;
                }

                var columns = SplitCsvLine(line);
                var productRaw = GetCsvColumn(columns, productCol);
                var itemRaw = productRaw;
                var whseRaw = productRaw;
                var separatorIndex = productRaw.IndexOf('-');
                if (separatorIndex >= 0)
                {
                    itemRaw = productRaw[..separatorIndex];
                    whseRaw = separatorIndex + 1 < productRaw.Length
                        ? productRaw[(separatorIndex + 1)..]
                        : string.Empty;
                }
                var qtyRaw = GetCsvColumn(columns, qtyCol);

                var itemCode = NormalizeItemCode(itemRaw);
                var whse = (whseRaw.Length >= 3 ? whseRaw[..3] : whseRaw).ToUpperInvariant();
                var qty = ToDecimal(qtyRaw);

                if (string.IsNullOrWhiteSpace(itemCode) || string.IsNullOrWhiteSpace(whse) || qty == 0 || !AllowedWhse.Contains(whse))
                {
                    continue;
                }
                AddQuantity(nittsuRows, (itemCode, whse), qty);
            }
        }

        var allKeys = masRows.Keys.Union(nittsuRows.Keys)
            .OrderBy(k => k.ItemCode)
            .ThenBy(k => k.Whse);

        var outWb = new XLWorkbook();
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

        var lastDataRow = Math.Max(1, rowNo - 1);
        var usedRange = ws.Range(1, 1, lastDataRow, 5);
        var headerRange = ws.Range(1, 1, 1, 5);

        usedRange.Style.Font.FontName = "Calibri";
        usedRange.Style.Font.FontSize = 10;
        headerRange.Style.Font.Bold = true;
        headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        ws.Columns(1, 5).AdjustToContents();
        ws.SheetView.FreezeRows(1);

        using (var stream = new MemoryStream())
        {
            outWb.SaveAs(stream);
            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "Compare_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
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

        if (value.TryGetText(out var txt))
        {
            return ToDecimal(txt);
        }

        return 0;
    }
    private static decimal ToDecimal(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return 0;
        }

        return decimal.TryParse(text.Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out var dec)
            ? dec
            : 0;
    }


    private static Dictionary<string, int> BuildHeaderIndex(IReadOnlyList<string> headerColumns)
    {
        var header = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var index = 0; index < headerColumns.Count; index++)
        {
            var name = headerColumns[index].Trim();
            if (!string.IsNullOrEmpty(name) && !header.ContainsKey(name))
            {
                header[name] = index;
            }
        }

        return header;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var columns = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else
                {
                    inQuotes = !inQuotes;
                }

                continue;
            }

            if (ch == ',' && !inQuotes)
            {
                columns.Add(sb.ToString());
                sb.Clear();
                continue;
            }

            sb.Append(ch);
        }

        columns.Add(sb.ToString());
        return columns;
    }

    private static string GetCsvColumn(IReadOnlyList<string> columns, int index)
    {
        if (index < 0 || index >= columns.Count)
        {
            return string.Empty;
        }

        return columns[index];
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