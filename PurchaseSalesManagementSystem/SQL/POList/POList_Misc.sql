SELECT
  PO_PurchaseOrderHeader.PurchaseOrderNo AS 'PoNo',
  PO_PurchaseOrderDetail.LineKey        AS 'LnKey',
  CAST(PO_PurchaseOrderHeader.PurchaseOrderDate AS DATE) AS 'PODate',
  PO_PurchaseOrderHeader.OrderStatus    AS 'Status',
  PO_PurchaseOrderHeader.APDivisionNo + '-' + PO_PurchaseOrderHeader.VendorNo AS 'Vendor',
  PO_PurchaseOrderHeader.PurchaseName   AS 'VendorName',
  PO_PurchaseOrderDetail.ItemCode,
  PO_PurchaseOrderDetail.UDF_ITEMDESC,
  PO_PurchaseOrderDetail.QuantityOrdered AS 'QtyOrdered',
  PO_PurchaseOrderDetail.QuantityReceived AS 'QtyRcpt',
  PO_PurchaseOrderDetail.QuantityOrdered - PO_PurchaseOrderDetail.QuantityReceived AS 'QtyBalance',
  PO_PurchaseOrderDetail.QuantityInvoiced AS 'QtyInvoiced',
  PO_PurchaseOrderDetail.UnitCost,
  PO_PurchaseOrderDetail.UnitCost * (PO_PurchaseOrderDetail.QuantityOrdered - PO_PurchaseOrderDetail.QuantityReceived) AS Amount,
  CAST(PO_PurchaseOrderDetail.RequiredDate AS DATE) AS RequiredDate
FROM
  PO_PurchaseOrderHeader, CI_Item, PO_PurchaseOrderDetail
WHERE
  PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo
  AND PO_PurchaseOrderDetail.QuantityOrdered > PO_PurchaseOrderDetail.QuantityReceived
  AND PO_PurchaseOrderDetail.ItemCode = CI_Item.ItemCode
  AND PO_PurchaseOrderDetail.ItemCode LIKE '/%'
  AND PO_PurchaseOrderHeader.PurchaseOrderDate >= {d '2016-01-01'}
ORDER BY
  PO_PurchaseOrderDetail.ItemCode,
  PO_PurchaseOrderDetail.UDF_PROMISE_DATE,
  PO_PurchaseOrderDetail.RequiredDate;