SELECT
  ROW_NUMBER() OVER ( 
    ORDER BY
      DocType
      , NewFileName
      , Whse1
      , Whse2
      , ShipTo1
      , Attn
      , ShipVia
      , ItemCode
  ) AS No
  , DocType
  , NewFileName AS FileName
  , CASE 
    WHEN DocType = 'CI' 
      THEN CAST( 
      CAST(RIGHT (NewFileName, 2) AS NUMERIC) AS VARCHAR
    ) + '-' + CAST( 
      DENSE_RANK() OVER (PARTITION BY NewFileName ORDER BY Whse1) AS VARCHAR
    ) 
    ELSE '' 
    END AS RALineNo
  , CASE 
    WHEN ShipVia <> '' 
      THEN ShipTo1 + '-' + ShipVia 
    ELSE ShipTo1 
    END AS ShipTo_ShipVia
  , Whse1 AS Whse_Out
  , Whse2 AS Whse_In
  , ItemCode
  , SUM(TranQty) AS Qty 
FROM
  U_CIDetailData 
WHERE
  NOT ( 
    Whse1 IN ('STX', 'UGP', 'UTX') 
    AND Whse2 IN ('PAA', 'PAX', 'PAS')
  ) 
  AND EntryDate = CONVERT(date, GETDATE())
GROUP BY
  DocType
  , NewFileName
  , Whse1
  , Whse2
  , ShipTo1
  , Attn
  , ShipVia
  , ItemCode 
ORDER BY
  DocType
  , NewFileName
  , Whse1
  , Whse2
  , ShipTo1
  , Attn
  , ShipVia
  , ItemCode;
