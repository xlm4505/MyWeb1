SELECT
  CAST( 
    ROW_NUMBER() OVER ( 
      ORDER BY
        DocType
        , NewFileName
        , Whse1
        , Whse2
        , ItemCode
    ) AS INT
  ) AS RowNumber
  , DocType
  , EntryDate
  , ShipDate
  , NewFileName
  , [FOA_CI#] AS FOA_CI
  , ShipTo1
  , ShipTo2
  , Attn
  , ShipVia
  , Account
  , Requestor AS RequestedBy
  , ItemCode
  , Whse1
  , Whse2
  , TranQty
  , SalesOrderNo
  , CustomerPONo
  , [LineNo] AS LineNumber
  , FujikinPartNo
  , CustomerPartNo
  , Category
  , Tariff
  , UnitPrice
  , TotalPrice
  , Instrucstions
  , OriginalFileName 
FROM
  U_CIDetailData 
WHERE
  EntryDate = CONVERT(date, GETDATE()) 
ORDER BY
  DocType
  , NewFileName
  , Whse1
  , Whse2
  , ItemCode;
