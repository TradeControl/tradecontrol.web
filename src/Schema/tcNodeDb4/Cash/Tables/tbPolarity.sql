CREATE TABLE [Cash].[tbPolarity] (
    [CashPolarityCode] SMALLINT      NOT NULL,
    [CashPolarity]     NVARCHAR (10) NULL,
    CONSTRAINT [PK_Cash_tbPolarity] PRIMARY KEY CLUSTERED ([CashPolarityCode] ASC) WITH (FILLFACTOR = 90)
);

