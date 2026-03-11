namespace PurchaseSalesManagementSystem.Models
{
    public class Model_POList_AllVendors
    {
        public string VendorNo { get; set; } = "";
        public string ?POLn { get; set; } = "";
        public string PONo { get; set; } = "";
        public string LnKey { get; set; } = "";
        public DateTime? PODate { get; set; }
        public string Status { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string ItemDesc { get; set; } = "";
        public string Whse { get; set; } = "";
        public decimal? Ordered { get; set; }
        public decimal? Received { get; set; }
        public decimal? Balance { get; set; }
        public decimal? Invoiced { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? StdUnitCost { get; set; }
        public decimal? LastCost { get; set; }
        public decimal? AvgCost { get; set; }
        public decimal? VenCostCM { get; set; }
        public DateTime? RequiredDate { get; set; }
        public DateTime? PromiseDate { get; set; }
        public string SalesOrderNo { get; set; } = "";
    }
}