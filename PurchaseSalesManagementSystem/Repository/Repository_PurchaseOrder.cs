using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using System.Data;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_PurchaseOrder
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_PurchaseOrder(CreateConnection connectionFactory, IWebHostEnvironment env)
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

        public IEnumerable<Model_SalesPerson> GetAllSalesPersons()
        {
            var list = new List<Model_SalesPerson>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "PurchaseOrder",
                "GetSalesPerson.sql"
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
                        list.Add(new Model_SalesPerson
                        {
                            SalesPerson = reader["SalesPerson"] as string ?? "",
                            CustomerNo = reader["CustomerCode"] as string ?? ""
                        });
                    }
                }
            }

            return list;
        }

        public IEnumerable<Model_PurchaseOrder> GetPurchaseOrder(string vendorNo, string productType)
        {
            var result = new List<Model_PurchaseOrder>();

            string sqlPath = "";
            if ("00-0000000".Equals(vendorNo))
            {
                if ("Valves".Equals(productType))
                {
                    sqlPath = Path.Combine(
                    _env.ContentRootPath,
                    "SQL",
                    "PurchaseOrder",
                    "PurchaseOrder_ALL_Valves.sql"
                  );
                }
                else
                {
                    sqlPath = Path.Combine(
                     _env.ContentRootPath,
                     "SQL",
                     "PurchaseOrder",
                     "PurchaseOrder_ALL_Mass.sql"
                   );
                }
            }
            else {
                if ("Valves".Equals(productType))
                {
                    sqlPath = Path.Combine(
                    _env.ContentRootPath,
                    "SQL",
                    "PurchaseOrder",
                    "PurchaseOrder_Valves.sql"
                  );
                }
                else
                {
                    sqlPath = Path.Combine(
                     _env.ContentRootPath,
                     "SQL",
                     "PurchaseOrder",
                     "PurchaseOrder_Mass.sql"
                   );
                }


            }

            // Mass Flow のときだけ
            Dictionary<string, string> salesPersonMap = new();

            if (productType != "Valves")
            {
                salesPersonMap = GetAllSalesPersons()
                .OrderBy(x => x.CustomerNo)
                .ThenBy(x => x.SalesPerson)
                .GroupBy(x => x.CustomerNo)
                .ToDictionary(
                    g => g.Key,
                    g => g.First().SalesPerson
                );
            }
            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {

                    if (!"00-0000000".Equals(vendorNo)) {
                        cmd.Parameters.AddWithValue("@VendorNo", vendorNo);
                    }

                    // cmd.Parameters.AddWithValue("@ProductType", productType);
                    var Seq = 0;
                    var VendorNoTmp = "";
                    var CustomerNoTmp = "";
                    using (var r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            var modelPurchaseOrder = new Model_PurchaseOrder();
                            modelPurchaseOrder.ConfirmTo = (object)r["ConfirmTo"] as string ?? "";

                            modelPurchaseOrder.ItemCode = (object)r["ItemCode"] as string ?? "";
                            modelPurchaseOrder.ItemCodeDesc = (object)r["ItemCodeDesc"] as string ?? "";
                            modelPurchaseOrder.CustReqDate = r.IsDBNull(r.GetOrdinal("CustReqDate"))? null: r.GetDateTime(r.GetOrdinal("CustReqDate"));
                            modelPurchaseOrder.POReqDate = r.IsDBNull(r.GetOrdinal("POReqDate"))? null: r.GetDateTime(r.GetOrdinal("POReqDate"));
                            modelPurchaseOrder.PurchaseOrderQty = r.IsDBNull(r.GetOrdinal("PurchaseOrderQty")) ? null:Convert.ToInt32(r["PurchaseOrderQty"]);
                            modelPurchaseOrder.WarehouseCode = (object)r["WarehouseCode"] as string ?? "";
                            modelPurchaseOrder.CustomerNo = (object)r["CustomerNo"] as string ?? "";
                            if (productType != "Valves") 
                            { 
                                if ((!r.IsDBNull(r.GetOrdinal("PurchaseOrderQty"))) && salesPersonMap.TryGetValue(modelPurchaseOrder.CustomerNo, out var sp))
                                {
                                    modelPurchaseOrder.SalesPerson = sp;
                                }
                                else
                                {
                                    modelPurchaseOrder.SalesPerson = "";
                                }
                            }
                            else
                            {
                                modelPurchaseOrder.SalesPerson = (object)r["SalesPerson"] as string ?? "";
                            }
                                modelPurchaseOrder.BillToName = (object)r["BillToName"] as string ?? "";
                            modelPurchaseOrder.SalesOrderNo = (object)r["SalesOrderNo"] as string ?? "";
                            modelPurchaseOrder.SalesOrderEntryDate = r.IsDBNull(r.GetOrdinal("SalesOrderEntryDate")) ? null : r.GetDateTime(r.GetOrdinal("SalesOrderEntryDate"));
                            modelPurchaseOrder.VendorNo = (object)r["VendorNo"] as string ?? "";
                            if (productType != "Valves")
                            {
                                if (!CustomerNoTmp.Equals(modelPurchaseOrder.CustomerNo))
                                {
                                    Seq++;
                                }
                            }
                            else { 
                                if (!VendorNoTmp.Equals(modelPurchaseOrder.VendorNo))
                                {
                                    Seq++;
                                }
                            }
                            modelPurchaseOrder.Seq = Seq;
                            VendorNoTmp = r["VendorNo"] as string ?? "";
                            CustomerNoTmp = (object)r["CustomerNo"] as string ?? "";

                            modelPurchaseOrder.Message = (object)r["Message"] as string ?? "";
                            modelPurchaseOrder.Message = modelPurchaseOrder.Message.Trim();
                            modelPurchaseOrder.AliasItemNo = (object)r["AliasItemNo"] as string ?? "";
                            modelPurchaseOrder.CustomerPONo = (object)r["CustomerPONo"] as string ?? "";

                            result.Add(modelPurchaseOrder);

                            if (modelPurchaseOrder.PurchaseOrderQty >50 )
                            {
                                var copy = new Model_PurchaseOrder(modelPurchaseOrder);
                                copy.PurchaseOrderQty = modelPurchaseOrder.PurchaseOrderQty - 50;
                                result.Add(copy);
                            }

                        }
                    }
                }
            }

            return result;
        }

    }
}
