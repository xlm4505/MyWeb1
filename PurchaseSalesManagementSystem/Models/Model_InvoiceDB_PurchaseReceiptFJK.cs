namespace PurchaseSalesManagementSystem.Models
{
    public class Model_InvoiceDB_PurchaseReceiptFJK
    {
        public string OrderStatus { get; set; }

        public string PONo { get; set; }

        
        public string LineKey { get; set; }
        public string ItemCode { get; set; }
     
        public decimal? QuantityOrdered { get; set; }
    }
}

