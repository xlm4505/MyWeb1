WITH SData AS ( 
    SELECT
        ItemCode
        , CASE 
            WHEN DueMonth <  @YM0
                THEN  @YM0 
            WHEN DueMonth >  @YM8 
                THEN  @YM8 
            ELSE DueMonth 
            END AS DueMonth
        , SUM(OpenQty) AS SOpenQty 
    FROM
        ( 
            SELECT
                ItemCode
                , CASE 
                    WHEN UDF_PUSHOUT IS NOT NULL 
                    AND UDF_PUSHOUT <> '01/01/1753' 
                        THEN LEFT(CONVERT(VARCHAR, UDF_PUSHOUT, 20), 7) 
                    WHEN UDF_REQUEST_DATE IS NOT NULL 
                    AND UDF_REQUEST_DATE <> '01/01/1753' 
                        THEN LEFT(CONVERT(VARCHAR, UDF_REQUEST_DATE, 20), 7) 
                    ELSE LEFT(CONVERT(VARCHAR, PromiseDate, 20), 7) 
                    END AS DueMonth
                , CASE 
                    WHEN SO_SalesOrderHeader.OrderStatus IN ('O', 'N') 
                        THEN SO_SalesOrderDetail.QuantityOrdered 
                    ELSE CASE 
                        WHEN COALESCE(SO_SalesOrderDetail.QuantityBackordered, 0) < 0 
                            THEN 0 
                        ELSE COALESCE(SO_SalesOrderDetail.QuantityBackordered, 0) 
                        END 
                    END AS OpenQty 
            FROM
                SO_SalesOrderDetail 
                LEFT JOIN SO_SalesOrderHeader 
                    ON SO_SalesOrderDetail.SalesOrderNo = SO_SalesOrderHeader.SalesOrderNo 
            WHERE
                LEFT(ItemCode, 1) <> '/' 
                AND CASE 
                    WHEN QuantityBackordered > 0 
                        THEN QuantityBackordered 
                    ELSE QuantityOrdered 
                    END <> 0
        ) AS DATA 
    GROUP BY
        ItemCode
        , CASE 
            WHEN DueMonth <  @YM0
                THEN  @YM0
            WHEN DueMonth >  @YM8
                THEN  @YM8
            ELSE DueMonth 
            END
) 
, PData AS ( 
    SELECT
        ItemCode
        , CASE 
            WHEN DueMonth <  @YM0
                THEN  @YM0
            WHEN DueMonth >  @YM8
                THEN  @YM8 
            ELSE DueMonth 
            END AS DueMonth
        , SUM(OpenQty) AS POpenQty 
    FROM
        ( 
            SELECT
                ItemCode
                , LEFT(CONVERT(VARCHAR, RequiredDate, 20), 7) AS DueMonth
                , CASE 
                    WHEN OrderStatus = 'B' 
                        THEN QuantityBackordered 
                    ELSE QuantityOrdered 
                    END AS OpenQty 
            FROM
                PO_PurchaseOrderDetail 
                LEFT JOIN PO_PurchaseOrderHeader 
                    ON PO_PurchaseOrderHeader.PurchaseOrderNo = PO_PurchaseOrderDetail.PurchaseOrderNo 
            WHERE
                LEFT(ItemCode, 1) <> '/' 
                AND OrderStatus <> 'X' 
                AND CASE 
                    WHEN OrderStatus = 'B' 
                        THEN QuantityBackordered 
                    ELSE QuantityOrdered 
                    END <> 0
        ) AS DATA 
    GROUP BY
        ItemCode
        , CASE 
            WHEN DueMonth < @YM0
                THEN @YM0 
            WHEN DueMonth > @YM8
                THEN  @YM8
            ELSE DueMonth 
            END
) 
, WData AS ( 
    SELECT
        ItemCode
        , SUM(QuantityOnHand) AS OnHand
        , SUM(QuantityOnPurchaseOrder) AS PurchaseOrder
        , SUM(QuantityOnSalesOrder + QuantityOnBackOrder) AS SalesOrder 
    FROM
        IM_ItemWarehouse 
    GROUP BY
        ItemCode 
    HAVING
        SUM(QuantityOnHand) <> 0 
        OR SUM(QuantityOnSalesOrder) <> 0 
        OR SUM(QuantityOnPurchaseOrder) <> 0 
        OR SUM(QuantityOnBackOrder) <> 0
) 
, Dates AS ( 
    SELECT
        DATEADD(MONTH, - 1, GETDATE()) AS Date 
    UNION ALL 
    SELECT
        DATEADD(MONTH, 1, Date) AS Date 
    FROM
        Dates 
    WHERE
        Date < DATEADD(MONTH, 7, GETDATE())
) 
, KData AS ( 
    SELECT
        * 
    FROM
        ( 
            SELECT
                ItemCode 
            FROM
                SData 
            UNION 
            SELECT
                ItemCode 
            FROM
                PData 
            UNION 
            SELECT
                ItemCode 
            FROM
                WData
        ) AS Item
        , ( 
            SELECT
                LEFT(CONVERT(VARCHAR, Date, 20), 7) AS DueMonth 
            FROM
                Dates
        ) AS DATEPARM
) 
, TData AS ( 
    SELECT
        KData.ItemCode
        , KData.DueMonth
        , 
        /* COALESCE(OnHand,0) AS OnHand, */
        /* COALESCE(SOpenQty,0) AS SOpenQty, */
        /* SUM(COALESCE(SOpenQty,0)) OVER (PARTITION BY KData.ItemCode ORDER BY KData.DueMonth) AS SalesCum, */
        /* COALESCE(POpenQty,0) AS POpenQty, */
        /* SUM(COALESCE(POpenQty,0)) OVER (PARTITION BY KData.ItemCode ORDER BY KData.DueMonth) AS PurchaseCum, */
        COALESCE(OnHand, 0) - SUM(COALESCE(SOpenQty, 0)) OVER ( 
            PARTITION BY
                KData.ItemCode 
            ORDER BY
                KData.DueMonth
        ) + SUM(COALESCE(POpenQty, 0)) OVER ( 
            PARTITION BY
                KData.ItemCode 
            ORDER BY
                KData.DueMonth
        ) AS Stock 
    FROM
        KData 
        LEFT JOIN WData 
            ON KData.ItemCode = WData.ItemCode 
        LEFT JOIN SData 
            ON KData.ItemCode = SData.ItemCode 
            AND KData.DueMonth = SData.DueMonth 
        LEFT JOIN PData 
            ON KData.ItemCode = PData.ItemCode 
            AND KData.DueMonth = PData.DueMonth
) 
, IData As ( 
    SELECT
        ItemKey.ItemCode
        , CASE 
            WHEN CI_Item.UDF_ITEMDESC = '' 
                THEN CI_Item.ItemCodeDesc 
            ELSE CI_Item.UDF_ITEMDESC 
            END AS ItemCodeDesc
        , ItemNo
        , Category1
        , VendorName
        , CASE 
            WHEN COALESCE(LastTotalUnitCost, 0) = 0 
                THEN StandardUnitCost 
            ELSE LastTotalUnitCost 
            END AS UnitCost
        , CAST(COALESCE(WData.OnHand, 0) AS int) AS OnHand
        , CAST(COALESCE(WData.PurchaseOrder, 0) AS int) AS PurchaseOrder
        , CAST(COALESCE(WData.SalesOrder, 0) AS int) AS SalesOrder
        , CAST( 
            COALESCE(WData.OnHand, 0) + COALESCE(WData.PurchaseOrder, 0) - COALESCE(WData.SalesOrder, 0) AS int
        ) AS Surplus 
    FROM
        ( 
            SELECT
                ItemCode 
            FROM
                SData 
            UNION 
            SELECT
                ItemCode 
            FROM
                PData 
            UNION 
            SELECT
                ItemCode 
            FROM
                WData
        ) AS ItemKey 
        LEFT JOIN WData 
            ON ItemKey.ItemCode = WData.ItemCode 
        LEFT JOIN CI_Item 
            ON CI_Item.ItemCode = ItemKey.ItemCode 
        LEFT JOIN AP_Vendor 
            ON PrimaryAPDivisionNo = APDivisionNo 
            AND PrimaryVendorNo = VendorNo 
        LEFT JOIN U_ForecastItem 
            ON U_ForecastItem.ItemCode = ItemKey.ItemCode
) 
SELECT
    IData.*
    , '1. Purchase Order' AS [Data Type]
    , CASE 
        WHEN COALESCE(M01.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M01.POpenQty AS int) 
        END AS M1
    , CASE 
        WHEN COALESCE(M02.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M02.POpenQty AS int) 
        END AS M2
    , CASE 
        WHEN COALESCE(M03.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M03.POpenQty AS int) 
        END AS M3
    , CASE 
        WHEN COALESCE(M04.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M04.POpenQty AS int) 
        END AS M4
    , CASE 
        WHEN COALESCE(M05.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M05.POpenQty AS int) 
        END AS M5
    , CASE 
        WHEN COALESCE(M06.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M06.POpenQty AS int) 
        END AS M6
    , CASE 
        WHEN COALESCE(M07.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M07.POpenQty AS int) 
        END AS M7
    , CASE 
        WHEN COALESCE(M08.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M08.POpenQty AS int) 
        END AS M8
    , CASE 
        WHEN COALESCE(M09.POpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M09.POpenQty AS int) 
        END AS M9
    , CAST( 
        COALESCE(M01.POpenQty, 0) + COALESCE(M02.POpenQty, 0) + COALESCE(M03.POpenQty, 0) + COALESCE(M04.POpenQty, 0)
         + COALESCE(M05.POpenQty, 0) + COALESCE(M06.POpenQty, 0) + COALESCE(M07.POpenQty, 0) + COALESCE(M08.POpenQty, 0)
         + COALESCE(M09.POpenQty, 0) AS int
    ) AS [Total] 
FROM
    IData 
    LEFT JOIN PData AS M01 
        ON M01.ItemCode = IData.ItemCode 
        AND M01.DueMonth = @YM0
    LEFT JOIN PData AS M02 
        ON M02.ItemCode = IData.ItemCode 
        AND M02.DueMonth = @YM1
    LEFT JOIN PData AS M03 
        ON M03.ItemCode = IData.ItemCode 
        AND M03.DueMonth = @YM2
    LEFT JOIN PData AS M04 
        ON M04.ItemCode = IData.ItemCode 
        AND M04.DueMonth = @YM3
    LEFT JOIN PData AS M05 
        ON M05.ItemCode = IData.ItemCode 
        AND M05.DueMonth = @YM4
    LEFT JOIN PData AS M06 
        ON M06.ItemCode = IData.ItemCode 
        AND M06.DueMonth = @YM5
    LEFT JOIN PData AS M07 
        ON M07.ItemCode = IData.ItemCode 
        AND M07.DueMonth = @YM6
    LEFT JOIN PData AS M08 
        ON M08.ItemCode = IData.ItemCode 
        AND M08.DueMonth = @YM7
    LEFT JOIN PData AS M09 
        ON M09.ItemCode = IData.ItemCode 
        AND M09.DueMonth = @YM8
UNION ALL 
SELECT
    IData.*
    , '2. Sales Order' AS [Data Type]
    , CASE 
        WHEN COALESCE(M01.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M01.SOpenQty AS int) 
        END AS M1
    , CASE 
        WHEN COALESCE(M02.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M02.SOpenQty AS int) 
        END AS M2
    , CASE 
        WHEN COALESCE(M03.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M03.SOpenQty AS int) 
        END AS M3
    , CASE 
        WHEN COALESCE(M04.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M04.SOpenQty AS int) 
        END AS M4
    , CASE 
        WHEN COALESCE(M05.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M05.SOpenQty AS int) 
        END AS M5
    , CASE 
        WHEN COALESCE(M06.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M06.SOpenQty AS int) 
        END AS M6
    , CASE 
        WHEN COALESCE(M07.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M07.SOpenQty AS int) 
        END AS M7
    , CASE 
        WHEN COALESCE(M08.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M08.SOpenQty AS int) 
        END AS M8
    , CASE 
        WHEN COALESCE(M09.SOpenQty, 0) = 0 
            THEN NULL 
        ELSE CAST(M09.SOpenQty AS int) 
        END AS M9
    , CAST( 
        COALESCE(M01.SOpenQty, 0) + COALESCE(M02.SOpenQty, 0) + COALESCE(M03.SOpenQty, 0) + COALESCE(M04.SOpenQty, 0)
         + COALESCE(M05.SOpenQty, 0) + COALESCE(M06.SOpenQty, 0) + COALESCE(M07.SOpenQty, 0) + COALESCE(M08.SOpenQty, 0)
         + COALESCE(M09.SOpenQty, 0) AS int
    ) AS [Total] 
FROM
    IData 
    LEFT JOIN SData AS M01 
        ON M01.ItemCode = IData.ItemCode 
        AND M01.DueMonth = @YM0
    LEFT JOIN SData AS M02 
        ON M02.ItemCode = IData.ItemCode 
        AND M02.DueMonth = @YM1
    LEFT JOIN SData AS M03 
        ON M03.ItemCode = IData.ItemCode 
        AND M03.DueMonth = @YM2
    LEFT JOIN SData AS M04 
        ON M04.ItemCode = IData.ItemCode 
        AND M04.DueMonth = @YM3
    LEFT JOIN SData AS M05 
        ON M05.ItemCode = IData.ItemCode 
        AND M05.DueMonth = @YM4
    LEFT JOIN SData AS M06 
        ON M06.ItemCode = IData.ItemCode 
        AND M06.DueMonth = @YM5
    LEFT JOIN SData AS M07 
        ON M07.ItemCode = IData.ItemCode 
        AND M07.DueMonth = @YM6
    LEFT JOIN SData AS M08 
        ON M08.ItemCode = IData.ItemCode 
        AND M08.DueMonth = @YM7
    LEFT JOIN SData AS M09 
        ON M09.ItemCode = IData.ItemCode 
        AND M09.DueMonth = @YM8
UNION ALL 
SELECT
    IData.*
    , '3. Inventory' AS [Data Type]
    , CAST(COALESCE(M01.Stock, 0) AS int) AS M1
    , CAST(COALESCE(M02.Stock, 0) AS int) AS M2
    , CAST(COALESCE(M03.Stock, 0) AS int) AS M3
    , CAST(COALESCE(M04.Stock, 0) AS int) AS M4
    , CAST(COALESCE(M05.Stock, 0) AS int) AS M5
    , CAST(COALESCE(M06.Stock, 0) AS int) AS M6
    , CAST(COALESCE(M07.Stock, 0) AS int) AS M7
    , CAST(COALESCE(M08.Stock, 0) AS int) AS M8
    , CAST(COALESCE(M09.Stock, 0) AS int) AS M9
    , CAST(COALESCE(M09.Stock, 0) AS int) AS [Total] 
FROM
    IData 
    LEFT JOIN TData AS M01 
        ON M01.ItemCode = IData.ItemCode 
        AND M01.DueMonth = @YM0 
    LEFT JOIN TData AS M02 
        ON M02.ItemCode = IData.ItemCode 
        AND M02.DueMonth = @YM1 
    LEFT JOIN TData AS M03 
        ON M03.ItemCode = IData.ItemCode 
        AND M03.DueMonth = @YM2
    LEFT JOIN TData AS M04 
        ON M04.ItemCode = IData.ItemCode 
        AND M04.DueMonth = @YM3 
    LEFT JOIN TData AS M05 
        ON M05.ItemCode = IData.ItemCode 
        AND M05.DueMonth = @YM4
    LEFT JOIN TData AS M06 
        ON M06.ItemCode = IData.ItemCode 
        AND M06.DueMonth = @YM5
    LEFT JOIN TData AS M07 
        ON M07.ItemCode = IData.ItemCode 
        AND M07.DueMonth = @YM6
    LEFT JOIN TData AS M08 
        ON M08.ItemCode = IData.ItemCode 
        AND M08.DueMonth = @YM7
    LEFT JOIN TData AS M09 
        ON M09.ItemCode = IData.ItemCode 
        AND M09.DueMonth = @YM8
ORDER BY
    ItemCode
    , [Data Type];
