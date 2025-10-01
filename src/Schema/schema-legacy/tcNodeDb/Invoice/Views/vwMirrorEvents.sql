CREATE VIEW Invoice.vwMirrorEvents
AS
	SELECT        Invoice.tbMirrorEvent.ContractAddress, Invoice.tbMirror.AccountCode, Org.tbOrg.AccountName, Invoice.tbMirror.InvoiceNumber, Invoice.tbMirrorEvent.LogId, Invoice.tbMirrorEvent.EventTypeCode, App.tbEventType.EventType, 
							 Invoice.tbMirrorEvent.InvoiceStatusCode, Invoice.tbStatus.InvoiceStatus, Invoice.tbMirrorEvent.DueOn, Invoice.tbMirrorEvent.PaidValue, Invoice.tbMirrorEvent.PaidTaxValue, 
							 Invoice.tbMirrorEvent.PaymentAddress, Invoice.tbMirrorEvent.InsertedOn, Invoice.tbMirrorEvent.RowVer
	FROM            Invoice.tbMirrorEvent INNER JOIN
							 Invoice.tbMirror ON Invoice.tbMirrorEvent.ContractAddress = Invoice.tbMirror.ContractAddress INNER JOIN
							 Invoice.tbType ON Invoice.tbMirror.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode INNER JOIN
							 App.tbEventType ON Invoice.tbMirrorEvent.EventTypeCode = App.tbEventType.EventTypeCode INNER JOIN
							 Invoice.tbStatus ON Invoice.tbMirrorEvent.InvoiceStatusCode = Invoice.tbStatus.InvoiceStatusCode INNER JOIN
							 Org.tbOrg ON Invoice.tbMirror.AccountCode = Org.tbOrg.AccountCode AND Invoice.tbMirror.AccountCode = Org.tbOrg.AccountCode AND Invoice.tbMirror.AccountCode = Org.tbOrg.AccountCode AND 
							 Invoice.tbMirror.AccountCode = Org.tbOrg.AccountCode AND Invoice.tbMirror.AccountCode = Org.tbOrg.AccountCode AND Invoice.tbMirror.AccountCode = Org.tbOrg.AccountCode;
