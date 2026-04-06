using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_KatsuoIssueDate
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_KatsuoIssueDate(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_KatsuoIssueDate> GetKatsuoIssueDateData(string? userName)
        {
            var result = new List<Model_KatsuoIssueDate>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "KatsuoIssueDate",
                "GetKatsuoIssueDateData.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection("FUJIKINDB"))
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;

                    if (userName != null && userName != "")
                    {
                        cmd.Parameters.AddWithValue("@UserName", userName);
                    }
                    else
                    {
                        cmd.Parameters.AddWithValue("@UserName", DBNull.Value);
                    }

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            result.Add(new Model_KatsuoIssueDate
                            {
                                UserName = reader["UserName"] as string ?? "",
                                ID = reader.IsDBNull(reader.GetOrdinal("ID"))
                                    ? null
                                    : reader.GetInt32(reader.GetOrdinal("ID")),
                                IssueDate = reader.IsDBNull(reader.GetOrdinal("IssueDate"))
                                    ? null
                                    : reader.GetDateTime(reader.GetOrdinal("IssueDate")),
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
