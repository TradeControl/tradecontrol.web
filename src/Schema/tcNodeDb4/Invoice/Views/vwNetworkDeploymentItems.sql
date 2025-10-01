CREATE VIEW Invoice.vwNetworkDeploymentItems
AS
	SELECT Invoice.tbItem.InvoiceNumber, Invoice.tbItem.CashCode ChargeCode, 
		CASE WHEN LEN(COALESCE(CAST(Invoice.tbItem.ItemReference AS NVARCHAR), '')) > 0 THEN Invoice.tbItem.ItemReference ELSE Cash.tbCode.CashDescription END ChargeDescription, 
			Invoice.tbItem.InvoiceValue, Invoice.tbItem.TaxValue, 0 AS InvoiceQuantity, Invoice.tbItem.TaxCode
	FROM  Invoice.tbItem 
		INNER JOIN Cash.tbCode ON Invoice.tbItem.CashCode = Cash.tbCode.CashCode;
