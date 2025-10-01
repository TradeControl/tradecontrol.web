CREATE VIEW Invoice.vwNetworkChangeLog
AS
	SELECT        Invoice.tbChangeLog.LogId, Invoice.tbInvoice.AccountCode, Org.tbOrg.AccountName, Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbChangeLog.InvoiceStatusCode, 
							 Invoice.tbStatus.InvoiceStatus, Invoice.tbChangeLog.TransmitStatusCode, Org.tbTransmitStatus.TransmitStatus, Invoice.tbType.CashModeCode, Cash.tbMode.CashMode, Invoice.tbChangeLog.DueOn, 
							 Invoice.tbChangeLog.InvoiceValue, Invoice.tbChangeLog.TaxValue, Invoice.tbChangeLog.PaidValue, Invoice.tbChangeLog.PaidTaxValue, Invoice.tbChangeLog.UpdatedBy, Invoice.tbChangeLog.ChangedOn, 
							 Invoice.tbChangeLog.RowVer
	FROM            Invoice.tbChangeLog INNER JOIN
							 Invoice.tbInvoice ON Invoice.tbChangeLog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Cash.tbMode ON Invoice.tbType.CashModeCode = Cash.tbMode.CashModeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbChangeLog.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode AND Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode INNER JOIN
							 Org.tbTransmitStatus ON Invoice.tbChangeLog.TransmitStatusCode = Org.tbTransmitStatus.TransmitStatusCode;

