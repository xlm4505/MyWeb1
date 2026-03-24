namespace PurchaseSalesManagementSystem.Models
{
    public class Model_InvoiceDetail_PurchaseReceiptTK
    {
        public string HSCode { get; set; }

        public string HSName { get; set; }

        public string PoNo { get; set; }
        public string PartNo { get; set; }

        public string Description { get; set; }

        public decimal? Quantity { get; set; }
        public decimal? UP { get; set; }
        public decimal? Amount { get; set; }


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

