CREATE   VIEW Invoice.vwTypes
AS
	SELECT Invoice.tbType.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbType.CashModeCode, Cash.tbMode.CashMode, Invoice.tbType.NextNumber
	FROM Invoice.tbType 
		JOIN Cash.tbMode ON Invoice.tbType.CashModeCode = Cash.tbMode.CashModeCode;
