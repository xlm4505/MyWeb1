namespace PurchaseSalesManagementSystem.Models
{
    public class Model_ProjectPartOpenOrderVolume
    {
        public string ItemCode { get; set; }
        public string ItemCodeDesc { get; set; }
        public string VendorName { get; set; }
        public decimal UnitCost { get; set; }

        public int OnHand1 { get; set; }
        public int OpenPO1 { get; set; }
        public int OpenSO1 { get; set; }
        public int Surplus1 { get; set; }
        public int OnHand2 { get; set; }
        public int OpenPO2 { get; set; }
        public int OpenSO2 { get; set; }
        public int Surplus2 { get; set; }   
        public int Available { get; set; }
        public int Total { get; set; }


        public int?[] MonthlyQty { get; set; } = new int?[12];
    }

}
