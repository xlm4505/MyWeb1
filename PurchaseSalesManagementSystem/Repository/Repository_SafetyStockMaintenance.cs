using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using System.Data;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_SafetyStockMaintenance
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_SafetyStockMaintenance(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_SafetyStockMaintenance> GetForecastItems(string? itemCode)
        {
            var list = new List<Model_SafetyStockMaintenance>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "SafetyStockMaintenance",
                "GetForecastItems.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@ItemCode", itemCode?.Trim() ?? string.Empty);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Model_SafetyStockMaintenance
                            {
                                ItemCode = reader["ItemCode"] as string ?? string.Empty,
                                ProcType = reader["ProcType"] as string ?? string.Empty,
                                ARDivisionNo = reader["ARDivisionNo"] as string ?? string.Empty,
                                CustomerNo = reader["CustomerNo"] as string ?? string.Empty,
                                WarehouseCode = reader["WarehouseCode"] as string ?? string.Empty,
                                Quantity = reader.IsDBNull(reader.GetOrdinal("Quantity"))
                                    ? 0
                                    : Convert.ToDecimal(reader["Quantity"]),
                                ItemNo = reader["ItemNo"] as string ?? string.Empty,
                                Comment = reader["Comment"] as string ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public int UpdateForecastItems(IEnumerable<Model_SafetyStockMaintenance> items)
        {
            if (items == null || !items.Any())
            {
                return 0;
            }

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "SafetyStockMaintenance",
                "UpdateForecastItem.sql"
            );

            var sql = File.ReadAllText(sqlPath);
            var affectedRows = 0;

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                foreach (var item in items)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                        cmd.Parameters.AddWithValue("@Comment", item.Comment ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ItemCode", item.ItemCode ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ProcType", item.ProcType ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ARDivisionNo", item.ARDivisionNo ?? string.Empty);
                        cmd.Parameters.AddWithValue("@CustomerNo", item.CustomerNo ?? string.Empty);
                        cmd.Parameters.AddWithValue("@WarehouseCode", item.WarehouseCode ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ItemNo", item.ItemNo ?? string.Empty);

                        affectedRows += cmd.ExecuteNonQuery();
                    }
                }
            }

            return affectedRows;
        }

        public int DeleteForecastItems(IEnumerable<Model_SafetyStockMaintenance> items)
        {
            if (items == null || !items.Any())
            {
                return 0;
            }

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "SafetyStockMaintenance",
                "DeleteForecastItem.sql"
            );

            var sql = File.ReadAllText(sqlPath);
            var affectedRows = 0;

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                foreach (var item in items)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@ItemCode", item.ItemCode ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ProcType", item.ProcType ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ARDivisionNo", item.ARDivisionNo ?? string.Empty);
                        cmd.Parameters.AddWithValue("@CustomerNo", item.CustomerNo ?? string.Empty);
                        cmd.Parameters.AddWithValue("@WarehouseCode", item.WarehouseCode ?? string.Empty);
                        cmd.Parameters.AddWithValue("@ItemNo", item.ItemNo ?? string.Empty);

                        affectedRows += cmd.ExecuteNonQuery();
                    }
                }
            }

            return affectedRows;
        }

    }
}
