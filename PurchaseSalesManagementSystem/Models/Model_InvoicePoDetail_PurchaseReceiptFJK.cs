namespace PurchaseSalesManagementSystem.Models
{
    public class Model_InvoicePoDetail_PurchaseReceiptFJK
    {
        public string POLn { get; set; }

        public string PoNo { get; set; }
        public string LnKey { get; set; }

        public DateTime? PODate { get; set; }

        public string Status { get; set; }

        public string ItemCode { get; set; }
        public string UDF_ITEMDESC { get; set; }

        public string Whse { get; set; }

        public decimal? QtyOrdered { get; set; }
        public decimal? QtyRcpt { get; set; }
        public decimal? QtyBalance { get; set; }
        public decimal? QtyInvoiced { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? LastTotalUnitCost { get; set; }
        public decimal? StandardUnitCost { get; set; }
        public decimal? QtyDiscCost { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? PromiseDate { get; set; }
    }
}

