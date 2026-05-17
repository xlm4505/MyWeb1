namespace PurchaseSalesManagementSystem.Models
{
    public class Model_FOAInventory
    {
        public string ProdLn { get; set; } = string.Empty;
        public string ItemCode { get; set; } = string.Empty;
        public string ItemCodeDesc { get; set; } = string.Empty;
        public string WHSE { get; set; } = string.Empty;
        public decimal OnHand { get; set; }
        public decimal QtyPO { get; set; }
        public decimal StandardUnitCost { get; set; }
        public string LastSoldDate { get; set; } = string.Empty;
        public string LastReceiptDate { get; set; } = string.Empty;
        public decimal LastTotalUnitCost { get; set; }
    }
}
