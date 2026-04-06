WITH CIData AS (
    SELECT
        *,
        CASE WHEN CustomerPONo LIKE 'PAC%' THEN 'Pace' ELSE '' END AS Pace,
        CASE WHEN LEFT(DocType, 2) = 'IT' THEN '' ELSE Requestor END AS RequestedBy2
    FROM U_CIDetailData
    WHERE NOT (
        Whse1 IN ('STX', 'UGP', 'UTX')
        AND Whse2 IN ('PAA', 'PAX', 'PAS')
    )
    AND EntryDate = CONVERT(date, GETDATE())
)
SELECT
    FORMAT(EDate, 'M/d') AS EntryDate,
    ROW_NUMBER() OVER (ORDER BY DocType, FileName, EDate) AS No,
    FileName,
    ShipTo,
    Attn,
    ShipVia,
    Account,
    WarehouseList,
    TotalQty,
    Unit,
    Requested_By,
    Pace,
    Instrucstions,
    Value
FROM (
    SELECT
        DocType,
        EntryDate AS EDate,
        NewFileName AS FileName,
        ShipTo1 AS ShipTo,
        Attn,
        ShipVia,
        Account,
        REPLACE(
            STUFF((
                SELECT ', ' + Whse
                FROM (
                    SELECT DISTINCT
                        NewFileName,
                        CASE
                            WHEN ShipTo1 = 'Rinku to Rinku' AND Whse2 <> ''
                                THEN Whse1 + '-' + Whse2
                            ELSE Whse1
                        END AS Whse
                    FROM U_CIDetailData
                ) AS b
                WHERE b.NewFileName = a.NewFileName
                FOR XML PATH ('')
            ), 1, 2, ''),
            '-', '>'
        ) AS WarehouseList,
        SUM(TranQty) AS TotalQty,
        'pcs' AS Unit,
        RequestedBy2 AS Requested_By,
        Pace,
        Instrucstions,
        SUM(TotalPrice) AS Value
    FROM CIData a
    WHERE DocType <> 'IT2'
    GROUP BY
        DocType, EntryDate, NewFileName, ShipTo1, Attn, ShipVia,
        Account, RequestedBy2, Instrucstions, Pace
    UNION ALL
    SELECT
        DocType,
        EntryDate AS EDate,
        NewFileName AS FileName,
        ShipTo1 AS ShipTo,
        Attn,
        ShipVia,
        Account,
        Whse1 + ' > ' + Whse2 AS WarehouseList,
        SUM(TranQty) AS TotalQty,
        'pcs' AS Unit,
        RequestedBy2 AS Requested_By,
        Pace,
        Instrucstions,
        SUM(TotalPrice) AS Value
    FROM CIData a
    WHERE DocType = 'IT2'
    GROUP BY
        DocType, EntryDate, NewFileName, ShipTo1, Attn, ShipVia,
        Account, RequestedBy2, Instrucstions, Pace, Whse1, Whse2
) AS DATA
ORDER BY DocType, FileName, EDate;
