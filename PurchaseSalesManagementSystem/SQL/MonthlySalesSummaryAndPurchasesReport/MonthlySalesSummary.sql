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
        AND IM_ItemTransactionHistory.TransactionDate BETWEEN DATEFROMPARTS(@YYYY, 1, 1) AND DATEFROMPARTS(@YYYY, 12, 31) 
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
            , COALESCE(Q01, 0) AS Qty1
            , COALESCE(A01, 0) AS Amt1
            , COALESCE(Q02, 0) AS Qty2
            , COALESCE(A02, 0) AS Amt2
            , COALESCE(Q03, 0) AS Qty3
            , COALESCE(A03, 0) AS Amt3
            , COALESCE(Q04, 0) AS Qty4
            , COALESCE(A04, 0) AS Amt4
            , COALESCE(Q05, 0) AS Qty5
            , COALESCE(A05, 0) AS Amt5
            , COALESCE(Q06, 0) AS Qty6
            , COALESCE(A06, 0) AS Amt6
            , COALESCE(Q07, 0) AS Qty7
            , COALESCE(A07, 0) AS Amt7
            , COALESCE(Q08, 0) AS Qty8
            , COALESCE(A08, 0) AS Amt8
            , COALESCE(Q09, 0) AS Qty9
            , COALESCE(A09, 0) AS Amt9
            , COALESCE(Q10, 0) AS Qty10
            , COALESCE(A10, 0) AS Amt10
            , COALESCE(Q11, 0) AS Qty11
            , COALESCE(A11, 0) AS Amt11
            , COALESCE(Q12, 0) AS Qty12
            , COALESCE(A12, 0) AS Amt12 
        FROM
            ( 
                SELECT
                    'Q' + RIGHT (
                        '00' + CAST(Month (TransactionDate) AS VARCHAR)
                        , 2
                    ) AS InvDate
                    , ItemCode
                    , Quantity 
                FROM
                    ARData 
                UNION ALL 
                SELECT
                    'A' + RIGHT ( 
                        '00' + CAST(Month (TransactionDate) AS VARCHAR)
                        , 2
                    ) AS InvDate
                    , ItemCode
                    , TotalAmount 
                FROM
                    ARData
            ) AS IMDATA PIVOT( 
                SUM(Quantity) FOR InvDate IN ( 
                    Q01
                    , A01
                    , Q02
                    , A02
                    , Q03
                    , A03
                    , Q04
                    , A04
                    , Q05
                    , A05
                    , Q06
                    , A06
                    , Q07
                    , A07
                    , Q08
                    , A08
                    , Q09
                    , A09
                    , Q10
                    , A10
                    , Q11
                    , A11
                    , Q12
                    , A12
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