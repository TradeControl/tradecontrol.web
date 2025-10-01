CREATE TABLE [Invoice].[tbType] (
    [InvoiceTypeCode] SMALLINT      NOT NULL,
    [InvoiceType]     NVARCHAR (20) NOT NULL,
    [CashPolarityCode]    SMALLINT      NOT NULL,
    [NextNumber]      INT           CONSTRAINT [DF_Invoice_tbType_NextNumber] DEFAULT ((1000)) NOT NULL,
    [RowVer]          ROWVERSION    NOT NULL,
    CONSTRAINT [PK_Invoice_tbType] PRIMARY KEY CLUSTERED ([InvoiceTypeCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Invoice_tbType_Cash_tbPolarity] FOREIGN KEY ([CashPolarityCode]) REFERENCES [Cash].[tbPolarity] ([CashPolarityCode])
);

