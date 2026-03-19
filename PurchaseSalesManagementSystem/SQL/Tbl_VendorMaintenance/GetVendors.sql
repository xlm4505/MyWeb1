SELECT
    APDivisionNo,
    VendorNo,
    VendorName
FROM FUJIKIN.dbo.Tbl_Vendor
WHERE (@VendorNo = '' OR VendorNo LIKE '%' + @VendorNo + '%')
ORDER BY VendorNo;