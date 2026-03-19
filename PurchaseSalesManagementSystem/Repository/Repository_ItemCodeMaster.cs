﻿using Microsoft.Data.SqlClient;
using System.Data;
using PurchaseSalesManagementSystem.Common;

namespace PurchaseSalesManagementSystem.Repository
{
    public class Repository_ItemCodeMaster
    {
        private readonly CreateConnection _connectionFactory;
        private readonly IWebHostEnvironment _env;

        public Repository_ItemCodeMaster(CreateConnection connectionFactory, IWebHostEnvironment env)
        {
            _connectionFactory = connectionFactory;
            _env = env;
        }

        public List<Dictionary<string, object?>> GetItemCodeMasterData(string? itemCode, bool excludeInactiveItems)
        {
            var dataTable = GetItemCodeMasterDataTable(itemCode, excludeInactiveItems);
            var result = new List<Dictionary<string, object?>>();

            foreach (DataRow row in dataTable.Rows)
            {
                var item = new Dictionary<string, object?>();

                foreach (DataColumn column in dataTable.Columns)
                {
                    item[column.ColumnName] = row[column] == DBNull.Value ? null : row[column];
                }

                result.Add(item);
            }

            return result;
        }

        public DataTable GetItemCodeMasterDataTable(string? itemCode, bool excludeInactiveItems)
        {
            string sqlPath = Path.Combine(
                _env.ContentRootPath,
                "SQL",
                "ItemCodeMaster",
                "ItemCodeMaster.sql"
            );

            var sql = File.ReadAllText(sqlPath);
            var dataTable = new DataTable();

            using var conn = _connectionFactory.GetConnection();
            conn.Open();

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@ItemCode",
                string.IsNullOrWhiteSpace(itemCode) ? DBNull.Value : itemCode.Trim());
            cmd.Parameters.AddWithValue("@ExcludeInactiveItems", excludeInactiveItems ? "Y" : "N");

            using var reader = cmd.ExecuteReader();
            dataTable.Load(reader);

            return dataTable;
        }
    }
}