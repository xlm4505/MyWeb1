/* ============================================================
   GetShippingList SQL 
============================================================ */
select
  PO_PurchaseOrderHeader.PurchaseOrderNo + '-' + 
  RIGHT (PO_PurchaseOrderDetail.LineKey, 2) AS 'PONo'
  , CAST( 
    PO_PurchaseOrderHeader.PurchaseOrderDate AS DATE
  ) AS 'PODate'
  , PO_PurchaseOrderHeader.OrderStatus AS 'Status'
  , PO_PurchaseOrderDetail.ItemCode
  , PO_PurchaseOrderDetail.UDF_ITEMDESC AS [Description]
  , PO_PurchaseOrderDetail.WarehouseCode AS 'Whse'
  , PO_PurchaseOrderDetail.QuantityOrdered AS 'QtyOrdered'
  , PO_PurchaseOrderDetail.QuantityReceived AS 'QtyRcpt'
  , PO_PurchaseOrderDetail.QuantityOrdered - PO_PurchaseOrderDetail.QuantityReceived AS 'QtyOpen'
  , PO_PurchaseOrderDetail.QuantityInvoiced AS 'QtyInvoiced'
  , PO_PurchaseOrderDetail.UnitCost
  , CI_Item.LastTotalUnitCost
  , CI_Item.StandardUnitCost
  , CAST(PO_PurchaseOrderDetail.RequiredDate AS DATE) AS RequiredDate
  , Category1 AS Category
  , PO_PurchaseOrderDetail.SalesOrderNo
  , SO_SalesOrderHeader.ARDivisionNo + '-' + SO_SalesOrderHeader.CustomerNo AS Customer
  , SO_SalesOrderHeader.BillToName AS CustomerName
  , CAST(SO_SalesOrderHeader.DateCreated AS DATE) AS SOEntryDate
  , CASE 
    WHEN SO_SalesOrderHeader.UserCreatedKey = '0000000340' 
      THEN SY_User2.FirstName + ' ' + SY_User2.LastName 
    ELSE SY_User1.FirstName + ' ' + SY_User1.LastName 
    END AS SOEntryUser 
FROM
  PO_PurchaseOrderHeader 
  LEFT JOIN PO_PurchaseOrderDetail 
    ON PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo 
  LEFT JOIN SO_SalesOrderHeader 
    ON SO_SalesOrderHeader.SalesOrderNo = PO_PurchaseOrderDetail.SalesOrderNo 
  LEFT JOIN SY_User AS SY_User1 
    ON SY_User1.UserKey = SO_SalesOrderHeader.UserCreatedKey 
  LEFT JOIN SY_User AS SY_User2 
    ON SY_User2.UserLogon = LOWER(SO_SalesOrderHeader.UDF_DISTRIBUTOR) 
  LEFT JOIN CI_Item 
    ON PO_PurchaseOrderDetail.ItemCode = CI_Item.ItemCode 
WHERE
  PO_PurchaseOrderDetail.QuantityOrdered > PO_PurchaseOrderDetail.QuantityReceived 
  AND PO_PurchaseOrderDetail.ItemType = 1 
  AND PO_PurchaseOrderHeader.PurchaseOrderDate >= '2016-01-01' 
ORDER BY
  PO_PurchaseOrderDetail.ItemCode
  , PO_PurchaseOrderDetail.UDF_PROMISE_DATE
  , PO_PurchaseOrderDetail.RequiredDate
