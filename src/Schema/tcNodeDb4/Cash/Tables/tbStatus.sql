CREATE TABLE [Cash].[tbStatus] (
    [CashStatusCode] SMALLINT      NOT NULL,
    [CashStatus]     NVARCHAR (15) NOT NULL,
    CONSTRAINT [PK_Cash_tbStatus] PRIMARY KEY CLUSTERED ([CashStatusCode] ASC) WITH (FILLFACTOR = 90)
);

