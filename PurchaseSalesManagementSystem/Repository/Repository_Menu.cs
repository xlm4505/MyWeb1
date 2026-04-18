using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using Microsoft.Data.SqlClient;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_Menu
    {

        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_Menu(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        // =========================
        // 11-1 / 11-2
        // =========================
        public IEnumerable<Model_InventoryForecast> GetInventoryForecasting(string reportName)
        {
            var results = new List<Model_InventoryForecast>();

            var yyyymm = new List<string>();
            for (int i = -1; i <= 7; i++)
                yyyymm.Add(DateTime.Today.AddMonths(i).ToString("yyyy-MM"));

            string sqlPath = Path.Combine(_env.ContentRootPath, "SQL", "Menu", "InventoryForecastingReport.sql");

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    for (int i = 0; i < yyyymm.Count; i++)
                        cmd.Parameters.AddWithValue($"@YM{i}", yyyymm[i]);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int unitCostOrdinal = reader.GetOrdinal("UnitCost");
                            int onHandOrdinal = reader.GetOrdinal("OnHand");
                            int purchaseOrderOrdinal = reader.GetOrdinal("PurchaseOrder");
                            int salesOrderOrdinal = reader.GetOrdinal("SalesOrder");
                            int surplusOrdinal = reader.GetOrdinal("Surplus");
                            int m1 = reader.IsDBNull(reader.GetOrdinal("M1")) ? 0 : reader.GetInt32(reader.GetOrdinal("M1"));
                            int m2 = reader.IsDBNull(reader.GetOrdinal("M2")) ? 0 : reader.GetInt32(reader.GetOrdinal("M2"));
                            int m3 = reader.IsDBNull(reader.GetOrdinal("M3")) ? 0 : reader.GetInt32(reader.GetOrdinal("M3"));
                            int m4 = reader.IsDBNull(reader.GetOrdinal("M4")) ? 0 : reader.GetInt32(reader.GetOrdinal("M4"));
                            int m5 = reader.IsDBNull(reader.GetOrdinal("M5")) ? 0 : reader.GetInt32(reader.GetOrdinal("M5"));
                            int m6 = reader.IsDBNull(reader.GetOrdinal("M6")) ? 0 : reader.GetInt32(reader.GetOrdinal("M6"));
                            int m7 = reader.IsDBNull(reader.GetOrdinal("M7")) ? 0 : reader.GetInt32(reader.GetOrdinal("M7"));
                            int m8 = reader.IsDBNull(reader.GetOrdinal("M8")) ? 0 : reader.GetInt32(reader.GetOrdinal("M8"));
                            int m9 = reader.IsDBNull(reader.GetOrdinal("M9")) ? 0 : reader.GetInt32(reader.GetOrdinal("M9"));
                            int total = reader.IsDBNull(reader.GetOrdinal("Total")) ? 0 : reader.GetInt32(reader.GetOrdinal("Total"));
                            int itemNo = 0;
                            int itemNoOrdinal = reader.GetOrdinal("ItemNo");
                            if (!reader.IsDBNull(itemNoOrdinal))
                            {
                                itemNo = Convert.ToInt32(reader.GetValue(itemNoOrdinal));
                            }

                            results.Add(new Model_InventoryForecast
                            {
                                ItemCode = reader["ItemCode"] as string ?? "",
                                ItemCodeDesc = reader["ItemCodeDesc"] as string ?? "",
                                ItemNo = itemNo,
                                Category1 = reader["Category1"] as string ?? "",
                                VendorName = reader["VendorName"] as string ?? "",

                                UnitCost = reader.IsDBNull(unitCostOrdinal) ? 0m : reader.GetDecimal(unitCostOrdinal),
                                OnHand = reader.IsDBNull(onHandOrdinal) ? 0 : reader.GetInt32(onHandOrdinal),
                                PurchaseOrder = reader.IsDBNull(purchaseOrderOrdinal) ? 0 : reader.GetInt32(purchaseOrderOrdinal),
                                SalesOrder = reader.IsDBNull(salesOrderOrdinal) ? 0 : reader.GetInt32(salesOrderOrdinal),
                                Surplus = reader.IsDBNull(surplusOrdinal) ? 0 : reader.GetInt32(surplusOrdinal),
                                DataType = reader["Data Type"] as string ?? "",
                                MonthlyQty1 = m1,
                                MonthlyQty2 = m2,
                                MonthlyQty3 = m3,
                                MonthlyQty4 = m4,
                                MonthlyQty5 = m5,
                                MonthlyQty6 = m6,
                                MonthlyQty7 = m7,
                                MonthlyQty8 = m8,
                                MonthlyQty9 = m9,
                                Total = total
                            });
                        }
                    }
                }
            }

            return results;
        }

        // =========================
        // 11-7 Inventory Forecast By Month
        // =========================
        public IEnumerable<Model_InventoryForecastByMonth> GetInventoryForecastingByMonth()
        {
            var results = new List<Model_InventoryForecastByMonth>();

            // YM0 ～ YM8
            var yyyymm = new List<string>();
            for (int i = -1; i <= 7; i++)
            {
                yyyymm.Add(DateTime.Today.AddMonths(i).ToString("yyyy-MM"));
            }

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "Menu",
                "InventoryForecastingReportByMonth.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {

                    cmd.CommandTimeout = 300;

                    // @YM0 ～ @YM8 をセット
                    for (int i = 0; i < yyyymm.Count; i++)
                    {
                        cmd.Parameters.AddWithValue($"@YM{i}", yyyymm[i]);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            // M0 ～ M8
                            var monthly = new int?[9];
                            for (int i = 0; i < 9; i++)
                            {
                                var col = $"M{i}";
                                monthly[i] = reader.IsDBNull(reader.GetOrdinal(col))
                                    ? (int?)null
                                    : reader.GetInt32(reader.GetOrdinal(col));
                            }

                            results.Add(new Model_InventoryForecastByMonth
                            {

                                ItemCode = reader["ItemCode"] as string ?? "",
                                ItemCodeDesc = reader["ItemCodeDesc"] as string ?? "",
                                ItemNo = reader["ItemNo"] as string ?? "",
                                Category1 = reader["Category1"] as string ?? "",
                                VendorName = reader["VendorName"] as string ?? "",

                                UnitCost = reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                                OnHand = reader.GetInt32(reader.GetOrdinal("OnHand")),
                                PurchaseOrder = reader.GetInt32(reader.GetOrdinal("PurchaseOrder")),
                                SalesOrder = reader.GetInt32(reader.GetOrdinal("SalesOrder")),
                                Surplus = reader.GetInt32(reader.GetOrdinal("Surplus")),

                                DataType = reader["Data Type"] as string ?? "",
                                MonthlyQty = monthly
                            });
                        }
                    }
                }
            }

            return results;
        }

        // =========================
        // 2 OnHandShortage
        // =========
        public IEnumerable<Model_OnHandShortageCheckList> GetOnHandShortage()
        {
            var list = new List<Model_OnHandShortageCheckList>();


            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "Menu",
                "OnHandShortageCheckList.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {
                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        list.Add(new Model_OnHandShortageCheckList
                        {

                            ItemCode = reader["ItemCode"] as string ?? "",
                            ItemDesc = reader["ItemDesc"] as string ?? "",
                            Category = reader["Category"] as string ?? "",

                            OnHand_Reg = Convert.ToInt32(reader["OnHand(Reg)"]),
                            OpenPO_Reg = Convert.ToInt32(reader["OpenPO(Reg)"]),
                            OpenSO_Reg = Convert.ToInt32(reader["OpenSO(Reg)"]),
                            Available_Reg = Convert.ToInt32(reader["Available(Reg)"]),

                            OnHand_Ex = Convert.ToInt32(reader["OnHand(Ex)"]),
                            OpenPO_Ex = Convert.ToInt32(reader["OpenPO(Ex)"]),
                            OpenSO_Ex = Convert.ToInt32(reader["OpenSO(Ex)"]),
                            Available_Ex = Convert.ToInt32(reader["Available(Ex)"]),

                            OnHand_Total = Convert.ToInt32(reader["OnHand(Total)"]),
                            OpenPO_Total = Convert.ToInt32(reader["OpenPO(Total)"]),
                            OpenSO_Total = Convert.ToInt32(reader["OpenSO(Total)"]),
                            Available_Total = Convert.ToInt32(reader["Available(Total)"])
                        });
                    }
                }
            }
            return list;
        }

        // =========================
        // 2 OpenOrderVolume
        // =========
        public IEnumerable<Model_ProjectPartOpenOrderVolume> GetProjectPartOpenOrderVolume()
        {
            var result = new List<Model_ProjectPartOpenOrderVolume>();

            var yyyymm = Enumerable.Range(-4, 12)
                .Select(i => DateTime.Today.AddMonths(i).ToString("yyyy-MM"))
                .ToList();


            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "Menu",
                "ProjectPartOpenOrderVolume.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            using (var cmd = new SqlCommand(sql, conn))
            {

                cmd.CommandTimeout = 300;

                for (int i = 0; i < 12; i++)
                    cmd.Parameters.AddWithValue($"@YM{i}", yyyymm[i]);

                conn.Open();
                using (var r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        result.Add(new Model_ProjectPartOpenOrderVolume
                        {
                            ItemCode = r["ItemCode"]?.ToString() ?? "",
                            ItemCodeDesc = r["ItemCodeDesc"]?.ToString() ?? "",
                            VendorName = r["VendorName"]?.ToString() ?? "",

                            UnitCost = r.IsDBNull(r.GetOrdinal("UnitCost"))
                                ? 0m
                                : r.GetDecimal(r.GetOrdinal("UnitCost")),

                            OnHand1 = r.IsDBNull(r.GetOrdinal("OnHand1"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("OnHand1"))),

                            OpenPO1 = r.IsDBNull(r.GetOrdinal("OpenPO1"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("OpenPO1"))),

                            OpenSO1 = r.IsDBNull(r.GetOrdinal("OpenSO1"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("OpenSO1"))),

                            Surplus1 = r.IsDBNull(r.GetOrdinal("Surplus1"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("Surplus1"))),

                            OnHand2 = r.IsDBNull(r.GetOrdinal("OnHand2"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("OnHand2"))),

                            OpenPO2 = r.IsDBNull(r.GetOrdinal("OpenPO2"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("OpenPO2"))),

                            OpenSO2 = r.IsDBNull(r.GetOrdinal("OpenSO2"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("OpenSO2"))),

                            Surplus2 = r.IsDBNull(r.GetOrdinal("Surplus2"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("Surplus2"))),

                            Available = r.IsDBNull(r.GetOrdinal("Available"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("Available"))),
                            MonthlyQty0 = r.IsDBNull(r.GetOrdinal("M0"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M0"))),
                            MonthlyQty1 = r.IsDBNull(r.GetOrdinal("M1"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M1"))),
                            MonthlyQty2 = r.IsDBNull(r.GetOrdinal("M2"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M2"))),
                            MonthlyQty3 = r.IsDBNull(r.GetOrdinal("M3"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M3"))),
                            MonthlyQty4 = r.IsDBNull(r.GetOrdinal("M4"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M4"))),
                            MonthlyQty5 = r.IsDBNull(r.GetOrdinal("M5"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M5"))),
                            MonthlyQty6 = r.IsDBNull(r.GetOrdinal("M6"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M6"))),
                            MonthlyQty7 = r.IsDBNull(r.GetOrdinal("M7"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M7"))),
                            MonthlyQty8 = r.IsDBNull(r.GetOrdinal("M8"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M8"))),
                            MonthlyQty9 = r.IsDBNull(r.GetOrdinal("M9"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M9"))),
                            MonthlyQty10 = r.IsDBNull(r.GetOrdinal("M10"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M10"))),
                            MonthlyQty11 = r.IsDBNull(r.GetOrdinal("M11"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("M11"))),
                            Total = r.IsDBNull(r.GetOrdinal("Total"))
                                ? 0
                                : Convert.ToInt32(r.GetValue(r.GetOrdinal("Total")))
                        });
                    }
                }
            }

            return result;
        }

		public IEnumerable<Model_CombineShippingList> GetCombineShippingList()
        {
			var result = new List<Model_CombineShippingList>();

			string sqlPath = Path.Combine(
				_env.ContentRootPath,
				"SQL",
				"CombineShippingList",
				"GetShippingList.sql"
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
                            result.Add(new Model_CombineShippingList
                            {
                                PONo = reader["PONo"] as string ?? "",
                                PODate = reader.IsDBNull(reader.GetOrdinal("PODate")) ? (DateTime?)null :
                                        reader.GetDateTime(reader.GetOrdinal("PODate")),
                                Status = reader["Status"] as string ?? "",
                                ItemCode = reader["ItemCode"] as string ?? "",
                                Description = reader["Description"] as string ?? "",
                                Whse = reader["Whse"] as string ?? "",
                                QtyOrdered = reader.IsDBNull(reader.GetOrdinal("QtyOrdered")) ? (decimal?)null :
                                            reader.GetDecimal(reader.GetOrdinal("QtyOrdered")),
                                QtyRcpt = reader.IsDBNull(reader.GetOrdinal("QtyRcpt")) ? (decimal?)null :
                                            reader.GetDecimal(reader.GetOrdinal("QtyRcpt")),
                                QtyOpen = reader.IsDBNull(reader.GetOrdinal("QtyOpen")) ? (decimal?)null :
                                            reader.GetDecimal(reader.GetOrdinal("QtyOpen")),
                                QtyInvoiced = reader.IsDBNull(reader.GetOrdinal("QtyInvoiced")) ? (decimal?)null :
                                            reader.GetDecimal(reader.GetOrdinal("QtyInvoiced")),
                                UnitCost = reader.IsDBNull(reader.GetOrdinal("UnitCost")) ? (decimal?)null :
                                            reader.GetDecimal(reader.GetOrdinal("UnitCost")),
                                LastTotalUnitCost = reader.IsDBNull(reader.GetOrdinal("LastTotalUnitCost")) ? (decimal?)null :
                                            reader.GetDecimal(reader.GetOrdinal("LastTotalUnitCost")),
                                StandardUnitCost = reader.IsDBNull(reader.GetOrdinal("StandardUnitCost")) ? (decimal?)null :
                                            reader.GetDecimal(reader.GetOrdinal("StandardUnitCost")),
                                RequiredDate = reader.IsDBNull(reader.GetOrdinal("RequiredDate")) ? (DateTime?)null :
                                            reader.GetDateTime(reader.GetOrdinal("RequiredDate")),
                                Category = reader["Category"] as string ?? "",
                                SalesOrderNo = reader["SalesOrderNo"] as string ?? "",
                                Customer = reader["Customer"] as string ?? "",
                                CustomerName = reader["CustomerName"] as string ?? "",
                                SOEntryDate = reader.IsDBNull(reader.GetOrdinal("SOEntryDate")) ? (DateTime?)null :
                                            reader.GetDateTime(reader.GetOrdinal("SOEntryDate")),
                                SOEntryUser = reader["SOEntryUser"] as string ?? "",
                            });
                        }
                    }
                }
			}

			return result;
		}

	}
}
