SELECT AR_Customer.ARDivisionNO + '-' + AR_Customer.CustomerNo AS QCode,
       AR_Customer.CustomerName AS QName
FROM AR_Customer
ORDER BY AR_Customer.ARDivisionNO, AR_Customer.CustomerNo;
