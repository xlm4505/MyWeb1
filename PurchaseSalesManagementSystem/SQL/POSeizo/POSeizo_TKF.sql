/* ============================================================
   PO Export SQL (TKF Vendor Only)
   Vendor = 08-0000250
============================================================ */

SELECT
    PO_PurchaseOrderDetail.PurchaseOrderNo AS PO,

    PO_PurchaseOrderDetail.UnitOfMeasure AS Unit,

    PO_PurchaseOrderDetail.ItemCode AS [ItemCode],

    CASE
        WHEN CHARINDEX('(Pack of', PO_PurchaseOrderDetail.UDF_ITEMDESC) > 0
        THEN LEFT(
              PO_PurchaseOrderDetail.UDF_ITEMDESC,
              CHARINDEX('(Pack of', PO_PurchaseOrderDetail.UDF_ITEMDESC) - 1
        )
        ELSE PO_PurchaseOrderDetail.UDF_ITEMDESC
    END AS [Description],

    IM_ItemVendor.VendorAliasItemNo AS [CustomerPartNumber],

    CASE
        WHEN PO_PurchaseOrderDetail.SalesOrderNo <> '' THEN '(1) '
        WHEN COALESCE(CI_Item.UDF_SAFETYSTOCK,0) > 0 THEN '(3) '
        ELSE '(2) '
    END
    + PO_PurchaseOrderDetail.UDF_NOTE AS Customer,

    PO_PurchaseOrderDetail.WarehouseCode AS [WHCode],

    CAST(PO_PurchaseOrderDetail.RequiredDate AS DATE)
        AS [RequiredDeliveryDate],

    PO_PurchaseOrderDetail.QuantityOrdered AS [OrderedQty],

    CONVERT(decimal(12,2),
        PO_PurchaseOrderDetail.UnitCost)
        AS [UnitPrice],

    CONVERT(decimal(14,2),
        PO_PurchaseOrderDetail.UnitCost
        * PO_PurchaseOrderDetail.QuantityOrdered)
        AS Amount

FROM PO_PurchaseOrderHeader
LEFT JOIN PO_PurchaseOrderDetail
    ON PO_PurchaseOrderHeader.PurchaseOrderNo
     = PO_PurchaseOrderDetail.PurchaseOrderNo

LEFT JOIN IM_ItemVendor
    ON PO_PurchaseOrderDetail.ItemCode
     = IM_ItemVendor.ItemCode
   AND PO_PurchaseOrderHeader.APDivisionNo
     = IM_ItemVendor.APDivisionNo
   AND PO_PurchaseOrderHeader.VendorNo
     = IM_ItemVendor.VendorNo

LEFT JOIN MAS_SYSTEM.dbo.SY_User u
    ON u.UserKey
     = PO_PurchaseOrderHeader.UserCreatedKey

LEFT JOIN CI_Item
    ON CI_Item.ItemCode
     = PO_PurchaseOrderDetail.ItemCode

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
         PO_PurchaseOrderHeader.VendorNo) = '08-0000250'

ORDER BY
    PO_PurchaseOrderDetail.PurchaseOrderNo,
    PO_PurchaseOrderDetail.ItemCode;
