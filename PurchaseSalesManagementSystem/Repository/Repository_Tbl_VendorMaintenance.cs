using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_Tbl_VendorMaintenance
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_Tbl_VendorMaintenance(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_Tbl_VendorMaintenance> GetVendors(string? vendorNo)
        {
            var list = new List<Model_Tbl_VendorMaintenance>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "Tbl_VendorMaintenance",
                "GetVendors.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@VendorNo", vendorNo?.Trim() ?? string.Empty);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Model_Tbl_VendorMaintenance
                            {
                                APDivisionNo = reader["APDivisionNo"] as string ?? string.Empty,
                                VendorNo = reader["VendorNo"] as string ?? string.Empty,
                                VendorName = reader["VendorName"] as string ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public int UpdateVendors(IEnumerable<Model_Tbl_VendorMaintenance> items)
        {
            if (items == null || !items.Any())
            {
                return 0;
            }

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "Tbl_VendorMaintenance",
                "UpdateVendor.sql"
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
                        cmd.Parameters.AddWithValue("@VendorName", item.VendorName ?? string.Empty);
                        cmd.Parameters.AddWithValue("@APDivisionNo", item.APDivisionNo ?? string.Empty);
                        cmd.Parameters.AddWithValue("@VendorNo", item.VendorNo ?? string.Empty);

                        affectedRows += cmd.ExecuteNonQuery();
                    }
                }
            }

            return affectedRows;
        }

        public int DeleteVendors(IEnumerable<Model_Tbl_VendorMaintenance> items)
        {
            if (items == null || !items.Any())
            {
                return 0;
            }

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "Tbl_VendorMaintenance",
                "DeleteVendor.sql"
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
                        cmd.Parameters.AddWithValue("@APDivisionNo", item.APDivisionNo ?? string.Empty);
                        cmd.Parameters.AddWithValue("@VendorNo", item.VendorNo ?? string.Empty);

                        affectedRows += cmd.ExecuteNonQuery();
                    }
                }
            }

            return affectedRows;
        }
    }
}