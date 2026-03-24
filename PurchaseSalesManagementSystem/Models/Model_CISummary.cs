using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using Microsoft.AspNetCore.Http;

namespace PurchaseSalesManagementSystem.Models
{
    public class Model_CISummary
	{
		public string? DocType { get; set; }
		public DateTime EntryDate { get; set; }
		public DateTime ShipDate { get; set; }
		public string? NewFileName { get; set; }
		public string? OriginalFileName { get; set; }
		public string? FOA_CI { get; set; }
		public string? ShipTo1 { get; set; }
		public string? ShipTo2 { get; set; }
		public string? Attn { get; set; }
		public string? ShipVia { get; set; }
		public string? Account { get; set; }
		public string? RequestedBy { get; set; }
		public string? ItemCode { get; set; }
		public string? Whse1 { get; set; }
		public string? Whse2 { get; set; }
		public decimal? TranQty { get; set; }
		public string? SalesOrderNo { get; set; }
		public string? CustomerPONo { get; set; }
		public string? LineNo { get; set; }
		public string? FujikinPartNo { get; set; }
		public string? CustomerPartNo { get; set; }
		public string? Category { get; set; }
		public string? Tariff {  get; set; }
		public decimal? UnitPrice { get; set; }
		public decimal? TotalPrice { get; set; }
		public string? Instrucstions { get; set; }
	}

}
