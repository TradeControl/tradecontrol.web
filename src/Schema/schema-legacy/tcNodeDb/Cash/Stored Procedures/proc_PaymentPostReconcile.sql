CREATE PROCEDURE Cash.proc_PaymentPostReconcile
	(
	@PaymentCode nvarchar(20),
	@PostValue decimal(18, 5),
	@CashCode nvarchar(50),
	@TaxCode nvarchar(5),
	@InvoiceTypeCode smallint
	)
 AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@InvoiceNumber nvarchar(20)
			, @NextNumber int;

		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode;
		
		SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + '.' + (SELECT UserId FROM Usr.vwCredentials)

		WHILE EXISTS (SELECT     InvoiceNumber
					  FROM         Invoice.tbInvoice
					  WHERE     (InvoiceNumber = @InvoiceNumber))
			BEGIN
			SET @NextNumber += @NextNumber 
			SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + '.' + (SELECT UserId FROM Usr.vwCredentials)
			END

		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)

		INSERT INTO Invoice.tbInvoice
								 (InvoiceNumber, UserId, AccountCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, DueOn, ExpectedOn, Printed)
		SELECT        @InvoiceNumber AS InvoiceNumber, Cash.tbPayment.UserId, Cash.tbPayment.AccountCode, @InvoiceTypeCode AS InvoiceTypeCode, 3 AS InvoiceStatusCode, 
								Cash.tbPayment.PaidOn, Cash.tbPayment.PaidOn AS DueOn, Cash.tbPayment.PaidOn AS ExpectedOn, 1 AS Printed
		FROM            Cash.tbPayment 
		WHERE        ( Cash.tbPayment.PaymentCode = @PaymentCode)

		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TotalValue, TaxCode)
		VALUES (@InvoiceNumber, @CashCode, @PostValue, @TaxCode)

		EXEC Invoice.proc_Total @InvoiceNumber

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
