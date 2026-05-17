UPDATE U_UploadPurchaseReceiptData
SET StatusFlag = '1'
WHERE StatusFlag = '0'
AND ClientAddress = @ClientAddress