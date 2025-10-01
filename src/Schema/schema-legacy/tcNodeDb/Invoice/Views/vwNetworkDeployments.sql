CREATE VIEW Invoice.vwNetworkDeployments
AS
	SELECT Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.AccountCode, 
		Invoice.tbType.CashModeCode AS PaymentPolarity, 
		CASE Invoice.tbType.InvoiceTypeCode 
			WHEN 0 THEN Invoice.tbType.CashModeCode 
			WHEN 1 THEN 1
			WHEN 2 THEN Invoice.tbType.CashModeCode 
			WHEN 3 THEN 0
		END InvoicePolarity, 
		Invoice.tbInvoice.InvoiceStatusCode,
		Invoice.tbInvoice.DueOn, Invoice.tbInvoice.InvoicedOn, Invoice.tbInvoice.InvoiceValue, Invoice.tbInvoice.TaxValue, 
		Invoice.tbInvoice.PaymentTerms, (SELECT TOP 1 UnitOfCharge FROM App.tbOptions) UnitOfCharge, Cash.tbChangeReference.PaymentAddress,
		Invoice.tbMirrorReference.ContractAddress,
		Invoice.tbMirror.InvoiceNumber ContractNumber
	FROM Invoice.tbMirrorReference 
		RIGHT OUTER JOIN Invoice.tbChangeLog 
		INNER JOIN Invoice.tbInvoice ON Invoice.tbChangeLog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber
		INNER JOIN Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode ON Invoice.tbMirrorReference.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber 
		LEFT OUTER JOIN Invoice.tbMirror ON Invoice.tbMirrorReference.ContractAddress = Invoice.tbMirror.ContractAddress AND Invoice.tbMirrorReference.ContractAddress = Invoice.tbMirror.ContractAddress
		LEFT OUTER JOIN Cash.tbChangeReference ON Invoice.tbInvoice.InvoiceNumber = Cash.tbChangeReference.InvoiceNumber
	WHERE        (Invoice.tbChangeLog.TransmitStatusCode = 1)

