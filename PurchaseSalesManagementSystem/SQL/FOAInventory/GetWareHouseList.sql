SELECT DISTINCT IM_ItemWarehouse.WarehouseCode
FROM CI_Item CI_Item, IM_ItemWarehouse IM_ItemWarehouse
WHERE IM_ItemWarehouse.ItemCode = CI_Item.ItemCode
  AND CI_Item.ProductLine = 'SEM'
ORDER BY IM_ItemWarehouse.WarehouseCode
