CREATE VIEW Invoice.vwRegisterPurchaseProjects
AS
	SELECT        StartOn, InvoiceNumber, ProjectCode, CashCode, CashDescription, TaxCode, TaxDescription, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, InvoiceValue, TaxValue, PaymentTerms, Printed, SubjectName, 
							 UserName, InvoiceStatus, CashPolarityCode, InvoiceType
	FROM            Invoice.vwRegisterDetail
	WHERE        (InvoiceTypeCode > 1);
