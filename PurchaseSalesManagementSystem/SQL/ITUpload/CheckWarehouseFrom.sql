SELECT CAST(QuantityOnhand AS NUMERIC) AS QuantityOnhand, COALESCE(IM_Warehouse.WarehouseCode,'') AS WarehouseCode
FROM IM_ItemWarehouse
LEFT JOIN IM_Warehouse ON IM_Warehouse.WarehouseCode = @ToWH
WHERE ItemCode = @ItemCode AND IM_ItemWarehouse.WarehouseCode = @FromWH;
