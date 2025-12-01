CREATE PROCEDURE Invoice.proc_RaiseBlank
	(
	@SubjectCode nvarchar(10),
	@InvoiceTypeCode smallint,
	@InvoiceNumber nvarchar(20) = null output
	)
  AS
  SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		DECLARE 
			@UserId nvarchar(10)
			, @NextNumber int
			, @InvoiceSuffix nvarchar(4)
			, @InvoicedOn datetime

		SELECT @UserId = UserId FROM Usr.vwCredentials

		SET @InvoiceSuffix = '.' + @UserId
	
		SELECT @NextNumber = NextNumber
		FROM Invoice.tbType
		WHERE InvoiceTypeCode = @InvoiceTypeCode
	
		SELECT @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
	
		WHILE EXISTS (SELECT     InvoiceNumber
						FROM         Invoice.tbInvoice
						WHERE     (InvoiceNumber = @InvoiceNumber))
			BEGIN
			SET @NextNumber = @NextNumber + 1
			SET @InvoiceNumber = FORMAT(@NextNumber, '000000') + @InvoiceSuffix
			END
		
		SET @InvoicedOn = isnull(CAST(@InvoicedOn AS DATE), CAST(CURRENT_TIMESTAMP AS DATE))

		BEGIN TRANSACTION
	
		EXEC Invoice.proc_Cancel
	
		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)
	
		INSERT INTO Invoice.tbInvoice
								(InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoicedOn, InvoiceStatusCode, PaymentTerms)
		 SELECT @InvoiceNumber, @UserId, @SubjectCode, @InvoiceTypeCode, @InvoicedOn, 0, PaymentTerms
		 FROM Subject.tbSubject
		 WHERE SubjectCode = @SubjectCode
	
		COMMIT TRANSACTION
	
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
