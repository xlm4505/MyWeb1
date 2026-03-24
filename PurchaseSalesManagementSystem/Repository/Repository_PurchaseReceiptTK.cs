using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using System.Data;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_PurchaseReceiptTK
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_PurchaseReceiptTK(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public bool InvoiceExists(string invoiceNo)
        {

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptTK",
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

        public IEnumerable<Model_InvoiceDB_PurchaseReceiptTK> GetPoDetails(string poNo, string partNo )
        {
            var list = new List<Model_InvoiceDB_PurchaseReceiptTK>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptTK",
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
                    cmd.Parameters.AddWithValue("@partNo", partNo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Model_InvoiceDB_PurchaseReceiptTK
                            {
                                PONo = reader["PurchaseOrderNo"] as string ?? "",
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
            Model_InvoiceHeader_PurchaseReceiptTK header,
            Model_InvoiceDetail_PurchaseReceiptTK detail,
            //Model_InvoiceDB_PurchaseReceiptTK po,
            string ipAddress,
            string userName)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptTK",
                "InsertUploadData.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@BatchNo", string.Concat("B", header.InvoiceNo.AsSpan(header.InvoiceNo.Length - 4)));
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
                "PurchaseReceiptTK",
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

        public string GetPassword(string id)
        {
            string password = "";

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptTK",
                "GetPassword.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", id);  

                    using (var reader = cmd.ExecuteReader()) 
                    {
                        while (reader.Read())
                        {
                            password = reader["Password"] as string ?? "";
                        }
                    }
                }
            }
            return password;
        }

    }

}
