using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_MF_SalesPerson
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_MF_SalesPerson(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_SalesPerson> GetSalesPersons(string? customerCode)
        {
            var list = new List<Model_SalesPerson>();
            var sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "MF_SalesPerson",
                "GetSalesPersons.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection("FUJIKINDB"))
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@CustomerCode", customerCode?.Trim() ?? string.Empty);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Model_SalesPerson
                            {
                                CustomerCode = reader["CustomerCode"] as string ?? string.Empty,
                                SalesPerson = reader["SalesPerson"] as string ?? string.Empty
                            });
                        }
                    }
                }
            }

            return list;
        }

        public int UpdateSalesPersons(IEnumerable<Model_SalesPerson> items)
        {
            if (items == null || !items.Any())
            {
                return 0;
            }

            var sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "MF_SalesPerson",
                "UpdateSalesPerson.sql"
            );

            var sql = File.ReadAllText(sqlPath);
            var affectedRows = 0;

            using (var conn = _connectionFactory.GetConnection("FUJIKINDB"))
            {
                conn.Open();

                foreach (var item in items)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerCode", item.CustomerCode ?? string.Empty);
                        cmd.Parameters.AddWithValue("@SalesPerson", item.SalesPerson ?? string.Empty);

                        affectedRows += cmd.ExecuteNonQuery();
                    }
                }
            }

            return affectedRows;
        }

        public int DeleteSalesPersons(IEnumerable<Model_SalesPerson> items)
        {
            if (items == null || !items.Any())
            {
                return 0;
            }

            var sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "MF_SalesPerson",
                "DeleteSalesPerson.sql"
            );

            var sql = File.ReadAllText(sqlPath);
            var affectedRows = 0;

            using (var conn = _connectionFactory.GetConnection("FUJIKINDB"))
            {
                conn.Open();

                foreach (var item in items)
                {
                    using (var cmd = new SqlCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@CustomerCode", item.CustomerCode ?? string.Empty);
                        affectedRows += cmd.ExecuteNonQuery();
                    }
                }
            }

            return affectedRows;
        }
    }
}