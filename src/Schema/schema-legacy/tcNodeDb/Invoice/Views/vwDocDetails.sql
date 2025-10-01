CREATE   VIEW Invoice.vwDocDetails
AS
	SELECT 
		InvoiceNumber, 
		TaskCode ItemCode,
		ActivityCode ItemDescription,
		CAST(SecondReference as nvarchar(MAX)) ItemReference,
		TaxCode,
		InvoiceValue,
		TaxValue,
		InvoiceValue + TaxValue AS TotalValue,
		CAST(1 as bit) IsTask,
		ActionedOn,
		Quantity,
		UnitOfMeasure
	FROM Invoice.vwDocTask

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
		CAST(0 as bit) IsTask,
		ActionedOn,
		1 Quantity,
		NULL UnitOfMeasure	
	FROM Invoice.vwDocItem;
