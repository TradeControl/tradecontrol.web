CREATE TABLE [Cash].[tbCategoryExp] (
    [CategoryCode] NVARCHAR (10)  NOT NULL,
    [Expression]   NVARCHAR (MAX) NOT NULL,
    [Format]       NVARCHAR (100) NOT NULL,
    [SyntaxTypeCode] SMALLINT NOT NULL CONSTRAINT DF_Cash_tbCategoryExp_SyntaxTypeCode DEFAULT(0),
    [IsError] BIT NOT NULL CONSTRAINT DF_Cash_tbCategoryExp_IsError DEFAULT(0),
    [ErrorMessage] NVARCHAR(MAX) NULL,
    [RowVer]       ROWVERSION     NOT NULL,
    CONSTRAINT [PK_Cash_tbCategoryExp] PRIMARY KEY CLUSTERED ([CategoryCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Cash_tbCategoryExp_Cash_tbCategory] FOREIGN KEY ([CategoryCode]) REFERENCES [Cash].[tbCategory] ([CategoryCode]) ON DELETE CASCADE ON UPDATE CASCADE, 
    CONSTRAINT [FK_tbCategoryExp_tbCategoryExpSyntax] FOREIGN KEY (SyntaxTypeCode) REFERENCES Cash.tbCategoryExpSyntax (SyntaxTypeCode)
);


GO

CREATE INDEX [IX_tbCategoryExp_Expressions] ON [Cash].[tbCategoryExp] (SyntaxTypeCode)

GO

CREATE TRIGGER [Cash].[Cash_tbCategoryExp_TriggerUpdate]
ON [Cash].[tbCategoryExp]
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS (
        SELECT 1
        FROM inserted i
        WHERE i.SyntaxTypeCode IN (0, 1)
          AND NOT EXISTS (
              SELECT 1
              FROM [Cash].[tbCategoryExprFormat] f
              WHERE f.TemplateCode = i.[Format]
          )
    )
    BEGIN
        RAISERROR ('Format must be a valid TemplateCode when SyntaxTypeCode is Both (0) or Libre (1).', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END

END
GO
