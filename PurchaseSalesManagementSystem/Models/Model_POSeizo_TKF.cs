namespace PurchaseSalesManagementSystem.Models
{
    public class Model_POSeizo_TKF
    {
        public string PO { get; set; } = "";
        public string Unit { get; set; } = "";
        public string ItemCode { get; set; } = "";
        public string Description { get; set; } = "";
        public string CustomerPartNumber { get; set; } = "";
        public string Customer { get; set; } = "";
        public string WHCode { get; set; } = "";

        public DateTime? RequiredDeliveryDate { get; set; }

        public decimal? OrderedQty { get; set; }

        public decimal? UnitPrice { get; set; }
        public decimal? Amount { get; set; }
    }


}
