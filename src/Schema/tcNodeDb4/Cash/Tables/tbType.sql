CREATE TABLE [Cash].[tbType] (
    [CashTypeCode] SMALLINT      NOT NULL,
    [CashType]     NVARCHAR (25) NULL,
    CONSTRAINT [PK_Cash_tbType] PRIMARY KEY CLUSTERED ([CashTypeCode] ASC) WITH (FILLFACTOR = 90)
);

