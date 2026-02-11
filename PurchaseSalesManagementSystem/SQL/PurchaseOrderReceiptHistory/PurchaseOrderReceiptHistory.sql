SELECT
    PO_ReceiptHistoryHeader.PurchaseOrderNo AS [PO No.],
    PO_ReceiptHistoryHeader.APDivisionNo + '-' + PO_ReceiptHistoryHeader.VendorNo AS [Vendor No.],
    PO_ReceiptHistoryHeader.PurchaseName AS [Vendor Name],
    CAST(OrderDate AS DATE) AS [PO Date],
    PO_ReceiptHistoryHeader.ReceiptType + PO_ReceiptHistoryHeader.ReceiptNo AS [Receipt No],
    CAST(PO_ReceiptHistoryHeader.ReceiptDate AS DATE) AS [Receipt Date],
    PO_ReceiptHistoryHeader.InvoiceNo AS [Invoice No.],
    CAST(PO_ReceiptHistoryHeader.InvoiceDate AS DATE) AS [Invoice Date],
    CASE
        WHEN PO_ReceiptHistoryHeader.ReceiptAmt <> 0 THEN PO_ReceiptHistoryHeader.ReceiptAmt
        ELSE PO_ReceiptHistoryHeader.InvoiceAmt
    END AS [Invoice Total],
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.DetailSeqNo
        ELSE PO_ReceiptHistoryDetail.LineKey
    END AS [Line Key],
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.ItemCode
        ELSE PO_ReceiptHistoryDetail.ItemCode
    END AS [Item Code],
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.ItemCodeDesc
        ELSE PO_ReceiptHistoryDetail.ItemCodeDesc
    END AS [Item Description],
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.WarehouseCode
        ELSE PO_ReceiptHistoryDetail.WarehouseCode
    END AS Warehouse,
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.QuantityReceived
        ELSE PO_ReceiptHistoryDetail.QuantityReceived
    END AS [Qty Rcvd.],
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.UnitCost
        ELSE PO_ReceiptHistoryDetail.UnitCost
    END AS [Unit Cost],
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.ExtensionAmt
        ELSE PO_ReceiptHistoryDetail.ExtensionAmt
    END AS [Extension Amt],
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.SalesOrderNo
        ELSE PO_ReceiptHistoryDetail.SalesOrderNo
    END AS [SO No],
    CASE
        WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.CommentText
        ELSE PO_ReceiptHistoryDetail.CommentText
    END AS [Comment],
    FirstName + ' ' + LastName AS [User Name],
    CAST(PO_ReceiptHistoryHeader.TransactionDate AS DATE) AS [Posting Date],
    CAST(PO_ReceiptHistoryHeader.DateCreated AS DATE) AS [Operation Date]
FROM
    PO_ReceiptHistoryHeader
    LEFT JOIN PO_ReceiptHistoryDetail ON PO_ReceiptHistoryHeader.PurchaseOrderNo = PO_ReceiptHistoryDetail.PurchaseOrderNo
    AND PO_ReceiptHistoryHeader.ReceiptType = PO_ReceiptHistoryDetail.ReceiptType
    AND PO_ReceiptHistoryHeader.ReceiptNo = PO_ReceiptHistoryDetail.ReceiptNo
    AND PO_ReceiptHistoryHeader.HeaderSeqNo = PO_ReceiptHistoryDetail.HeaderSeqNo
    LEFT JOIN AP_InvoiceHistoryHeader ON AP_InvoiceHistoryHeader.InvoiceNo = PO_ReceiptHistoryHeader.InvoiceNo
    AND AP_InvoiceHistoryHeader.PurchaseOrderNo = PO_ReceiptHistoryHeader.PurchaseOrderNo
    AND AP_InvoiceHistoryHeader.ReceiptNo = PO_ReceiptHistoryHeader.ReceiptNo
    AND AP_InvoiceHistoryHeader.InvoiceType = PO_ReceiptHistoryHeader.ReceiptType
    LEFT JOIN AP_InvoiceHistoryDetail ON AP_InvoiceHistoryHeader.InvoiceNo = AP_InvoiceHistoryDetail.InvoiceNo
    AND AP_InvoiceHistoryHeader.HeaderSeqNo = AP_InvoiceHistoryDetail.HeaderSeqNo
    AND AP_InvoiceHistoryDetail.QuantityInvoiced <> 0
    LEFT JOIN SY_User ON PO_ReceiptHistoryHeader.UserCreatedKey = SY_User.UserKey
WHERE
    (
        PO_ReceiptHistoryDetail.QuantityReceived <> 0
        OR PO_ReceiptHistoryDetail.QuantityReceived IS NULL
    )
    AND PO_ReceiptHistoryHeader.ReceiptDate BETWEEN @from AND @to

    AND (@VendorNo = 'ALL'
         OR PO_ReceiptHistoryHeader.APDivisionNo + '-' + PO_ReceiptHistoryHeader.VendorNo = @vendorNo)

    AND (@PoNo = 'ALL'
         OR PO_ReceiptHistoryHeader.PurchaseOrderNo = @poNo)

    AND (@invoiceNo = 'ALL'
         OR PO_ReceiptHistoryHeader.InvoiceNo = @invoiceNo)

    AND (@receiptNo = 'ALL'
         OR PO_ReceiptHistoryHeader.ReceiptType + PO_ReceiptHistoryHeader.ReceiptNo = @receiptNo)
    AND (@userName = 'ALL'
         OR PO_ReceiptHistoryHeader.FirstName + ' ' + PO_ReceiptHistoryHeader.LastName = @userName)
ORDER BY
    PO_ReceiptHistoryHeader.PurchaseOrderNo,
    PO_ReceiptHistoryHeader.ReceiptDate,
    PO_ReceiptHistoryHeader.ReceiptNo,
    PO_ReceiptHistoryHeader.HeaderSeqNo;