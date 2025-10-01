CREATE   VIEW Invoice.vwDocDetails
AS
	SELECT 
		InvoiceNumber, 
		ProjectCode ItemCode,
		ObjectCode ItemDescription,
		CAST(SecondReference as nvarchar(MAX)) ItemReference,
		TaxCode,
		InvoiceValue,
		TaxValue,
		InvoiceValue + TaxValue AS TotalValue,
		CAST(1 as bit) IsProject,
		ActionedOn,
		Quantity,
		UnitOfMeasure
	FROM Invoice.vwDocProject

	UNION

	SELECT
		InvoiceNumber,
		CashCode ItemCode,
		CashDescription ItemDescription,
		CAST(ItemReference as nvarchar(MAX)) ItemReference,
		TaxCode,
		InvoiceValue,
		TaxValue,
		InvoiceValue + TaxValue AS TotalValue,
		CAST(0 as bit) IsProject,
		ActionedOn,
		1 Quantity,
		NULL UnitOfMeasure	
	FROM Invoice.vwDocItem;
