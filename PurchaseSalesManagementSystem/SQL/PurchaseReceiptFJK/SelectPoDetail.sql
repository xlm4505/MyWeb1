Select
    PO_PurchaseOrderHeader.OrderStatus
    , PO_PurchaseOrderHeader.PurchaseOrderNo
    , PO_PurchaseOrderDetail.LineKey
    , PO_PurchaseOrderDetail.ItemCode
    , PO_PurchaseOrderDetail.QuantityOrdered 
From
    PO_PurchaseOrderDetail
    , PO_PurchaseOrderHeader 
Where
    PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo 
    And PO_PurchaseOrderDetail.QuantityOrdered > PO_PurchaseOrderDetail.QuantityReceived 
    And PO_PurchaseOrderHeader.PurchaseOrderNo =　@poNo
;