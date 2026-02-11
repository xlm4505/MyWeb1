namespace PurchaseSalesManagementSystem.Models
{
    public class Model_POSeizo_ALL
    {
        public decimal? No { get; set; } 

        public string ItemCode { get; set; } = "";
        public string Desc { get; set; } = "";
        public string PartNumber { get; set; } = "";

        public string Unit { get; set; } = "";

        public DateTime? RequiredDate { get; set; }

        public string EstDeliveryDate { get; set; } = "";

        public decimal? QuantityOrdered { get; set; }

        public decimal? UnitCost { get; set; }

        public decimal? ExtensionAmt { get; set; }

        public string SalesOffice { get; set; } = "";

        public string SalesClass { get; set; } = "";

        public string Customer { get; set; } = "";

        public string EndCustmer { get; set; } = "";

        public string ShipTo { get; set; } = "";

        public string PO { get; set; } = "";

        public string Line { get; set; } = "";

        public string Factory { get; set; } = "";

        public string Filled { get; set; } = "";

        public string Confirmed { get; set; } = "";

        public string Approved { get; set; } = "";

        public DateTime? DateApproved { get; set; }

        public string ProductionControlNotice { get; set; } = "";
    }


}
