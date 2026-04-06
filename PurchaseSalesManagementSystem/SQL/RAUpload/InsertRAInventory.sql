/* ============================================================
   DeleteRAInventory SQL 
============================================================ */
INSERT 
INTO U_RAInventory( 
  EntryDate
  , WarehouseCode
  , ItemCode
  , Description
  , OriginalQty
  , Qty
  , InvoiceNo
  , Box
  , Weight
  , DateReceived
  , [From]
  , VantecRef#
  , UnitPrice
  , ShipMark
  , [Comment]
) 
VALUES ( 
  @EntryDate
  , @WarehouseCode
  , @ItemCode
  , @Description
  , @OriginalQty
  , @Qty
  , @InvoiceNo
  , @Box
  , @Weight
  , @DateReceived
  , @From
  , @VantecRef
  , @UnitPrice
  , @ShipMark
  , @Comment
);
