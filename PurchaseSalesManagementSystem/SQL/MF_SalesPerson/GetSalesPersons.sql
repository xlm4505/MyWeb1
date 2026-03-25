SELECT
    CustomerCode,
    SalesPerson
FROM dbo.MF_SalesPerson
WHERE @CustomerCode = ''
   OR CustomerCode = @CustomerCode
ORDER BY CustomerCode, SalesPerson;