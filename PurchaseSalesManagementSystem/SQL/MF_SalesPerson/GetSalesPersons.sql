SELECT
    CustomerCode,
    SalesPerson
FROM FUJIKIN.dbo.MF_SalesPerson
WHERE @CustomerCode = ''
   OR CustomerCode LIKE '%' + @CustomerCode + '%'
ORDER BY CustomerCode, SalesPerson;