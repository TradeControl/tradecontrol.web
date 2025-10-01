CREATE TABLE [Invoice].[tbMirrorReference] (
    [ContractAddress] NVARCHAR (42) NOT NULL,
    [InvoiceNumber]   NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Invoice_tbMirrorReference] PRIMARY KEY CLUSTERED ([ContractAddress] ASC),
    CONSTRAINT [FK_Invoice_tbMirrorReference_tbInvoice] FOREIGN KEY ([InvoiceNumber]) REFERENCES [Invoice].[tbInvoice] ([InvoiceNumber]) ON DELETE CASCADE,
    CONSTRAINT [FK_Invoice_tbMirrorReference_tbMirror] FOREIGN KEY ([ContractAddress]) REFERENCES [Invoice].[tbMirror] ([ContractAddress]) ON DELETE CASCADE
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Invoice_tbMirrorReference_InvoiceNumber]
    ON [Invoice].[tbMirrorReference]([InvoiceNumber] ASC)
    INCLUDE([ContractAddress]);

