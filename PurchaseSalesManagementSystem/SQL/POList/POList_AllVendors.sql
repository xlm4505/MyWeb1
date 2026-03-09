SELECT
  PO_PurchaseOrderHeader.APDivisionNo + '-' + PO_PurchaseOrderHeader.VendorNo AS VendorNo,
  PO_PurchaseOrderHeader.PurchaseOrderNo + '-' + RIGHT(PO_PurchaseOrderDetail.LineKey,2) AS [PO-Ln],
  PO_PurchaseOrderHeader.PurchaseOrderNo AS PONo,
  PO_PurchaseOrderDetail.LineKey AS LnKey,
  PO_PurchaseOrderHeader.PurchaseOrderDate AS PODate,
  PO_PurchaseOrderHeader.OrderStatus AS [Status],
  PO_PurchaseOrderDetail.ItemCode,
  PO_PurchaseOrderDetail.UDF_ITEMDESC AS ItemDesc,
  PO_PurchaseOrderDetail.WarehouseCode AS Whse,
  PO_PurchaseOrderDetail.QuantityOrdered AS Ordered,
  PO_PurchaseOrderDetail.QuantityReceived AS Received,
  PO_PurchaseOrderDetail.QuantityOrdered - PO_PurchaseOrderDetail.QuantityReceived AS Balance,
  PO_PurchaseOrderDetail.QuantityInvoiced AS Invoiced,
  PO_PurchaseOrderDetail.UnitCost,
  CI_Item.StandardUnitCost AS StdUnitCost,
  CI_Item.LastTotalUnitCost AS LastCost,
  CI_Item.AverageUnitCost AS AvgCost,
  CASE WHEN VenderCost.BreakQuantity1 Is NULL THEN 0
       WHEN VenderCost.BreakQuantity1 = 99999999 THEN VenderCost.DiscountMarkup1
	   ELSE 0 END AS [VenCost(CM)],
  PO_PurchaseOrderDetail.RequiredDate,
  PO_PurchaseOrderDetail.UDF_PROMISE_DATE AS PromiseDate,
  PO_PurchaseOrderDetail.SalesOrderNo
FROM PO_PurchaseOrderHeader
LEFT JOIN PO_PurchaseOrderDetail
  ON PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo 
LEFT JOIN CI_Item
  ON  PO_PurchaseOrderDetail.ItemCode = CI_Item.ItemCode
LEFT JOIN (
  SELECT * FROM PO_VendorPriceLevel
  WHERE APDivisionNo = '06' AND VendorNo = '0000200' AND PricingType = 'I') AS VenderCost
  ON PO_PurchaseOrderDetail.ItemCode = VenderCost.ItemCode
WHERE
  PO_PurchaseOrderDetail.ItemType = 1
  AND PO_PurchaseOrderDetail.QuantityOrdered > PO_PurchaseOrderDetail.QuantityReceived
  AND (@PurchaseOrderNo IS NULL OR PO_PurchaseOrderHeader.PurchaseOrderNo = @PurchaseOrderNo)
ORDER BY PO_PurchaseOrderDetail.ItemCode, PO_PurchaseOrderDetail.UDF_PROMISE_DATE, PO_PurchaseOrderDetail.RequiredDate;