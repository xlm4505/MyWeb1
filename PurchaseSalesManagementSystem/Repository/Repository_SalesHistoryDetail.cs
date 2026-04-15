using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_SalesHistoryDetail
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_SalesHistoryDetail(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_SalesHistoryDetailItem> GetItemCodes()
        {
            var list = new List<Model_SalesHistoryDetailItem>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "SalesHistoryDetail",
                "GetItemCodes.sql"
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
                            list.Add(new Model_SalesHistoryDetailItem
                            {
                                ItemCode = reader["ItemCode"] as string ?? "",
                                ItemDesc = reader["UDF_ITEMDESC"] as string ?? ""
                            });
                        }
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_SalesHistoryDetailItemDesc> GetItemDescs()
        {
            var list = new List<Model_SalesHistoryDetailItemDesc>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "SalesHistoryDetail",
                "GetItemDescs.sql"
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
                            list.Add(new Model_SalesHistoryDetailItemDesc
                            {
                                ItemDesc = reader["UDF_ITEMDESC"] as string ?? ""
                            });
                        }
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_SalesHistoryDetailCustomer> GetCustomers()
        {
            var list = new List<Model_SalesHistoryDetailCustomer>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "SalesHistoryDetail",
                "GetCustomers.sql"
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
                            list.Add(new Model_SalesHistoryDetailCustomer
                            {
                                QCode = reader["QCode"] as string ?? "",
                                QName = reader["QName"] as string ?? ""
                            });
                        }
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_SalesHistoryDetailResult> GetSalesHistoryDetail(
            string customer, string itemCode, string itemDescription,
            string dateFrom, string dateTo)
        {
            var list = new List<Model_SalesHistoryDetailResult>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "SalesHistoryDetail",
                "GetSalesHistoryDetail.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@Customer", customer);
                    cmd.Parameters.AddWithValue("@ItemCode", itemCode);
                    cmd.Parameters.AddWithValue("@ItemDescription", itemDescription);
                    cmd.Parameters.AddWithValue("@OperationDateFrom", dateFrom);
                    cmd.Parameters.AddWithValue("@OperationDateTo", dateTo);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Model_SalesHistoryDetailResult
                            {
                                InvoiceDate   = reader["InvoiceDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["InvoiceDate"]),
                                Customer      = reader["Customer"] as string ?? "",
                                SalesOrderNo  = reader["SalesOrderNo"] as string ?? "",
                                ItemCode      = reader["ItemCode"] as string ?? "",
                                ItemCodeDesc  = reader["ItemCodeDesc"] as string ?? "",
                                CustomerPartNo = reader["CustomerPartNo"] as string ?? "",
                                CustomerPONo  = reader["CustomerPONo"] as string ?? "",
                                POLineNo      = reader["POLineNo"] as string ?? "",
                                InvoiceNo     = reader["InvoiceNo"] as string ?? "",
                                ShipToCode    = reader["ShipToCode"] as string ?? "",
                                ShipToName    = reader["ShipToName"] as string ?? "",
                                ShipVia       = reader["ShipVia"] as string ?? "",
                                Tracking1     = reader["Tracking1"] as string ?? "",
                                Tracking2     = reader["Tracking2"] as string ?? "",
                                Tracking3     = reader["Tracking3"] as string ?? "",
                                Warehouse     = reader["Warehouse"] as string ?? "",
                                ShippedQty    = reader["ShippedQty"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["ShippedQty"]),
                                UnitPrice     = reader["UnitPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["UnitPrice"]),
                                ExtensionAmt  = reader["ExtensionAmt"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["ExtensionAmt"]),
                                StdUnitPrice  = reader["StdUnitPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["StdUnitPrice"]),
                                UnitCost      = reader["UnitCost"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["UnitCost"]),
                                StdUnitCost   = reader["StdUnitCost"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["StdUnitCost"]),
                                LastUnitPrice = reader["LastUnitPrice"] == DBNull.Value ? (decimal?)null : Convert.ToDecimal(reader["LastUnitPrice"]),
                                RequestDate   = reader["RequestDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["RequestDate"]),
                                PromiseDate   = reader["PromiseDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PromiseDate"]),
                                CommitDate    = reader["CommitDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["CommitDate"]),
                                ShipDate      = reader["ShipDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["ShipDate"]),
                                PostingDate   = reader["PostingDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["PostingDate"]),
                                EntryDate     = reader["EntryDate"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(reader["EntryDate"]),
                            });
                        }
                    }
                }
            }

            return list;
        }
    }
}
