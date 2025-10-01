CREATE TABLE [Cash].[tbCategoryType] (
    [CategoryTypeCode] SMALLINT      NOT NULL,
    [CategoryType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbCategoryType] PRIMARY KEY CLUSTERED ([CategoryTypeCode] ASC) WITH (FILLFACTOR = 90)
);

