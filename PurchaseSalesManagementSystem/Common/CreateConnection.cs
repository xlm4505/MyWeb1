using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PurchaseSalesManagementSystem.Common
{
    public class CreateConnection
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;

        public CreateConnection(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = configuration.GetConnectionString("MyDB");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }

        public SqlConnection GetConnection(string connectionName)
        {
            var connStr = _configuration.GetConnectionString(connectionName);
            return new SqlConnection(connStr);
        }

    }
}
