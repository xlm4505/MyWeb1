SELECT
    APDivisionNo,
    VendorNo,
    VendorName
FROM FUJIKIN.dbo.Tbl_Vendor
WHERE (@VendorNo = '' OR VendorNo = @VendorNo)
ORDER BY VendorNo;