CREATE   FUNCTION App.fnDocInvoiceType
	(
	@InvoiceTypeCode SMALLINT
	)
RETURNS SMALLINT
AS
	BEGIN
	DECLARE @DocTypeCode SMALLINT
	
	SET @DocTypeCode = CASE @InvoiceTypeCode
		WHEN 0 THEN 4		--sales invoice
		WHEN 1 THEN 5		--credit note
		WHEN 3 THEN 6		--debit note
		ELSE 8				--error
		END
	
	RETURN @DocTypeCode
	END

