using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_POList
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_POList(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_POList> GetPOListData(string purchaseOrderNo, string exportTarget)
        {
            var result = new List<Model_POList>();

            string sqlFileName = "Misc".Equals(exportTarget, StringComparison.OrdinalIgnoreCase)
                ? "POList_Misc.sql"
                : "POList_AllVendors.sql";

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "POList",
                sqlFileName
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@PurchaseOrderNo",
                        string.IsNullOrWhiteSpace(purchaseOrderNo) ? DBNull.Value : purchaseOrderNo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Model_POList
                            {
                                PoNo = reader["PoNo"] as string ?? "",
                                LnKey = reader["LnKey"] as string ?? "",
                                PODate = reader.IsDBNull(reader.GetOrdinal("PODate"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("PODate")),
                                Status = reader["Status"] as string ?? "",
                                Vendor = reader["Vendor"] as string ?? "",
                                VendorName = reader["VendorName"] as string ?? "",
                                ItemCode = reader["ItemCode"] as string ?? "",
                                UDF_ITEMDESC = reader["UDF_ITEMDESC"] as string ?? "",
                                QtyOrdered = reader.IsDBNull(reader.GetOrdinal("QtyOrdered"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("QtyOrdered")),
                                QtyRcpt = reader.IsDBNull(reader.GetOrdinal("QtyRcpt"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("QtyRcpt")),
                                QtyBalance = reader.IsDBNull(reader.GetOrdinal("QtyBalance"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("QtyBalance")),
                                QtyInvoiced = reader.IsDBNull(reader.GetOrdinal("QtyInvoiced"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("QtyInvoiced")),
                                UnitCost = reader.IsDBNull(reader.GetOrdinal("UnitCost"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                                Amount = reader.IsDBNull(reader.GetOrdinal("Amount"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("Amount")),
                                RequiredDate = reader.IsDBNull(reader.GetOrdinal("RequiredDate"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("RequiredDate"))
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
