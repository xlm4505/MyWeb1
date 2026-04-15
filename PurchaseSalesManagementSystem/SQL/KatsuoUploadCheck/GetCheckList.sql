WITH Katsuo AS (
    SELECT PurchaseOrderNo, ItemCode, MIN(PromiseDate) AS PromiseDate, SUM(OpenQty) AS OpenQty
    FROM U_Katsuo
    GROUP BY PurchaseOrderNo, ItemCode
)
SELECT
    Katsuo.PurchaseOrderNo AS 'PONo',
    ''                     AS 'OrderDate',
    ''                     AS Catergory,
    Katsuo.ItemCode,
    ''                     AS ItemDesc,
    0                      AS 'Ordered',
    0                      AS 'Received',
    0                      AS 'Open(FOA)',
    0                      AS 'Open(JPN)',
    0                      AS 'InTransit',
    0                      AS 'JFI(Original)',
    0                      AS 'JFI(OnHand)',
    ''                     AS RequiredDate,
    ''                     AS ShipDate,
    'PO Missing'           AS 'Status'
FROM Katsuo
WHERE OpenQty <> 0
  AND NOT EXISTS (
      SELECT 1 FROM PO_PurchaseOrderDetail
      WHERE PO_PurchaseOrderDetail.PurchaseOrderNo + '-' +
            RIGHT(PO_PurchaseOrderDetail.LineKey, 2) = Katsuo.PurchaseOrderNo)

UNION ALL

SELECT
    PO_PurchaseOrderDetail.PurchaseOrderNo + '-' + RIGHT(PO_PurchaseOrderDetail.LineKey, 2) AS 'PONo',
    CAST(PurchaseOrderDate AS DATE)                                                           AS 'OrderDate',
    CASE WHEN Category1 IN (
            'CLEAN PACK', 'CONC METER', 'FCS CABLE', 'FLOW DIAGN', 'LABEL',
            'LV ADAPTOR', 'MASS FLOW', 'PRINTER', 'PRS CNTLR', 'VAPORIZER')
         THEN 'MASS FLOW' ELSE 'VALVE' END                                                   AS Catergory,
    PO_PurchaseOrderDetail.ItemCode,
    PO_PurchaseOrderDetail.UDF_ITEMDESC                                                       AS ItemDesc,
    QuantityOrdered                                                                           AS 'Ordered',
    QuantityReceived                                                                          AS 'Received',
    QuantityOrdered - QuantityReceived                                                        AS 'Open(FOA)',
    CAST(COALESCE(Katsuo.OpenQty, 0) AS int)                                                 AS 'Open(JPN)',
    CAST(CASE WHEN Katsuo.OpenQty IS NULL THEN 0
              WHEN QuantityOrdered - QuantityReceived - Katsuo.OpenQty - COALESCE(RAInv.JFIOriginal, 0) <= 0 THEN 0
              ELSE QuantityOrdered - QuantityReceived - Katsuo.OpenQty - COALESCE(RAInv.JFIOriginal, 0)
         END AS int)                                                                          AS 'InTransit',
    CAST(COALESCE(RAInv.JFIOriginal, 0) AS int)                                              AS 'JFI(Original)',
    CAST(COALESCE(RAInv.JFIQty, 0)      AS int)                                              AS 'JFI(OnHand)',
    CAST(PO_PurchaseOrderDetail.RequiredDate AS DATE)                                         AS RequiredDate,
    CASE WHEN PromiseDate = '1/1/1900' THEN NULL ELSE PromiseDate END                         AS ShipDate,
    CASE
        WHEN Katsuo.OpenQty IS NULL THEN 'Not Booked(JPN)'
        WHEN Katsuo.OpenQty > QuantityOrdered - QuantityReceived THEN 'Insufficient PO Qty'
        WHEN Katsuo.OpenQty < QuantityOrdered - QuantityReceived THEN
            CASE WHEN QuantityOrdered < Katsuo.OpenQty + COALESCE(RAInv.JFIOriginal, 0) THEN 'Insufficient PO Qty'
                 WHEN QuantityOrdered - QuantityReceived - Katsuo.OpenQty - COALESCE(RAInv.JFIOriginal, 0) > 0 THEN 'InTransit'
                 WHEN Katsuo.OpenQty = 0 THEN 'JFI'
                 ELSE 'JFI (Partial)' END
        WHEN PromiseDate = '1/1/1900' THEN 'No ShipDate(JPN)'
        ELSE '' END                                                                           AS 'Status'
FROM PO_PurchaseOrderHeader
LEFT JOIN PO_PurchaseOrderDetail
    ON PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo
LEFT JOIN Katsuo
    ON Katsuo.PurchaseOrderNo =
       PO_PurchaseOrderHeader.PurchaseOrderNo + '-' + RIGHT(PO_PurchaseOrderDetail.LineKey, 2)
LEFT JOIN CI_Item
    ON CI_Item.ItemCode = PO_PurchaseOrderDetail.ItemCode
LEFT JOIN (
    SELECT InvoiceNo, SUM(Qty) AS JFIQty, SUM(OriginalQty) AS JFIOriginal
    FROM U_RAInventory
    WHERE WarehouseCode = 'JFI'
    GROUP BY InvoiceNo
) AS RAInv
    ON PO_PurchaseOrderDetail.PurchaseOrderNo + '-' + RIGHT(PO_PurchaseOrderDetail.LineKey, 2) = RAInv.InvoiceNo
WHERE
    CASE
        WHEN Katsuo.OpenQty IS NULL THEN 'Not Booked(JPN)'
        WHEN Katsuo.OpenQty > QuantityOrdered - QuantityReceived THEN 'Insufficient PO Qty'
        WHEN Katsuo.OpenQty < QuantityOrdered - QuantityReceived THEN
            CASE WHEN QuantityOrdered < Katsuo.OpenQty + COALESCE(RAInv.JFIOriginal, 0) THEN 'Insufficient PO Qty'
                 WHEN QuantityOrdered - QuantityReceived - Katsuo.OpenQty - COALESCE(RAInv.JFIOriginal, 0) > 0 THEN 'InTransit'
                 WHEN Katsuo.OpenQty = 0 THEN 'JFI'
                 ELSE 'JFI (Partial)' END
        WHEN PromiseDate = '1/1/1900' THEN 'No ShipDate(JPN)'
        ELSE '' END NOT IN ('', 'JFI', 'JFI (Partial)')
    AND (
            (@IsValves = 1 AND Category1 NOT IN (
                'CLEAN PACK', 'CONC METER', 'FCS CABLE', 'FLOW DIAGN', 'LABEL',
                'LV ADAPTOR', 'MASS FLOW', 'PRINTER', 'PRS CNTLR', 'VAPORIZER'))
         OR (@IsValves = 0 AND Category1 IN (
                'CLEAN PACK', 'CONC METER', 'FCS CABLE', 'FLOW DIAGN', 'LABEL',
                'LV ADAPTOR', 'MASS FLOW', 'PRINTER', 'PRS CNTLR', 'VAPORIZER'))
        )
    AND PO_PurchaseOrderDetail.QuantityOrdered > PO_PurchaseOrderDetail.QuantityReceived
    AND PO_PurchaseOrderDetail.ItemType = 1
    AND OrderStatus IN ('N', 'B', 'O')
    AND APDivisionNo = '06' AND VendorNo = '0000200'
    AND PO_PurchaseOrderHeader.PurchaseOrderDate >= '2016-01-01'
ORDER BY [Status], ShipDate, RequiredDate
