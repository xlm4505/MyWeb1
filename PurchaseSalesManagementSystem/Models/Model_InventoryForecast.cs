namespace PurchaseSalesManagementSystem.Models
{
    public class Model_InventoryForecast
    {
        public string ItemCode { get; set; }
        public string ItemCodeDesc { get; set; }
        public string ItemNo { get; set; }
        public string Category1 { get; set; }
        public string VendorName { get; set; }

        public decimal UnitCost { get; set; }

        public int OnHand { get; set; }
        public int PurchaseOrder { get; set; }
        public int SalesOrder { get; set; }
        public int Surplus { get; set; }

        public string DataType { get; set; }

        public int?[] MonthlyQty { get; set; }

    }
}

