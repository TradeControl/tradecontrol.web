CREATE VIEW Invoice.vwNetworkChangeLog
AS
	SELECT        Invoice.tbChangeLog.LogId, Invoice.tbInvoice.SubjectCode, Subject.tbSubject.SubjectName, Invoice.tbInvoice.InvoiceNumber, Invoice.tbInvoice.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbChangeLog.InvoiceStatusCode, 
							 Invoice.tbStatus.InvoiceStatus, Invoice.tbChangeLog.TransmitStatusCode, Subject.tbTransmitStatus.TransmitStatus, Invoice.tbType.CashPolarityCode, Cash.tbPolarity.CashPolarity, Invoice.tbChangeLog.DueOn, 
							 Invoice.tbChangeLog.InvoiceValue, Invoice.tbChangeLog.TaxValue, Invoice.tbChangeLog.PaidValue, Invoice.tbChangeLog.PaidTaxValue, Invoice.tbChangeLog.UpdatedBy, Invoice.tbChangeLog.ChangedOn, 
							 Invoice.tbChangeLog.RowVer
	FROM            Invoice.tbChangeLog INNER JOIN
							 Invoice.tbInvoice ON Invoice.tbChangeLog.InvoiceNumber = Invoice.tbInvoice.InvoiceNumber INNER JOIN
							 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 Cash.tbPolarity ON Invoice.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbChangeLog.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Subject.tbSubject ON Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode AND Invoice.tbInvoice.SubjectCode = Subject.tbSubject.SubjectCode INNER JOIN
							 Subject.tbTransmitStatus ON Invoice.tbChangeLog.TransmitStatusCode = Subject.tbTransmitStatus.TransmitStatusCode;

