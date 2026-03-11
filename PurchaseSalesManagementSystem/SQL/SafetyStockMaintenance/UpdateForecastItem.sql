UPDATE dbo.U_ForecastItem
SET
    Quantity = @Quantity
WHERE
    ItemCode = @ItemCode;