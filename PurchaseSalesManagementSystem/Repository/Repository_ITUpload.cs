using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using DocumentFormat.OpenXml.Office2016.Drawing.ChartDrawing;
using Microsoft.Data.SqlClient;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using PurchaseSalesManagementSystem.Common;
using PurchaseSalesManagementSystem.Models;
using System.Net;
using System.Net.Sockets;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_ITUpload
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;
        private readonly string _indent = "&nbsp;&nbsp;&nbsp;";

        public Repository_ITUpload(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }


        public (int, List<string>) Upload(IFormFile excelFile, string userName, string ipAddress)
        {
            List<string> errorList = new List<string>();

            int insertCount = 0;

            // IPアドレスのチェック
            if (string.IsNullOrEmpty(ipAddress))
            {
                errorList.Add("Failed to obtain an IP address.");
                return (insertCount, errorList);
            }

            // ファイルチェック
            var fileName = Path.GetFileName(excelFile.FileName);
            if (!".xlsx".Equals(Path.GetExtension(fileName)))
            {
                errorList.Add("Please select the IT Upload file (Excel).");
                return (insertCount, errorList);
            }

            // データのInsert
            try
            {
                UpdateU_UploadITData(ipAddress);
            }
            catch (Exception e)
            {
                errorList.Add("Error!: Unable to connect to the database.\r\nError message: " + e.Message);
                return (insertCount, errorList);
            }

            // Excel を読み込み
            var uploadData = new List<Model_ITUpload>();

            using (var stream = new MemoryStream())
            {
                excelFile.CopyTo(stream);
                stream.Position = 0;

                using (var workbook = new XLWorkbook(stream))
                {
                    var ws = workbook.Worksheets.First();
                    var lastRow = ws.LastRowUsed()?.RowNumber() ?? 0;

                    for (int r = 5; r <= lastRow; r++)
                    {
                        var date = ws.Cell(r, 1).GetValue<string>();   // A
                        var invoiceNo = ws.Cell(r, 2).GetValue<string>();   // B
                        var itemCode = ws.Cell(r, 10).GetValue<string>();   // J
                        var qty = ws.Cell(r, 11).GetValue<string>();  // K
                        var fromWH = ws.Cell(r, 12).GetValue<string>(); // L
                        var toWH = ws.Cell(r, 13).GetValue<string>(); // M
                        var memo = ws.Cell(r, 15).GetValue<string>(); // O

                        // 必須対象列が空の行はスキップ
                        if (string.IsNullOrWhiteSpace(date) ||
                            string.IsNullOrWhiteSpace(invoiceNo) ||
                            string.IsNullOrWhiteSpace(itemCode) ||
                            string.IsNullOrWhiteSpace(qty) ||
                            string.IsNullOrWhiteSpace(fromWH) ||
                            string.IsNullOrWhiteSpace(toWH))
                        {
                            continue;
                        }

                        // Qtyのチェック
                        int excelQty = 0;
                        try
                        {
                            excelQty = int.Parse(qty);
                        }
                        catch (Exception e)
                        {
                            // 数値が入力されていない場合スキップ
                            continue;
                        }

                        uploadData.Add(new Model_ITUpload
                        {
                            Date = date,
                            InvoiceNo = invoiceNo,
                            ItemCode = itemCode,
                            Qty = qty,
                            FromWH = fromWH,
                            ToWH = toWH,
                            Memo = memo
                        });

                        // From W/Hのチェック
                        List<Model_ITUpload_FromWH> fromWHList;
                        try
                        {
                            fromWHList = CheckWarehouseFrom(fromWH, toWH, itemCode);
                        }
                        catch (Exception e)
                        {
                            errorList.Add("Error!: Unable to connect to the database.\r\nError message: " + e.Message);
                            return (insertCount, errorList);
                        }
                        if (fromWHList.Count <= 0)
                        {
                            errorList.Add("Error!: Wrong Warehouse (from).\r\n" +
                                _indent + "ItemCode: " + itemCode + "\r\n" +
                                _indent + "WhseCode: " + fromWH);
                            continue;
                        }

                        // Qtyのチェック                   
                        if (int.Parse(qty) > fromWHList[0].QuantityOnhand)
                        {
                            errorList.Add("Error!: No sufficient inventory.\r\n" +
                                _indent + "ItemCode: " + itemCode + "\r\n" +
                                _indent + "TranQty: " + qty + "\r\n" +
                                _indent + "OnHand: " + fromWHList[0].QuantityOnhand);
                            continue;
                        }

                        // データのInsert
                        try
                        {
                            InsertU_UploadITData(fromWH, toWH, itemCode, qty, memo, userName, ipAddress);
                            insertCount++;
                        }
                        catch (Exception e)
                        {
                            errorList.Add("Error!: Unable to connect to the database.\r\nError message: " + e.Message);
                            return (insertCount, errorList);
                        }
                    }
                }
            }

            // 有効なUploadデータ無し
            if (uploadData.Count <= 0 && errorList.Count <= 0)
            {
                errorList.Add("Error!: There's no IT data to upload.");
                return (insertCount, errorList);
            }



            return (insertCount, errorList);
        }

        private string GetIPv4Address()
        {
            string result = string.Empty;
            foreach (IPAddress ipaddress in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ipaddress.AddressFamily == AddressFamily.InterNetwork)
                {
                    result = ipaddress.ToString();
                }
            }
            return result;
        }

        public void UpdateU_UploadITData(string ipAddress)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "ITUpload",
                "UpdateU_UploadITData.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@IpAddress", ipAddress);
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public List<Model_ITUpload_FromWH> CheckWarehouseFrom(string fromWH, string toWH, string itemCode)
        {
            var list = new List<Model_ITUpload_FromWH>();

            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "ITUpload",
                "CheckWarehouseFrom.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@FromWH", fromWH);
                    cmd.Parameters.AddWithValue("@ToWH", toWH);
                    cmd.Parameters.AddWithValue("@ItemCode", itemCode);

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            list.Add(new Model_ITUpload_FromWH
                            {
                                QuantityOnhand = reader["QuantityOnhand"] == DBNull.Value ? 0 : Convert.ToInt32(reader["QuantityOnhand"]),
                                WarehouseCode = reader["WarehouseCode"] as string ?? ""
                            });
                        }
                    }
                }
            }

            return list;
        }

        public void InsertU_UploadITData(string fromWH, string toWH, string itemCode, string qty, string memo, string userName, string ipAddress)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "ITUpload",
                "InsertU_UploadITData.sql"
            );

            var sql = File.ReadAllText(sqlPath);

            using (var conn = _connectionFactory.GetConnection())
            {
                conn.Open();

                using (var cmd = new SqlCommand(sql, conn))
                {
                    cmd.CommandTimeout = 300;
                    cmd.Parameters.AddWithValue("@FromWH", fromWH);
                    cmd.Parameters.AddWithValue("@ToWH", toWH);
                    cmd.Parameters.AddWithValue("@ItemCode", itemCode);
                    cmd.Parameters.AddWithValue("@Qty", qty);
                    cmd.Parameters.AddWithValue("@Memo", memo);
                    cmd.Parameters.AddWithValue("@UserName", userName);
                    cmd.Parameters.AddWithValue("@IpAddress", ipAddress);
                    cmd.Parameters.AddWithValue("@NowDate", DateAndTime.Today.ToString("M/d/yyyy"));
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
