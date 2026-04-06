using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using Microsoft.AspNetCore.Http;

namespace PurchaseSalesManagementSystem.Models
{
    public class Model_CombineShippingList
	{
		public string? PONo { get; set; }
		public DateTime? PODate { get; set; }
		public string? Status { get; set; }
		public string? ItemCode { get; set; }
		public string? Description { get; set; }
		public string? Whse { get; set; }
		public decimal? QtyOrdered { get; set; }
		public decimal? QtyRcpt { get; set; }
		public decimal? QtyOpen { get; set; }
		public decimal? QtyInvoiced { get; set; }
		public decimal? UnitCost { get; set; }
		public decimal? LastTotalUnitCost { get; set; }
		public decimal? StandardUnitCost { get; set; }
		public DateTime? RequiredDate { get; set; }
		public string? Category { get; set; }
		public string? SalesOrderNo { get; set; }
		public string? Customer { get; set; }
		public string? CustomerName { get; set; }
		public DateTime? SOEntryDate { get; set; }
		public string? SOEntryUser { get; set; }
	}
}
