CREATE PROCEDURE Invoice.proc_Pay
	(
	@InvoiceNumber nvarchar(20),
	@PaidOn datetime,
	@Post bit = 1,
	@PaymentCode nvarchar(20) NULL OUTPUT
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	DECLARE 
		@PaidOut decimal(18, 5) = 0
		, @PaidIn decimal(18, 5) = 0
		, @BalanceOutstanding decimal(18, 5) = 0
		, @InvoiceOutstanding decimal(18, 5) = 0
		, @CashModeCode smallint
		, @AccountCode nvarchar(10)
		, @CashAccountCode nvarchar(10)
		, @InvoiceStatusCode smallint
		, @UserId nvarchar(10)
		, @PaymentReference nvarchar(20)
		, @PayBalance BIT

		SELECT 
			@CashModeCode = Invoice.tbType.CashModeCode, 
			@AccountCode = Invoice.tbInvoice.AccountCode, 
			@PayBalance = Org.tbOrg.PayBalance,
			@InvoiceStatusCode = Invoice.tbInvoice.InvoiceStatusCode,
			@InvoiceOutstanding = InvoiceValue + TaxValue - PaidValue - PaidTaxValue
		FROM Invoice.tbInvoice 
			INNER JOIN Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
			INNER JOIN Org.tbOrg ON Invoice.tbInvoice.AccountCode = Org.tbOrg.AccountCode
		WHERE     ( Invoice.tbInvoice.InvoiceNumber = @InvoiceNumber)
	
		EXEC Org.proc_BalanceOutstanding @AccountCode, @BalanceOutstanding OUTPUT
		IF @BalanceOutstanding = 0 
		BEGIN
			DECLARE @Msg NVARCHAR(MAX);
			SELECT @Msg = Message FROM App.tbText WHERE TextId = 3018;
			RAISERROR (@Msg, 10, 1)
		END
		ELSE IF @InvoiceStatusCode > 2
			RETURN 1

		SELECT @UserId = UserId FROM Usr.vwCredentials	
		SET @PaidOn = CAST(@PaidOn AS DATE)

		SET @PaymentCode = CONCAT(@UserId, '_', FORMAT(@PaidOn, 'yyyymmdd_hhmmss'))

		WHILE EXISTS (SELECT * FROM Cash.tbPayment WHERE PaymentCode = @PaymentCode)
			BEGIN
			SET @PaidOn = DATEADD(s, 1, @PaidOn)
			SET @PaymentCode = CONCAT(@UserId, '_', FORMAT(@PaidOn, 'yyyymmdd_hhmmss'))
			END
			
		IF @PayBalance = 0
			BEGIN	
			SET @PaymentReference = @InvoiceNumber
														
			IF @CashModeCode = 0
				BEGIN
				SET @PaidOut = @InvoiceOutstanding
				SET @PaidIn = 0
				END
			ELSE
				BEGIN
				SET @PaidIn = @InvoiceOutstanding
				SET @PaidOut = 0
				END
			END
		ELSE
			BEGIN
			SET @PaidIn = CASE WHEN @BalanceOutstanding > 0 THEN @BalanceOutstanding ELSE 0 END
			SET @PaidOut = CASE WHEN @BalanceOutstanding < 0 THEN ABS(@BalanceOutstanding) ELSE 0 END
			END
	
		EXEC Cash.proc_CurrentAccount @CashAccountCode OUTPUT

		BEGIN TRANSACTION

		IF @PaidIn + @PaidOut > 0
			BEGIN			

			INSERT INTO Cash.tbPayment
								  (PaymentCode, UserId, PaymentStatusCode, AccountCode, CashAccountCode, PaidOn, PaidInValue, PaidOutValue, PaymentReference)
			VALUES     (@PaymentCode,@UserId, 0, @AccountCode, @CashAccountCode, @PaidOn, @PaidIn, @PaidOut, @PaymentReference)		
		
			IF @Post <> 0
				EXEC Cash.proc_PaymentPostInvoiced @PaymentCode			
			END
		
		IF @@TRANCOUNT > 0
			COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
