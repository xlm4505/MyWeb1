namespace PurchaseSalesManagementSystem.Models
{
    public class Model_SafetyStockMaintenance
    {
        public string ItemCode { get; set; } = string.Empty;
        public string ProcType { get; set; } = string.Empty;
        public string ARDivisionNo { get; set; } = string.Empty;
        public string CustomerNo { get; set; } = string.Empty;
        public string WarehouseCode { get; set; } = string.Empty;
        public decimal Quantity { get; set; }
        public decimal ItemNo { get; set; } 
        public string Comment { get; set; } = string.Empty;
    }
}