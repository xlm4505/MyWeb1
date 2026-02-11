using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace PurchaseSalesManagementSystem.Common
{
    public class CreateConnection
    {
        private readonly string _connectionString;

        public CreateConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MyDB");
        }

        public SqlConnection GetConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
