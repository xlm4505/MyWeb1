using DocumentFormat.OpenXml.Office.Word;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using System.Data;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_PurchaseReceiptFJK
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_PurchaseReceiptFJK(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }


        public IEnumerable<Model_InvoiceDB_PurchaseReceiptCCL> GetPoDetails(string poNo, string partNo )
        {
            var list = new List<Model_InvoiceDB_PurchaseReceiptCCL>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptCCL",
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
                            list.Add(new Model_InvoiceDB_PurchaseReceiptCCL
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
    List<Model_InvoiceSummary_PurchaseReceiptFJK> summaryList,
    string ipAddress,
    string userName)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptFJK",
                "InsertUploadData.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                foreach (var x in summaryList)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@BatchNo", x.BatchNo ?? "");
                        cmd.Parameters.AddWithValue("@InvoiceNo", x.InvoiceNo ?? "");
                        cmd.Parameters.AddWithValue("@InvoiceDate",
                            x.InvoiceDate?.ToString("MM/dd/yyyy") ?? "");

                        // POは7桁ゼロ埋め
                        cmd.Parameters.AddWithValue("@PONo",
                            (x.PoNo ?? "").PadLeft(7, '0'));

                        cmd.Parameters.AddWithValue("@LineNo", x.Ln ?? "");
                        cmd.Parameters.AddWithValue("@ItemCode", x.ItemCode ?? "");

                        cmd.Parameters.AddWithValue("@Quantity",
                            x.Quantity.ToString());

                        cmd.Parameters.AddWithValue("@UnitPrice",
                            x.UnitPrice.ToString());

                        cmd.Parameters.AddWithValue("@ClientAddress",
                            ipAddress ?? "");

                        cmd.Parameters.AddWithValue("@UserCreated",
                            userName ?? "");

                        cmd.Parameters.AddWithValue("@DateCreated",
                            DateTime.Now.ToString("MM/dd/yyyy"));

                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }

        public void UpdateStatus(string ipAddress)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseReceiptCCL",
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
                "PurchaseReceiptCCL",
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
