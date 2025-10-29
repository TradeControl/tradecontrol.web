CREATE TABLE [Cash].[tbCategoryTotal] (
    [ParentCode] NVARCHAR (10) NOT NULL,
    [ChildCode]  NVARCHAR (10) NOT NULL,
    DisplayOrder  smallint     NOT NULL CONSTRAINT DF_Cash_tbCategoryTotal_DisplayOrder DEFAULT (0),
    [RowVer]     ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbCategoryTotal] PRIMARY KEY CLUSTERED ([ParentCode] ASC, [ChildCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Cash_tbCategoryTotal_Cash_tbCategory_Child] FOREIGN KEY ([ChildCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]),
    CONSTRAINT [FK_Cash_tbCategoryTotal_Cash_tbCategory_Parent] FOREIGN KEY ([ParentCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode])
);


GO

CREATE TRIGGER [Cash].[Cash_tbCategoryTotal_Trigger_NoCycles]
ON [Cash].[tbCategoryTotal]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (SELECT 1 FROM inserted WHERE ParentCode = ChildCode)
    BEGIN
        RAISERROR ('ParentCode cannot equal ChildCode.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

    DECLARE @violations TABLE (ChildCode nvarchar(10), ParentCode nvarchar(10));

    ;WITH down_paths AS
    (
        SELECT i.ChildCode AS StartChild, e.ChildCode AS Descendant
        FROM inserted i
        JOIN Cash.tbCategoryTotal e
          ON e.ParentCode = i.ChildCode

        UNION ALL

        SELECT dp.StartChild, e.ChildCode
        FROM down_paths dp
        JOIN Cash.tbCategoryTotal e
          ON e.ParentCode = dp.Descendant
    )
    INSERT INTO @violations (ChildCode, ParentCode)
    SELECT DISTINCT i.ChildCode, i.ParentCode
    FROM inserted i
    JOIN down_paths dp
      ON dp.StartChild = i.ChildCode
     AND dp.Descendant = i.ParentCode
    OPTION (MAXRECURSION 1000);

    IF EXISTS (SELECT 1 FROM @violations)
    BEGIN
        RAISERROR ('Insert/Update would create a cyclic hierarchy in Cash.tbCategoryTotal.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
END