CREATE TABLE [Cash].[tbCoinType] (
    [CoinTypeCode] SMALLINT      NOT NULL,
    [CoinType]     NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbCoinType] PRIMARY KEY CLUSTERED ([CoinTypeCode] ASC)
);

