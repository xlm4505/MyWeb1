/* ============================================================
   ORDER FOR Export SQL 
============================================================ */
SELECT
  SO_SalesOrderHeader.UDF_SALES_OFFICE AS SalesOffice,
  SO_SalesOrderDetail.SalesOrderNo,
  CAST(SO_SalesOrderHeader.OrderDate AS DATE) AS OrderDate,
  SO_SalesOrderHeader.OrderType,
  SO_SalesOrderHeader.OrderStatus,
  SO_SalesOrderHeader.CustomerPONo,
  SO_SalesOrderHeader.CustomerNo,
  SO_SalesOrderHeader.BillToName,
  SO_SalesOrderHeader.ShipToCity,
  SO_SalesOrderHeader.ShipVia,
  SO_SalesOrderHeader.Comment AS HeaderComment,
  SO_SalesOrderDetail.UDF_CUST_PO_LN AS CustPO_Ln,
  SO_SalesOrderDetail.ItemCode,
  SO_SalesOrderDetail.UDF_ITEMDESC AS ItemDescription,
  SO_SalesOrderDetail.AliasItemNo,
  SO_SalesOrderDetail.WarehouseCode AS Whs,
  SO_SalesOrderDetail.UDF_SHIPWEIGHT AS [Weight],
  SO_SalesOrderDetail.QuantityOrdered AS [#Ordded],
  SO_SalesOrderDetail.QuantityShipped AS [#Shipped],
  SO_SalesOrderDetail.QuantityBackordered AS [#BO],
  SO_SalesOrderDetail.UnitPrice,
  SO_SalesOrderDetail.ExtensionAmt,
  CAST(SO_SalesOrderDetail.UDF_REQUEST_DATE AS DATE) AS ReqDate,
  CAST(SO_SalesOrderDetail.UDF_PUSHOUT AS DATE) AS PushOut,
  CAST(SO_SalesOrderDetail.PromiseDate AS DATE) AS PromiseDate,
  CAST(SO_SalesOrderDetail.UDF_COMMIT_DATE AS DATE) AS CommitDate,
  CAST( SO_SalesOrderDetail.UDF_DELIVERYDATE AS DATE) AS DeliveryDate,
  SO_SalesOrderDetail.CommentText,
  SO_SalesOrderDetail.UnitCost,
  SO_SalesOrderDetail.PurchaseOrderNo,
  SO_SalesOrderDetail.UDF_CUSTPONO,
  SO_SalesOrderDetail.UDF_NOTES AS InternalNotes
FROM SO_SalesOrderDetail
INNER JOIN SO_SalesOrderHeader
  ON SO_SalesOrderDetail.SalesOrderNo = SO_SalesOrderHeader.SalesOrderNo
WHERE
  SO_SalesOrderDetail.ItemType<>'4' AND
  NOT (SO_SalesOrderHeader.OrderType='Q' OR SO_SalesOrderHeader.OrderType='R')
  AND (@SalesOrderNo IS NULL OR SO_SalesOrderDetail.SalesOrderNo = @SalesOrderNo)
ORDER BY SO_SalesOrderHeader.UDF_SALES_OFFICE, SO_SalesOrderDetail.SalesOrderNo