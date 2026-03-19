DELETE FROM FUJIKIN.dbo.Tbl_Vendor
WHERE APDivisionNo = @APDivisionNo
  AND VendorNo = @VendorNo;