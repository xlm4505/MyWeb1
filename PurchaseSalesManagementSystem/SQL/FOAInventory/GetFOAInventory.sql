SELECT
  CI_Item.ProductLine AS 'ProdLn',
  IM_ItemWarehouse.ItemCode,
  CI_Item.ItemCodeDesc,
  IM_ItemWarehouse.WarehouseCode AS 'WHSE',
  IM_ItemWarehouse.QuantityOnHand AS 'OnHand',
  IM_ItemWarehouse.QuantityOnPurchaseOrder AS 'Qty PO',
  CI_Item.StandardUnitCost,
  CASE WHEN CI_Item.LastSoldDate = '1/1/1753' THEN ''
    ELSE LEFT(CONVERT(VARCHAR, CI_Item.LastSoldDate, 101), 10) END AS [LastSoldDate],
  CASE WHEN CI_Item.LastReceiptDate = '1/1/1753' THEN ''
    ELSE LEFT(CONVERT(VARCHAR, CI_Item.LastReceiptDate, 101), 10) END AS [LastReceiptDate],
  CI_Item.LastTotalUnitCost
FROM CI_Item CI_Item, IM_ItemWarehouse IM_ItemWarehouse
WHERE IM_ItemWarehouse.ItemCode = CI_Item.ItemCode
  AND CI_Item.ProductLine = 'SEM'
  AND (@ItemCode = '' OR IM_ItemWarehouse.ItemCode = @ItemCode)
  AND (@WareHouse = '' OR IM_ItemWarehouse.WarehouseCode = @WareHouse)
  AND (@MinusOnly = 0 OR IM_ItemWarehouse.QuantityOnHand < 0)
