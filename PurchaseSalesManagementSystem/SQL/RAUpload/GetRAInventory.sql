/* ============================================================
   GetRAInventory SQL
============================================================ */
SELECT
  r.ItemCode
  , MAX(i.UDF_ITEMDESC) AS ItemDesc
  , SUM(CASE WHEN r.WarehouseCode = 'JFI' THEN r.Qty ELSE 0 END) AS JFI
  , SUM(CASE WHEN r.WarehouseCode = 'NAL' THEN r.Qty ELSE 0 END) AS NAL
  , SUM(CASE WHEN r.WarehouseCode = 'NCA' THEN r.Qty ELSE 0 END) AS NCA
  , SUM(CASE WHEN r.WarehouseCode = 'NTX' THEN r.Qty ELSE 0 END) AS NTX
  , SUM(CASE WHEN r.WarehouseCode = 'UTX' THEN r.Qty ELSE 0 END) AS UTX
  , SUM(CASE WHEN r.WarehouseCode = 'UGP' THEN r.Qty ELSE 0 END) AS UGP
  , SUM(CASE WHEN r.WarehouseCode = 'IFS' THEN r.Qty ELSE 0 END) AS IFS
  , SUM(CASE WHEN r.WarehouseCode = 'NNJ' THEN r.Qty ELSE 0 END) AS NNJ
  , SUM(CASE WHEN r.WarehouseCode = 'XIT' THEN r.Qty ELSE 0 END) AS XIT
  , SUM(r.Qty) AS Total
FROM
  U_RAInventory r
  LEFT JOIN CI_Item i ON r.ItemCode = i.ItemCode
GROUP BY
  r.ItemCode
ORDER BY
  r.ItemCode;
