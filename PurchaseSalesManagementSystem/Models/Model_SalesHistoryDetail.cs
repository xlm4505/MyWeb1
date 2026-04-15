namespace PurchaseSalesManagementSystem.Models
{
    public class Model_SalesHistoryDetailCustomer
    {
        public string QCode { get; set; } = "";
        public string QName { get; set; } = "";
    }

    public class Model_SalesHistoryDetailItem
    {
        public string ItemCode { get; set; } = "";
        public string ItemDesc { get; set; } = "";
    }

    public class Model_SalesHistoryDetailItemDesc
    {
        public string ItemDesc { get; set; } = "";
    }

    public class Model_SalesHistoryDetailResult
    {
        public DateTime? InvoiceDate { get; set; }
        public string Customer { get; set; } = "";
        public string SalesOrderNo { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string ItemCodeDesc { get; set; } = "";
        public string CustomerPartNo { get; set; } = "";
        public string CustomerPONo { get; set; } = "";
        public string POLineNo { get; set; } = "";
        public string InvoiceNo { get; set; } = "";
        public string ShipToCode { get; set; } = "";
        public string ShipToName { get; set; } = "";
        public string ShipVia { get; set; } = "";
        public string Tracking1 { get; set; } = "";
        public string Tracking2 { get; set; } = "";
        public string Tracking3 { get; set; } = "";
        public string Warehouse { get; set; } = "";
        public decimal? ShippedQty { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? ExtensionAmt { get; set; }
        public decimal? StdUnitPrice { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? StdUnitCost { get; set; }
        public decimal? LastUnitPrice { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? PromiseDate { get; set; }
        public DateTime? CommitDate { get; set; }
        public DateTime? ShipDate { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime? EntryDate { get; set; }
    }
}
