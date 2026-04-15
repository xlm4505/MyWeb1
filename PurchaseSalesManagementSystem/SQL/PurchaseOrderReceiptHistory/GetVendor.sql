SELECT 
	AP_Vendor.APDivisionNo + '-' + AP_Vendor.VendorNo AS QCode,
	AP_Vendor.VendorName AS QName 
FROM AP_Vendor
WHERE AP_Vendor.APDivisionNO <> '00'
ORDER BY AP_Vendor.APDivisionNO, AP_Vendor.VendorNo;