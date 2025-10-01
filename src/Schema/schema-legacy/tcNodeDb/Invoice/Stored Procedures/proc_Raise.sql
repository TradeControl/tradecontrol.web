
CREATE   PROCEDURE Invoice.proc_Raise
	(
	@TaskCode nvarchar(20),
	@InvoiceTypeCode smallint,
	@InvoicedOn datetime,
	@InvoiceNumber nvarchar(20) = null output
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@UserId nvarchar(10)
		, @NextNumber int
		, @InvoiceSuffix nvarchar(4)
		, @AccountCode nvarchar(10)
	
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
		SELECT @AccountCode = AccountCode FROM Task.tbTask WHERE TaskCode = @TaskCode


		BEGIN TRANSACTION
	
		EXEC Invoice.proc_Cancel
	
		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode)
	
		INSERT INTO Invoice.tbInvoice
							(InvoiceNumber, UserId, AccountCode, InvoiceTypeCode, InvoicedOn, InvoiceStatusCode, PaymentTerms)
		SELECT     @InvoiceNumber AS InvoiceNumber, @UserId AS UserId, Task.tbTask.AccountCode, @InvoiceTypeCode AS InvoiceTypeCode, @InvoicedOn AS InvoicedOn, 
							0 AS InvoiceStatusCode, Org.tbOrg.PaymentTerms
		FROM         Task.tbTask INNER JOIN
							Org.tbOrg ON Task.tbTask.AccountCode = Org.tbOrg.AccountCode
		WHERE     ( Task.tbTask.TaskCode = @TaskCode)

		EXEC Invoice.proc_AddTask @InvoiceNumber, @TaskCode
	
		IF @@TRANCOUNT > 0		
			COMMIT TRANSACTION
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
