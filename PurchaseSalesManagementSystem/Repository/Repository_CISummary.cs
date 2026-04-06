using System.Data;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_CISummary
	{
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_CISummary(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public void DeleteUCIDetailData()
        {
			string sqlPath = Path.Combine(
	            _env.ContentRootPath,
	            "SQL",
				"CISummary",
				"DeleteUCIDetailData.sql"
			);

			var sql = File.ReadAllText(sqlPath);

			using (var conn = _connectionFactory.GetConnection())
			{
				conn.Open();
				using (var cmd = new SqlCommand(sql, conn))
				{
					cmd.ExecuteNonQuery();
				}
			}
		}

		public void InsertUCIDetailData(Model_CISummary uCIDetailData_insert)
		{
			string sqlPath = Path.Combine(
				_env.ContentRootPath,
				"SQL",
				"CISummary",
				"InsertUCIDetailData.sql"
			);

			var sql = File.ReadAllText(sqlPath);

			using (var conn = _connectionFactory.GetConnection())
			{
				conn.Open();
				using (var cmd = new SqlCommand(sql, conn))
				{

                    cmd.CommandTimeout = 300;

                    cmd.Parameters.AddWithValue("@DocType", uCIDetailData_insert.DocType);
					cmd.Parameters.AddWithValue("@EntryDate", uCIDetailData_insert.EntryDate);
					cmd.Parameters.AddWithValue("@ShipDate", uCIDetailData_insert.ShipDate);
					cmd.Parameters.AddWithValue("@NewFileName", uCIDetailData_insert.NewFileName);
					cmd.Parameters.AddWithValue("@OriginalFileName", uCIDetailData_insert.OriginalFileName);
					cmd.Parameters.AddWithValue("@FOA_CI", uCIDetailData_insert.FOA_CI);
					cmd.Parameters.AddWithValue("@ShipTo1", uCIDetailData_insert.ShipTo1);
					cmd.Parameters.AddWithValue("@ShipTo2", uCIDetailData_insert.ShipTo2);
					cmd.Parameters.AddWithValue("@Attn", uCIDetailData_insert.Attn);
					cmd.Parameters.AddWithValue("@ShipVia", uCIDetailData_insert.ShipVia);
					cmd.Parameters.AddWithValue("@Account", uCIDetailData_insert.Account);
					cmd.Parameters.AddWithValue("@RequestedBy", uCIDetailData_insert.RequestedBy);
					cmd.Parameters.AddWithValue("@ItemCode", uCIDetailData_insert.ItemCode);
					cmd.Parameters.AddWithValue("@Whse1", uCIDetailData_insert.Whse1);
					cmd.Parameters.AddWithValue("@Whse2", uCIDetailData_insert.Whse2);
					cmd.Parameters.AddWithValue("@TranQty", uCIDetailData_insert.TranQty);
					cmd.Parameters.AddWithValue("@SalesOrderNo", uCIDetailData_insert.SalesOrderNo);
					cmd.Parameters.AddWithValue("@CustomerPONo", uCIDetailData_insert.CustomerPONo);
					cmd.Parameters.AddWithValue("@LineNo", uCIDetailData_insert.LineNo);
					cmd.Parameters.AddWithValue("@FujikinPartNo", uCIDetailData_insert.FujikinPartNo);
					cmd.Parameters.AddWithValue("@CustomerPartNo", uCIDetailData_insert.CustomerPartNo);
					cmd.Parameters.AddWithValue("@Category", uCIDetailData_insert.Category);
					cmd.Parameters.AddWithValue("@Tariff", uCIDetailData_insert.Tariff);
					cmd.Parameters.AddWithValue("@UnitPrice", uCIDetailData_insert.UnitPrice);
					cmd.Parameters.AddWithValue("@TotalPrice", uCIDetailData_insert.TotalPrice);
					cmd.Parameters.AddWithValue("@Instrucstions", uCIDetailData_insert.Instrucstions);

					cmd.ExecuteNonQuery();
				}
			}
		}

		public List<Model_StockCheck> GetStockCheckData()
		{
			var result = new List<Model_StockCheck>();

			string sqlPath = Path.Combine(
				_env.ContentRootPath,
				"SQL",
				"CISummary",
				"GetStockCheck.sql"
			);

			var sql = File.ReadAllText(sqlPath);

			using (var conn = _connectionFactory.GetConnection())
			{
				conn.Open();
				using (var cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = 300;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							decimal GetDecimalOrZero(string col) =>
								reader.IsDBNull(reader.GetOrdinal(col)) ? 0m : reader.GetDecimal(reader.GetOrdinal(col));

							result.Add(new Model_StockCheck
							{
								No = reader.IsDBNull(reader.GetOrdinal("No")) ? (int?)null : (int)reader.GetInt64(reader.GetOrdinal("No")),
								ItemCode = reader["ItemCode"] as string ?? "",
								Whse = reader["Whse"] as string ?? "",
								Qty_Current = GetDecimalOrZero("Qty_Current"),
								Ship = GetDecimalOrZero("Ship"),
								IT_Out = GetDecimalOrZero("IT_Out"),
								IT_In = GetDecimalOrZero("IT_In"),
								Qty_After = GetDecimalOrZero("Qty_After"),
								Comment = reader["Comment"] as string ?? "",
							});
						}
					}
				}
			}

			return result;
		}

		public List<Model_RAInput> GetRAInputData()
		{
			var result = new List<Model_RAInput>();

			string sqlPath = Path.Combine(
				_env.ContentRootPath,
				"SQL",
				"CISummary",
				"GetRAInput.sql"
			);

			var sql = File.ReadAllText(sqlPath);

			using (var conn = _connectionFactory.GetConnection())
			{
				conn.Open();
				using (var cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = 300;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							decimal? qty = reader.IsDBNull(reader.GetOrdinal("Qty"))
								? (decimal?)null
								: reader.GetDecimal(reader.GetOrdinal("Qty"));

							result.Add(new Model_RAInput
							{
								No = reader.IsDBNull(reader.GetOrdinal("No")) ? (int?)null : (int)reader.GetInt64(reader.GetOrdinal("No")),
								DocType = reader["DocType"] as string ?? "",
								FileName = reader["FileName"] as string ?? "",
								RALineNo = reader["RALineNo"] as string ?? "",
								ShipTo_ShipVia = reader["ShipTo_ShipVia"] as string ?? "",
								Whse_Out = reader["Whse_Out"] as string ?? "",
								Whse_In = reader["Whse_In"] as string ?? "",
								ItemCode = reader["ItemCode"] as string ?? "",
								Qty = qty,
							});
						}
					}
				}
			}

			return result;
		}

		public List<Model_CISummary_Detail> GetCIDetailData()
		{
			var result = new List<Model_CISummary_Detail>();

			string sqlPath = Path.Combine(
				_env.ContentRootPath,
				"SQL",
				"CISummary",
				"GetCIDetailData.sql"
			);

			var sql = File.ReadAllText(sqlPath);

			using (var conn = _connectionFactory.GetConnection())
			{
				conn.Open();
				using (var cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = 300;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							decimal? tranQty = reader.IsDBNull(reader.GetOrdinal("TranQty"))
								? (decimal?)null
								: reader.GetDecimal(reader.GetOrdinal("TranQty"));

							decimal? unitPrice = reader.IsDBNull(reader.GetOrdinal("UnitPrice"))
								? (decimal?)null
								: reader.GetDecimal(reader.GetOrdinal("UnitPrice"));

							decimal? totalPrice = reader.IsDBNull(reader.GetOrdinal("TotalPrice"))
								? (decimal?)null
								: reader.GetDecimal(reader.GetOrdinal("TotalPrice"));

							result.Add(new Model_CISummary_Detail
                            {
								No = reader.GetInt32("RowNumber"),
								DocType = reader["DocType"] as string ?? "",
								EntryDate = reader.GetDateTime(reader.GetOrdinal("EntryDate")),
								ShipDate = reader.GetDateTime(reader.GetOrdinal("ShipDate")),
								FileName = reader["NewFileName"] as string ?? "",
								FOA_CI = reader["FOA_CI"] as string ?? "",
								ShipTo = reader["ShipTo1"] as string ?? "",
								Address = reader["ShipTo2"] as string ?? "",
								Attn = reader["Attn"] as string ?? "",
								ShipVia = reader["ShipVia"] as string ?? "",
								Account = reader["Account"] as string ?? "",
								RequestedBy = reader["RequestedBy"] as string ?? "",
								ItemCode = reader["ItemCode"] as string ?? "",
								Whse_Out = reader["Whse1"] as string ?? "",
                                Whse_In = reader["Whse2"] as string ?? "",
								TranQty = tranQty,
								SalesOrderNo = reader["SalesOrderNo"] as string ?? "",
								CustomerPONo = reader["CustomerPONo"] as string ?? "",
								LineNo = reader["LineNumber"] as string ?? "",
								FujikinPartNo = reader["FujikinPartNo"] as string ?? "",
								CustomerPartNo = reader["CustomerPartNo"] as string ?? "",
								Category = reader["Category"] as string ?? "",
								Tariff = reader["Tariff"] as string ?? "",
								UnitPrice = unitPrice,
								TotalPrice = totalPrice,
								Instrucstions = reader["Instrucstions"] as string ?? "",
								OriginalFileName = reader["OriginalFileName"] as string ?? "",
							});
						}
					}
				}
			}

			return result;
		}

		public List<Model_RINKU> GetRINKUData()
		{
			var result = new List<Model_RINKU>();

			string sqlPath = Path.Combine(
				_env.ContentRootPath,
				"SQL",
				"CISummary",
				"GetRINKU.sql"
			);

			var sql = File.ReadAllText(sqlPath);

			using (var conn = _connectionFactory.GetConnection())
			{
				conn.Open();
				using (var cmd = new SqlCommand(sql, conn))
				{
					cmd.CommandTimeout = 300;
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							decimal? totalQty = reader.IsDBNull(reader.GetOrdinal("TotalQty"))
								? (decimal?)null
								: reader.GetDecimal(reader.GetOrdinal("TotalQty"));

							decimal? value = reader.IsDBNull(reader.GetOrdinal("Value"))
								? (decimal?)null
								: reader.GetDecimal(reader.GetOrdinal("Value"));

							result.Add(new Model_RINKU
							{
								EntryDate = reader["EntryDate"] as string ?? "",
								No = reader.IsDBNull(reader.GetOrdinal("No")) ? (int?)null : (int)reader.GetInt64(reader.GetOrdinal("No")),
								FileName = reader["FileName"] as string ?? "",
								ShipTo = reader["ShipTo"] as string ?? "",
								Attn = reader["Attn"] as string ?? "",
								ShipVia = reader["ShipVia"] as string ?? "",
								Account = reader["Account"] as string ?? "",
								WarehouseList = reader["WarehouseList"] as string ?? "",
								TotalQty = totalQty,
								Unit = reader["Unit"] as string ?? "",
								Requested_By = reader["Requested_By"] as string ?? "",
								Pace = reader["Pace"] as string ?? "",
								Instrucstions = reader["Instrucstions"] as string ?? "",
								Value = value,
							});
						}
					}
				}
			}

			return result;
		}
	}
}
