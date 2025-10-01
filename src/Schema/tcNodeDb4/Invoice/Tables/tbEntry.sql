CREATE TABLE [Invoice].[tbEntry] (
    [UserId]          NVARCHAR (10)   NOT NULL,
    [SubjectCode]     NVARCHAR (10)   NOT NULL,
    [CashCode]        NVARCHAR (50)   NOT NULL,
    [InvoiceTypeCode] SMALLINT        NOT NULL,
    [InvoicedOn]      DATETIME        CONSTRAINT [DF_Invoice_tbEntry_InvoicedOn] DEFAULT (CONVERT([date],getdate())) NOT NULL,
    [TaxCode]         NVARCHAR (10)   NULL,
    [ItemReference]   NTEXT           NULL,
    [TotalValue]      DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbEntry_TotalValue] DEFAULT ((0)) NOT NULL,
    [InvoiceValue]    DECIMAL (18, 5) CONSTRAINT [DF_Invoice_tbEntry_InvoiceValue] DEFAULT ((0)) NOT NULL,
    [RowVer]          ROWVERSION      NOT NULL,
    CONSTRAINT [PK_Invoice_tbEntry] PRIMARY KEY CLUSTERED ([SubjectCode] ASC, [CashCode] ASC) WITH (FILLFACTOR = 90),
    CONSTRAINT [FK_Invoice_tbEntry_App_tbTaxCode] FOREIGN KEY ([TaxCode]) REFERENCES [App].[tbTaxCode] ([TaxCode]),
    CONSTRAINT [FK_Invoice_tbEntry_Cash_tbCode] FOREIGN KEY ([CashCode]) REFERENCES [Cash].[tbCode] ([CashCode]) ON UPDATE CASCADE,
    CONSTRAINT [FK_Invoice_tbEntry_Invoice_tbType] FOREIGN KEY ([InvoiceTypeCode]) REFERENCES [Invoice].[tbType] ([InvoiceTypeCode]),
    CONSTRAINT [FK_Invoice_tbEntry_Subject_tb] FOREIGN KEY ([SubjectCode]) REFERENCES [Subject].[tbSubject] ([SubjectCode]),
    CONSTRAINT [FK_Invoice_tbEntry_Usr_tb] FOREIGN KEY ([UserId]) REFERENCES [Usr].[tbUser] ([UserId]) ON UPDATE CASCADE
);


GO
CREATE NONCLUSTERED INDEX [IX_Invoice_tbEntry_UserId]
    ON [Invoice].[tbEntry]([UserId] ASC);

