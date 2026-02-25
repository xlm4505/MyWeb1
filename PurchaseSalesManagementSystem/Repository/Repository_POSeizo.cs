using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_POSeizo
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_POSeizo(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_Vendor> GetVendors()
        {
            var list = new List<Model_Vendor>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "Common",
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
                            VendorNo = reader["VendorCode"] as string ?? "",
                            VendorName = reader["VendorName"] as string ?? ""
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_UserName> GetUser()
        {
            var list = new List<Model_UserName>();


            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "Common",
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
                        list.Add(new Model_UserName
                        {
                            UserName = reader["UserName"] as string ?? "",
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_POSeizo_TKF> GetPOSeizo_TKF(string vendorNo, string userName, string orderStatus, string poEntryDate)
        {
            var result = new List<Model_POSeizo_TKF>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "POSeizo",
                "POSeizo_TKF.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@VendorCode", vendorNo);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@OrderStatus", orderStatus);
                    cmd.Parameters.AddWithValue("@EntryDate", poEntryDate);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            result.Add(new Model_POSeizo_TKF
                            {
                                PO = r["PO"] as string ?? "",
                                Unit = r["Unit"] as string ?? "",
                                ItemCode = r["ItemCode"] as string ?? "",
                                Description = r["Description"] as string ?? "",

                                CustomerPartNumber = r["CustomerPartNumber"] as string ?? "",

                                Customer = r["Customer"] as string ?? "",

                                WHCode = r["WHCode"] as string ?? "",


                                RequiredDeliveryDate = r.IsDBNull(r.GetOrdinal("RequiredDeliveryDate"))
                                    ? null
                                     : r.GetDateTime(r.GetOrdinal("RequiredDeliveryDate")),

                                OrderedQty = r.IsDBNull(r.GetOrdinal("OrderedQty"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("OrderedQty")),

                                UnitPrice = r.IsDBNull(r.GetOrdinal("UnitPrice"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("UnitPrice")),

                                Amount = r.IsDBNull(r.GetOrdinal("Amount"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("Amount")),

                            });
                        }
                    }
                }
            }

            return result;
        }

        public IEnumerable<Model_POSeizo_ALL> GetPOSeizo_ALL(string vendorNo, string userName, string orderStatus, string poEntryDate )
        {
            var result = new List<Model_POSeizo_ALL>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "POSeizo",
                "POSeizo_ALL.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@VendorCode", vendorNo);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@OrderStatus", orderStatus);
                    cmd.Parameters.AddWithValue("@EntryDate", poEntryDate);

                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            result.Add(new Model_POSeizo_ALL
                            {
                                No = r.IsDBNull(r.GetOrdinal("No"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("No")),

                                ItemCode = r["ItemCode"] as string ?? "",
                                Desc = r["Desc"] as string ?? "",
                                PartNumber = r["PartNumber"] as string ?? "",

                                Unit = r["Unit"] as string ?? "",

                                RequiredDate = r.IsDBNull(r.GetOrdinal("RequiredDate"))
                                    ? null
                                    : r.GetDateTime(r.GetOrdinal("RequiredDate")),


                                EstDeliveryDate = r["EstDeliveryDate"] as string ?? "",

                                QuantityOrdered = r.IsDBNull(r.GetOrdinal("QuantityOrdered"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("QuantityOrdered")),

                                UnitCost = r.IsDBNull(r.GetOrdinal("UnitCost"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("UnitCost")),

                                ExtensionAmt = r.IsDBNull(r.GetOrdinal("ExtensionAmt"))
                                    ? null
                                    : r.GetDecimal(r.GetOrdinal("ExtensionAmt")),

                                SalesOffice = r["SalesOffice"] as string ?? "",
                                SalesClass = r["SalesClass"] as string ?? "",

                                Customer = r["Customer"] as string ?? "",
                                EndCustmer = r["EndCustmer"] as string ?? "",
                                ShipTo = r["ShipTo"] as string ?? "",

                                PO = r["PO"] as string ?? "",
                                Line = r["Line"] as string ?? "",

                                Factory = r["Factory"] as string ?? "",

                                Filled = r["Filled"] as string ?? "",
                                Confirmed = r["Confirmed"] as string ?? "",
                                Approved = r["Approved"] as string ?? "",

                                DateApproved = r.IsDBNull(r.GetOrdinal("DateApproved"))
                                    ? null
                                    : r.GetDateTime(r.GetOrdinal("DateApproved")),

                                ProductionControlNotice = r["ProductionControlNotice"] as string ?? ""
                            });
                        }
                    }
                }
            }

            return result;
        }

        public int UpdatePurchaseOrderStatusToOpen(string vendorNo, string userName, DateTime entryDate)
        {
            string sql = @"
            UPDATE h
               SET h.OrderStatus = 'O'
              FROM PO_PurchaseOrderHeader h
              LEFT JOIN MAS_SYSTEM.dbo.SY_User u
                ON u.UserKey = h.UserCreatedKey
             WHERE h.DateCreated = @EntryDate
               AND h.OrderStatus = 'N'
               AND (u.FirstName + ' ' + u.LastName) = @UserName
               AND (@VendorCode = '00-0000000'
                    OR (h.APDivisionNo + '-' + h.VendorNo) = @VendorCode);";

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@EntryDate", entryDate.Date);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@VendorCode", vendorNo);

                    return cmd.ExecuteNonQuery();
                }
            }
        }


    }
}
