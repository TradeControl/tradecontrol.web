CREATE TABLE [Cash].[tbEntryType] (
    [CashEntryTypeCode] SMALLINT      NOT NULL,
    [CashEntryType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbEntryType] PRIMARY KEY CLUSTERED ([CashEntryTypeCode] ASC) WITH (FILLFACTOR = 90)
);

