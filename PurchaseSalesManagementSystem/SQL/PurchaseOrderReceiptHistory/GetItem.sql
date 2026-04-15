SELECT DISTINCT
  PO_ReceiptHistoryDetail.ItemCode,
  CI_Item.UDF_ITEMDESC
FROM PO_ReceiptHistoryDetail
LEFT JOIN CI_Item
   ON PO_ReceiptHistoryDetail.ItemCode  = CI_Item.ItemCode
WHERE LastReceiptDate > '2020/1/1' 
ORDER BY PO_ReceiptHistoryDetail.ItemCode;