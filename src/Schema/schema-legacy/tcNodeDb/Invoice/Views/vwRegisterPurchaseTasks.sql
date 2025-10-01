CREATE VIEW Invoice.vwRegisterPurchaseTasks
AS
	SELECT        StartOn, InvoiceNumber, TaskCode, CashCode, CashDescription, TaxCode, TaxDescription, AccountCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaymentTerms, Printed, AccountName, 
							 UserName, InvoiceStatus, CashModeCode, InvoiceType
	FROM            Invoice.vwRegisterDetail
	WHERE        (InvoiceTypeCode > 1);
