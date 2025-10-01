CREATE TABLE [Web].[tbAttachmentInvoice] (
    [InvoiceTypeCode] SMALLINT NOT NULL,
    [AttachmentId]    INT      NOT NULL,
    CONSTRAINT [PK_Web_tbInvoiceAttachment] PRIMARY KEY CLUSTERED ([InvoiceTypeCode] ASC, [AttachmentId] ASC),
    CONSTRAINT [FK_tbAttachmentInvoice_tbAttachment] FOREIGN KEY ([AttachmentId]) REFERENCES [Web].[tbAttachment] ([AttachmentId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbAttachmentInvoice_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbAttachmentInvoice]
    ON [Web].[tbAttachmentInvoice]([AttachmentId] ASC, [InvoiceTypeCode] ASC);

