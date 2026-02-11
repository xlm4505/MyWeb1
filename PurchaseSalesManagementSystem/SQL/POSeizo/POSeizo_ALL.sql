/* ============================================================
   PO Export SQL
   All Vendors Except ALL (08-0000250)
============================================================ */

SELECT
    CAST(ROW_NUMBER() OVER (ORDER BY PO_PurchaseOrderDetail.PurchaseOrderNo, PO_PurchaseOrderDetail.UDF_LINE_NO) AS DECIMAL) AS [No],
    PO_PurchaseOrderDetail.ItemCode,
    '' AS [Desc],
    CASE
        WHEN CHARINDEX('(Pack of', PO_PurchaseOrderDetail.UDF_ITEMDESC) > 0
        THEN LEFT(
              PO_PurchaseOrderDetail.UDF_ITEMDESC,
              CHARINDEX('(Pack of', PO_PurchaseOrderDetail.UDF_ITEMDESC) - 1
        )
        ELSE PO_PurchaseOrderDetail.UDF_ITEMDESC
    END AS PartNumber,
    '' AS Unit,
    CAST(PO_PurchaseOrderDetail.RequiredDate AS DATE)
        AS RequiredDate,
    '' AS EstDeliveryDate,
    PO_PurchaseOrderDetail.QuantityOrdered,
    /* ===============================
       UnitCost
    =============================== */
    CASE
        WHEN PO_PurchaseOrderHeader.APDivisionNo + '-' +
             PO_PurchaseOrderHeader.VendorNo = '06-0000200'
        THEN CONVERT(decimal(7,0),
            CASE
                WHEN JPYCost.BreakQuantity1 IS NULL THEN 0
                WHEN JPYCost.BreakQuantity1 = 99999999
                     THEN JPYCost.DiscountMarkup1
                ELSE 0
            END
        )
        ELSE CONVERT(decimal(12,2),
            PO_PurchaseOrderDetail.UnitCost)
    END AS UnitCost,

    /* ===============================
       ExtensionAmt
    =============================== */
    CASE
        WHEN PO_PurchaseOrderHeader.APDivisionNo + '-' +
             PO_PurchaseOrderHeader.VendorNo = '06-0000200'
        THEN CONVERT(decimal(9,0),
            CASE
                WHEN JPYCost.BreakQuantity1 IS NULL THEN 0
                WHEN JPYCost.BreakQuantity1 = 99999999
                     THEN JPYCost.DiscountMarkup1
                ELSE 0
            END
            * PO_PurchaseOrderDetail.QuantityOrdered
        )
        ELSE CONVERT(decimal(14,2),
            PO_PurchaseOrderDetail.UnitCost
            * PO_PurchaseOrderDetail.QuantityOrdered)
    END AS ExtensionAmt,

    /* ===============================
       SalesOffice
    =============================== */
    CASE
        WHEN PO_PurchaseOrderDetail.WarehouseCode = 'JFI'
        THEN 'UR'
        ELSE 'UA'
    END AS SalesOffice,

    '8000010628' AS SalesClass,

    /* ===============================
       Customer
    =============================== */
    CASE
        WHEN PO_PurchaseOrderDetail.SalesOrderNo <> '' THEN '(1) '
        WHEN COALESCE(CI_Item.UDF_SAFETYSTOCK,0) > 0 THEN '(3) '
        ELSE '(2) '
    END
    + PO_PurchaseOrderDetail.UDF_NOTE AS Customer,

    /* ===============================
       EndCustmer
    =============================== */
    CASE
        WHEN PO_PurchaseOrderDetail.WarehouseCode IN
            ('FLC','FCA','FSC','HCA','LGR','STR','UCA','UOR')
            THEN '8000011823'
        WHEN PO_PurchaseOrderDetail.WarehouseCode IN
            ('FLJ','IBA','TNJ')
            THEN '8000011824'
        WHEN PO_PurchaseOrderDetail.WarehouseCode IN
            ('FLT','BSR','FTX','ITX','JIT','MSN','STX','TTX')
            THEN '8000011825'
        WHEN PO_PurchaseOrderDetail.WarehouseCode IN ('JFI')
            THEN '8000011808'
        ELSE '-'
    END AS EndCustmer,

    /* ===============================
       ShipTo
    =============================== */
    CASE
        WHEN PO_PurchaseOrderDetail.WarehouseCode IN
            ('FLC','FCA','FSC','HCA','LGR','STR','UCA','UOR')
            THEN 'FCA'
        WHEN PO_PurchaseOrderDetail.WarehouseCode IN
            ('FLJ','IBA','TNJ')
            THEN 'FNJ'
        WHEN PO_PurchaseOrderDetail.WarehouseCode IN
            ('FLT','BSR','FTX','ITX','JIT','MSN','STX','TTX')
            THEN 'FTX'
        WHEN PO_PurchaseOrderDetail.WarehouseCode IN ('JFI')
            THEN 'JFI'
        ELSE '-'
    END AS ShipTo,

    /* ===============================
       PO / Line
    =============================== */
    PO_PurchaseOrderDetail.PurchaseOrderNo
        + '-' + RIGHT(PO_PurchaseOrderDetail.LineKey,2)
        AS PO,

    PO_PurchaseOrderDetail.PurchaseOrderNo
        + '-' + RIGHT(PO_PurchaseOrderDetail.LineKey,2)
        AS Line,

    '' AS Factory,

    /* ===============================
       Filled
    =============================== */
    CASE
        WHEN PO_PurchaseOrderHeader.Comment = ''
        THEN @UserName
        ELSE PO_PurchaseOrderHeader.Comment
    END AS Filled,

    PO_PurchaseOrderHeader.ConfirmTo AS Confirmed,

    /* ===============================
       Approved
    =============================== */
    CASE
        WHEN PO_PurchaseOrderHeader.ConfirmTo IN
            ('Shingo Hashimoto','Koji Kanazawa')
        THEN 'Hida/Tsujitani'
        ELSE 'Hida/Tsujitani'
    END AS Approved,

    CAST(PO_PurchaseOrderHeader.PurchaseOrderDate AS DATE)
        AS DateApproved,

    /* ===============================
       Production Control Notice
    =============================== */
    CASE
        WHEN PO_PurchaseOrderDetail.ItemCode IN ('705041','705042')
        THEN 'Export License Required'
        ELSE ''
    END AS [ProductionControlNotice]

FROM PO_PurchaseOrderHeader

LEFT JOIN PO_PurchaseOrderDetail
    ON PO_PurchaseOrderHeader.PurchaseOrderNo
     = PO_PurchaseOrderDetail.PurchaseOrderNo
LEFT JOIN MAS_SYSTEM.dbo.SY_User u
   ON u.UserKey
     = PO_PurchaseOrderHeader.UserCreatedKey

LEFT JOIN CI_Item
    ON CI_Item.ItemCode
     = PO_PurchaseOrderDetail.ItemCode

LEFT JOIN (
    SELECT *
    FROM PO_VendorPriceLevel
    WHERE APDivisionNo = '06'
      AND VendorNo     = '9000200'
      AND PricingType  = 'I'
) AS JPYCost
    ON CI_Item.ItemCode = JPYCost.ItemCode

WHERE
    PO_PurchaseOrderHeader.DateCreated = @EntryDate
    AND (
        @OrderStatus <> 'New'
        OR PO_PurchaseOrderHeader.OrderStatus = 'N'
    )
    AND (u.FirstName + ' ' + u.LastName) = @UserName
    AND (
          @VendorCode = '00-0000000'
       OR (PO_PurchaseOrderHeader.APDivisionNo + '-' +
           PO_PurchaseOrderHeader.VendorNo) = @VendorCode
    )
    AND (PO_PurchaseOrderHeader.APDivisionNo + '-' +
         PO_PurchaseOrderHeader.VendorNo) <> '08-0000250'

ORDER BY
    PO_PurchaseOrderDetail.PurchaseOrderNo,
    PO_PurchaseOrderDetail.UDF_LINE_NO,
    PO_PurchaseOrderDetail.LineKey;
