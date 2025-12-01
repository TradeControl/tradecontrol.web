CREATE TABLE [Cash].[tbTxStatus] (
    [TxStatusCode] SMALLINT      NOT NULL,
    [TxStatus]     NVARCHAR (10) NOT NULL,
    CONSTRAINT [PK_Cash_tbTxStatus] PRIMARY KEY CLUSTERED ([TxStatusCode] ASC)
);

