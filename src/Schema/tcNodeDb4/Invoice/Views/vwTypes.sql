CREATE   VIEW Invoice.vwTypes
AS
	SELECT Invoice.tbType.InvoiceTypeCode, Invoice.tbType.InvoiceType, Invoice.tbType.CashPolarityCode, Cash.tbPolarity.CashPolarity, Invoice.tbType.NextNumber
	FROM Invoice.tbType 
		JOIN Cash.tbPolarity ON Invoice.tbType.CashPolarityCode = Cash.tbPolarity.CashPolarityCode;
