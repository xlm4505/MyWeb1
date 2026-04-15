using System.Data;
using PurchaseSalesManagementSystem.Common;
using Microsoft.Data.SqlClient;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_KatsuoUploadCheck
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_KatsuoUploadCheck(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public void DeleteUKatsuoDummy()
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "KatsuoUploadCheck",
                "DeleteUKatsuoDummy.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertUKatsuo(DataRow row)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "KatsuoUploadCheck",
                "InsertUKatsuo.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@PurchaseOrderNo", row["PurchaseOrderNo"]);
                    cmd.Parameters.AddWithValue("@PromiseDate",     row["PromiseDate"] == DBNull.Value ? DBNull.Value : row["PromiseDate"]);
                    cmd.Parameters.AddWithValue("@ItemCode",        row["ItemCode"]);
                    cmd.Parameters.AddWithValue("@OpenQty",         row["OpenQty"]);
                    cmd.Parameters.AddWithValue("@DummyFlag",       row["DummyFlag"]);
                    cmd.Parameters.AddWithValue("@IssueDate",       row["IssueDate"] == DBNull.Value ? DBNull.Value : row["IssueDate"]);
                    cmd.Parameters.AddWithValue("@CreateDate",      row["CreateDate"]);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void DeleteUKatsuoByPurchaseOrderNos(IEnumerable<string> purchaseOrderNos)
        {
            var poList = purchaseOrderNos.ToList();
            if (!poList.Any()) return;

            var paramNames = poList.Select((_, i) => $"@p{i}");
            var sql = $"DELETE FROM U_Katsuo WHERE PurchaseOrderNo IN ({string.Join(", ", paramNames)})";

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    for (int i = 0; i < poList.Count; i++)
                        cmd.Parameters.AddWithValue($"@p{i}", poList[i]);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertUKatsuoDummy(string purchaseOrderNo)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "KatsuoUploadCheck",
                "InsertUKatsuoDummy.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@PurchaseOrderNo", purchaseOrderNo);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public DataTable GetCheckList(bool isValves)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "KatsuoUploadCheck",
                "GetCheckList.sql"
            );

            var sql = File.ReadAllText(sqlPath);
            var dt = new DataTable();

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@IsValves", isValves ? 1 : 0);

                    using (var reader = cmd.ExecuteReader())
                        dt.Load(reader);
                }
            }

            return dt;
        }

        public void UpdateIssueDate(string userName, DateTime? issueDate)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "KatsuoUploadCheck",
                "UpdateIssueDate.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection("FUJIKINDB"))
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@IssueDate", (object?)issueDate ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
