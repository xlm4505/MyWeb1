SET NOCOUNT ON;

-- =====================================
-- Base Purchase Data
-- =====================================
IF OBJECT_ID('tempdb..#P_BaseData') IS NOT NULL
    DROP TABLE #P_BaseData;

SELECT
    ItemCode,
    CASE
        WHEN DueMonth < @YM0 THEN @YM0
        WHEN DueMonth > @YM8 THEN @YM8
        ELSE DueMonth
    END AS DueMonth,
    SUM(OpenQty)    AS OpenQty,
    SUM(InTransit)  AS InTransit,
    SUM(NotBooked)  AS NotBooked
INTO #P_BaseData
FROM (
    SELECT
        PO_PurchaseOrderDetail.ItemCode,
        CASE
            WHEN Katsuo.PromiseDate IS NULL OR Katsuo.PromiseDate = '1900-01-01'
                THEN LEFT(CONVERT(VARCHAR, PO_PurchaseOrderDetail.RequiredDate, 23), 7)
            ELSE LEFT(CONVERT(VARCHAR, Katsuo.PromiseDate, 23), 7)
        END AS DueMonth,

        CASE
            WHEN Katsuo.OpenQty IS NULL THEN 0
            WHEN Katsuo.CumOpen < Katsuo.TotalOpen THEN 0
            WHEN QuantityOrdered - QuantityReceived < Katsuo.TotalOpen THEN 0
            ELSE QuantityOrdered - QuantityReceived - Katsuo.TotalOpen
        END AS InTransit,

        CASE
            WHEN PromiseDate IS NULL THEN QuantityOrdered - QuantityReceived
            WHEN PromiseDate = '1900-01-01' THEN COALESCE(Katsuo.OpenQty, 0)
            ELSE 0
        END AS NotBooked,

        CASE
            WHEN PromiseDate IS NULL OR PromiseDate = '1900-01-01' THEN 0
            ELSE Katsuo.OpenQty
        END AS OpenQty
    FROM PO_PurchaseOrderHeader
    LEFT JOIN PO_PurchaseOrderDetail
        ON PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo
    LEFT JOIN (
        SELECT *,
            SUM(OpenQty) OVER (PARTITION BY PurchaseOrderNo ORDER BY PromiseDate DESC) AS CumOpen,
            SUM(OpenQty) OVER (PARTITION BY PurchaseOrderNo) AS TotalOpen
        FROM U_Katsuo
    ) Katsuo
        ON Katsuo.PurchaseOrderNo =
           PO_PurchaseOrderHeader.PurchaseOrderNo + '-' + RIGHT(PO_PurchaseOrderDetail.LineKey, 2)
    WHERE
        PO_PurchaseOrderDetail.QuantityOrdered > PO_PurchaseOrderDetail.QuantityReceived
        AND OrderStatus IN ('N','B','O')
) D
GROUP BY
    ItemCode,
    CASE
        WHEN DueMonth < @YM0 THEN @YM0
        WHEN DueMonth > @YM8 THEN @YM8
        ELSE DueMonth
    END;

-- =====================================
-- Sales Order
-- =====================================
WITH SData AS (
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
            LEFT(CONVERT(VARCHAR,
                COALESCE(UDF_PUSHOUT, UDF_REQUEST_DATE, PromiseDate), 23), 7) AS DueMonth,
            CASE
                WHEN SO_SalesOrderHeader.OrderStatus IN ('O','N')
                    THEN QuantityOrdered
                ELSE COALESCE(QuantityBackordered,0)
            END AS OpenQty
        FROM SO_SalesOrderDetail
        LEFT JOIN SO_SalesOrderHeader
            ON SO_SalesOrderDetail.SalesOrderNo = SO_SalesOrderHeader.SalesOrderNo
        WHERE LEFT(ItemCode,1) <> '/'
    ) S
    GROUP BY
        ItemCode,
        CASE
            WHEN DueMonth < @YM0 THEN @YM0
            WHEN DueMonth > @YM8 THEN @YM8
            ELSE DueMonth
        END
),

PData AS (
    SELECT ItemCode, DueMonth, SUM(OpenQty) AS POpenQty
    FROM #P_BaseData
    WHERE OpenQty > 0
    GROUP BY ItemCode, DueMonth
),

NotData AS (
    SELECT ItemCode, DueMonth, SUM(NotBooked) AS NotBooked
    FROM #P_BaseData
    WHERE NotBooked > 0
    GROUP BY ItemCode, DueMonth
),

IntData AS (
    SELECT ItemCode, DueMonth, SUM(InTransit) AS InTransit
    FROM #P_BaseData
    WHERE InTransit > 0
    GROUP BY ItemCode, DueMonth
),

WData AS (
    SELECT
        ItemCode,
        SUM(QuantityOnHand) AS OnHand,
        SUM(QuantityOnPurchaseOrder) AS PurchaseOrder,
        SUM(QuantityOnSalesOrder + QuantityOnBackOrder) AS SalesOrder
    FROM IM_ItemWarehouse
    GROUP BY ItemCode
),

KData AS (
    SELECT I.ItemCode, D.DueMonth
    FROM (
        SELECT ItemCode FROM SData
        UNION
        SELECT ItemCode FROM #P_BaseData
        UNION
        SELECT ItemCode FROM WData
    ) I
    CROSS JOIN (
        SELECT @YM0 AS DueMonth UNION ALL SELECT @YM1 UNION ALL SELECT @YM2
        UNION ALL SELECT @YM3 UNION ALL SELECT @YM4 UNION ALL SELECT @YM5
        UNION ALL SELECT @YM6 UNION ALL SELECT @YM7 UNION ALL SELECT @YM8
    ) D
),

TData AS (
    SELECT
        K.ItemCode,
        K.DueMonth,
        COALESCE(W.OnHand,0)
        - SUM(COALESCE(S.SOpenQty,0)) OVER (PARTITION BY K.ItemCode ORDER BY K.DueMonth)
        + SUM(
            COALESCE(P.POpenQty,0)
          + COALESCE(I.InTransit,0)
          + COALESCE(N.NotBooked,0)
        ) OVER (PARTITION BY K.ItemCode ORDER BY K.DueMonth) AS Stock
    FROM KData K
    LEFT JOIN WData W ON W.ItemCode = K.ItemCode
    LEFT JOIN SData S ON S.ItemCode = K.ItemCode AND S.DueMonth = K.DueMonth
    LEFT JOIN PData P ON P.ItemCode = K.ItemCode AND P.DueMonth = K.DueMonth
    LEFT JOIN IntData I ON I.ItemCode = K.ItemCode AND I.DueMonth = K.DueMonth
    LEFT JOIN NotData N ON N.ItemCode = K.ItemCode AND N.DueMonth = K.DueMonth
)

-- =====================================
-- Final Output (M0 ～ M8)
-- =====================================
SELECT
    K.ItemCode,
    'Inventory Forecast By Month' AS [Data Type],
    CAST(COALESCE(T0.Stock,0) AS INT) AS M0,
    CAST(COALESCE(T1.Stock,0) AS INT) AS M1,
    CAST(COALESCE(T2.Stock,0) AS INT) AS M2,
    CAST(COALESCE(T3.Stock,0) AS INT) AS M3,
    CAST(COALESCE(T4.Stock,0) AS INT) AS M4,
    CAST(COALESCE(T5.Stock,0) AS INT) AS M5,
    CAST(COALESCE(T6.Stock,0) AS INT) AS M6,
    CAST(COALESCE(T7.Stock,0) AS INT) AS M7,
    CAST(COALESCE(T8.Stock,0) AS INT) AS M8
FROM KData K
LEFT JOIN TData T0 ON T0.ItemCode = K.ItemCode AND T0.DueMonth = @YM0
LEFT JOIN TData T1 ON T1.ItemCode = K.ItemCode AND T1.DueMonth = @YM1
LEFT JOIN TData T2 ON T2.ItemCode = K.ItemCode AND T2.DueMonth = @YM2
LEFT JOIN TData T3 ON T3.ItemCode = K.ItemCode AND T3.DueMonth = @YM3
LEFT JOIN TData T4 ON T4.ItemCode = K.ItemCode AND T4.DueMonth = @YM4
LEFT JOIN TData T5 ON T5.ItemCode = K.ItemCode AND T5.DueMonth = @YM5
LEFT JOIN TData T6 ON T6.ItemCode = K.ItemCode AND T6.DueMonth = @YM6
LEFT JOIN TData T7 ON T7.ItemCode = K.ItemCode AND T7.DueMonth = @YM7
LEFT JOIN TData T8 ON T8.ItemCode = K.ItemCode AND T8.DueMonth = @YM8
ORDER BY ItemCode;
