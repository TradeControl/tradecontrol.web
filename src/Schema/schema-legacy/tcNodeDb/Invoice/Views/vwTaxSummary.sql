CREATE VIEW Invoice.vwTaxSummary
AS
	WITH base AS
	(
		SELECT        InvoiceNumber, TaxCode, SUM(InvoiceValue) AS InvoiceValueTotal, SUM(TaxValue) AS TaxValueTotal
		FROM            Invoice.tbItem
		GROUP BY InvoiceNumber, TaxCode
		HAVING        (NOT (TaxCode IS NULL))
		UNION
		SELECT        InvoiceNumber, TaxCode, SUM(InvoiceValue) AS InvoiceValueTotal, SUM(TaxValue) AS TaxValueTotal
		FROM            Invoice.tbTask
		GROUP BY InvoiceNumber, TaxCode
		HAVING        (NOT (TaxCode IS NULL))
	)
	SELECT        InvoiceNumber, TaxCode, CAST(SUM(InvoiceValueTotal) as decimal(18, 5)) AS InvoiceValueTotal, CAST(SUM(TaxValueTotal) as decimal(18, 5)) AS TaxValueTotal, 
	 CASE WHEN SUM(InvoiceValueTotal) <> 0 THEN CAST((SUM(TaxValueTotal) / SUM(InvoiceValueTotal)) as decimal(18, 5)) ELSE 0 END AS TaxRate
	FROM            base
	GROUP BY InvoiceNumber, TaxCode;

