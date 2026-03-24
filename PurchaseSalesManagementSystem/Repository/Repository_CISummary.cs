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

		public IEnumerable<RAUpload_ExportToExcel> GetDownloadData()
		{
			var result = new List<RAUpload_ExportToExcel>();

			string sqlPath = Path.Combine(
				_env.ContentRootPath,
				"SQL",
				"RAUpload",
				"GetRAInventory.sql"
			  );

			var sql = File.ReadAllText(sqlPath);

			using (var conn = _connectionFactory.GetConnection())
			{
				conn.Open();

				using (var cmd = new SqlCommand(sql, conn))
				{
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							decimal jfi = reader.IsDBNull(reader.GetOrdinal("JFI"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("JFI"));

							decimal nal = reader.IsDBNull(reader.GetOrdinal("NAL"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("NAL"));

							decimal nca = reader.IsDBNull(reader.GetOrdinal("NCA"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("NCA"));

							decimal ntx = reader.IsDBNull(reader.GetOrdinal("NTX"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("NTX"));

							decimal utx = reader.IsDBNull(reader.GetOrdinal("UTX"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("UTX"));

							decimal ugp = reader.IsDBNull(reader.GetOrdinal("UGP"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("UGP"));

							decimal ifs = reader.IsDBNull(reader.GetOrdinal("IFS"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("IFS"));

							decimal nnj = reader.IsDBNull(reader.GetOrdinal("NNJ"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("NNJ"));

							decimal xit = reader.IsDBNull(reader.GetOrdinal("XIT"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("XIT"));

							decimal total = reader.IsDBNull(reader.GetOrdinal("Total"))
										? 0
										: reader.GetDecimal(reader.GetOrdinal("Total"));

							result.Add(new RAUpload_ExportToExcel
							{
								ItemCode = reader["ItemCode"] as string ?? "",
								ItemDesc = reader["ItemDesc"] as string ?? "",
								JFI = (int)jfi,
								NAL = (int)nal,
								NCA = (int)nca,
								NTX = (int)ntx,
								UTX = (int)utx,
								UGP = (int)ugp,
								IFS = (int)ifs,
								NNJ = (int)nnj,
								XIT = (int)xit,
								Total = (int)total,
							});
						}
					}
				}
			}

			return result;
		}
	}
}
