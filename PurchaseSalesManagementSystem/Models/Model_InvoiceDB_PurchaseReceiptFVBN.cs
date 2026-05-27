namespace PurchaseSalesManagementSystem.Models
{
    public class Model_InvoiceDB_PurchaseReceiptFVBN
    {
        public string? OrderStatus { get; set; }

        public string? LineKey { get; set; }
        public string? ItemCode { get; set; }

        public string? WarehouseCode { get; set; }

        public decimal? UnitCost { get; set; }
        public decimal? QuantityOrdered { get; set; }
        public decimal? QuantityReceived { get; set; }
    }
}
