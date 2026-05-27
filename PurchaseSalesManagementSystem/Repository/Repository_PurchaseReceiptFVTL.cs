using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_PurchaseReceiptFVTL
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_PurchaseReceiptFVTL(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public bool InvoiceExists(string invoiceNo)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptFVTL",
                "CheckInvoiceExists.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@InvoiceNo", invoiceNo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        return reader.Read();
                    }
                }
            }
        }

        public IEnumerable<Model_InvoiceDB_PurchaseReceiptFVTL> GetPoDetails(string poNo, string description)
        {
            var list = new List<Model_InvoiceDB_PurchaseReceiptFVTL>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptFVTL",
                "SelectPoDetail.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.AddWithValue("@poNo", poNo);
                    cmd.Parameters.AddWithValue("@description", description);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Model_InvoiceDB_PurchaseReceiptFVTL
                            {
                                LineKey = reader["LineKey"] as string ?? "",
                                ItemCode = reader["ItemCode"] as string ?? "",
                                WarehouseCode = reader["WarehouseCode"] as string ?? "",
                                OrderStatus = reader["OrderStatus"] as string ?? "",
                                UnitCost = reader.IsDBNull(reader.GetOrdinal("UnitCost"))
                                        ? null
                                        : reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                                QuantityOrdered = reader.IsDBNull(reader.GetOrdinal("QuantityOrdered"))
                                        ? null
                                        : reader.GetDecimal(reader.GetOrdinal("QuantityOrdered")),
                                QuantityReceived = reader.IsDBNull(reader.GetOrdinal("QuantityReceived"))
                                        ? null
                                        : reader.GetDecimal(reader.GetOrdinal("QuantityReceived")),
                            });
                        }
                    }
                }
            }

            return list;
        }

        public void InsertUploadData(
            Model_InvoiceHeader_PurchaseReceiptFVTL header,
            Model_InvoiceDetail_PurchaseReceiptFVTL detail,
            string ipAddress,
            string userName)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptFVTL",
                "InsertUploadData.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@BatchNo", header.InvoiceNo.Substring(header.InvoiceNo.Length - 5));
                    cmd.Parameters.AddWithValue("@InvoiceNo", header.InvoiceNo);
                    cmd.Parameters.AddWithValue("@InvoiceDate", header.InvoiceDate.ToString("M/dd/yyyy"));
                    cmd.Parameters.AddWithValue("@PONo", detail.PoNo.PadLeft(7, '0'));
                    cmd.Parameters.AddWithValue("@LineNo", detail.LineKey);
                    cmd.Parameters.AddWithValue("@ItemCode", detail.ItemCode);
                    cmd.Parameters.AddWithValue("@Quantity", detail.Quantity?.ToString() ?? "0");
                    cmd.Parameters.AddWithValue("@UnitPrice", detail.UP?.ToString() ?? "0");
                    cmd.Parameters.AddWithValue("@ClientAddress", ipAddress ?? "");
                    cmd.Parameters.AddWithValue("@UserCreated", userName);
                    cmd.Parameters.AddWithValue("@DateCreated", DateTime.Now.ToString("MM/dd/yyyy"));

                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void UpdateStatus(string ipAddress)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptFVTL",
                "UpdateStatusFlag.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ClientAddress", ipAddress ?? "");
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
