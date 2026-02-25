UPDATE PO_PurchaseOrderHeader
   SET OrderStatus = 'O'
  FROM PO_PurchaseOrderHeader
  LEFT JOIN SY_User
    ON SY_User.UserKey = PO_PurchaseOrderHeader.UserCreatedKey
 WHERE PO_PurchaseOrderHeader.DateCreated = @EntryDate
   AND OrderStatus = 'N'
   AND (FirstName + ' ' + LastName) = @UserName
   AND (
       @VendorCode = '00-0000000'
       OR (
           @VendorCode = '99-9999999'
           AND PO_PurchaseOrderHeader.APDivisionNo + '-' + PO_PurchaseOrderHeader.VendorNo NOT IN (
               '06-0000200',
               '08-0000350',
               '08-0000250',
               '14-0000300',
               '10-0000500',
               '08-0000220'
           )
       )
       OR (
           @VendorCode NOT IN ('00-0000000', '99-9999999')
           AND PO_PurchaseOrderHeader.APDivisionNo + '-' + PO_PurchaseOrderHeader.VendorNo = @VendorCode
       )
   );