
CREATE   PROCEDURE Invoice.proc_Credit
	(
		@InvoiceNumber nvarchar(20) output
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@InvoiceTypeCode smallint
		, @CreditNumber nvarchar(20)
		, @UserId nvarchar(10)
		, @NextNumber int
		, @InvoiceSuffix nvarchar(4)

		SELECT @UserId = UserId FROM Usr.vwCredentials
	
		SELECT @InvoiceTypeCode =	CASE InvoiceTypeCode 
										WHEN 0 THEN 1 
										WHEN 2 THEN 3 
										ELSE 3 
									END 
		FROM Invoice.tbInvoice WHERE InvoiceNumber = @InvoiceNumber
	
	
		SELECT @UserId = UserId FROM Usr.vwCredentials

		SET @InvoiceSuffix = '.' + @UserId
	
		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode
	
		SELECT @CreditNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
	
		WHILE EXISTS (SELECT     InvoiceNumber
					  FROM         Invoice.tbInvoice
					  WHERE     (InvoiceNumber = @CreditNumber))
			BEGIN
			SET @NextNumber = @NextNumber + 1
			SET @CreditNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
			END

		BEGIN TRANSACTION

		EXEC Invoice.proc_Cancel
	
		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)	
	
		INSERT INTO Invoice.tbInvoice	
							(InvoiceNumber, InvoiceStatusCode, SubjectCode, InvoiceValue, TaxValue, UserId, InvoiceTypeCode, InvoicedOn)
		SELECT     @CreditNumber AS InvoiceNumber, 0 AS InvoiceStatusCode, SubjectCode, InvoiceValue, TaxValue, @UserId AS UserId, 
							@InvoiceTypeCode AS InvoiceTypeCode, CURRENT_TIMESTAMP AS InvoicedOn
		FROM         Invoice.tbInvoice
		WHERE     (InvoiceNumber = @InvoiceNumber)
	
		INSERT INTO Invoice.tbItem
							  (InvoiceNumber, CashCode, TaxCode, InvoiceValue, TaxValue)
		SELECT     @CreditNumber AS InvoiceNumber, CashCode, TaxCode, InvoiceValue, TaxValue
		FROM         Invoice.tbItem
		WHERE     (InvoiceNumber = @InvoiceNumber)
	
		INSERT INTO Invoice.tbProject
							  (InvoiceNumber, ProjectCode, Quantity, InvoiceValue, TaxValue, CashCode, TaxCode)
		SELECT     @CreditNumber AS InvoiceNumber, ProjectCode, Quantity, InvoiceValue, TaxValue, CashCode, TaxCode
		FROM         Invoice.tbProject
		WHERE     (InvoiceNumber = @InvoiceNumber)

		SET @InvoiceNumber = @CreditNumber
	
		COMMIT TRANSACTION
	 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
