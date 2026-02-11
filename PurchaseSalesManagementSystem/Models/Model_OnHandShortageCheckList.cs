namespace PurchaseSalesManagementSystem.Models
{
    public class Model_OnHandShortageCheckList
    {
        public string ItemCode { get; set; }
        public string ItemDesc { get; set; }
        public string Category { get; set; }

        public int OnHand_Reg { get; set; }
        public int OpenPO_Reg { get; set; }
        public int OpenSO_Reg { get; set; }

        public int Available_Reg { get; set; }

        public int OnHand_Ex { get; set; }
        public int OpenPO_Ex { get; set; }
        public int OpenSO_Ex { get; set; }

        public int Available_Ex { get; set; }

        public int OnHand_Total { get; set; }
        public int OpenPO_Total { get; set; }
        public int OpenSO_Total { get; set; }

        public int Available_Total { get; set; }
    }
}
