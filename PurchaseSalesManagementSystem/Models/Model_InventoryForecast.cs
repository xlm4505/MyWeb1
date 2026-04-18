namespace PurchaseSalesManagementSystem.Models
{
    public class Model_InventoryForecast
    {
        public string ItemCode { get; set; }
        public string ItemCodeDesc { get; set; }
        public int ItemNo { get; set; }
        public string Category1 { get; set; }
        public string VendorName { get; set; }

        public decimal UnitCost { get; set; }

        public int OnHand { get; set; }
        public int PurchaseOrder { get; set; }
        public int SalesOrder { get; set; }
        public int Surplus { get; set; }

        public string DataType { get; set; }

        public int MonthlyQty1 { get; set; }
        public int MonthlyQty2 { get; set; }
        public int MonthlyQty3 { get; set; }
        public int MonthlyQty4 { get; set; }
        public int MonthlyQty5 { get; set; }
        public int MonthlyQty6 { get; set; }
        public int MonthlyQty7 { get; set; }
        public int MonthlyQty8 { get; set; }
        public int MonthlyQty9 { get; set; }
        public int Total { get; set; }

    }
}

