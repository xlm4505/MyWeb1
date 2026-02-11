WITH
SData AS (
    SELECT
        ItemCode,
        CASE
            WHEN DueMonth < @YM0 THEN @YM0
            WHEN DueMonth > @YM8 THEN @YM8
            ELSE DueMonth
        END AS DueMonth,
        SUM(OpenQty) AS SOpenQty
    FROM (
        SELECT
            ItemCode,
            CASE
                WHEN UDF_PUSHOUT IS NOT NULL AND UDF_PUSHOUT <> '1753-01-01'
                    THEN LEFT(CONVERT(VARCHAR, UDF_PUSHOUT, 23), 7)
                WHEN UDF_REQUEST_DATE IS NOT NULL AND UDF_REQUEST_DATE <> '1753-01-01'
                    THEN LEFT(CONVERT(VARCHAR, UDF_REQUEST_DATE, 23), 7)
                ELSE LEFT(CONVERT(VARCHAR, PromiseDate, 23), 7)
            END AS DueMonth,
            CASE
                WHEN SO_SalesOrderHeader.OrderStatus IN ('O','N')
                    THEN SO_SalesOrderDetail.QuantityOrdered
                ELSE
                    CASE
                        WHEN COALESCE(SO_SalesOrderDetail.QuantityBackordered,0) < 0 THEN 0
                        ELSE COALESCE(SO_SalesOrderDetail.QuantityBackordered,0)
                    END
            END AS OpenQty
        FROM SO_SalesOrderDetail
        LEFT JOIN SO_SalesOrderHeader
            ON SO_SalesOrderDetail.SalesOrderNo = SO_SalesOrderHeader.SalesOrderNo
        WHERE
            LEFT(ItemCode,1) <> '/'
            AND
            CASE
                WHEN QuantityBackordered > 0 THEN QuantityBackordered
                ELSE QuantityOrdered
            END <> 0
    ) DATA
    GROUP BY
        ItemCode,
        CASE
            WHEN DueMonth < @YM0 THEN @YM0
            WHEN DueMonth > @YM8 THEN @YM8
            ELSE DueMonth
        END
),

WData AS (
    SELECT
        ItemCode,
        SUM(QuantityOnHand) AS OnHand,
        SUM(QuantityOnSalesOrder + QuantityOnBackOrder) AS SalesOrder
    FROM IM_ItemWarehouse
    GROUP BY ItemCode
    HAVING
        SUM(QuantityOnHand) <> 0
        OR SUM(QuantityOnSalesOrder) <> 0
        OR SUM(QuantityOnBackOrder) <> 0
),

Dates AS (
    SELECT DATEADD(MONTH, -1, GETDATE()) AS [Date]
    UNION ALL
    SELECT DATEADD(MONTH, 1, [Date])
    FROM Dates
    WHERE [Date] < DATEADD(MONTH, 7, GETDATE())
),

KData AS (
    SELECT I.ItemCode, D.DueMonth
    FROM (
        SELECT ItemCode FROM SData
        UNION
        SELECT ItemCode FROM WData
    ) I
    CROSS JOIN (
        SELECT LEFT(CONVERT(VARCHAR, [Date], 23), 7) AS DueMonth
        FROM Dates
    ) D
),

TData AS (
    SELECT
        K.ItemCode,
        K.DueMonth,
        COALESCE(W.OnHand,0)
        - SUM(COALESCE(S.SOpenQty,0))
            OVER (PARTITION BY K.ItemCode ORDER BY K.DueMonth)
        AS Stock,
        CASE
            WHEN SUM(COALESCE(S.SOpenQty,0))
                 OVER (PARTITION BY K.ItemCode ORDER BY K.DueMonth)
                 < COALESCE(W.OnHand,0)
            THEN 0
            ELSE
                CASE
                    WHEN
                        SUM(COALESCE(S.SOpenQty,0))
                        OVER (PARTITION BY K.ItemCode ORDER BY K.DueMonth)
                        - COALESCE(W.OnHand,0)
                        > COALESCE(S.SOpenQty,0)
                    THEN COALESCE(S.SOpenQty,0)
                    ELSE
                        SUM(COALESCE(S.SOpenQty,0))
                        OVER (PARTITION BY K.ItemCode ORDER BY K.DueMonth)
                        - COALESCE(W.OnHand,0)
                END
        END AS Req
    FROM KData K
    LEFT JOIN WData W ON W.ItemCode = K.ItemCode
    LEFT JOIN SData S ON S.ItemCode = K.ItemCode AND S.DueMonth = K.DueMonth
),

IData AS (
    SELECT
        IK.ItemCode,
        CASE
            WHEN CI.UDF_ITEMDESC = '' THEN CI.ItemCodeDesc
            ELSE CI.UDF_ITEMDESC
        END AS ItemCodeDesc,
        CI.ItemNo,
        CI.Category1,
        V.VendorName,
        CASE
            WHEN COALESCE(CI.LastTotalUnitCost,0) = 0
                THEN CI.StandardUnitCost
            ELSE CI.LastTotalUnitCost
        END AS UnitCost,
        CAST(COALESCE(W.OnHand,0) AS INT) AS OnHand,
        CAST(COALESCE(W.SalesOrder,0) AS INT) AS SalesOrder,
        CAST(
            COALESCE(W.OnHand,0)
          - COALESCE(W.SalesOrder,0)
        AS INT) AS Surplus
    FROM (
        SELECT ItemCode FROM SData
        UNION
        SELECT ItemCode FROM WData
    ) IK
    LEFT JOIN WData W ON W.ItemCode = IK.ItemCode
    LEFT JOIN CI_Item CI ON CI.ItemCode = IK.ItemCode
    LEFT JOIN AP_Vendor V
        ON CI.PrimaryAPDivisionNo = V.APDivisionNo
       AND CI.PrimaryVendorNo = V.VendorNo
)

-- =========================
-- 1. Sales Order
-- =========================
SELECT
    IData.*,
    '1. Sales Order' AS [Data Type],
    CAST(COALESCE(M0.SOpenQty,0) AS INT) AS M0,
    CAST(COALESCE(M1.SOpenQty,0) AS INT) AS M1,
    CAST(COALESCE(M2.SOpenQty,0) AS INT) AS M2,
    CAST(COALESCE(M3.SOpenQty,0) AS INT) AS M3,
    CAST(COALESCE(M4.SOpenQty,0) AS INT) AS M4,
    CAST(COALESCE(M5.SOpenQty,0) AS INT) AS M5,
    CAST(COALESCE(M6.SOpenQty,0) AS INT) AS M6,
    CAST(COALESCE(M7.SOpenQty,0) AS INT) AS M7,
    CAST(COALESCE(M8.SOpenQty,0) AS INT) AS M8
FROM IData
LEFT JOIN SData M0 ON M0.ItemCode = IData.ItemCode AND M0.DueMonth = @YM0
LEFT JOIN SData M1 ON M1.ItemCode = IData.ItemCode AND M1.DueMonth = @YM1
LEFT JOIN SData M2 ON M2.ItemCode = IData.ItemCode AND M2.DueMonth = @YM2
LEFT JOIN SData M3 ON M3.ItemCode = IData.ItemCode AND M3.DueMonth = @YM3
LEFT JOIN SData M4 ON M4.ItemCode = IData.ItemCode AND M4.DueMonth = @YM4
LEFT JOIN SData M5 ON M5.ItemCode = IData.ItemCode AND M5.DueMonth = @YM5
LEFT JOIN SData M6 ON M6.ItemCode = IData.ItemCode AND M6.DueMonth = @YM6
LEFT JOIN SData M7 ON M7.ItemCode = IData.ItemCode AND M7.DueMonth = @YM7
LEFT JOIN SData M8 ON M8.ItemCode = IData.ItemCode AND M8.DueMonth = @YM8

UNION ALL

-- =========================
-- 2. Inventory
-- =========================
SELECT
    IData.*,
    '2. Inventory' AS [Data Type],
    CAST(COALESCE(M0.Stock,0) AS INT),
    CAST(COALESCE(M1.Stock,0) AS INT),
    CAST(COALESCE(M2.Stock,0) AS INT),
    CAST(COALESCE(M3.Stock,0) AS INT),
    CAST(COALESCE(M4.Stock,0) AS INT),
    CAST(COALESCE(M5.Stock,0) AS INT),
    CAST(COALESCE(M6.Stock,0) AS INT),
    CAST(COALESCE(M7.Stock,0) AS INT),
    CAST(COALESCE(M8.Stock,0) AS INT)
FROM IData
LEFT JOIN TData M0 ON M0.ItemCode = IData.ItemCode AND M0.DueMonth = @YM0
LEFT JOIN TData M1 ON M1.ItemCode = IData.ItemCode AND M1.DueMonth = @YM1
LEFT JOIN TData M2 ON M2.ItemCode = IData.ItemCode AND M2.DueMonth = @YM2
LEFT JOIN TData M3 ON M3.ItemCode = IData.ItemCode AND M3.DueMonth = @YM3
LEFT JOIN TData M4 ON M4.ItemCode = IData.ItemCode AND M4.DueMonth = @YM4
LEFT JOIN TData M5 ON M5.ItemCode = IData.ItemCode AND M5.DueMonth = @YM5
LEFT JOIN TData M6 ON M6.ItemCode = IData.ItemCode AND M6.DueMonth = @YM6
LEFT JOIN TData M7 ON M7.ItemCode = IData.ItemCode AND M7.DueMonth = @YM7
LEFT JOIN TData M8 ON M8.ItemCode = IData.ItemCode AND M8.DueMonth = @YM8

UNION ALL

-- =========================
-- 3. Required
-- =========================
SELECT
    IData.*,
    '3. Required' AS [Data Type],
    CAST(COALESCE(M0.Req,0) AS INT),
    CAST(COALESCE(M1.Req,0) AS INT),
    CAST(COALESCE(M2.Req,0) AS INT),
    CAST(COALESCE(M3.Req,0) AS INT),
    CAST(COALESCE(M4.Req,0) AS INT),
    CAST(COALESCE(M5.Req,0) AS INT),
    CAST(COALESCE(M6.Req,0) AS INT),
    CAST(COALESCE(M7.Req,0) AS INT),
    CAST(COALESCE(M8.Req,0) AS INT)
FROM IData
LEFT JOIN TData M0 ON M0.ItemCode = IData.ItemCode AND M0.DueMonth = @YM0
LEFT JOIN TData M1 ON M1.ItemCode = IData.ItemCode AND M1.DueMonth = @YM1
LEFT JOIN TData M2 ON M2.ItemCode = IData.ItemCode AND M2.DueMonth = @YM2
LEFT JOIN TData M3 ON M3.ItemCode = IData.ItemCode AND M3.DueMonth = @YM3
LEFT JOIN TData M4 ON M4.ItemCode = IData.ItemCode AND M4.DueMonth = @YM4
LEFT JOIN TData M5 ON M5.ItemCode = IData.ItemCode AND M5.DueMonth = @YM5
LEFT JOIN TData M6 ON M6.ItemCode = IData.ItemCode AND M6.DueMonth = @YM6
LEFT JOIN TData M7 ON M7.ItemCode = IData.ItemCode AND M7.DueMonth = @YM7
LEFT JOIN TData M8 ON M8.ItemCode = IData.ItemCode AND M8.DueMonth = @YM8

ORDER BY ItemCode, [Data Type]
OPTION (MAXRECURSION 100);
