using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using Microsoft.Data.SqlClient;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_OrderForExport
	{
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_OrderForExport(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public IEnumerable<Model_OrderForExport> GetOrderData(String salesOrderNo)
        {
            var result = new List<Model_OrderForExport>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
				"OrderForExport",
                "GetOrderData.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (salesOrderNo != null)
                    {
						cmd.Parameters.AddWithValue("@SalesOrderNo", salesOrderNo);
                    }
                    else
                    {
						cmd.Parameters.AddWithValue("@SalesOrderNo", DBNull.Value);
					}
					using (var reader = cmd.ExecuteReader())
					{
						while (reader.Read())
						{
							result.Add(new Model_OrderForExport
							{
								SalesOffice = reader["SalesOffice"] as string ?? "",
								SalesOrderNo = reader["SalesOrderNo"] as string ?? "",
								OrderDate = reader.IsDBNull(reader.GetOrdinal("OrderDate"))
										? null
										: reader.GetDateTime(reader.GetOrdinal("OrderDate")),
								OrderType = reader["OrderType"] as string ?? "",
								OrderStatus = reader["OrderStatus"] as string ?? "",
								CustomerPONo = reader["CustomerPONo"] as string ?? "",
								CustomerNo = reader["CustomerNo"] as string ?? "",
								BillToName = reader["BillToName"] as string ?? "",
								ShipToCity = reader["ShipToCity"] as string ?? "",
								ShipVia = reader["ShipVia"] as string ?? "",
								HeaderComment = reader["HeaderComment"] as string ?? "",
								CustPO_Ln = reader["CustPO_Ln"] as string ?? "",
								ItemCode = reader["ItemCode"] as string ?? "",
								ItemDescription = reader["ItemDescription"] as string ?? "",
								AliasItemNo = reader["AliasItemNo"] as string ?? "",
								Whs = reader["Whs"] as string ?? "",
								Weight = reader["Weight"] as string ?? "",
								Ordded = reader.IsDBNull(reader.GetOrdinal("#Ordded"))
										? null
										: reader.GetDecimal(reader.GetOrdinal("#Ordded")),

								Shipped = reader.IsDBNull(reader.GetOrdinal("#Shipped"))
										? null
										: reader.GetDecimal(reader.GetOrdinal("#Shipped")),
								BO = reader.IsDBNull(reader.GetOrdinal("#BO"))
										? null
										: reader.GetDecimal(reader.GetOrdinal("#BO")),
								UnitPrice = reader.IsDBNull(reader.GetOrdinal("UnitPrice"))
										? null
										: reader.GetDecimal(reader.GetOrdinal("UnitPrice")),
								ExtensionAmt = reader.IsDBNull(reader.GetOrdinal("ExtensionAmt"))
										? null
										: reader.GetDecimal(reader.GetOrdinal("ExtensionAmt")),
								ReqDate = reader.IsDBNull(reader.GetOrdinal("ReqDate"))
										? null
										: reader.GetDateTime(reader.GetOrdinal("ReqDate")),
								PushOut = reader.IsDBNull(reader.GetOrdinal("PushOut"))
										? null
										: reader.GetDateTime(reader.GetOrdinal("PushOut")),
								PromiseDate = reader.IsDBNull(reader.GetOrdinal("PromiseDate"))
										? null
										: reader.GetDateTime(reader.GetOrdinal("PromiseDate")),
								CommitDate = reader.IsDBNull(reader.GetOrdinal("CommitDate"))
										? null
										: reader.GetDateTime(reader.GetOrdinal("CommitDate")),
								DeliveryDate = reader.IsDBNull(reader.GetOrdinal("DeliveryDate"))
										? null
										: reader.GetDateTime(reader.GetOrdinal("DeliveryDate")),
								CommentText = reader["CommentText"] as string ?? "",
								UnitCost = reader.IsDBNull(reader.GetOrdinal("UnitCost"))
										? null
										: reader.GetDecimal(reader.GetOrdinal("UnitCost")),
								PurchaseOrderNo = reader["PurchaseOrderNo"] as string ?? "",
								Udf_custpono = reader["UDF_CUSTPONO"] as string ?? "",
								InternalNotes = reader["InternalNotes"] as string ?? "",
							});
						}
					}
				}
            }

            return result;
        }
    }
}
