SELECT DISTINCT
	FirstName + ' ' + LastName AS UserName,
	UserCreatedKey
FROM MAS_FOA.dbo.PO_ReceiptHistoryHeader
LEFT JOIN MAS_SYSTEM.dbo.SY_User
  ON UserCreatedKey = SY_User.UserKey
WHERE PO_ReceiptHistoryHeader.DateCreated > '11/1/2020';
