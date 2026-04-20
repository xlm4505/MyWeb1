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

        public DataTable GetMonthlySalesSummary(int targetYear, bool allData)
        {
            var fileName = allData ? "MonthlySalesSummaryAll.sql" : "MonthlySalesSummary.sql";
            var sql = LoadSql(fileName);

            // SQL内で固定になっている対象年(2025)を選択年に置換
            sql = sql.Replace("2025", targetYear.ToString());

            return ExecuteDataTable(sql);
        }

        public DataTable GetMonthlySalesAndPurchasesReport(int targetYear)
        {
            var sql = LoadSql("MonthlySalesSummaryAndPurchasesReport.sql");

            // SQL内で固定になっている対象年(2023)を選択年に置換
            sql = sql.Replace("2023", targetYear.ToString());

            return ExecuteDataTable(sql);
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

        private DataTable ExecuteDataTable(string sql)
        {
            var dt = new DataTable();

            using var conn = _connectionFactory.GetConnection();
            conn.Open();
            using var cmd = new SqlCommand(sql, conn);
            cmd.CommandTimeout = 300;
            using var adapter = new SqlDataAdapter(cmd);
            adapter.Fill(dt);

            return dt;
        }
    }
}