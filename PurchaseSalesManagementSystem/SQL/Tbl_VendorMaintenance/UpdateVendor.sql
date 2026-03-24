UPDATE FUJIKIN.dbo.Tbl_Vendor
SET
    APDivisionNo = @APDivisionNo,
    VendorNo = @VendorNo,
    VendorName = @VendorName
WHERE ID = @ID;