SELECT
    ItemCode,
    ProcType,
    ARDivisionNo,
    CustomerNo,
    WarehouseCode,
    Quantity,
    ItemNo,
    [Comment] AS Comment
FROM dbo.U_ForecastItem
WHERE (@ItemCode = '' OR ItemCode = @ItemCode)
ORDER BY ItemCode, ProcType, ARDivisionNo, CustomerNo, WarehouseCode, ItemNo;