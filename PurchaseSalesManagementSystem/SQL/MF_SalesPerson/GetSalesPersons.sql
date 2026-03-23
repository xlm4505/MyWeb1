SELECT
    CustomerCode,
    SalesPerson
FROM FUJIKIN.dbo.MF_SalesPerson
WHERE @CustomerCode = ''
   OR CustomerCode = @CustomerCode
ORDER BY CustomerCode, SalesPerson;