CREATE TABLE [Web].[tbTemplateInvoice] (
    [InvoiceTypeCode] SMALLINT NOT NULL,
    [TemplateId]      INT      NOT NULL,
    [LastUsedOn]      DATETIME NULL,
    CONSTRAINT [PK_Web_tbTemplateInvoice] PRIMARY KEY CLUSTERED ([InvoiceTypeCode] ASC, [TemplateId] ASC),
    CONSTRAINT [FK_tbTemplateInvoice_tbTemplate] FOREIGN KEY ([TemplateId]) REFERENCES [Web].[tbTemplate] ([TemplateId]) ON DELETE CASCADE ON UPDATE CASCADE,
    CONSTRAINT [FK_tbTemplateInvoice_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Web_tbTemplateInvoice]
    ON [Web].[tbTemplateInvoice]([TemplateId] ASC, [InvoiceTypeCode] ASC);


GO
CREATE NONCLUSTERED INDEX [IX_Web_tbTemplateInvoice_LastUsedOn]
    ON [Web].[tbTemplateInvoice]([InvoiceTypeCode] ASC, [LastUsedOn] DESC);

