using System.Data;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_RAUpload
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_RAUpload(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public void DeleteRAInventory()
        {
			string sqlPath = Path.Combine(
	            _env.ContentRootPath,
	            "SQL",
				"RAUpload",
				"DeleteRAInventory.sql"
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

		public void InsertRAInventory(RAUpload_Insert rAUpload_insert)
		{
			string sqlPath = Path.Combine(
				_env.ContentRootPath,
				"SQL",
				"RAUpload",
				"InsertRAInventory.sql"
			);

			var sql = File.ReadAllText(sqlPath);

			using (var conn = _connectionFactory.GetConnection())
			{
				conn.Open();
				using (var cmd = new SqlCommand(sql, conn))
				{

                    cmd.CommandTimeout = 300;

                    cmd.Parameters.AddWithValue("@EntryDate", rAUpload_insert.EntryDate);
					cmd.Parameters.AddWithValue("@WarehouseCode", rAUpload_insert.WarehouseCode);
					cmd.Parameters.AddWithValue("@ItemCode", rAUpload_insert.ItemCode);
					cmd.Parameters.AddWithValue("@Description", rAUpload_insert.Description);
					cmd.Parameters.AddWithValue("@OriginalQty", rAUpload_insert.OriginalQty);
					cmd.Parameters.AddWithValue("@Qty", rAUpload_insert.Qty);
					cmd.Parameters.AddWithValue("@InvoiceNo", rAUpload_insert.InvoiceNo);
					cmd.Parameters.AddWithValue("@Box", rAUpload_insert.Box);
					cmd.Parameters.AddWithValue("@Weight", rAUpload_insert.Weight);
					cmd.Parameters.AddWithValue("@DateReceived", rAUpload_insert.DateReceived);
					cmd.Parameters.AddWithValue("@From", rAUpload_insert.From);
					cmd.Parameters.AddWithValue("@VantecRef", rAUpload_insert.VantecRef);
					cmd.Parameters.AddWithValue("@UnitPrice", rAUpload_insert.UnitPrice);
					cmd.Parameters.AddWithValue("@ShipMark", rAUpload_insert.ShipMark);
					cmd.Parameters.AddWithValue("@Comment", rAUpload_insert.Comment);

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

                    cmd.CommandTimeout = 300;

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
