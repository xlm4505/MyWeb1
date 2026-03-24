using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Globalization;
using DocumentFormat.OpenXml.Office.CoverPageProps;
using Microsoft.AspNetCore.Http;

namespace PurchaseSalesManagementSystem.Models
{
    public class Model_RAUpload
	{
		public RAUpload_ExportToExcel? RAUpload_ExportToExcel { get; set; }
		public RAUpload_Insert? RAUpload_Insert { get; set; }
	}

	public class RAUpload_ExportToExcel
	{
		public string? ItemCode { get; set; }
		public string? ItemDesc { get; set; }
		public int? JFI { get; set; }
		public int? NAL { get; set; }
		public int? NCA { get; set; }
		public int? NTX { get; set; }
		public int? UTX { get; set; }
		public int? UGP { get; set; }
		public int? IFS { get; set; }
		public int? NNJ { get; set; }
		public int? XIT { get; set; }
		public int? Total { get; set; }
	}

	public class RAUpload_Insert {
		public DateTime? EntryDate { get; set; }
		public string? WarehouseCode { get; set; }
		public string? ItemCode { get; set; }
		public string? Description { get; set; }
		public decimal? OriginalQty { get; set; }
		public decimal? Qty { get; set; }
		public string? InvoiceNo { get; set; }
		public string? Box { get; set; }
		public decimal? Weight { get; set; }
		public DateTime? DateReceived { get; set; }
		public string? From { get; set; }
		public string? VantecRef { get; set; }
		public decimal? UnitPrice { get; set; }
		public string? ShipMark { get; set; }
		public string? Comment { get; set; }
	}
}
