CREATE TABLE [Cash].[tbChangeReference] (
    [PaymentAddress] NVARCHAR (42) NOT NULL,
    [InvoiceNumber]  NVARCHAR (20) NOT NULL,
    CONSTRAINT [PK_Cash_tbChangeReference] PRIMARY KEY CLUSTERED ([PaymentAddress] ASC),
    CONSTRAINT [FK_Cash_tbChangeReferencee_Cash_tbChange] FOREIGN KEY ([PaymentAddress]) REFERENCES [Cash].[tbChange] ([PaymentAddress]),
    CONSTRAINT [FK_Cash_tbChangeReferencee_Invoice_tbInvoice] FOREIGN KEY ([InvoiceNumber]) REFERENCES [Invoice].[tbInvoice] ([InvoiceNumber])
);


GO
CREATE UNIQUE NONCLUSTERED INDEX [IX_Cash_tbChangeReference_InvoiceNumber]
    ON [Cash].[tbChangeReference]([InvoiceNumber] ASC);


GO
CREATE   TRIGGER Cash.Cash_tbChangeReference_TriggerInsert
ON Cash.tbChangeReference
FOR INSERT, UPDATE
AS
	SET NOCOUNT ON;
	BEGIN TRY
		INSERT INTO Invoice.tbChangeLog (InvoiceNumber, TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue)
		SELECT invoices.InvoiceNumber, 2 TransmitStatusCode, InvoiceStatusCode, DueOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue
		FROM Cash.tbChangeReference inserted 
			JOIN Invoice.tbInvoice invoices ON inserted.InvoiceNumber = invoices.InvoiceNumber;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

