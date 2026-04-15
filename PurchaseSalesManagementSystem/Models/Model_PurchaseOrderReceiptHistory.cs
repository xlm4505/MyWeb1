using DocumentFormat.OpenXml.Vml;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Globalization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PurchaseSalesManagementSystem.Models
{
    public class Model_PurchaseOrderReceiptHistory
    {
        public string? PONo { get; set; }
        public string? VendorNo { get; set; }
        public string? VendorName { get; set; }
        public DateTime? PODate { get; set; }
        public string? ReceiptNo { get; set; }
        public DateTime? ReceiptDate { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? InvoiceDate { get; set; }
        public decimal? InvoiceTotal { get; set; }
        public string? LineKey { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemDescription { get; set; }
        public string? Warehouse { get; set; }
        public int? QtyRcvd { get; set; }
        public decimal? UnitCost { get; set; }
        public decimal? ExtensionAmt { get; set; }
        public string? SONo { get; set; }
        public string? Comment { get; set; }
        public string? UserName { get; set; }
        public DateTime? PostingDate { get; set; }
        public DateTime? OperationDate { get; set; }
    }
}
