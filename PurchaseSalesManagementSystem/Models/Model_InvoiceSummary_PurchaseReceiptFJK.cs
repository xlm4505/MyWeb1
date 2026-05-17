namespace PurchaseSalesManagementSystem.Models
{
    public class Model_InvoiceSummary_PurchaseReceiptFJK
    {

        public string PoNo { get; set; }

        public string Description { get; set; }

        public string ItemCode { get; set; }

        public string WarehouseCode { get; set; }

        public decimal? Quantity { get; set; }

        public decimal? UnitPrice { get; set; }

        public decimal? Amount { get; set; }

        public string Status { get; set; }

        public string InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public DateTime? Consolidation { get; set; }

        public string BatchNo { get; set; }
        public string Ln { get; set; }

    }
}

