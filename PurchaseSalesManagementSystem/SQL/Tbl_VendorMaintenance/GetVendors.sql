SELECT
    ID,
    APDivisionNo,
    VendorNo,
    VendorName
FROM dbo.Tbl_Vendor
WHERE (@ID = '' OR ID = @ID)
ORDER BY ID;