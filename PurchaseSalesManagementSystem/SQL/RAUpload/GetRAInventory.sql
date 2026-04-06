/* ============================================================
   GetRAInventory SQL 
============================================================ */
SELECT DISTINCT
  U_RAInventory.ItemCode
  , CI_Item.UDF_ITEMDESC AS ItemDesc
  , COALESCE(JFI.Qty, 0) AS JFI
  , COALESCE(NAL.Qty, 0) AS NAL
  , COALESCE(NCA.Qty, 0) AS NCA
  , COALESCE(NTX.Qty, 0) AS NTX
  , COALESCE(UTX.Qty, 0) AS UTX
  , COALESCE(UGP.Qty, 0) AS UGP
  , COALESCE(IFS.Qty, 0) AS IFS
  , COALESCE(NNJ.Qty, 0) AS NNJ
  , COALESCE(XIT.Qty, 0) AS XIT
  , COALESCE(JFI.Qty, 0) + COALESCE(NAL.Qty, 0) + COALESCE(NCA.Qty, 0) + COALESCE(NTX.Qty, 0) + COALESCE
  (UTX.Qty, 0) + COALESCE(UGP.Qty, 0) + COALESCE(IFS.Qty, 0) + COALESCE(NNJ.Qty, 0) + COALESCE(XIT.Qty, 0)
   AS Total 
FROM
  U_RAInventory 
  LEFT JOIN CI_Item 
    ON U_RAInventory.ItemCode = CI_Item.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'JFI' 
    GROUP BY
      ItemCode
  ) AS JFI 
    ON U_RAInventory.ItemCode = JFI.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'NAL' 
    GROUP BY
      ItemCode
  ) AS NAL 
    ON U_RAInventory.ItemCode = NAL.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'NCA' 
    GROUP BY
      ItemCode
  ) AS NCA 
    ON U_RAInventory.ItemCode = NCA.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'NTX' 
    GROUP BY
      ItemCode
  ) AS NTX 
    ON U_RAInventory.ItemCode = NTX.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'UTX' 
    GROUP BY
      ItemCode
  ) AS UTX 
    ON U_RAInventory.ItemCode = UTX.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'UGP' 
    GROUP BY
      ItemCode
  ) AS UGP 
    ON U_RAInventory.ItemCode = UGP.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'IFS' 
    GROUP BY
      ItemCode
  ) AS IFS 
    ON U_RAInventory.ItemCode = IFS.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'NNJ' 
    GROUP BY
      ItemCode
  ) AS NNJ 
    ON U_RAInventory.ItemCode = NNJ.ItemCode 
  LEFT JOIN ( 
    SELECT
      ItemCode
      , SUM(Qty) AS Qty 
    FROM
      U_RAInventory 
    WHERE
      WarehouseCode = 'XIT' 
    GROUP BY
      ItemCode
  ) AS XIT 
    ON U_RAInventory.ItemCode = XIT.ItemCode 
ORDER BY
  U_RAInventory.ItemCode;


