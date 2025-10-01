CREATE TABLE [Cash].[tbMode] (
    [CashModeCode] SMALLINT      NOT NULL,
    [CashMode]     NVARCHAR (10) NULL,
    CONSTRAINT [PK_Cash_tbMode] PRIMARY KEY CLUSTERED ([CashModeCode] ASC) WITH (FILLFACTOR = 90)
);

