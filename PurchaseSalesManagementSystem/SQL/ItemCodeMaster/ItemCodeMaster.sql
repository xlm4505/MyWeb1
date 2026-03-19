SELECT 
  CI_Item.ItemCode,
  UDF_ITEMDESC AS ItemDesc,
  UDF_IM_DESC_2 AS ItemDesc2,
  Category1 AS Category,
  ProductLineDesc,
  CASE CI_Item.ProductType
    WHEN 'F' THEN 'Finished Good'
	WHEN 'R' THEN 'Raw Material'
	WHEN 'D' THEN 'Discontinue'
	WHEN 'R' THEN 'Kit'
	ELSE '' END AS ProductType,
  CASE WHEN CI_Item.InactiveItem = 'Y' THEN '*'
  ELSE '' END AS Inactive,
  ShipWeight AS [Weight(lb)],
  DefaultWarehouseCode AS 'Whse',
  PrimaryAPDivisionNo + '-' + PrimaryVendorNo AS PrimaryVendor,
  CASE WHEN VenderCost.BreakQuantity1 < 99999999 THEN '*'
	   ELSE '' END AS QtyDisc,
  StandardUnitPrice AS StdSalesPrice,
  CI_Item.StandardUnitCost AS StdUnitCost,
  LastTotalUnitCost AS LastCost,
  AverageUnitCost AS AvgCost,
  CASE WHEN VenderCost.BreakQuantity1 Is NULL THEN 0
       WHEN VenderCost.BreakQuantity1 = 99999999 THEN VenderCost.DiscountMarkup1
	   ELSE 0 END AS [VenCost(USD)],
  CASE WHEN JPYCost.BreakQuantity1 Is NULL OR JPYCost.BreakQuantity1 <> 99999999 THEN 0
       ELSE JPYCost.DiscountMarkup1 END AS [VenCost(JPY)],

  CONVERT(decimal(7,0), COALESCE(OnHand,0)) AS OnHand,
  CONVERT(decimal(7,0), COALESCE(OpenSO,0)) AS OpenSO,
  CONVERT(decimal(7,0), COALESCE(OnHand-OpenSO,0)) AS Available,
  CONVERT(decimal(7,0), COALESCE(OpenPO,0)) AS OpenPO,
  CONVERT(decimal(7,0), COALESCE(InShip,0)) AS [(InShip)],
  CONVERT(decimal(7,0), COALESCE(OnHand_Ex,0)) AS [OnHand ],
  CONVERT(decimal(7,0), COALESCE(OpenSO_Ex,0)) AS [OpenSO ],
  CONVERT(decimal(7,0), COALESCE(OnHand_Ex-OpenSO_Ex,0)) AS [Available ],
  CONVERT(decimal(7,0), COALESCE(OpenPO_Ex,0)) AS [OpenPO ],
  CONVERT(decimal(7,0), COALESCE(InShip_Ex,0)) AS [(InShip) ],
  CASE WHEN LastSoldDate = '1/1/1753' THEN Null
       ELSE CAST(LastSoldDate AS date) END AS LastSold,
  CASE WHEN LastReceiptDate = '1/1/1753' THEN Null
       ELSE CAST(LastReceiptDate AS date) END AS LastReceipt,
  ExtendedDescriptionText,
  CAST(CI_Item.DateCreated AS date) AS DateCreated, 
  COALESCE(User1.FirstName,'') + ' ' + COALESCE(User1.LastName,'') AS UserCreated,
  CAST(CI_Item.DateUpdated AS date) AS DateUpdated, 
  User2.FirstName + ' ' + User2.LastName AS UserUpdated,
  ROUND(CASE WHEN JPYCost.BreakQuantity1 Is NULL OR JPYCost.BreakQuantity1 <> 99999999 THEN 0
             ELSE JPYCost.DiscountMarkup1 / 95 * 1.15 * 0.95 * 100 / 85 / 0.7 + JPYCost.DiscountMarkup1 / 95 * 0.08 END,2) AS [List COP],
  ROUND(CASE WHEN JPYCost.BreakQuantity1 Is NULL OR JPYCost.BreakQuantity1 <> 99999999 THEN 0
             ELSE JPYCost.DiscountMarkup1 / 95 * 1.15 * 0.95 * 100 / 85 / 0.7 * 0.9 + JPYCost.DiscountMarkup1 / 95 * 0.08 END,2) AS [Standard],
  ROUND(CASE WHEN JPYCost.BreakQuantity1 Is NULL OR JPYCost.BreakQuantity1 <> 99999999 THEN 0
             ELSE JPYCost.DiscountMarkup1 / 95 * 1.15 * 0.95 * 100 / 85 + JPYCost.DiscountMarkup1 / 95 * 0.08 END,2) AS [Discount],
  ROUND(CASE WHEN JPYCost.BreakQuantity1 Is NULL OR JPYCost.BreakQuantity1 <> 99999999 THEN 0
             ELSE JPYCost.DiscountMarkup1 / 95 * 1.15 * 0.95 * 100 / 85 / 0.7 * 0.63 + JPYCost.DiscountMarkup1 / 95 * 0.08 END,2) AS [Class 4],
  ROUND(CASE WHEN JPYCost.BreakQuantity1 Is NULL OR JPYCost.BreakQuantity1 <> 99999999 THEN 0
             ELSE JPYCost.DiscountMarkup1 / 95 * 1.15 * 0.95 * 100 / 85 / 0.7 * 0.6 + JPYCost.DiscountMarkup1 / 95 * 0.08 END,2) AS [Class 5],
  ROUND(CASE WHEN JPYCost.BreakQuantity1 Is NULL OR JPYCost.BreakQuantity1 <> 99999999 THEN 0
             ELSE JPYCost.DiscountMarkup1 / 95 * 1.15 * 0.95 + JPYCost.DiscountMarkup1 / 95 * 0.08 END,2) AS [Contract],
  ROUND(CASE WHEN JPYCost.BreakQuantity1 Is NULL OR JPYCost.BreakQuantity1 <> 99999999 THEN 0
             ELSE JPYCost.DiscountMarkup1 / 95 * 1.15 * 0.95 * 100 / 85 / 0.7 * 0.57 + JPYCost.DiscountMarkup1 / 95 * 0.08 END,2) AS [Class 6]
FROM CI_Item
LEFT JOIN (
SELECT
  ItemCode,
    SUM(CASE WHEN WarehouseCode NOT IN ('000','MOK','MOT','PAA','PAS','PAX','RJP','SCP','STR','STX','TIS','UGP','UTX','XUS') THEN QuantityOnHand
        ELSE 0 END) AS OnHand,
    SUM(CASE WHEN WarehouseCode NOT IN ('000','MOK','MOT','PAA','PAS','PAX','RJP','SCP','STR','STX','TIS','UGP','UTX','XUS') THEN QuantityOnSalesOrder+QuantityOnBackOrder
        ELSE 0 END) AS OpenSO,
    SUM(CASE WHEN WarehouseCode NOT IN ('000','MOK','MOT','PAA','PAS','PAX','RJP','SCP','STR','STX','TIS','UGP','UTX','XUS') THEN QuantityOnPurchaseOrder
        ELSE 0 END) AS OpenPO,
    SUM(CASE WHEN WarehouseCode NOT IN ('000','MOK','MOT','PAA','PAS','PAX','RJP','SCP','STR','STX','TIS','UGP','UTX','XUS') THEN QuantityInShipping
        ELSE 0 END) AS InShip,
    SUM(CASE WHEN WarehouseCode IN ('000','MOK','MOT','PAA','PAS','PAX','RJP','SCP','STR','STX','TIS','UGP','UTX','XUS') THEN QuantityOnHand
        ELSE 0 END) AS OnHand_Ex,
    SUM(CASE WHEN WarehouseCode IN ('000','MOK','MOT','PAA','PAS','PAX','RJP','SCP','STR','STX','TIS','UGP','UTX','XUS') THEN QuantityOnSalesOrder+QuantityOnBackOrder
        ELSE 0 END) AS OpenSO_Ex,
    SUM(CASE WHEN WarehouseCode IN ('000','MOK','MOT','PAA','PAS','PAX','RJP','SCP','STR','STX','TIS','UGP','UTX','XUS') THEN QuantityOnPurchaseOrder
        ELSE 0 END) AS OpenPO_Ex,
    SUM(CASE WHEN WarehouseCode IN ('000','MOK','MOT','PAA','PAS','PAX','RJP','SCP','STR','STX','TIS','UGP','UTX','XUS') THEN QuantityInShipping
        ELSE 0 END) AS InShip_Ex
  FROM IM_ItemWarehouse
  WHERE QuantityOnSalesOrder <> 0 OR QuantityOnBackOrder <> 0 OR QuantityOnPurchaseOrder <> 0  OR QuantityOnHand <> 0
  GROUP BY ItemCode) AS QtyTable
  ON CI_Item.ItemCode = QtyTable.ItemCode 
LEFT JOIN CI_ExtendedDescription
  ON CI_Item.ExtendedDescriptionKey = CI_ExtendedDescription.ExtendedDescriptionKey
LEFT JOIN IM_ProductLine
  ON CI_Item.ProductLine = IM_ProductLine.ProductLine
LEFT JOIN (
  SELECT * FROM PO_VendorPriceLevel
  WHERE APDivisionNo = '06' AND VendorNo = '0000200' AND PricingType = 'I'
) AS VenderCost
  ON CI_Item.ItemCode = VenderCost.ItemCode
LEFT JOIN (
  SELECT * FROM PO_VendorPriceLevel
  WHERE APDivisionNo = '06' AND VendorNo = '9000200' AND PricingType = 'I'
) AS JPYCost
  ON CI_Item.ItemCode = JPYCost.ItemCode
LEFT JOIN SY_User AS User1
  ON CI_Item.UserCreatedKey = User1.UserKey
LEFT JOIN SY_User AS User2
  ON CI_Item.UserUpdatedKey = User2.UserKey
WHERE
  ItemType = 1
ORDER BY CI_Item.ItemCode;