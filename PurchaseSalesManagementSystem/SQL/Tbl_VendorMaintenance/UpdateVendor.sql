UPDATE FUJIKIN.dbo.Tbl_Vendor
SET VendorName = @VendorName
WHERE APDivisionNo = @APDivisionNo
  AND VendorNo = @VendorNo;