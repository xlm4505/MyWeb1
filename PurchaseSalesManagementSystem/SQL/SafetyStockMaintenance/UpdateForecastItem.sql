UPDATE dbo.U_ForecastItem
SET
    Quantity = @Quantity,
    [Comment] = @Comment
WHERE
    ItemCode = @ItemCode
    AND ProcType = @ProcType
    AND ARDivisionNo = @ARDivisionNo
    AND CustomerNo = @CustomerNo
    AND WarehouseCode = @WarehouseCode
    AND ItemNo = @ItemNo;