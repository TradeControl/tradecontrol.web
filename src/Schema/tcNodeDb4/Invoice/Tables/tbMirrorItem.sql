CREATE TABLE [Invoice].[tbMirrorItem] (
    [ContractAddress]   NVARCHAR (42)   NOT NULL,
    [ChargeCode]        NVARCHAR (50)   NOT NULL,
    [ChargeDescription] NVARCHAR (100)  NULL,
    [TaxCode]           NVARCHAR (10)   NULL,
    [RowVer]            ROWVERSION      NOT NULL,
    [InvoiceValue]      DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirrorItem_InvoiceValue] DEFAULT ((0)) NOT NULL,
    [TaxValue]          DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbMirrorItem_TaxValue] DEFAULT ((0)) NOT NULL,
    CONSTRAINT [PK_Invoice_tbMirrorItem] PRIMARY KEY CLUSTERED ([ContractAddress] ASC, [ChargeCode] ASC),
    CONSTRAINT [FK_Invoice_tbMirrorItem_ContractAddress] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]) ON DELETE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbMirrorItem_InvoiceNumber]
    ON [Invoice].[tbMirrorItem]([ChargeCode] ASC, [ContractAddress] ASC);

