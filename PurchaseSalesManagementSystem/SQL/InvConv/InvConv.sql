SELECT
    PO_PurchaseOrderHeader.PurchaseOrderNo AS PoNo,
    PO_PurchaseOrderDetail.LineKey AS LnKey,
    PO_PurchaseOrderHeader.PurchaseOrderDate AS PODate,
    PO_PurchaseOrderHeader.OrderStatus AS Status,
    PO_PurchaseOrderDetail.ItemCode,
    PO_PurchaseOrderDetail.UDF_ITEMDESC,
    PO_PurchaseOrderDetail.WarehouseCode AS Whse,
    PO_PurchaseOrderDetail.QuantityOrdered AS QtyOrdered,
    PO_PurchaseOrderDetail.QuantityReceived AS QtyRcpt,
    PO_PurchaseOrderDetail.QuantityOrdered - PO_PurchaseOrderDetail.QuantityReceived AS QtyBalance,
    PO_PurchaseOrderDetail.QuantityInvoiced AS QtyInvoiced,
    PO_PurchaseOrderDetail.UnitCost,
    CI_Item.LastTotalUnitCost,
    CI_Item.StandardUnitCost,
    CASE
        WHEN PO_VendorPriceLevel.ItemCode IS NULL THEN 0
        WHEN PO_PurchaseOrderDetail.QuantityOrdered <= BreakQuantity1 THEN DiscountMarkup1
        WHEN PO_PurchaseOrderDetail.QuantityOrdered <= BreakQuantity2 THEN DiscountMarkup2
        WHEN PO_PurchaseOrderDetail.QuantityOrdered <= BreakQuantity3 THEN DiscountMarkup3
        WHEN PO_PurchaseOrderDetail.QuantityOrdered <= BreakQuantity4 THEN DiscountMarkup4
        WHEN PO_PurchaseOrderDetail.QuantityOrdered <= BreakQuantity5 THEN DiscountMarkup5
        WHEN PO_PurchaseOrderDetail.QuantityOrdered <= BreakQuantity6 THEN DiscountMarkup6
        WHEN PO_PurchaseOrderDetail.QuantityOrdered <= BreakQuantity7 THEN DiscountMarkup7
        WHEN PO_PurchaseOrderDetail.QuantityOrdered <= BreakQuantity8 THEN DiscountMarkup8
        ELSE 0
    END AS QtyDiscCost,
    PO_PurchaseOrderDetail.RequiredDate,
    PO_PurchaseOrderDetail.UDF_PROMISE_DATE AS PromiseDate
FROM PO_PurchaseOrderHeader, CI_Item, PO_PurchaseOrderDetail
LEFT JOIN PO_VendorPriceLevel
    ON PO_VendorPriceLevel.ItemCode = PO_PurchaseOrderDetail.ItemCode
    AND APDivisionNo = '06'
    AND VendorNo = '0000200'
    AND PricingType = 'I'
WHERE PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo
  AND PO_PurchaseOrderDetail.QuantityOrdered > PO_PurchaseOrderDetail.QuantityReceived
  AND PO_PurchaseOrderDetail.ItemCode = CI_Item.ItemCode
  AND PO_PurchaseOrderDetail.ItemCode NOT LIKE '/%'
  AND PO_PurchaseOrderHeader.PurchaseOrderDate >= '2016-01-01'
ORDER BY PO_PurchaseOrderDetail.ItemCode, PO_PurchaseOrderDetail.UDF_PROMISE_DATE, PO_PurchaseOrderDetail.RequiredDate;