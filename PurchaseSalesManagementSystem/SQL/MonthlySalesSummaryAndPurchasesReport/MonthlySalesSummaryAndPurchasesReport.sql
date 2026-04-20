SET
    NOCOUNT 
        ON IF OBJECT_ID('tempdb..#ProjItems') IS NOT NULL DROP TABLE #ProjItems 
SELECT
    * 
INTO #ProjItems 
FROM
    ( 
        SELECT
            ItemCode 
        FROM
            IM_ItemWhseHistoryByPeriod 
        WHERE
            FiscalCalYear >= @YYYY 
        UNION 
        SELECT
            ItemCode 
        FROM
            IM_ItemWarehouse 
        WHERE
            QuantityOnHand <> 0
    ) AS ItemData; 

WITH KData AS ( 
    SELECT
        #ProjItems.ItemCode
        , CASE 
            WHEN CHARINDEX('(Pack of', CI_Item.UDF_ITEMDESC) > 0 
                THEN 
        LEFT ( 
            CI_Item.UDF_ITEMDESC
            , CHARINDEX('(Pack of', CI_Item.UDF_ITEMDESC) - 1
        ) 
        ELSE CI_Item.UDF_ITEMDESC 
        END AS ItemCodeDesc
        , CASE 
        WHEN Category1 IN ( 
            'CLEAN PACK'
            , 'CONC METER'
            , 'FCS CABLE'
            , 'FLOW DIAGN'
            , 'LABEL'
            , 'LV ADAPTOR'
            , 'MASS FLOW'
            , 'PRINTER'
            , 'PRS CNTLR'
            , 'VAPORIZER'
        ) 
            THEN 'Mass Flow' 
        WHEN Category1 = 'VALVE MOD' 
            THEN 'Valve Module' 
        WHEN Category1 IN ('CHK VALVE', 'VALVE', 'VALVE PART', 'REGULATOR') 
            THEN 'Valve' 
        WHEN Category1 IN ('BASE BLOCK', 'FITTING', 'FILTER', 'GASKET') 
            THEN 'Fitting' 
        ELSE 'Others' 
        END AS [PrdsGroup]
        , PrimaryAPDivisionNo + '-' + PrimaryVendorNo AS [VenderNo]
        , U_ForecastItem.ItemNo
        , Category1 AS Catergory
        , VendorName
        , DataType 
    FROM
        ( 
            SELECT
                '1.Shipped' AS DataType 
            UNION 
            SELECT
                '2.Received' 
            UNION 
            SELECT
                '3.Transfer' 
            UNION 
            SELECT
                '4.Inventory'
        ) AS DataType
        , #ProjItems 
        LEFT JOIN CI_Item 
            ON CI_Item.ItemCode = #ProjItems.ItemCode 
        LEFT JOIN U_ForecastItem 
            ON U_ForecastItem.ItemCode = #ProjItems.ItemCode 
        LEFT JOIN AP_Vendor 
            ON PrimaryAPDivisionNo = APDivisionNo 
            AND VendorNo = PrimaryVendorNo
) 
, ARData AS ( 
    SELECT
          FiscalCalPeriod AS InvDate
        , IM_ItemWhseHistoryByPeriod.ItemCode
        , QuantitySold - QuantityReturnedCustomer AS Quantity
        , CostOfGoodsSold AS TotalAmount 
    FROM
        IM_ItemWhseHistoryByPeriod 
        LEFT JOIN #ProjItems 
            ON #ProjItems.ItemCode = IM_ItemWhseHistoryByPeriod.ItemCode 
    WHERE
        FiscalCalYear = @YYYY 
        AND #ProjItems.ItemCode IS NOT NULL
) 
, APData AS ( 
    SELECT
        FiscalCalPeriod AS InvDate
        , IM_ItemWhseHistoryByPeriod.ItemCode
        , QuantityReceived - QuantityReturnedVendor AS Quantity
        , CostOfGoodsReceived AS TotalAmount 
    FROM
        IM_ItemWhseHistoryByPeriod 
        LEFT JOIN #ProjItems 
            ON #ProjItems.ItemCode = IM_ItemWhseHistoryByPeriod.ItemCode 
    WHERE
        FiscalCalYear = @YYYY 
        AND #ProjItems.ItemCode IS NOT NULL
) 
, ITData AS ( 
    SELECT
        FiscalCalPeriod AS InvDate
        , IM_ItemWhseHistoryByPeriod.ItemCode
        , QuantityTransferred + QuantityIssued * - 1 + QuantityAdjusted AS Quantity
        , TransfersCost + IssuesCost * - 1 + AdjustmentsCost AS TotalAmount 
    FROM
        IM_ItemWhseHistoryByPeriod 
        LEFT JOIN #ProjItems 
            ON #ProjItems.ItemCode = IM_ItemWhseHistoryByPeriod.ItemCode 
    WHERE
        FiscalCalYear = @YYYY 
        AND #ProjItems.ItemCode IS NOT NULL
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
        ( 
            SELECT
                *
                , @YYYY AS YYYY
                , '01' AS MM 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '02' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '03' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '04' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '05' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '06' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '07' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '08' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '09' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '10' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '11' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , @YYYY
                , '12' 
            FROM
                #ProjItems 
            UNION 
            SELECT
                *
                , '9999'
                , '99' 
            FROM
                #ProjItems
        ) AS BASEDATA 
        LEFT JOIN ( 
            SELECT
                ItemCode
                , CASE 
                    WHEN FiscalCalYear > @YYYY 
                        THEN '9999' 
                    ELSE FiscalCalYear 
                    END AS FiscalCalYear
                , CASE 
                    WHEN FiscalCalYear > @YYYY 
                        THEN '99' 
                    ELSE FiscalCalPeriod 
                    END AS FiscalCalPeriod
                , SUM(TransactionQty) AS TransactionQty
                , SUM(ExtendedCost) AS ExtendedCost 
            FROM
                IM_ItemTransactionHistory 
            WHERE
                TransactionDate >= DATEFROMPARTS(@YYYY, 1, 1) 
            GROUP BY
                ItemCode
                , CASE 
                    WHEN FiscalCalYear > @YYYY 
                        THEN '9999' 
                    ELSE FiscalCalYear 
                    END
                , CASE 
                    WHEN FiscalCalYear > @YYYY 
                        THEN '99' 
                    ELSE FiscalCalPeriod 
                    END
        ) AS SumData 
            ON SumData.ItemCode = BASEDATA.ItemCode 
            AND FiscalCalYear = YYYY 
            AND FiscalCalPeriod = MM 
        LEFT JOIN CI_Item 
            ON CI_Item.ItemCode = BASEDATA.ItemCode
) 
SELECT
    KData.ItemCode
    , KData.ItemCodeDesc
    , KData.ItemNo
    , KData.[PrdsGroup]
    , KData.Catergory
    , KData.VenderNo
    , KData.VendorName
    , KData.DataType
    , CONVERT(decimal (7, 0), COALESCE(Qty1, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt1, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty2, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt2, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty3, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt3, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty4, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt4, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty5, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt5, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty6, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt6, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty7, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt7, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty8, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt8, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty9, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt9, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty10, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt10, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty11, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt11, 0), 0) AS Amt
    , CONVERT(decimal (7, 0), COALESCE(Qty12, 0), 0) AS Qty
    , CONVERT(decimal (9, 2), COALESCE(Amt12, 0), 0) AS Amt 
FROM
    KData 
    LEFT JOIN ( 
        SELECT
            ItemCode
            , '1.Shipped' AS DataType
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
                    'Q' + InvDate AS InvDate
                    , ItemCode
                    , Quantity 
                FROM
                    ARData 
                UNION ALL 
                SELECT
                    'A' + InvDate AS InvDate
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
        UNION ALL 
        SELECT
            ItemCode
            , '2.Received' AS [Data Type]
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
                    'Q' + InvDate AS InvDate
                    , ItemCode
                    , Quantity 
                FROM
                    APData 
                UNION ALL 
                SELECT
                    'A' + InvDate AS InvDate
                    , ItemCode
                    , TotalAmount 
                FROM
                    APData
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
            ) AS APPivot 
        UNION ALL 
        SELECT
            ItemCode
            , '3.Transfer' AS [Data Type]
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
                    'Q' + InvDate AS InvDate
                    , ItemCode
                    , Quantity 
                FROM
                    ITData 
                UNION ALL 
                SELECT
                    'A' + InvDate AS InvDate
                    , ItemCode
                    , TotalAmount 
                FROM
                    ITData
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
            ) AS ITPivot 
        UNION ALL 
        SELECT
            #ProjItems.ItemCode
            , '4.Inventory' AS Datatype
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 1 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I01.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 1 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I01.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 2 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I02.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 2 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I02.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 3 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I03.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 3 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I03.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 4 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I04.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 4 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I04.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 5 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I05.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 5 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I05.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 6 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I06.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 6 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I06.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 7 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I07.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 7 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I07.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 8 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I08.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 8 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I08.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 9 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I09.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 9 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I09.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 10 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I10.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 10 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I10.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 11 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I11.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 11 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I11.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 12 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (7, 0)
                    , COALESCE(I12.[OnHand(EOM)], 0)
                    , 0
                ) 
                END
            , CASE 
                WHEN YEAR (GETDATE()) = @YYYY 
                AND MONTH (GETDATE()) < 12 
                    THEN 0 
                ELSE CONVERT( 
                    decimal (9, 2)
                    , COALESCE(I12.[TotalValue(EOM)], 0)
                    , 0
                ) 
                END 
        FROM
            #ProjItems 
            LEFT JOIN InvData AS I01 
                ON #ProjItems.ItemCode = I01.ItemCode 
                AND I01.YYYY = @YYYY 
                AND I01.MM = '01' 
            LEFT JOIN InvData AS I02 
                ON #ProjItems.ItemCode = I02.ItemCode 
                AND I02.YYYY = @YYYY 
                AND I02.MM = '02' 
            LEFT JOIN InvData AS I03 
                ON #ProjItems.ItemCode = I03.ItemCode 
                AND I03.YYYY = @YYYY 
                AND I03.MM = '03' 
            LEFT JOIN InvData AS I04 
                ON #ProjItems.ItemCode = I04.ItemCode 
                AND I04.YYYY = @YYYY 
                AND I04.MM = '04' 
            LEFT JOIN InvData AS I05 
                ON #ProjItems.ItemCode = I05.ItemCode 
                AND I05.YYYY = @YYYY 
                AND I05.MM = '05' 
            LEFT JOIN InvData AS I06 
                ON #ProjItems.ItemCode = I06.ItemCode 
                AND I06.YYYY = @YYYY 
                AND I06.MM = '06' 
            LEFT JOIN InvData AS I07 
                ON #ProjItems.ItemCode = I07.ItemCode 
                AND I07.YYYY = @YYYY 
                AND I07.MM = '07' 
            LEFT JOIN InvData AS I08 
                ON #ProjItems.ItemCode = I08.ItemCode 
                AND I08.YYYY = @YYYY 
                AND I08.MM = '08' 
            LEFT JOIN InvData AS I09 
                ON #ProjItems.ItemCode = I09.ItemCode 
                AND I09.YYYY = @YYYY 
                AND I09.MM = '09' 
            LEFT JOIN InvData AS I10 
                ON #ProjItems.ItemCode = I10.ItemCode 
                AND I10.YYYY = @YYYY 
                AND I10.MM = '10' 
            LEFT JOIN InvData AS I11 
                ON #ProjItems.ItemCode = I11.ItemCode 
                AND I11.YYYY = @YYYY 
                AND I11.MM = '11' 
            LEFT JOIN InvData AS I12 
                ON #ProjItems.ItemCode = I12.ItemCode 
                AND I12.YYYY = @YYYY 
                AND I12.MM = '12'
    ) AS PivotData 
        ON KData.ItemCode = PivotData.ItemCode 
        AND KData.DataType = PivotData.DataType 
ORDER BY
    KData.ItemCode
    , DataType
;
