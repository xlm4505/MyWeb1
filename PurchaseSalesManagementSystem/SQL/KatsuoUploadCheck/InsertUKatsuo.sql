INSERT INTO U_Katsuo (
    PurchaseOrderNo,
    PromiseDate,
    ItemCode,
    OpenQty,
    DummyFlag,
    IssueDate,
    CreateDate
) VALUES (
    @PurchaseOrderNo,
    @PromiseDate,
    @ItemCode,
    @OpenQty,
    @DummyFlag,
    @IssueDate,
    @CreateDate
)
