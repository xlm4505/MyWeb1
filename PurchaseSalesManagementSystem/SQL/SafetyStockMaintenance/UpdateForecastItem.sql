UPDATE dbo.U_ForecastItem
SET
    CustomerNo = @CustomerNo,
    WarehouseCode = @WarehouseCode,
    Quantity = @Quantity,
    Comment = @Comment
WHERE
    ItemCode = @ItemCode;