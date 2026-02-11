using DocumentFormat.OpenXml.Office2010.Excel;

namespace PurchaseSalesManagementSystem.Models
{
    public class Model_PurchaseOrder
    {
        public int Seq { get; set; }
        public string ConfirmTo { get; set; }
        public string SalesPerson { get; set; }

        public string ItemCode { get; set; }
        public string ItemCodeDesc { get; set; }

        public DateTime? CustReqDate { get; set; }
        public DateTime? POReqDate { get; set; }

        public int? PurchaseOrderQty { get; set; }

        public string WarehouseCode { get; set; }
        public string CustomerNo { get; set; }
        public string BillToName { get; set; }

        public string SalesOrderNo { get; set; }

        public DateTime? SalesOrderEntryDate { get; set; }

        public string VendorNo { get; set; }

        public string Message { get; set; }

        public string AliasItemNo { get; set; }
        public string CustomerPONo { get; set; }
        public Model_PurchaseOrder()
        {

        }

        public Model_PurchaseOrder(Model_PurchaseOrder src)
        {
            Seq = src.Seq;
            ConfirmTo = src.ConfirmTo;
            SalesPerson = src.SalesPerson;
            ItemCode = src.ItemCode;
            ItemCodeDesc = src.ItemCodeDesc;
            CustReqDate = src.CustReqDate;
            POReqDate = src.POReqDate;
            PurchaseOrderQty = src.PurchaseOrderQty;
            WarehouseCode = src.WarehouseCode;
            CustomerNo = src.CustomerNo;
            BillToName = src.BillToName;
            SalesOrderNo = src.SalesOrderNo;
            SalesOrderEntryDate = src.SalesOrderEntryDate;
            VendorNo = src.VendorNo;
            Message = src.Message;
            AliasItemNo = src.AliasItemNo;
            CustomerPONo = src.CustomerPONo;
        }

    }

}
