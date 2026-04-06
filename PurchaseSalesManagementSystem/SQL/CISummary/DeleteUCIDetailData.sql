/* ============================================================
   DeleteUCIDetailData SQL 
============================================================ */
DELETE FROM U_CIDetailData WHERE EntryDate = CONVERT(varchar, GETDATE(), 23);