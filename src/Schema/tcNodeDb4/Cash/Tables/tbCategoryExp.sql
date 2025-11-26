CREATE TABLE [Cash].[tbCategoryExp] (
    [CategoryCode] NVARCHAR (10)  NOT NULL,
    [Expression]   NVARCHAR (256) NOT NULL,
    [Format]       NVARCHAR (100) NOT NULL,
    [RowVer]       ROWVERSION     NOT NULL,
    [IsError] BIT NOT NULL CONSTRAINT DF_Cash_tbCategoryExp_IsError DEFAULT(0),
    [ErrorMessage] NVARCHAR(MAX) NULL,
    CONSTRAINT [PK_Cash_tbCategoryExp] PRIMARY KEY CLUSTERED ([CategoryCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Cash_tbCategoryExp_Cash_tbCategory] FOREIGN KEY ([CategoryCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]) ON DELETE CASCADE ON UPDATE CASCADE
);

