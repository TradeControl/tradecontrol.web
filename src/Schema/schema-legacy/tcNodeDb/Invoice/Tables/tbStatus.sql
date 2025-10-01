CREATE TABLE [Invoice].[tbStatus] (
    [InvoiceStatusCode] SMALLINT      NOT NULL,
    [InvoiceStatus]     NVARCHAR (50) NULL,
    CONSTRAINT [PK_Invoice_tbStatus] PRIMARY KEY NONCLUSTERED ([InvoiceStatusCode] ASC) WITH (FILLFACTOR = 90)
);

