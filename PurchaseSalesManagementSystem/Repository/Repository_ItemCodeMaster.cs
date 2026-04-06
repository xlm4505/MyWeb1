using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using Microsoft.Data.SqlClient;
using System.Globalization;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_ItemCodeMaster
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_ItemCodeMaster(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        // ★ decimal を安全に読み取る関数（string / float / int / decimal 全対応）
        private decimal? GetDecimalSafe(SqlDataReader reader, string column)
        {
            int idx = reader.GetOrdinal(column);

            if (reader.IsDBNull(idx))
                return null;

            object value = reader.GetValue(idx);

            return value switch
            {
                decimal d => d,
                double db => (decimal)db,
                float fl => (decimal)fl,
                int i => i,
                long l => l,

                string s => ParseDecimalString(s),

                _ => null
            };
        }

        private decimal? ParseDecimalString(string s)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;

            s = s.Trim().Replace(",", "");

            if (decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
                return result;

            return null;
        }

        public IEnumerable<Model_ItemCodeMaster> GetItemCodeMaster(string ItemCode, bool excludeInactive)
        {
            var result = new List<Model_ItemCodeMaster>();

            string sqlPath = "";
            if (excludeInactive)
            {
                sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "ItemCodeMaster",
                "GetItemCodeMaster_ActiveOnly.sql");
            }
            else {
                sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "ItemCodeMaster",
                "GetItemCodeMaster_ALL.sql");
            }

                //string sqlPath = Path.Combine(
                //    _env.ContentRootPath,
                //    "SQL",
                //    "ItemCodeMaster",
                //    "GetItemCodeMaster.sql"
                //);

                var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;

                    cmd.Parameters.AddWithValue("@ItemCode",
                        string.IsNullOrEmpty(ItemCode) ? DBNull.Value : ItemCode);
                    cmd.Parameters.AddWithValue("@inactiveFlg", excludeInactive ? 1 : 0);


                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Model_ItemCodeMaster
                            {
                                ItemCode = reader["ItemCode"] as string ?? "",
                                ItemDesc = reader["ItemDesc"] as string ?? "",
                                ItemDesc2 = reader["ItemDesc2"] as string ?? "",
                                Category = reader["Category"] as string ?? "",
                                ProductLineDesc = reader["ProductLineDesc"] as string ?? "",
                                ProductType = reader["ProductType"] as string ?? "",
                                Inactive = reader["Inactive"] as string ?? "",

                                // ★ Weight(lb) は string の可能性が高い → Safe 変換
                                Weight = GetDecimalSafe(reader, "Weight(lb)"),

                                Whse = reader["Whse"] as string ?? "",
                                PrimaryVendor = reader["PrimaryVendor"] as string ?? "",
                                QtyDisc = reader["QtyDisc"] as string ?? "",

                                StdSalesPrice = GetDecimalSafe(reader, "StdSalesPrice"),
                                StdUnitCost = GetDecimalSafe(reader, "StdUnitCost"),
                                LastCost = GetDecimalSafe(reader, "LastCost"),
                                AvgCost = GetDecimalSafe(reader, "AvgCost"),

                                VenCost_USD_ = GetDecimalSafe(reader, "VenCost(USD)"),
                                VenCost_JPY = GetDecimalSafe(reader, "VenCost(JPY)"),

                                OnHand = GetDecimalSafe(reader, "OnHand"),
                                OpenSO = GetDecimalSafe(reader, "OpenSO"),
                                Available = GetDecimalSafe(reader, "Available"),
                                OpenPO = GetDecimalSafe(reader, "OpenPO"),
                                InShip = GetDecimalSafe(reader, "(InShip)"),

                                OnHand_ = GetDecimalSafe(reader, "OnHand "),
                                OpenSO_ = GetDecimalSafe(reader, "OpenSO "),
                                Available_ = GetDecimalSafe(reader, "Available "),
                                OpenPO_ = GetDecimalSafe(reader, "OpenPO "),
                                InShip_ = GetDecimalSafe(reader, "(InShip) "),

                                LastSold = reader.IsDBNull(reader.GetOrdinal("LastSold"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("LastSold")),

                                LastReceipt = reader.IsDBNull(reader.GetOrdinal("LastReceipt"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("LastReceipt")),

                                ExtendedDescriptionText = reader["ExtendedDescriptionText"] as string ?? "",

                                DateCreated = reader.IsDBNull(reader.GetOrdinal("DateCreated"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("DateCreated")),

                                UserCreated = reader["UserCreated"] as string ?? "",

                                DateUpdated = reader.IsDBNull(reader.GetOrdinal("DateUpdated"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("DateUpdated")),

                                UserUpdated = reader["UserUpdated"] as string ?? "",

                                ListCOP = GetDecimalSafe(reader, "List COP"),
                                Standard = GetDecimalSafe(reader, "Standard"),
                                Discount = GetDecimalSafe(reader, "Discount"),
                                Class4 = GetDecimalSafe(reader, "Class 4"),
                                Class5 = GetDecimalSafe(reader, "Class 5"),
                                Contract = GetDecimalSafe(reader, "Contract"),
                                Class6 = GetDecimalSafe(reader, "Class 6")
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
