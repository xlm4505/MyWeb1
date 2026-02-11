WITH
    OpenPO AS (
        SELECT
            PO_PurchaseOrderDetail.ItemCode AS PO_ItemCode,
            PO_PurchaseOrderDetail.WarehouseCode AS PO_WarehouseCode,
            SUM(
                CASE
                    WHEN PO_PurchaseOrderHeader.OrderStatus IN ('N', 'O') THEN PO_PurchaseOrderDetail.QuantityOrdered
                    ELSE CASE
                        WHEN COALESCE(PO_PurchaseOrderDetail.QuantityBackordered, 0) < 0 THEN 0
                        ELSE COALESCE(PO_PurchaseOrderDetail.QuantityBackordered, 0)
                    END
                END
            ) AS PO_OpenQty
        FROM
            PO_PurchaseOrderDetail
            LEFT JOIN PO_PurchaseOrderHeader ON PO_PurchaseOrderDetail.PurchaseOrderNo = PO_PurchaseOrderHeader.PurchaseOrderNo
        WHERE
            PO_PurchaseOrderDetail.ItemType = 1
            AND PO_PurchaseOrderHeader.OrderStatus <> 'X'
        GROUP BY
            PO_PurchaseOrderDetail.ItemCode,
            PO_PurchaseOrderDetail.WarehouseCode
    ),
    OpenSO AS (
        SELECT
            ItemCode AS SO_ItemCode,
            SO_SalesOrderDetail.WarehouseCode AS SO_WarehouseCode,
            SUM(
                CASE
                    WHEN COALESCE(SO_SalesOrderDetail.QuantityOrdered, 0) - COALESCE(SO_SalesOrderDetail.QuantityShipped, 0) < 0 THEN 0
                    ELSE COALESCE(SO_SalesOrderDetail.QuantityOrdered, 0) - COALESCE(SO_SalesOrderDetail.QuantityShipped, 0)
                END
            ) AS SO_OpenQty
        FROM
            SO_SalesOrderDetail
            LEFT JOIN SO_SalesOrderHeader ON SO_SalesOrderDetail.SalesOrderNo = SO_SalesOrderHeader.SalesOrderNo
        WHERE
            SO_SalesOrderHeader.OrderType IN ('S', 'B')
            AND ItemType = 1
        GROUP BY
            ItemCode,
            SO_SalesOrderDetail.WarehouseCode
    ),
    StockData AS (
        SELECT
            IM_ItemWarehouse.ItemCode AS TtlItemCode,
            SUM(QuantityOnHand) AS TtlOnHand,
            SUM(COALESCE(PO_OpenQty, 0)) AS TtlPurchaseOrder,
            SUM(COALESCE(SO_OpenQty, 0)) AS TtlSalesOrder,
            COALESCE(U_ForecastItem.Quantity, 0) AS SafetyStock,
            (
                SUM(
                    QuantityOnHand + COALESCE(PO_OpenQty, 0) - COALESCE(SO_OpenQty, 0)
                ) - COALESCE(U_ForecastItem.Quantity, 0)
            ) * -1 AS TtlQty,
            (
                SUM(
                    CASE
                        WHEN IM_ItemWarehouse.WarehouseCode IN (
                            'STR',
                            'STX',
                            'UTX',
                            'RJP',
                            'MOT',
                            'MOK',
                            'SCP',
                            'PAA',
                            'PAS',
                            'UGP',
                            'TIS',
                            'XUS',
                            'PAX',
                            '000',
                            'BSR'
                        ) THEN 0
                        ELSE QuantityOnHand + COALESCE(PO_OpenQty, 0) - COALESCE(SO_OpenQty, 0)
                    END
                ) - COALESCE(U_ForecastItem.Quantity, 0)
            ) * -1 AS TtlReqQty
        FROM
            IM_ItemWarehouse
            LEFT JOIN OpenPO ON PO_ItemCode = IM_ItemWarehouse.ItemCode
            AND PO_WarehouseCode = IM_ItemWarehouse.WarehouseCode
            LEFT JOIN OpenSO ON SO_ItemCode = IM_ItemWarehouse.ItemCode
            AND SO_WarehouseCode = IM_ItemWarehouse.WarehouseCode
            LEFT JOIN U_ForecastItem ON U_ForecastItem.ItemCode = IM_ItemWarehouse.ItemCode
        GROUP BY
            IM_ItemWarehouse.ItemCode,
            U_ForecastItem.Quantity
        HAVING
            SUM(COALESCE(SO_OpenQty, 0)) + COALESCE(U_ForecastItem.Quantity, 0) > 0
            AND SUM(
                CASE
                    WHEN IM_ItemWarehouse.WarehouseCode IN (
                        'STR',
                        'STX',
                        'UTX',
                        'RJP',
                        'MOT',
                        'MOK',
                        'SCP',
                        'PAA',
                        'PAS',
                        'UGP',
                        'TIS',
                        'XUS',
                        'PAX',
                        '000',
                        'BSR'
                    ) THEN 0
                    ELSE QuantityOnHand + COALESCE(PO_OpenQty, 0) - COALESCE(SO_OpenQty, 0)
                END
            ) - COALESCE(U_ForecastItem.Quantity, 0) < 0
    ),
    SalesData AS (
        SELECT
            SO_SalesOrderDetail.ItemCode,
            SO_SalesOrderDetail.UDF_ITEMDESC,
            CI_Item.UDF_IM_DESC_2,
            ExtendedDescriptionText,
            CI_Item.Category1,
            ARDivisionNo + '-' + CustomerNo AS CustomerNo,
            CI_Item.PrimaryAPDivisionNo + '-' + CI_Item.PrimaryVendorNo AS VendorNo,
            BillToName,
            SO_SalesOrderHeader.SalesOrderNo,
            SO_SalesOrderHeader.CustomerPONo,
            LineKey,
            /* FirstName + ' ' + LastName AS SalesPerson, */ '' AS SalesPerson,
            CASE
                WHEN UDF_PUSHOUT <> '1/1/1753' THEN UDF_PUSHOUT
                ELSE UDF_REQUEST_DATE
            END AS CustReqDate,
            SO_SalesOrderDetail.WarehouseCode,
            SO_SalesOrderHeader.DateCreated,
            QuantityOrdered - QuantityShipped AS OpenQty,
            SO_SalesOrderDetail.AliasItemNo AS AliasItemNo
        FROM
            SO_SalesOrderDetail
            LEFT JOIN SO_SalesOrderHeader ON SO_SalesOrderHeader.SalesOrderNo = SO_SalesOrderDetail.SalesOrderNo
            LEFT JOIN CI_Item ON SO_SalesOrderDetail.ItemCode = CI_Item.ItemCode
            LEFT JOIN CI_ExtendedDescription ON CI_Item.ExtendedDescriptionKey = CI_ExtendedDescription.ExtendedDescriptionKey
        WHERE
            SO_SalesOrderHeader.OrderType IN ('S', 'B')
            AND QuantityOrdered - QuantityShipped > 0
            AND SO_SalesOrderDetail.ItemType = 1
            AND SO_SalesOrderDetail.WarehouseCode NOT IN (
                'STR',
                'STX',
                'UTX',
                'RJP',
                'MOT',
                'MOK',
                'SCP',
                'PAA',
                'PAS',
                'UGP',
                'TIS',
                'XUS',
                'PAX',
                '000',
                'BSR'
            )
            AND SO_SalesOrderDetail.ItemCode NOT IN (
                SELECT
                    ItemCode
                FROM
                    U_ForecastItem
                WHERE
                    ProcType = 'M'
            )
        UNION ALL
        SELECT
            U_ForecastItem.ItemCode,
            CI_Item.UDF_ITEMDESC,
            CI_Item.UDF_IM_DESC_2,
            ExtendedDescriptionText,
            CI_Item.Category1,
            U_ForecastItem.ARDivisionNo + '-' + U_ForecastItem.CustomerNo AS CustomerNo,
            CI_Item.PrimaryAPDivisionNo + '-' + CI_Item.PrimaryVendorNo AS VendorNo,
            CustomerName AS BillToName,
            'PreOrder' AS SalesOrderNo,
            '' AS CustomerPONo,
            '' LineKey,
            '' AS SalesPerson,
            DATEADD(WEEK, 7, GETDATE()) AS CustReqDate,
            U_ForecastItem.WarehouseCode,
            '1/1/1900' AS DateCreated,
            U_ForecastItem.Quantity AS OpenQty,
            '' AS AliasItemNo
        FROM
            U_ForecastItem
            LEFT JOIN CI_Item ON U_ForecastItem.ItemCode = CI_Item.ItemCode
            LEFT JOIN CI_ExtendedDescription ON CI_Item.ExtendedDescriptionKey = CI_ExtendedDescription.ExtendedDescriptionKey
            LEFT JOIN AR_Customer ON U_ForecastItem.ARDivisionNo = AR_Customer.ARDivisionNo
            AND U_ForecastItem.CustomerNo = AR_Customer.CustomerNo
        WHERE
            U_ForecastItem.ProcType = 'A'
    ),
    BaseData1 AS (
        SELECT
            SalesData.*,
            SUM(OpenQty) OVER (
                PARTITION BY
                    SalesData.ItemCode
                ORDER BY
                    SalesData.DateCreated DESC,
                    SalesData.SalesOrderNo DESC,
                    LineKey DESC
            ) AS CumOpen,
            TtlReqQty
        FROM
            SalesData
            LEFT JOIN StockData ON SalesData.ItemCode = TtlItemCode
        WHERE
            TtlReqQty IS NOT NULL
            AND NOT (
                SalesData.Category1 IN (
                    'CONC METER',
                    'FCS CABLE',
                    'FLOW DIAGN',
                    'LABEL',
                    'MASS FLOW',
                    'PRINTER',
                    'PRS CNTLR',
                    'VAPORIZER'
                )
                OR SalesData.ItemCode = '000050'
                AND SalesData.WarehouseCode IN ('FLC', 'FLJ', 'FLT', 'IFS', 'JTX', 'NNJ')
            )
    ),
    BaseData2 AS (
        SELECT
            ItemCode,
            UDF_ITEMDESC,
            UDF_IM_DESC_2,
            ExtendedDescriptionText,
            CAST(CustReqDate AS DATE) AS CustReqDate,
            CASE
                WHEN CustReqDate < DATEADD(WEEK, 3, GETDATE()) THEN CAST(DATEADD(WEEK, 2, GETDATE()) AS DATE)
                ELSE CAST(DATEADD(WEEK, -1, CustReqDate) AS DATE)
            END AS POReqDate,
            CASE
                WHEN TtlReqQty >= CumOpen THEN OpenQty
                ELSE TtlReqQty - (CumOpen - OpenQty)
            END AS PurchaseOrderQty,
            WarehouseCode,
            CustomerNo,
            BillToName,
            SalesOrderNo,
            CustomerPONo,
            LineKey,
            SalesPerson,
            CAST(OpenQty AS decimal(7, 0)) AS SalesOrderQty,
            CAST(DateCreated AS DATE) AS SalesOrderEntryDate,
            VendorNo,
            AliasItemNo
        FROM
            BaseData1
        WHERE
            CumOpen - OpenQty < TtlReqQty
            AND VendorNo = @VendorNo
    )
SELECT
    1 AS Seq,
    'Shingo Hashimoto' AS ConfirmTo,
    SalesPerson AS SalesPerson,
    ItemCode,
    CASE
        WHEN CHARINDEX('(Pack of', UDF_ITEMDESC) > 0 THEN LEFT(
            UDF_ITEMDESC,
            CHARINDEX('(Pack of', UDF_ITEMDESC) -1
        )
        ELSE UDF_ITEMDESC
    END AS ItemCodeDesc,
    MIN(CustReqDate) AS CustReqDate,
    MIN(POReqDate) AS POReqDate,
    CAST(SUM(PurchaseOrderQty) AS decimal(7, 0)) AS PurchaseOrderQty,
    WarehouseCode,
    CustomerNo,
    BillToName,
    SalesOrderNo,
    CASE
        WHEN SalesOrderEntryDate = '1900-01-01' THEN NULL
        ELSE SalesOrderEntryDate
    END AS SalesOrderEntryDate,
    VendorNo,
    CASE
        WHEN QtyDiscount IS NOT NULL
        AND ExtendedDescriptionText LIKE '%Special Price%' THEN 'Qty Discount; Special Price;' + UDF_IM_DESC_2
        WHEN QtyDiscount IS NOT NULL THEN 'Qty Discount; ' + UDF_IM_DESC_2
        WHEN ExtendedDescriptionText LIKE '%Special Price%' THEN 'Special Price; ' + UDF_IM_DESC_2
        ELSE UDF_IM_DESC_2
    END AS Message,
    AliasItemNo,
    CustomerPONo
FROM
    BaseData2
    LEFT JOIN (
        SELECT DISTINCT
            ItemCode AS QtyDiscount
        FROM
            PO_VendorPriceLevel
        WHERE
            APDivisionNo = '06'
            AND VendorNo = '0000200'
            AND PricingType = 'I'
            AND BreakQuantity1 < 99999999
    ) AS QtyDiscount ON ItemCode = QtyDiscount
GROUP BY
    ItemCode,
    UDF_ITEMDESC,
    UDF_IM_DESC_2,
    ExtendedDescriptionText,
    LEFT(CONVERT(varchar, CustReqDate, 112), 6),
    WarehouseCode,
    CustomerNo,
    BillToName,
    SalesOrderNo,
    CustomerPONo,
    SalesPerson,
    SalesOrderEntryDate,
    VendorNo,
    AliasItemNo,
    QtyDiscount
ORDER BY
    VendorNo,
    ItemCode,
    SalesOrderNo;