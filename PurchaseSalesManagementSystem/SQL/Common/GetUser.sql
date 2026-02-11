SELECT DISTINCT
    FirstName + ' ' + LastName AS UserName
FROM
    PO_PurchaseOrderHeader
    --LEFT JOIN SY_User ON SY_User.UserKey = PO_PurchaseOrderHeader.UserCreatedKey
    LEFT JOIN MAS_SYSTEM.dbo.SY_User u ON u.UserKey = PO_PurchaseOrderHeader.UserCreatedKey
WHERE
    PO_PurchaseOrderHeader.DateCreated > DATEADD(year, -1, SYSDATETIME())
ORDER BY
    UserName;