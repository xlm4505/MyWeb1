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

        public IEnumerable<Model_POList_Misc> GetPOListDataMisc(string purchaseOrderNo, string exportTarget)
        {
            var result = new List<Model_POList_Misc>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "POList",
                "POList_Misc.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {

                    cmd.CommandTimeout = 300;

                    cmd.Parameters.AddWithValue(
                        "@PurchaseOrderNo",
                        string.IsNullOrWhiteSpace(purchaseOrderNo) ? DBNull.Value : purchaseOrderNo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Model_POList_Misc
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

        public IEnumerable<Model_POList_AllVendors> GetPOListDataAllVendors(string purchaseOrderNo, string exportTarget)
        {
            var result = new List<Model_POList_AllVendors>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "POList",
                "POList_AllVendors.sql"
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
                            result.Add(new Model_POList_AllVendors
                            {
                                VendorNo = reader["VendorNo"] as string ?? "",
                                POLn = reader["PO-Ln"] as string ?? "",
                                PONo = reader["PONo"] as string ?? "",
                                LnKey = reader["LnKey"] as string ?? "",
                                PODate = reader.IsDBNull(reader.GetOrdinal("PODate"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("PODate")),
                                Status = reader["Status"] as string ?? "",
                                ItemCode = reader["ItemCode"] as string ?? "",
                                ItemDesc = reader["ItemDesc"] as string ?? "",
                                Whse = reader["Whse"] as string ?? "",
                                Ordered = reader.IsDBNull(reader.GetOrdinal("Ordered"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("Ordered")),
                                Received = reader.IsDBNull(reader.GetOrdinal("Received"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("Received")),
                                Balance = reader.IsDBNull(reader.GetOrdinal("Balance"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("Balance")),
                                Invoiced = reader.IsDBNull(reader.GetOrdinal("Invoiced"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("Invoiced")),
                                UnitCost = reader.IsDBNull(reader.GetOrdinal("UnitCost"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                                StdUnitCost = reader.IsDBNull(reader.GetOrdinal("StdUnitCost"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("StdUnitCost")),
                                LastCost = reader.IsDBNull(reader.GetOrdinal("LastCost"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("LastCost")),
                                AvgCost = reader.IsDBNull(reader.GetOrdinal("AvgCost"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("AvgCost")),
                                VenCostCM = reader.IsDBNull(reader.GetOrdinal("VenCost(CM)"))
                                    ? null
                                    : reader.GetDecimal(reader.GetOrdinal("VenCost(CM)")),
                                RequiredDate = reader.IsDBNull(reader.GetOrdinal("RequiredDate"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("RequiredDate")),
                                PromiseDate = reader.IsDBNull(reader.GetOrdinal("PromiseDate"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("PromiseDate")),
                                SalesOrderNo = reader["SalesOrderNo"] as string ?? ""
                            });
                        }
                    }
                }
            }

            return result;
        }

    }
}
