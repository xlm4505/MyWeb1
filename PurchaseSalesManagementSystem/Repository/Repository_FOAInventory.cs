using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_FOAInventory
    {
        private readonly IWebHostEnvironment _env;
        private readonly CreateConnection _connectionFactory;

        public Repository_FOAInventory(IWebHostEnvironment env, CreateConnection connectionFactory)
        {
            _env = env;
            _connectionFactory = connectionFactory;
        }

        public List<string> GetWareHouseList()
        {
            var sqlPath = Path.Combine(_env.ContentRootPath, "SQL", "FOAInventory", "GetWareHouseList.sql");
            var sql = File.ReadAllText(sqlPath);
            var result = new List<string>();

            using var conn = _connectionFactory.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader["WarehouseCode"] as string ?? "");
            }
            return result;
        }

        public List<Model_FOAInventory> GetFOAInventory(string? itemCode, string? wareHouse, bool minusOnly)
        {
            var sqlPath = Path.Combine(_env.ContentRootPath, "SQL", "FOAInventory", "GetFOAInventory.sql");
            var sql = File.ReadAllText(sqlPath);
            var result = new List<Model_FOAInventory>();

            using var conn = _connectionFactory.GetConnection();
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ItemCode", itemCode?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@WareHouse", wareHouse?.Trim() ?? string.Empty);
            cmd.Parameters.AddWithValue("@MinusOnly", minusOnly ? 1 : 0);

            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new Model_FOAInventory
                {
                    ProdLn            = reader["ProdLn"] as string ?? "",
                    ItemCode          = reader["ItemCode"] as string ?? "",
                    ItemCodeDesc      = reader["ItemCodeDesc"] as string ?? "",
                    WHSE              = reader["WHSE"] as string ?? "",
                    OnHand            = reader["OnHand"] != DBNull.Value ? (decimal)reader["OnHand"] : 0,
                    QtyPO             = reader["Qty PO"] != DBNull.Value ? (decimal)reader["Qty PO"] : 0,
                    StandardUnitCost  = reader["StandardUnitCost"] != DBNull.Value ? (decimal)reader["StandardUnitCost"] : 0,
                    LastSoldDate      = reader["LastSoldDate"] as string ?? "",
                    LastReceiptDate   = reader["LastReceiptDate"] as string ?? "",
                    LastTotalUnitCost = reader["LastTotalUnitCost"] != DBNull.Value ? (decimal)reader["LastTotalUnitCost"] : 0,
                });
            }
            return result;
        }
    }
}
