SELECT
  CASE
    WHEN AR_InvoiceHistoryHeader.InvoiceDate Is Null
      THEN ''
    WHEN AR_InvoiceHistoryHeader.InvoiceDate = '1/1/1753'
      THEN ''
    ELSE CAST(AR_InvoiceHistoryHeader.InvoiceDate AS DATE)
    END AS InvoiceDate
  , AR_InvoiceHistoryHeader.ARDivisionNo + '-' + AR_InvoiceHistoryHeader.CustomerNo AS Customer
  , AR_InvoiceHistoryHeader.SalesOrderNo
  , AR_InvoiceHistoryDetail.ItemCode
  , CASE
    WHEN AR_InvoiceHistoryDetail.UDF_ITEMDESC = ''
      THEN AR_InvoiceHistoryDetail.ItemCodeDesc
    WHEN CHARINDEX(
      '(Pack of'
      , AR_InvoiceHistoryDetail.UDF_ITEMDESC
    ) > 0
      THEN
  LEFT (
    AR_InvoiceHistoryDetail.UDF_ITEMDESC
    , CHARINDEX(
      '(Pack of'
      , AR_InvoiceHistoryDetail.UDF_ITEMDESC
    ) - 1
  )
  ELSE AR_InvoiceHistoryDetail.UDF_ITEMDESC
  END AS ItemCodeDesc
  , AR_InvoiceHistoryDetail.AliasItemNo AS CustomerPartNo
  , AR_InvoiceHistoryHeader.CustomerPONo
  , AR_InvoiceHistoryDetail.UDF_CUSTPOLN AS POLineNo
  , AR_InvoiceHistoryHeader.InvoiceNo
  , AR_InvoiceHistoryHeader.ShipToCode
  , AR_InvoiceHistoryHeader.ShipToName
  , AR_InvoiceHistoryHeader.ShipVia
  , COALESCE(Track1.TrackingID, '') AS [Tracking1]
  , COALESCE(Track2.TrackingID, '') AS [Tracking2]
  , COALESCE(Track3.TrackingID, '') AS [Tracking3]
  , AR_InvoiceHistoryDetail.WarehouseCode AS Warehouse
  , AR_InvoiceHistoryDetail.QuantityShipped AS ShippedQty
  , AR_InvoiceHistoryDetail.UnitPrice
  , AR_InvoiceHistoryDetail.ExtensionAmt
  , CI_Item.StandardUnitPrice AS StdUnitPrice
  , AR_InvoiceHistoryDetail.UnitCost
  , CI_Item.StandardUnitCost AS StdUnitCost
  , CI_Item.LastTotalUnitCost AS LastUnitPrice
  , CASE
  WHEN UDF_REQUEST_DATE = ''
    THEN NULL
  ELSE CAST(UDF_REQUEST_DATE AS DATE)
  END AS RequestDate
  , CASE
  WHEN PromiseDate = '1/1/1753'
    THEN NULL
  ELSE CAST(PromiseDate AS DATE)
  END AS PromiseDate
  , CASE
  WHEN UDF_COMMITDATE Is Null
    THEN NULL
  WHEN UDF_COMMITDATE = '1/1/1753'
    THEN NULL
  ELSE CAST(UDF_COMMITDATE AS DATE)
  END AS CommitDate
  , CASE
  WHEN AR_InvoiceHistoryHeader.ShipDate Is Null
    THEN ''
  WHEN AR_InvoiceHistoryHeader.ShipDate = '1/1/1753'
    THEN ''
  ELSE CAST(AR_InvoiceHistoryHeader.ShipDate AS DATE)
  END AS ShipDate
  , CASE
  WHEN AR_InvoiceHistoryHeader.TransactionDate Is Null
    THEN ''
  WHEN AR_InvoiceHistoryHeader.TransactionDate = '1/1/1753'
    THEN ''
  ELSE CAST(AR_InvoiceHistoryHeader.TransactionDate AS DATE)
  END AS PostingDate
  , CASE
  WHEN AR_InvoiceHistoryHeader.DateCreated = '1/1/1753'
    THEN CAST(AR_InvoiceHistoryHeader.DateUpdated AS DATE)
  ELSE CAST(AR_InvoiceHistoryHeader.DateCreated AS DATE)
  END AS EntryDate
FROM
  AR_InvoiceHistoryDetail
  LEFT JOIN AR_InvoiceHistoryHeader
    ON AR_InvoiceHistoryDetail.HeaderSeqNo = AR_InvoiceHistoryHeader.HeaderSeqNo
    AND AR_InvoiceHistoryDetail.InvoiceNo = AR_InvoiceHistoryHeader.InvoiceNo
  LEFT JOIN CI_Item
    ON CI_Item.ItemCode = AR_InvoiceHistoryDetail.ItemCode
  LEFT JOIN AR_InvoiceHistoryTracking AS Track1
    ON Track1.InvoiceNo = AR_InvoiceHistoryHeader.InvoiceNo
    AND Track1.HeaderSeqNo = AR_InvoiceHistoryHeader.HeaderSeqNo
    AND Track1.PackageNo = '0001'
  LEFT JOIN AR_InvoiceHistoryTracking AS Track2
    ON Track2.InvoiceNo = AR_InvoiceHistoryHeader.InvoiceNo
    AND Track2.HeaderSeqNo = AR_InvoiceHistoryHeader.HeaderSeqNo
    AND Track2.PackageNo = '0002'
  LEFT JOIN AR_InvoiceHistoryTracking AS Track3
    ON Track3.InvoiceNo = AR_InvoiceHistoryHeader.InvoiceNo
    AND Track3.HeaderSeqNo = AR_InvoiceHistoryHeader.HeaderSeqNo
    AND Track3.PackageNo = '0003'
WHERE
  AR_InvoiceHistoryHeader.InvoiceDate BETWEEN @OperationDateFrom AND @OperationDateTo
  AND AR_InvoiceHistoryDetail.QuantityShipped <> 0
  AND (@Customer = '' OR ARDivisionNo + '-' + CustomerNo = @Customer)
  AND (@ItemCode = '' OR AR_InvoiceHistoryDetail.ItemCode = @ItemCode)
  AND (@ItemDescription = '' OR AR_InvoiceHistoryDetail.UDF_ITEMDESC = @ItemDescription)
ORDER BY
  AR_InvoiceHistoryHeader.InvoiceDate
  , AR_InvoiceHistoryHeader.SalesOrderNo
  , AR_InvoiceHistoryDetail.UDF_CUSTPOLN
  , ItemCode;
