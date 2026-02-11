SELECT
    APDivisionNo + '-' + VendorNo AS VendorCode,
    VendorName
FROM
    FUJIKIN.dbo.Tbl_Vendor
ORDER BY
    APDivisionNo,
    VendorNo;