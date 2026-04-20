using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using System.Data;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_MonthlySalesSummary
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_MonthlySalesSummary(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public List<int> GetTargetYears()
        {
            var years = new List<int>();
            var sql = LoadSql("GetYear.sql");

            using var conn = _connectionFactory.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                years.Add(Convert.ToInt32(reader["YearNum"]));
            }

            return years;
        }

        public DataTable GetMonthlySalesSummary(int targetYear)
        {
            var sql = LoadSql("MonthlySalesSummary.sql");

            return ExecuteDataTable(sql, targetYear);
        }

        public DataTable GetMonthlySalesAndPurchasesReport(int targetYear)
        {
            var sql = LoadSql("MonthlySalesSummaryAndPurchasesReport.sql");

            return ExecuteDataTable(sql, targetYear);
        }

        private string LoadSql(string fileName)
        {
            var sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "MonthlySalesSummaryAndPurchasesReport",
                fileName
            );

            return File.ReadAllText(sqlPath);
        }

        private DataTable ExecuteDataTable(string sql, int? targetYear = null)
        {
            var dt = new DataTable();

            using var conn = _connectionFactory.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 300;
            if (targetYear.HasValue)
            {
                cmd.Parameters.AddWithValue("@YYYY", targetYear.Value);
            }
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);

            return dt;
        }
    }
}