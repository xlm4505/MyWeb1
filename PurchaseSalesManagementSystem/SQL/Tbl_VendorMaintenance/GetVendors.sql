SELECT
    ID,
    APDivisionNo,
    VendorNo,
    VendorName
FROM FUJIKIN.dbo.Tbl_Vendor
WHERE (@ID = '' OR ID = @ID)
ORDER BY ID;