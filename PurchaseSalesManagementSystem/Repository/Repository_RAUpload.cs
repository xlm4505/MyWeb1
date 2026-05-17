using System.Data;
using Microsoft.Data.SqlClient;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_RAUpload
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_RAUpload(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public void DeleteRAInventory()
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "RAUpload",
                "DeleteRAInventory.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();
                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public void InsertRAInventoryBulk(List<RAUpload_Insert> rows)
        {
            var dt = new DataTable();
            dt.Columns.Add("EntryDate", typeof(DateTime));
            dt.Columns.Add("WarehouseCode", typeof(string));
            dt.Columns.Add("ItemCode", typeof(string));
            dt.Columns.Add("Description", typeof(string));
            dt.Columns.Add("OriginalQty", typeof(decimal));
            dt.Columns.Add("Qty", typeof(decimal));
            dt.Columns.Add("InvoiceNo", typeof(string));
            dt.Columns.Add("Box", typeof(string));
            dt.Columns.Add("Weight", typeof(decimal));
            dt.Columns.Add("DateReceived", typeof(DateTime));
            dt.Columns.Add("From", typeof(string));
            dt.Columns.Add("VantecRef#", typeof(string));
            dt.Columns.Add("UnitPrice", typeof(decimal));
            dt.Columns.Add("ShipMark", typeof(string));
            dt.Columns.Add("Comment", typeof(string));

            foreach (var r in rows)
            {
                dt.Rows.Add(
                    r.EntryDate,
                    r.WarehouseCode,
                    r.ItemCode,
                    r.Description,
                    r.OriginalQty,
                    r.Qty,
                    r.InvoiceNo,
                    r.Box,
                    r.Weight,
                    r.DateReceived,
                    r.From,
                    r.VantecRef,
                    r.UnitPrice,
                    r.ShipMark,
                    r.Comment
                );
            }

            using var conn = _connectionFactory.GetConnection();
            conn.Open();
            using var bulk = new SqlBulkCopy(conn)
            {
                DestinationTableName = "U_RAInventory",
                BulkCopyTimeout = 300,
            };
            bulk.ColumnMappings.Add("EntryDate", "EntryDate");
            bulk.ColumnMappings.Add("WarehouseCode", "WarehouseCode");
            bulk.ColumnMappings.Add("ItemCode", "ItemCode");
            bulk.ColumnMappings.Add("Description", "Description");
            bulk.ColumnMappings.Add("OriginalQty", "OriginalQty");
            bulk.ColumnMappings.Add("Qty", "Qty");
            bulk.ColumnMappings.Add("InvoiceNo", "InvoiceNo");
            bulk.ColumnMappings.Add("Box", "Box");
            bulk.ColumnMappings.Add("Weight", "Weight");
            bulk.ColumnMappings.Add("DateReceived", "DateReceived");
            bulk.ColumnMappings.Add("From", "From");
            bulk.ColumnMappings.Add("VantecRef#", "VantecRef#");
            bulk.ColumnMappings.Add("UnitPrice", "UnitPrice");
            bulk.ColumnMappings.Add("ShipMark", "ShipMark");
            bulk.ColumnMappings.Add("Comment", "Comment");
            bulk.WriteToServer(dt);
        }

        public IEnumerable<RAUpload_ExportToExcel> GetDownloadData()
        {
            var result = new List<RAUpload_ExportToExcel>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "RAUpload",
                "GetRAInventory.sql"
              );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;

                    using (var reader = cmd.ExecuteReader())
                    {
                        int ordItemCode = reader.GetOrdinal("ItemCode");
                        int ordItemDesc = reader.GetOrdinal("ItemDesc");
                        int ordJFI = reader.GetOrdinal("JFI");
                        int ordNAL = reader.GetOrdinal("NAL");
                        int ordNCA = reader.GetOrdinal("NCA");
                        int ordNTX = reader.GetOrdinal("NTX");
                        int ordUTX = reader.GetOrdinal("UTX");
                        int ordUGP = reader.GetOrdinal("UGP");
                        int ordIFS = reader.GetOrdinal("IFS");
                        int ordNNJ = reader.GetOrdinal("NNJ");
                        int ordXIT = reader.GetOrdinal("XIT");
                        int ordTotal = reader.GetOrdinal("Total");

                        while (reader.Read())
                        {
                            result.Add(new RAUpload_ExportToExcel
                            {
                                ItemCode = reader.IsDBNull(ordItemCode) ? "" : (reader.GetString(ordItemCode) ?? ""),
                                ItemDesc = reader.IsDBNull(ordItemDesc) ? "" : (reader.GetString(ordItemDesc) ?? ""),
                                JFI = reader.IsDBNull(ordJFI) ? 0 : (int)reader.GetDecimal(ordJFI),
                                NAL = reader.IsDBNull(ordNAL) ? 0 : (int)reader.GetDecimal(ordNAL),
                                NCA = reader.IsDBNull(ordNCA) ? 0 : (int)reader.GetDecimal(ordNCA),
                                NTX = reader.IsDBNull(ordNTX) ? 0 : (int)reader.GetDecimal(ordNTX),
                                UTX = reader.IsDBNull(ordUTX) ? 0 : (int)reader.GetDecimal(ordUTX),
                                UGP = reader.IsDBNull(ordUGP) ? 0 : (int)reader.GetDecimal(ordUGP),
                                IFS = reader.IsDBNull(ordIFS) ? 0 : (int)reader.GetDecimal(ordIFS),
                                NNJ = reader.IsDBNull(ordNNJ) ? 0 : (int)reader.GetDecimal(ordNNJ),
                                XIT = reader.IsDBNull(ordXIT) ? 0 : (int)reader.GetDecimal(ordXIT),
                                Total = reader.IsDBNull(ordTotal) ? 0 : (int)reader.GetDecimal(ordTotal),
                            });
                        }
                    }
                }
            }

            return result;
        }
    }
}
