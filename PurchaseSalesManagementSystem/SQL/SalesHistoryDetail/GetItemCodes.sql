SELECT ItemCode, UDF_ITEMDESC
FROM CI_Item
WHERE Len(ItemCode) = 6 AND (ISNUMERIC(ItemCode) = 1 OR Left(ItemCode,2) = 'TK')
ORDER BY ItemCode;
