using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_PurchaseOrderReceiptHistory
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_PurchaseOrderReceiptHistory(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_Vendor> GetVendors()
        {
            var list = new List<Model_Vendor>();
            // ALL追加
            list.Add(new Model_Vendor
            {
                VendorNo = "*ALL",
                VendorName = ""
            });

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseOrderReceiptHistory",
                "GetVendor.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Model_Vendor
                        {
                            VendorNo = reader["QCode"] as string ?? "",
                            VendorName = reader["QName"] as string ?? ""
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_ReceiptHistoryHeader_ItemCode> GetItems()
        {
            var list = new List<Model_ReceiptHistoryHeader_ItemCode>();
            // ALL追加
            list.Add(new Model_ReceiptHistoryHeader_ItemCode
            {
                ItemCode = "*ALL",
                Desc = ""
            });

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseOrderReceiptHistory",
                "GetItem.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Model_ReceiptHistoryHeader_ItemCode
                        {
                            ItemCode = reader["ItemCode"] as string ?? "",
                            Desc = reader["UDF_ITEMDESC"] as string ?? ""
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_ReceiptHistoryHeader_User> GetUsers()
        {
            var list = new List<Model_ReceiptHistoryHeader_User>();
            // ALL追加
            list.Add(new Model_ReceiptHistoryHeader_User
            {
                UserName = "*ALL",
                UserCreatedKey = ""
            });

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseOrderReceiptHistory",
                "GetUser.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Model_ReceiptHistoryHeader_User
                        {
                            UserName = reader["UserName"] as string ?? "",
                            UserCreatedKey = reader["UserCreatedKey"] as string ?? ""
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_PurchaseOrderReceiptHistory> GetPOReceiptHistory(string dateFrom, string dateTo, string vendorCode, string poNo, string invoiceNo, string receiptNo, string itemCode, string userName)
        {
            var result = new List<Model_PurchaseOrderReceiptHistory>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseOrderReceiptHistory",
                "GetPOReceiptHistory.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {

                    cmd.CommandTimeout = 300;

                    cmd.Parameters.AddWithValue("@DateFrom", SetParameter(dateFrom));
                    cmd.Parameters.AddWithValue("@DateTo", SetParameter(dateTo));
                    cmd.Parameters.AddWithValue("@VendorCode", SetParameter(vendorCode));
                    cmd.Parameters.AddWithValue("@PoNo", SetParameter(poNo));
                    cmd.Parameters.AddWithValue("@InvoiceNo", SetParameter(invoiceNo));
                    cmd.Parameters.AddWithValue("@ReceiptNo", SetParameter(receiptNo));
                    cmd.Parameters.AddWithValue("@ItemCode", SetParameter(itemCode));
                    cmd.Parameters.AddWithValue("@UserName", SetParameter(userName));

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            result.Add(new Model_PurchaseOrderReceiptHistory
                            {
                                PONo = r["PONo"] as string ?? "",
                                VendorNo = r["VendorNo"] as string ?? "",
                                VendorName = r["VendorName"] as string ?? "",
                                PODate = r.IsDBNull(r.GetOrdinal("PODate"))
                                    ? null
                                     : r.GetDateTime(r.GetOrdinal("PODate")),
                                ReceiptNo = r["ReceiptNo"] as string ?? "",
                                ReceiptDate = r.IsDBNull(r.GetOrdinal("ReceiptDate"))
                                    ? null
                                     : r.GetDateTime(r.GetOrdinal("ReceiptDate")),
                                InvoiceNo = r["InvoiceNo"] as string ?? "",
                                InvoiceDate = r.IsDBNull(r.GetOrdinal("InvoiceDate"))
                                    ? null
                                     : r.GetDateTime(r.GetOrdinal("InvoiceDate")),
                                InvoiceTotal = r.IsDBNull(r.GetOrdinal("InvoiceTotal"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("InvoiceTotal")),
                                LineKey = r["LineKey"] as string ?? "",
                                ItemCode = r["ItemCode"] as string ?? "",
                                ItemDescription = r["ItemDescription"] as string ?? "",
                                Warehouse = r["Warehouse"] as string ?? "",
                                QtyRcvd = r.IsDBNull(r.GetOrdinal("QtyRcvd"))
                                    ? null
                                    : (int)r.GetDecimal(r.GetOrdinal("QtyRcvd")),
                                UnitCost = r.IsDBNull(r.GetOrdinal("UnitCost"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("UnitCost")),
                                ExtensionAmt = r.IsDBNull(r.GetOrdinal("ExtensionAmt"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("ExtensionAmt")),
                                SONo = r["SONo"] as string ?? "",
                                Comment = r["Comment"] as string ?? "",
                                UserName = r["UserName"] as string ?? "",
                                PostingDate = r.IsDBNull(r.GetOrdinal("PostingDate"))
                                    ? null
                                     : r.GetDateTime(r.GetOrdinal("PostingDate")),
                                OperationDate = r.IsDBNull(r.GetOrdinal("OperationDate"))
                                    ? null
                                     : r.GetDateTime(r.GetOrdinal("OperationDate")),
                            });
                        }
                    }
                }
            }

            return result;
        }

        public string SetParameter(string val)
        {
            if (string.IsNullOrEmpty(val) || "*ALL".Equals(val)) {
                return "";
            }

            return val;
        }
    }
}
