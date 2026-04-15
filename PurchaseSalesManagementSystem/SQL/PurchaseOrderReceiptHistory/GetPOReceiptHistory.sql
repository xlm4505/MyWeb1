 SELECT
	PO_ReceiptHistoryHeader.PurchaseOrderNo AS [PONo],
	PO_ReceiptHistoryHeader.APDivisionNo + '-' + PO_ReceiptHistoryHeader.VendorNo AS [VendorNo],
	PO_ReceiptHistoryHeader.PurchaseName AS [VendorName],
	CAST(OrderDate AS DATE) AS [PODate],
	PO_ReceiptHistoryHeader.ReceiptType + PO_ReceiptHistoryHeader.ReceiptNo AS [ReceiptNo],
	CAST(PO_ReceiptHistoryHeader.ReceiptDate AS DATE) AS [ReceiptDate],
	PO_ReceiptHistoryHeader.InvoiceNo AS [InvoiceNo],
	CAST(PO_ReceiptHistoryHeader.InvoiceDate AS DATE) AS [InvoiceDate],
	CASE WHEN PO_ReceiptHistoryHeader.ReceiptAmt <> 0 THEN PO_ReceiptHistoryHeader.ReceiptAmt ELSE PO_ReceiptHistoryHeader.InvoiceAmt END AS [InvoiceTotal],
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.DetailSeqNo ELSE PO_ReceiptHistoryDetail.LineKey END AS [LineKey],
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.ItemCode ELSE PO_ReceiptHistoryDetail.ItemCode END AS [ItemCode],
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.ItemCodeDesc ELSE PO_ReceiptHistoryDetail.ItemCodeDesc END AS [ItemDescription],
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.WarehouseCode ELSE PO_ReceiptHistoryDetail.WarehouseCode END AS Warehouse,
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.QuantityReceived ELSE PO_ReceiptHistoryDetail.QuantityReceived END AS [QtyRcvd],  
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.UnitCost ELSE PO_ReceiptHistoryDetail.UnitCost END AS [UnitCost],
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.ExtensionAmt ELSE PO_ReceiptHistoryDetail.ExtensionAmt END AS [ExtensionAmt],
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.SalesOrderNo ELSE PO_ReceiptHistoryDetail.SalesOrderNo END AS [SONo],
	CASE WHEN PO_ReceiptHistoryDetail.PurchaseOrderNo IS NULL THEN AP_InvoiceHistoryDetail.CommentText ELSE PO_ReceiptHistoryDetail.CommentText END AS [Comment],
	FirstName + ' ' + LastName AS [UserName],
	CAST(PO_ReceiptHistoryHeader.TransactionDate AS DATE) AS [PostingDate],
	CAST(PO_ReceiptHistoryHeader.DateCreated AS DATE) AS [OperationDate]
FROM PO_ReceiptHistoryHeader
	LEFT JOIN PO_ReceiptHistoryDetail ON 
		PO_ReceiptHistoryHeader.PurchaseOrderNo = PO_ReceiptHistoryDetail.PurchaseOrderNo
		AND PO_ReceiptHistoryHeader.ReceiptType = PO_ReceiptHistoryDetail.ReceiptType 
		AND PO_ReceiptHistoryHeader.ReceiptNo = PO_ReceiptHistoryDetail.ReceiptNo
		AND PO_ReceiptHistoryHeader.HeaderSeqNo = PO_ReceiptHistoryDetail.HeaderSeqNo
	LEFT JOIN AP_InvoiceHistoryHeader ON
		AP_InvoiceHistoryHeader.InvoiceNo = PO_ReceiptHistoryHeader.InvoiceNo 
		AND AP_InvoiceHistoryHeader.PurchaseOrderNo = PO_ReceiptHistoryHeader.PurchaseOrderNo
		AND AP_InvoiceHistoryHeader.ReceiptNo = PO_ReceiptHistoryHeader.ReceiptNo 
		AND AP_InvoiceHistoryHeader.InvoiceType = PO_ReceiptHistoryHeader.ReceiptType
	LEFT JOIN AP_InvoiceHistoryDetail ON 
		AP_InvoiceHistoryHeader.InvoiceNo = AP_InvoiceHistoryDetail.InvoiceNo 
		AND AP_InvoiceHistoryHeader.HeaderSeqNo = AP_InvoiceHistoryDetail.HeaderSeqNo
		AND AP_InvoiceHistoryDetail.QuantityInvoiced <> 0
	LEFT JOIN MAS_SYSTEM.dbo.SY_User ON
		PO_ReceiptHistoryHeader.UserCreatedKey = MAS_SYSTEM.dbo.SY_User.UserKey
WHERE (PO_ReceiptHistoryDetail.QuantityReceived <> 0 OR PO_ReceiptHistoryDetail.QuantityReceived IS NULL)
	AND PO_ReceiptHistoryHeader.ReceiptDate BETWEEN @DateFrom AND @DateTo
	AND (
		ISNULL(@VendorCode, '') = ''
		OR PO_ReceiptHistoryHeader.APDivisionNo + '-' + PO_ReceiptHistoryHeader.VendorNo = @VendorCode
	)
	AND (
		ISNULL(@PoNo, '') = ''
		OR PO_ReceiptHistoryHeader.PurchaseOrderNo = @PoNo 
	)
	AND (
		ISNULL(@InvoiceNo, '') = ''
		OR PO_ReceiptHistoryHeader.InvoiceNo = @InvoiceNo 
	)
	AND (
		ISNULL(@ReceiptNo, '') = ''
		OR PO_ReceiptHistoryHeader.ReceiptType + PO_ReceiptHistoryHeader.ReceiptNo = @ReceiptNo 
	)
	AND (
		ISNULL(@ItemCode, '') = ''
		OR PO_ReceiptHistoryDetail.ItemCode = @ItemCode 
	)
	AND (
		ISNULL(@UserName, '') = ''
		OR PO_ReceiptHistoryHeader.UserCreatedKey = @UserName
	)
ORDER BY PO_ReceiptHistoryHeader.PurchaseOrderNo, PO_ReceiptHistoryHeader.ReceiptDate,PO_ReceiptHistoryHeader.ReceiptNo, PO_ReceiptHistoryHeader.HeaderSeqNo;        