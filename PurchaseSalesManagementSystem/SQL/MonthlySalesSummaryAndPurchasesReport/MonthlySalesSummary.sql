WITH TargetItem AS ( 
    SELECT
        ItemCode
        , ItemNo 
    FROM
        U_ForecastItem 
    WHERE
        ProcType = 'M'
) 
, BASEDATA AS ( 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE())) AS YYYY
        , '01' AS MM 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '02' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '03' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '04' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '05' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '06' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '07' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '08' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '09' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '10' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '11' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '12' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '01' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '02' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '03' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '04' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '05' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '06' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '07' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '08' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '09' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '10' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '11' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 4, GETDATE()))
        , '12' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '01' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '02' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '03' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '04' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '05' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '06' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '07' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '08' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '09' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '10' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '11' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 3, GETDATE()))
        , '12' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '01' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '02' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '03' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '04' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '05' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '06' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '07' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '08' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '09' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '10' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '11' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 2, GETDATE()))
        , '12' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '01' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '02' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '03' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '04' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '05' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '06' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '07' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '08' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '09' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '10' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '11' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (DATEADD(YEAR, - 1, GETDATE()))
        , '12' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '01' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '02' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '03' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '04' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '05' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '06' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '07' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '08' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '09' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '10' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '11' 
    FROM
        TargetItem 
    UNION 
    SELECT
        *
        , YEAR (GETDATE())
        , '12' 
    FROM
        TargetItem
) 
, KData AS ( 
    SELECT
        TargetItem.ItemNo
        , TargetItem.ItemCode
        , CASE 
            WHEN CHARINDEX('(Pack of', CI_Item.UDF_ITEMDESC) > 0 
                THEN 
        LEFT ( 
            CI_Item.UDF_ITEMDESC
            , CHARINDEX('(Pack of', CI_Item.UDF_ITEMDESC) - 1
        ) 
        ELSE CI_Item.UDF_ITEMDESC 
        END AS ItemCodeDesc
        , TotalQuantityOnHand
        , [JFI(Rinku)]
        , TotalSalesOrder
        , TotalPurchaseOrder 
    FROM
        TargetItem 
        LEFT JOIN CI_Item 
            ON CI_Item.ItemCode = TargetItem.ItemCode 
        LEFT JOIN ( 
            SELECT
                ItemCode
                , SUM(QuantityOnSalesOrder + QuantityOnBackOrder) AS TotalSalesOrder
                , SUM(QuantityOnPurchaseOrder) AS TotalPurchaseOrder 
            FROM
                IM_ItemWarehouse 
            GROUP BY
                ItemCode 
            HAVING
                SUM(QuantityOnHand) <> 0 
                OR SUM(QuantityOnSalesOrder) <> 0 
                OR SUM(QuantityOnPurchaseOrder) <> 0 
                OR SUM(QuantityOnBackOrder) <> 0
        ) AS SOQ 
            ON SOQ.ItemCode = TargetItem.ItemCode 
        LEFT JOIN ( 
            SELECT
                ItemCode
                , SUM( 
                    CASE 
                        WHEN WarehouseCode = 'JFI' 
                            THEN Qty 
                        ELSE 0 
                        END
                ) AS [JFI(Rinku)] 
            FROM
                U_RAInventory 
            GROUP BY
                ItemCode
        ) AS RAI 
            ON RAI.ItemCode = TargetItem.ItemCode
) 
, ARData AS ( 
    SELECT
        IM_ItemTransactionHistory.TransactionDate
        , IM_ItemTransactionHistory.ItemCode
        , IM_ItemTransactionHistory.TransactionQty * - 1 AS Quantity
        , IM_ItemTransactionHistory.ExtendedCost * - 1 AS TotalAmount 
    FROM
        IM_ItemTransactionHistory 
        LEFT JOIN TargetItem 
            ON TargetItem.ItemCode = IM_ItemTransactionHistory.ItemCode 
    WHERE
        IM_ItemTransactionHistory.TransactionCode = 'SO' 
        AND IM_ItemTransactionHistory.TransactionDate BETWEEN '1/1/2025' AND '12/31/2025' 
        AND ( 
            IM_ItemTransactionHistory.TransactionQty <> 0 
            OR IM_ItemTransactionHistory.ExtendedCost <> 0
        ) 
        AND TargetItem.ItemCode IS NOT NULL
) 
, InvData AS ( 
    SELECT
        BASEDATA.ItemCode
        , YYYY
        , MM
        , TotalQuantityOnHand - SUM(COALESCE(TransactionQty, 0)) OVER ( 
            PARTITION BY
                BASEDATA.ItemCode 
            ORDER BY
                YYYY DESC
                , MM DESC
        ) + COALESCE(TransactionQty, 0) AS [OnHand(EOM)]
        , TotalInventoryValue - SUM(COALESCE(ExtendedCost, 0)) OVER ( 
            PARTITION BY
                BASEDATA.ItemCode 
            ORDER BY
                YYYY DESC
                , MM DESC
        ) + COALESCE(ExtendedCost, 0) AS [TotalValue(EOM)] 
    FROM
        BASEDATA 
        LEFT JOIN ( 
            SELECT
                ItemCode
                , FiscalCalYear
                , FiscalCalPeriod
                , SUM(TransactionQty) AS TransactionQty
                , SUM(ExtendedCost) AS ExtendedCost 
            FROM
                IM_ItemTransactionHistory 
            GROUP BY
                ItemCode
                , FiscalCalYear
                , FiscalCalPeriod
        ) AS SumData 
            ON SumData.ItemCode = BASEDATA.ItemCode 
            AND FiscalCalYear = YYYY 
            AND FiscalCalPeriod = MM 
        LEFT JOIN CI_Item 
            ON CI_Item.ItemCode = BASEDATA.ItemCode
) 
SELECT
    KData.ItemNo
    , KData.ItemCode
    , KData.ItemCodeDesc
    , CONVERT(decimal (7, 0), COALESCE(Qty1, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty2, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty3, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty4, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty5, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty6, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty7, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty8, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty9, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty10, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty11, 0), 0) AS Qty
    , CONVERT(decimal (7, 0), COALESCE(Qty12, 0), 0) AS Qty
    , CONVERT( 
        decimal (7, 0)
        , COALESCE(InvData.[OnHand(EOM)], 0)
    ) AS [OnHand (EOM)]
    , CONVERT( 
        decimal (7, 0)
        , COALESCE(KData.TotalQuantityOnHand, 0)
    ) AS [OnHand (Current)]
    , CONVERT(decimal (7, 0), COALESCE(KData.[JFI(Rinku)], 0)) AS [JFI (Rinku)]
    , CONVERT( 
        decimal (7, 0)
        , COALESCE(KData.TotalSalesOrder, 0)
    ) AS [SO (Current)]
    , CONVERT( 
        decimal (7, 0)
        , COALESCE(KData.TotalPurchaseOrder, 0)
    ) AS [PO (Current)] 
FROM
    KData 
    LEFT JOIN ( 
        SELECT
            ItemCode
            , COALESCE(Q202501, 0) AS Qty1
            , COALESCE(A202501, 0) AS Amt1
            , COALESCE(Q202502, 0) AS Qty2
            , COALESCE(A202502, 0) AS Amt2
            , COALESCE(Q202503, 0) AS Qty3
            , COALESCE(A202503, 0) AS Amt3
            , COALESCE(Q202504, 0) AS Qty4
            , COALESCE(A202504, 0) AS Amt4
            , COALESCE(Q202505, 0) AS Qty5
            , COALESCE(A202505, 0) AS Amt5
            , COALESCE(Q202506, 0) AS Qty6
            , COALESCE(A202506, 0) AS Amt6
            , COALESCE(Q202507, 0) AS Qty7
            , COALESCE(A202507, 0) AS Amt7
            , COALESCE(Q202508, 0) AS Qty8
            , COALESCE(A202508, 0) AS Amt8
            , COALESCE(Q202509, 0) AS Qty9
            , COALESCE(A202509, 0) AS Amt9
            , COALESCE(Q202510, 0) AS Qty10
            , COALESCE(A202510, 0) AS Amt10
            , COALESCE(Q202511, 0) AS Qty11
            , COALESCE(A202511, 0) AS Amt11
            , COALESCE(Q202512, 0) AS Qty12
            , COALESCE(A202512, 0) AS Amt12 
        FROM
            ( 
                SELECT
                    'Q' + CAST(Year (TransactionDate) AS VARCHAR) + 
                    RIGHT ( 
                        '00' + CAST(Month (TransactionDate) AS VARCHAR)
                        , 2
                    ) AS InvDate
                    , ItemCode
                    , Quantity 
                FROM
                    ARData 
                UNION ALL 
                SELECT
                    'A' + CAST(Year (TransactionDate) AS VARCHAR) + 
                    RIGHT ( 
                        '00' + CAST(Month (TransactionDate) AS VARCHAR)
                        , 2
                    ) AS InvDate
                    , ItemCode
                    , TotalAmount 
                FROM
                    ARData
            ) AS IMDATA PIVOT( 
                SUM(Quantity) FOR InvDate IN ( 
                    Q202501
                    , A202501
                    , Q202502
                    , A202502
                    , Q202503
                    , A202503
                    , Q202504
                    , A202504
                    , Q202505
                    , A202505
                    , Q202506
                    , A202506
                    , Q202507
                    , A202507
                    , Q202508
                    , A202508
                    , Q202509
                    , A202509
                    , Q202510
                    , A202510
                    , Q202511
                    , A202511
                    , Q202512
                    , A202512
                )
            ) AS ARPivot
    ) AS PivotData 
        ON KData.ItemCode = PivotData.ItemCode 
    LEFT JOIN InvData 
        ON KData.ItemCode = InvData.ItemCode 
        AND CONCAT(InvData.YYYY, InvData.MM) = CAST( 
            DATEPART(YYYY, DATEADD(M, - 1, GETDATE())) AS VARCHAR
        ) + 
    RIGHT ( 
        '00' + CAST( 
            DATEPART(M, DATEADD(M, - 1, GETDATE())) AS VARCHAR
        ) 
        , 2
    ) 
ORDER BY
    KData.ItemNo
;