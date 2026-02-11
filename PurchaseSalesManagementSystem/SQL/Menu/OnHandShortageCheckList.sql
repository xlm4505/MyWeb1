SELECT
    IW.ItemCode,
    CI.UDF_ITEMDESC AS ItemDesc,
    CI.Category1   AS Category,

    /* ===== Regular Warehouse ===== */
    CAST(SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN 0
            ELSE IW.QuantityOnHand
        END
    ) AS decimal(7,0)) AS [OnHand(Reg)],

    CAST(SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN 0
            ELSE IW.QuantityOnPurchaseOrder
        END
    ) AS decimal(7,0)) AS [OpenPO(Reg)],

    CAST(SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN 0
            ELSE IW.QuantityOnSalesOrder + IW.QuantityOnBackOrder
        END
    ) AS decimal(7,0)) AS [OpenSO(Reg)],

    CAST(SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN 0
            ELSE IW.QuantityOnHand
               + IW.QuantityOnPurchaseOrder
               - IW.QuantityOnSalesOrder
               - IW.QuantityOnBackOrder
        END
    ) AS decimal(7,0)) AS [Available(Reg)],

    /* ===== Excluded Warehouse ===== */
    CAST(SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN IW.QuantityOnHand
            ELSE 0
        END
    ) AS decimal(7,0)) AS [OnHand(Ex)],

    CAST(SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN IW.QuantityOnPurchaseOrder
            ELSE 0
        END
    ) AS decimal(7,0)) AS [OpenPO(Ex)],

    CAST(SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN IW.QuantityOnSalesOrder + IW.QuantityOnBackOrder
            ELSE 0
        END
    ) AS decimal(7,0)) AS [OpenSO(Ex)],

    CAST(SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN IW.QuantityOnHand
               + IW.QuantityOnPurchaseOrder
               - IW.QuantityOnSalesOrder
               - IW.QuantityOnBackOrder
            ELSE 0
        END
    ) AS decimal(7,0)) AS [Available(Ex)],

    /* ===== Total ===== */
    CAST(SUM(IW.QuantityOnHand) AS decimal(7,0)) AS [OnHand(Total)],
    CAST(SUM(IW.QuantityOnPurchaseOrder) AS decimal(7,0)) AS [OpenPO(Total)],
    CAST(SUM(IW.QuantityOnSalesOrder + IW.QuantityOnBackOrder) AS decimal(7,0)) AS [OpenSO(Total)],
    CAST(SUM(
        IW.QuantityOnHand
      + IW.QuantityOnPurchaseOrder
      - IW.QuantityOnSalesOrder
      - IW.QuantityOnBackOrder
    ) AS decimal(7,0)) AS [Available(Total)]

FROM
    IM_ItemWarehouse IW
    LEFT JOIN CI_Item CI
        ON IW.ItemCode = CI.ItemCode

GROUP BY
    IW.ItemCode,
    CI.UDF_ITEMDESC,
    CI.Category1

HAVING
    /* Regular */
    SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN 0
            ELSE IW.QuantityOnHand
               + IW.QuantityOnPurchaseOrder
               - IW.QuantityOnSalesOrder
               - IW.QuantityOnBackOrder
        END
    ) < 0

    OR

    /* Excluded */
    SUM(
        CASE
            WHEN IW.WarehouseCode IN
                ('STR','STX','UTX','RJP','MOT','MOK','SCP','PAA','PAS','UGP','TIS','XUS','PAX','000','BSR')
            THEN IW.QuantityOnHand
               + IW.QuantityOnPurchaseOrder
               - IW.QuantityOnSalesOrder
               - IW.QuantityOnBackOrder
            ELSE 0
        END
    ) < 0

    OR

    /* Total */
    SUM(
        IW.QuantityOnHand
      + IW.QuantityOnPurchaseOrder
      - IW.QuantityOnSalesOrder
      - IW.QuantityOnBackOrder
    ) < 0

ORDER BY
    IW.ItemCode;
