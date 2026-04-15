namespace PurchaseSalesManagementSystem.Models
{
    public class Model_InvoiceDB_PurchaseReceiptCCL
    {
        public string OrderStatus { get; set; }

        public string PONo { get; set; }

        
        public string LineKey { get; set; }
        public string ItemCode { get; set; }
        public string UDF_ITEMDESC { get; set; }

        public string VendorAliasItemNo { get; set; }

        public string WarehouseCode { get; set; }

        public decimal? UnitCost { get; set; }
        public decimal? QuantityOrdered { get; set; }
        public decimal? QuantityReceived { get; set; }

        public decimal? QtyBalance { get; set; }

        public DateTime? RequiredDate { get; set; }
    }
}

