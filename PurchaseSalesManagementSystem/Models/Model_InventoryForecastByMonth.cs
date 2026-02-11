namespace PurchaseSalesManagementSystem.Models
{
    /// <summary>
    /// Inventory Forecast by Month (11-7)
    /// </summary>
    public class Model_InventoryForecastByMonth
    {
        // ===== Key / Master =====
        public string ItemCode { get; set; }
        public string ItemCodeDesc { get; set; }
        public string ItemNo { get; set; }
        public string Category1 { get; set; }
        public string VendorName { get; set; }

        // ===== Cost / Stock =====
        public decimal UnitCost { get; set; }
        public int OnHand { get; set; }
        public int PurchaseOrder { get; set; }
        public int SalesOrder { get; set; }
        public int Surplus { get; set; }

        // ===== Report Type =====
        public string DataType { get; set; }

        // ===== Month Values =====
        /// <summary>
        /// M0 ～ M8（Controller / Excel 側で年月と紐づける）
        /// </summary>
        public int?[] MonthlyQty { get; set; } = new int?[9];
    }
}
