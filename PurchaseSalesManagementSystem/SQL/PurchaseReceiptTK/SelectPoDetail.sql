Select
    PO_PurchaseOrderHeader.OrderStatus
    , PO_PurchaseOrderHeader.PurchaseOrderNo
    , PO_PurchaseOrderDetail.LineKey
    , PO_PurchaseOrderDetail.ItemCode
    , PO_PurchaseOrderDetail.UDF_ITEMDESC AS ItemDesc
    , IM_ItemVendor.VendorAliasItemNo
    , PO_PurchaseOrderDetail.WarehouseCode
    , PO_PurchaseOrderDetail.UnitCost
    , PO_PurchaseOrderDetail.QuantityOrdered
    , PO_PurchaseOrderDetail.QuantityReceived
    , PO_PurchaseOrderDetail.QuantityOrdered - PO_PurchaseOrderDetail.QuantityReceived AS 'QtyBalance'
    , PO_PurchaseOrderDetail.RequiredDate 
From
    PO_PurchaseOrderDetail 
    Left Join PO_PurchaseOrderHeader 
        On PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo 
    Left JOIN IM_ItemVendor 
        On IM_ItemVendor.ItemCode = PO_PurchaseOrderDetail.ItemCode 
        And IM_ItemVendor.APDivisionNo = '08' 
        AND IM_ItemVendor.VendorNo = '0000250' 
Where
    PO_PurchaseOrderDetail.QuantityOrdered > PO_PurchaseOrderDetail.QuantityReceived 
   And PO_PurchaseOrderHeader.PurchaseOrderNo = @poNo
   And IM_ItemVendor.VendorAliasItemNo = @partNo
Order by RequiredDate;