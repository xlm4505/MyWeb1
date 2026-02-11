
WITH SData AS (
    SELECT
        D.ItemCode,
        CASE
            WHEN D.DueMonth < @YM0 THEN @YM0
            WHEN D.DueMonth > @YM11 THEN @YM11
            ELSE D.DueMonth
        END AS DueMonth,
        SUM(D.OpenQty) AS OpenQty
    FROM (
        SELECT
            ItemCode,
            CASE
                WHEN UDF_PUSHOUT IS NOT NULL AND UDF_PUSHOUT <> '1753-01-01'
                    THEN LEFT(CONVERT(varchar(10), UDF_PUSHOUT, 23), 7)
                WHEN UDF_REQUEST_DATE IS NOT NULL AND UDF_REQUEST_DATE <> '1753-01-01'
                    THEN LEFT(CONVERT(varchar(10), UDF_REQUEST_DATE, 23), 7)
                ELSE LEFT(CONVERT(varchar(10), PromiseDate, 23), 7)
            END AS DueMonth,
            CASE
                WHEN QuantityBackordered > 0 THEN QuantityBackordered
                ELSE QuantityOrdered
            END AS OpenQty
        FROM SO_SalesOrderDetail
        WHERE LEFT(ItemCode,1) <> '/'
    ) D
    GROUP BY
        D.ItemCode,
        CASE
            WHEN D.DueMonth < @YM0 THEN @YM0
            WHEN D.DueMonth > @YM11 THEN @YM11
            ELSE D.DueMonth
        END
),
WData AS (
    SELECT
        ItemCode,
        SUM(QuantityOnHand) AS TotalOnHand,
        SUM(CASE WHEN WarehouseCode IN ('STR', 'STX', 'UTX', 'RJP', 'MOT', 'MOK', 'SCP', 'PAA', 'PAS', 'UGP', 'TIS', 'XUS', 'PAX', '000', 'BSR')   
            THEN QuantityOnHand                                                                                          
            ELSE 0 END) AS OnHold,
        SUM(QuantityOnPurchaseOrder) AS PurchaseOpen,
        SUM(CASE WHEN WarehouseCode IN ('STR', 'STX', 'UTX', 'RJP', 'MOT', 'MOK', 'SCP', 'PAA', 'PAS', 'UGP', 'TIS', 'XUS', 'PAX', '000', 'BSR')   
            THEN QuantityOnPurchaseOrder                                                                                 
            ELSE 0 END) AS PurchaseOnHold,
        SUM(QuantityOnSalesOrder + QuantityOnBackOrder) AS SalesOpen,                                                         
        SUM(CASE WHEN WarehouseCode IN ('STR', 'STX', 'UTX', 'RJP', 'MOT', 'MOK', 'SCP', 'PAA', 'PAS', 'UGP', 'TIS', 'XUS', 'PAX', '000', 'BSR')   
            THEN QuantityOnSalesOrder + QuantityOnBackOrder                                                              
            ELSE 0 END) AS SalesOnHold 
    FROM IM_ItemWarehouse
    GROUP BY ItemCode
    HAVING SUM(QuantityOnHand) <> 0 OR SUM(QuantityOnSalesOrder) <> 0 OR SUM(QuantityOnPurchaseOrder) <> 0 OR SUM(QuantityOnBackOrder) <> 0),
Bdata AS ( 
  SELECT * FROM (SELECT ItemCode FROM Wdata UNION SELECT ItemCode FROM Sdata) AS UData WHERE ItemCode IN (SELECT ItemCode FROM U_ForecastItem WHERE ProcType = 'M')) 
SELECT
    B.ItemCode,
    CASE WHEN CI.UDF_ITEMDESC = '' THEN CI.ItemCodeDesc 
         ELSE CI.UDF_ITEMDESC END AS ItemCodeDesc,
    V.VendorName,
    --COALESCE(CI.LastTotalUnitCost, CI.StandardUnitCost) AS UnitCost,
    CASE WHEN COALESCE(CI.LastTotalUnitCost,0) = 0 THEN StandardUnitCost 
         ELSE CI.LastTotalUnitCost END AS UnitCost, 
    CAST(W.TotalOnHand AS int) AS OnHand1,
    CAST(W.PurchaseOpen AS int) AS OpenPO1,
    CAST(W.SalesOpen AS int) AS OpenSO1,
    CAST(W.TotalOnHand + W.PurchaseOpen - W.SalesOpen AS int) AS Surplus1,
    CAST(W.OnHold AS int) AS OnHand2,
    CAST(W.PurchaseOnHold AS int) AS OpenPO2,
    CAST(W.SalesOnHold AS int) AS OpenSO2,
    CAST(W.OnHold + W.PurchaseOnHold - W.SalesOnHold AS int) AS Surplus2,
    CAST((W.TotalOnHand - W.OnHold) + (W.PurchaseOpen - W.PurchaseOnHold) - (W.SalesOpen - W.SalesOnHold) AS int) AS Available, 
    CASE WHEN COALESCE(M0.OpenQty,0) = 0 THEN NULL ELSE CAST(M0.OpenQty AS int) END AS M0,
    CASE WHEN COALESCE(M1.OpenQty,0) = 0 THEN NULL ELSE CAST(M1.OpenQty AS int) END AS M1,
    CASE WHEN COALESCE(M2.OpenQty,0) = 0 THEN NULL ELSE CAST(M2.OpenQty AS int) END AS M2,
    CASE WHEN COALESCE(M3.OpenQty,0) = 0 THEN NULL ELSE CAST(M3.OpenQty AS int) END AS M3,
    CASE WHEN COALESCE(M4.OpenQty,0) = 0 THEN NULL ELSE CAST(M4.OpenQty AS int) END AS M4,
    CASE WHEN COALESCE(M5.OpenQty,0) = 0 THEN NULL ELSE CAST(M5.OpenQty AS int) END AS M5,
    CASE WHEN COALESCE(M6.OpenQty,0) = 0 THEN NULL ELSE CAST(M6.OpenQty AS int) END AS M6,
    CASE WHEN COALESCE(M7.OpenQty,0) = 0 THEN NULL ELSE CAST(M7.OpenQty AS int) END AS M7,
    CASE WHEN COALESCE(M8.OpenQty,0) = 0 THEN NULL ELSE CAST(M8.OpenQty AS int) END AS M8,
    CASE WHEN COALESCE(M9.OpenQty,0) = 0 THEN NULL ELSE CAST(M9.OpenQty AS int) END AS M9,
    CASE WHEN COALESCE(M10.OpenQty,0) = 0 THEN NULL ELSE CAST(M10.OpenQty AS int) END AS M10,
    CASE WHEN COALESCE(M11.OpenQty,0) = 0 THEN NULL ELSE CAST(M11.OpenQty AS int) END AS M11,
    CAST (COALESCE(M0.OpenQty,0)+COALESCE(M1.OpenQty,0)+COALESCE(M2.OpenQty,0)+COALESCE(M3.OpenQty,0)+COALESCE(M4.OpenQty,0)+
    COALESCE(M5.OpenQty,0)+COALESCE(M6.OpenQty,0)+COALESCE(M7.OpenQty,0)+COALESCE(M8.OpenQty,0)+
    COALESCE(M9.OpenQty,0)+COALESCE(M10.OpenQty,0)+COALESCE(M11.OpenQty,0) AS int) AS Total
FROM BData B
LEFT JOIN WData W ON W.ItemCode = B.ItemCode
LEFT JOIN CI_Item CI ON CI.ItemCode = B.ItemCode
LEFT JOIN AP_Vendor V ON CI.PrimaryAPDivisionNo = V.APDivisionNo  AND CI.PrimaryVendorNo = V.VendorNo
LEFT JOIN SData M0  ON M0.ItemCode = B.ItemCode AND M0.DueMonth = @YM0
LEFT JOIN SData M1  ON M1.ItemCode = B.ItemCode AND M1.DueMonth = @YM1
LEFT JOIN SData M2  ON M2.ItemCode = B.ItemCode AND M2.DueMonth = @YM2
LEFT JOIN SData M3  ON M3.ItemCode = B.ItemCode AND M3.DueMonth = @YM3
LEFT JOIN SData M4  ON M4.ItemCode = B.ItemCode AND M4.DueMonth = @YM4
LEFT JOIN SData M5  ON M5.ItemCode = B.ItemCode AND M5.DueMonth = @YM5
LEFT JOIN SData M6  ON M6.ItemCode = B.ItemCode AND M6.DueMonth = @YM6
LEFT JOIN SData M7  ON M7.ItemCode = B.ItemCode AND M7.DueMonth = @YM7
LEFT JOIN SData M8  ON M8.ItemCode = B.ItemCode AND M8.DueMonth = @YM8
LEFT JOIN SData M9  ON M9.ItemCode = B.ItemCode AND M9.DueMonth = @YM9
LEFT JOIN SData M10 ON M10.ItemCode = B.ItemCode AND M10.DueMonth = @YM10
LEFT JOIN SData M11 ON M11.ItemCode = B.ItemCode AND M11.DueMonth = @YM11
ORDER BY B.ItemCode;
