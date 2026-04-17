using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace PurchaseSalesManagementSystem.Common
{
	public class FormattedDataTableExcelExporter
	{
		public byte[] ExportDataTableWithFormatting(DataTable dataTable, String sheetName, String ColorType)
		{
			// 空白Excelを出力するため、不要
			//if (dataTable == null || dataTable.Rows.Count == 0)
			//{
			//	throw new ArgumentException("DataTable is null or empty.");
			//}

			using (var workbook = new XLWorkbook())
			{
				// ワークシートを追加
				var worksheet = workbook.Worksheets.Add(dataTable, sheetName);

				// ヘッダー行のスタイルを設定
				var headerRow = worksheet.Row(1);
				headerRow.Style.Font.Bold = true;
				headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                for (int col = 0; col < dataTable.Columns.Count; col++)
				{

                    if ("SO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                    {
                        headerRow.Cell(col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(112, 173, 71);
                    }
                    else if ("PO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                    {
                        headerRow.Cell(col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 192, 0);
                    }
                }

				// 列のデータ型に基づいた書式設定
				for (int col = 0; col < dataTable.Columns.Count; col++)
				{
					var column = dataTable.Columns[col];
					var xlColumn = worksheet.Column(col + 1);

					if (column.DataType == typeof(DateTime))
					{
						xlColumn.Style.DateFormat.Format = "mm/dd/yyyy";
					}
					else if (column.DataType == typeof(decimal) || column.DataType == typeof(double))
					{
						xlColumn.Style.NumberFormat.Format = "#,##0.00";
					}
					else if (column.DataType == typeof(int))
					{
						xlColumn.Style.NumberFormat.Format = "#,##0";
					}
				}

				// 列幅を自動調整
				worksheet.Columns().AdjustToContents();
				// ヘッダー行を固定
				worksheet.SheetView.FreezeRows(1);
				// 目盛線（グリッド線）を非表示にする
				worksheet.ShowGridLines = false;

				using (var stream = new MemoryStream())
				{
					workbook.SaveAs(stream);
					return stream.ToArray();
				}
			}
		}
        public XLWorkbook ExportDataTableWithFormattingForWorkbook(DataTable dataTable, String sheetName, String ColorType)
        {
            //if (dataTable == null || dataTable.Rows.Count == 0)
            //{
            //    throw new ArgumentException("DataTableはnullまたは空です。");
            //}

            var workbook = new XLWorkbook();

            // ワークシートを追加
            var worksheet = workbook.Worksheets.Add(dataTable, sheetName);

            // ヘッダー行のスタイルを設定
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int col = 0; col < dataTable.Columns.Count; col++)
            {

                if ("SO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                {
                    headerRow.Cell(col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(112, 173, 71);
                }
                else if ("PO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                {
                    headerRow.Cell(col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 192, 0);
                }
            }

            // 列のデータ型に基づいた書式設定
            for (int col = 0; col < dataTable.Columns.Count; col++)
            {
                var column = dataTable.Columns[col];
                var xlColumn = worksheet.Column(col + 1);

                if (column.DataType == typeof(DateTime))
                {
                    xlColumn.Style.DateFormat.Format = "mm/dd/yyyy";
                }
                else if (column.DataType == typeof(decimal) || column.DataType == typeof(double))
                {
                    xlColumn.Style.NumberFormat.Format = "#,##0.00";
                }
                else if (column.DataType == typeof(int))
                {
                    xlColumn.Style.NumberFormat.Format = "#,##0";
                }
            }

            // 列幅を自動調整
            worksheet.Columns().AdjustToContents();
            // ヘッダー行を固定
            worksheet.SheetView.FreezeRows(1);
            // 目盛線（グリッド線）を非表示にする
            worksheet.ShowGridLines = false;

            return workbook;
        }

        public OfficeOpenXml.ExcelWorkbook ExportDataTableWithFormattingForWorkbook(OfficeOpenXml.ExcelWorkbook workbook, DataTable dataTable, String sheetName, String ColorType)
        {
            // ワークシートを追加
            var worksheet = workbook.Worksheets.Add(sheetName);

            // ヘッダー行を書き込む
            for (int col = 0; col < dataTable.Columns.Count; col++)
            {
                worksheet.Cells[1, col + 1].Value = dataTable.Columns[col].ColumnName;
            }

            // データ行を書き込む
            for (int row = 0; row < dataTable.Rows.Count; row++)
            {
                for (int col = 0; col < dataTable.Columns.Count; col++)
                {
                    worksheet.Cells[row + 2, col + 1].Value = dataTable.Rows[row][col];
                }
            }

            // ヘッダー行のスタイルを設定
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int col = 0; col < dataTable.Columns.Count; col++)
            {

                if ("SO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                {
                    headerRow.Cell(col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(112, 173, 71);
                }
                else if ("PO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                {
                    headerRow.Cell(col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 192, 0);
                }
            }

            // データ行のスタイル（行ごとに色を変える）
            for (int i = 0; i < dataTable.Rows.Count; i++)
            {
                if (i % 2 == 0)
                {
                    for (int col = 0; col < dataTable.Columns.Count; col++)
                    {
                        var cell = worksheet.Cells[i + 2, col + 1];
                        cell.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                        if ("SO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                        {
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(226, 239, 218));
                        }
                        else if ("PO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                        {
                            cell.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(255, 230, 153));
                        }
                    }
                }
            }

            // 列のデータ型に基づいた書式設定
            for (int col = 0; col < dataTable.Columns.Count; col++)
            {
                var column = dataTable.Columns[col];
                if (column.DataType == typeof(DateTime))
                {
                    worksheet.Column(col + 1).Style.Numberformat.Format = "mm/dd/yyyy";
                }
                else if (column.DataType == typeof(int))
                {
                    worksheet.Column(col + 1).Style.Numberformat.Format = "#,##0";
                }
            }

            // 列幅を自動調整
            if (worksheet.Dimension != null)
            {
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            }
            // ヘッダー行を固定
            worksheet.View.FreezePanes(2, 1);
            // 目盛線（グリッド線）を非表示にする
            worksheet.View.ShowGridLines = false;

            return workbook;
        }

        public XLWorkbook ExportDataTableWithFormattingForWorkbook(XLWorkbook workbook, DataTable dataTable, String sheetName, String ColorType)
        {
            // ワークシートを追加
            var worksheet = workbook.Worksheets.Add(dataTable, sheetName);

            // ヘッダー行のスタイルを設定
            var headerRow = worksheet.Row(1);
            headerRow.Style.Font.Bold = true;
            headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            for (int col = 0; col < dataTable.Columns.Count; col++)
            {

                if ("SO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                {
                    headerRow.Cell(col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(112, 173, 71);
                }
                else if ("PO".Equals(ColorType, StringComparison.OrdinalIgnoreCase))
                {
                    headerRow.Cell(col + 1).Style.Fill.BackgroundColor = XLColor.FromArgb(255, 192, 0);
                }
            }

            // 列のデータ型に基づいた書式設定
            for (int col = 0; col < dataTable.Columns.Count; col++)
            {
                var column = dataTable.Columns[col];
                var xlColumn = worksheet.Column(col + 1);

                if (column.DataType == typeof(DateTime))
                {
                    xlColumn.Style.DateFormat.Format = "mm/dd/yyyy";
                }
                else if (column.DataType == typeof(int))
                {
                    xlColumn.Style.NumberFormat.Format = "#,##0";
                }
            }

            // 列幅を自動調整
            worksheet.Columns().AdjustToContents();
            // ヘッダー行を固定
            worksheet.SheetView.FreezeRows(1);
            // 目盛線（グリッド線）を非表示にする
            worksheet.ShowGridLines = false;

            return workbook;
        }

        public DataTable ConvertToDataTableFast<T>(IEnumerable<T> list)
		{
			DataTable dataTable = new DataTable();

			if (list == null || !list.Any())
			{
				Console.WriteLine("data is null or empty.");
				return dataTable;
			}

			// プロパティ情報を取得
			PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

			// カラムを作成
			foreach (PropertyInfo prop in properties)
			{
				Type columnType = prop.PropertyType;
				if (columnType.IsGenericType && columnType.GetGenericTypeDefinition() == typeof(Nullable<>))
				{
					columnType = Nullable.GetUnderlyingType(columnType);
				}

				dataTable.Columns.Add(prop.Name, columnType ?? typeof(string));
			}

			// プロパティアクセスを事前コンパイル（式ツリー）
			var propertyAccessors = new Func<T, object>[properties.Length];
			for (int i = 0; i < properties.Length; i++)
			{
				var prop = properties[i];
				var param = System.Linq.Expressions.Expression.Parameter(typeof(T), "item");
				var propAccess = System.Linq.Expressions.Expression.Property(param, prop);
				var castToObject = System.Linq.Expressions.Expression.Convert(propAccess, typeof(object));
				propertyAccessors[i] = System.Linq.Expressions.Expression.Lambda<Func<T, object>>(castToObject, param).Compile();
			}

			// データ行を追加（高速）
			foreach (T item in list)
			{
				DataRow row = dataTable.NewRow();

				for (int i = 0; i < properties.Length; i++)
				{
					object value = propertyAccessors[i](item);
					row[properties[i].Name] = value ?? DBNull.Value;
				}

				dataTable.Rows.Add(row);
			}

			return dataTable;
		}
	}
}
