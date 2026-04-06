using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace PurchaseSalesManagementSystem.Models
{
    public class Model_ItemCodeMaster
    {

        public string? ItemCode {  get; set; }
		public string? ItemDesc { get; set; }
		public string? ItemDesc2 { get; set; }
		public string? Category { get; set; }
		public string? ProductLineDesc { get; set; }
		public string? ProductType { get; set; }
		public string? Inactive { get; set; }
		public decimal? Weight { get; set; }
		public string? Whse { get; set; }
		public string? PrimaryVendor { get; set; }
		public string? QtyDisc { get; set; }
        public decimal? StdSalesPrice { get; set; }
        public decimal? StdUnitCost { get; set; }
        public decimal? LastCost { get; set; }
        public decimal? AvgCost { get; set; }
        public decimal? VenCost_USD_ { get; set; }

        public decimal? VenCost_JPY { get; set; }

        public decimal? OnHand { get; set; }
        public decimal? OpenSO { get; set; }
        public decimal? Available { get; set; }
        public decimal? OpenPO { get; set; }
        public decimal? InShip { get; set; }
        public decimal? OnHand_ { get; set; }
        public decimal? OpenSO_ { get; set; }
        public decimal? Available_ { get; set; }
        public decimal? OpenPO_ { get; set; }
        public decimal? InShip_ { get; set; }

        public DateTime? LastSold { get; set; }
        public DateTime? LastReceipt { get; set; }

        public string? ExtendedDescriptionText { get; set; }
        public DateTime? DateCreated { get; set; }

        public string? UserCreated { get; set; }

        public DateTime? DateUpdated { get; set; }

        public string? UserUpdated { get; set; }

        public decimal? ListCOP { get; set; }
        public decimal? Standard { get; set; }
        public decimal? Discount { get; set; }
        public decimal? Class4 { get; set; }
        public decimal? Class5 { get; set; }
        public decimal? Contract { get; set; }
        public decimal? Class6 { get; set; }

    }
}
