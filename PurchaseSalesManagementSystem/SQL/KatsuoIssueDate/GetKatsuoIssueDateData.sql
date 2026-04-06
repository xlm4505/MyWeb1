SELECT
    [UserName],
    ID,
    [IssueDate]
FROM Katsuo_Issue_Date
WHERE (@UserName IS NULL OR [UserName] = @UserName)
