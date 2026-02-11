using System.Data;
using System.Reflection;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;

namespace PurchaseSalesManagementSystem.Common
{
	public class FormattedDataTableExcelExporter
	{
		public byte[] ExportDataTableWithFormatting(DataTable dataTable, String sheetName)
		{
			if (dataTable == null || dataTable.Rows.Count == 0)
			{
				throw new ArgumentException("DataTableはnullまたは空です。");
			}

			using (var workbook = new XLWorkbook())
			{
				// ワークシートを追加
				var worksheet = workbook.Worksheets.Add(dataTable, sheetName);

				// ヘッダー行のスタイルを設定
				var headerRow = worksheet.Row(1);
				headerRow.Style.Font.Bold = true;
				headerRow.Style.Fill.BackgroundColor = XLColor.FromArgb(112, 173, 71);
				headerRow.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

				// データ行のスタイル（行ごとに色を変える）
				for (int i = 0; i < dataTable.Rows.Count; i++)
				{
					var row = worksheet.Row(i + 2);  // ヘッダー行の次から
					if (i % 2 == 0)
					{
						row.Style.Fill.BackgroundColor = XLColor.FromArgb(226, 239, 218);
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

		public DataTable ConvertToDataTableFast<T>(IEnumerable<T> list)
		{
			DataTable dataTable = new DataTable();

			if (list == null || !list.Any())
			{
				Console.WriteLine("データが空です。");
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
