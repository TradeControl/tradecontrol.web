CREATE TABLE [Cash].[tbCategoryTotal] (
    [ParentCode] NVARCHAR (10) NOT NULL,
    [ChildCode]  NVARCHAR (10) NOT NULL,
    DisplayOrder  smallint     NOT NULL CONSTRAINT DF_Cash_tbCategoryTotal_DisplayOrder DEFAULT (0),
    [RowVer]     ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Cash_tbCategoryTotal] PRIMARY KEY CLUSTERED ([ParentCode] ASC, [ChildCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Cash_tbCategoryTotal_Cash_tbCategory_Child] FOREIGN KEY ([ChildCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]),
    CONSTRAINT [FK_Cash_tbCategoryTotal_Cash_tbCategory_Parent] FOREIGN KEY ([ParentCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode])
);

