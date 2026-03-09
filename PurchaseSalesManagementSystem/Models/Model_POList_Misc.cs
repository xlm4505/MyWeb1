namespace PurchaseSalesManagementSystem.Models
{
    public class Model_POList_Misc
    {
        public string PoNo { get; set; } = "";
        public string LnKey { get; set; } = "";
        public DateTime? PODate { get; set; }
        public string Status { get; set; } = "";
        public string Vendor { get; set; } = "";
        public string VendorName { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string UDF_ITEMDESC { get; set; } = "";
        public decimal? QtyOrdered { get; set; }
        public decimal? QtyRcpt { get; set; }
        public decimal? QtyBalance { get; set; }
        public decimal? QtyInvoiced { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? Amount { get; set; }
        public DateTime? RequiredDate { get; set; }
    }
}