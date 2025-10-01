CREATE VIEW Invoice.vwChangeLog
AS
	SELECT        changelog.LogId, changelog.InvoiceNumber, Org.tbOrg.AccountCode, Org.tbOrg.AccountName, changelog.ChangedOn, changelog.TransmitStatusCode, transmit.TransmitStatus, changelog.InvoiceStatusCode, 
							 invoicestatus.InvoiceStatus, changelog.DueOn, changelog.InvoiceValue, changelog.TaxValue, changelog.PaidValue, changelog.PaidTaxValue, changelog.UpdatedBy
	FROM            Invoice.tbChangeLog AS changelog INNER JOIN
							 Org.tbTransmitStatus AS transmit ON changelog.TransmitStatusCode = transmit.TransmitStatusCode INNER JOIN
							 Invoice.tbStatus AS invoicestatus ON changelog.InvoiceStatusCode = invoicestatus.InvoiceStatusCode INNER JOIN
							 Invoice.tbInvoice ON changelog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber AND changelog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
							 Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode AND Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode;
