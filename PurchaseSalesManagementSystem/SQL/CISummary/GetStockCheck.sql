SELECT
    ROW_NUMBER() OVER (ORDER BY CIData.ItemCode, Whse) AS No,
    CIData.ItemCode,
    Whse,
    COALESCE(Stock_Qty, 0) AS Qty_Current,
    SUM(Ship) AS Ship,
    SUM(ITOut) AS IT_Out,
    SUM(ITIn) AS IT_In,
    COALESCE(Stock_Qty, 0) - SUM(Ship + ITOut - ITIn) AS Qty_After,
    CASE
        WHEN COALESCE(Stock_Qty, 0) - SUM(Ship + ITOut - ITIn) < 0
            THEN 'Not enough in stock. '
        ELSE ''
    END + CASE
        WHEN SUM(PaceIT) > 0
            THEN '(Excluded ' + CAST(SUM(PaceIT) AS VARCHAR) + ' pcs for Pace IT)'
        ELSE ''
    END AS Comment
FROM (
    SELECT
        EntryDate,
        ItemCode,
        Whse1 AS Whse,
        CASE WHEN DocType = 'CI' THEN TranQty ELSE 0 END AS Ship,
        CASE
            WHEN DocType LIKE 'IT%' THEN
                CASE
                    WHEN Whse1 IN ('STX', 'UGP', 'UTX') AND Whse2 IN ('PAA', 'PAX', 'PAS') THEN 0
                    ELSE TranQty
                END
            ELSE 0
        END AS ITOut,
        0 AS ITIn,
        CASE
            WHEN Whse1 IN ('STX', 'UGP', 'UTX') AND Whse2 IN ('PAA', 'PAX', 'PAS') THEN TranQty
            ELSE 0
        END AS PaceIT
    FROM U_CIDetailData
    UNION ALL
    SELECT
        EntryDate,
        ItemCode,
        Whse2,
        0,
        0,
        TranQty,
        0
    FROM U_CIDetailData
    WHERE DocType = 'IT2'
) AS CIData
LEFT JOIN (
    SELECT
        ItemCode,
        WarehouseCode,
        SUM(Qty) AS Stock_Qty
    FROM U_RAInventory
    GROUP BY ItemCode, WarehouseCode
) AS RAStock
    ON RAStock.ItemCode = CIData.ItemCode
    AND RAStock.WarehouseCode = CIData.Whse
WHERE EntryDate = CONVERT(date, GETDATE())
GROUP BY CIData.ItemCode, Whse, Stock_Qty
ORDER BY CIData.ItemCode, Whse;
