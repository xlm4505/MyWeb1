using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace PurchaseSalesManagementSystem.Models
{
    public class Model_OrderForExport
	{

        public string? SalesOffice {  get; set; }
		public string? SalesOrderNo { get; set; }
		public DateTime? OrderDate { get; set; }
		public string? OrderType { get; set; }
		public string? OrderStatus { get; set; }
		public string? CustomerPONo { get; set; }
		public string? CustomerNo { get; set; }
		public string? BillToName { get; set; }
		public string? ShipToCity { get; set; }
		public string? ShipVia { get; set; }
		public string? HeaderComment { get; set; }
		public string? CustPO_Ln { get; set; }
		public string? ItemCode { get; set; }
		public string? ItemDescription { get; set; }
		public string? AliasItemNo { get; set; }
		public string? Whs { get; set; }
		public string? Weight { get; set; }
		public decimal? Ordded { get; set; }
		public decimal? Shipped { get; set; }
		public decimal? BO { get; set; }
		public decimal? UnitPrice { get; set; }
		public decimal? ExtensionAmt { get; set; }
		public DateTime? ReqDate { get; set; }
		public DateTime? PushOut { get; set; }
		public DateTime? PromiseDate { get; set; }
		public DateTime? CommitDate { get; set; }
		public DateTime? DeliveryDate { get; set; }
		public string? CommentText { get; set; }
		public decimal? UnitCost { get; set; }
		public string? PurchaseOrderNo { get; set; }
		public string? Udf_custpono { get; set; }
		public string? InternalNotes { get; set; }
		public string OrderDateText
		{
			get
			{
				
				if (OrderDate != null)
				{
					return OrderDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				}
				else
				{
					return "";
				}
			}
		}

		public string ReqDateText
		{
			get
			{
				if (ReqDate != null)
				{
					return ReqDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				}
				else
				{
					return "";
				}
			}
		}

		public string PushOutText
		{
			get
			{
				if (PushOut != null)
				{
					return PushOut.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				}
				else
				{
					return "";
				}
			}
		}

		public string PromiseDateText
		{
			get
			{
				if (PromiseDate != null)
				{
					return PromiseDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				}
				else
				{
					return "";
				}
			}
		}

		public string CommitDateText
		{
			get
			{
				if (CommitDate != null)
				{
					return CommitDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				}
				else
				{
					return "";
				}
			}
		}

		public	string DeliveryDateText
		{
			get
			{
				if (DeliveryDate != null)
				{
					return DeliveryDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
				}
				else
				{
					return "";
				}
			}
		}
	}
}
