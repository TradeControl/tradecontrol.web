CREATE PROCEDURE Cash.proc_PaymentPostMisc
	(
	@PaymentCode nvarchar(20) 
	)
 AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@InvoiceNumber nvarchar(20), 
			@NextNumber int, 
			@InvoiceTypeCode smallint;

		IF NOT EXISTS (SELECT        Cash.tbPayment.PaymentCode
						FROM            Cash.tbPayment INNER JOIN
												 Cash.tbCode ON Cash.tbPayment.CashCode = Cash.tbCode.CashCode INNER JOIN
												 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
						WHERE        (Cash.tbPayment.PaymentStatusCode <> 1)  
							AND Cash.tbPayment.UserId = (SELECT UserId FROM Usr.vwCredentials))
			RETURN 

		SELECT @InvoiceTypeCode = CASE WHEN PaidInValue != 0 THEN 0 ELSE 2 END 
		FROM         Cash.tbPayment
		WHERE     (PaymentCode = @PaymentCode)
	
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
		
		BEGIN TRANSACTION

		UPDATE    Invoice.tbType
		SET              NextNumber = @NextNumber + 1
		WHERE     (InvoiceTypeCode = @InvoiceTypeCode);

		WITH payment AS
		(
			SELECT UserId, SubjectCode, PaidOn, PaidInValue, PaidOutValue,
					CASE TaxRate WHEN 0 THEN 0
					ELSE
					(
						CASE App.tbTaxCode.RoundingCode 
							WHEN 0 THEN ROUND(Cash.tbPayment.PaidInValue - ( Cash.tbPayment.PaidInValue / (1 + App.tbTaxCode.TaxRate)), Decimals) 
							WHEN 1 THEN ROUND(Cash.tbPayment.PaidInValue - ( Cash.tbPayment.PaidInValue / (1 + App.tbTaxCode.TaxRate)), Decimals, 1) 
						END
					)
					END TaxInValue, 			 
					CASE TaxRate WHEN 0 THEN 0
					ELSE
					(
						CASE App.tbTaxCode.RoundingCode 
							WHEN 0 THEN ROUND(Cash.tbPayment.PaidOutValue - ( Cash.tbPayment.PaidOutValue / (1 + App.tbTaxCode.TaxRate)), Decimals) 
							WHEN 1 THEN ROUND(Cash.tbPayment.PaidOutValue - ( Cash.tbPayment.PaidOutValue / (1 + App.tbTaxCode.TaxRate)), Decimals, 1) 
						END
					)
					END TaxOutValue
			FROM Cash.tbPayment
				INNER JOIN App.tbTaxCode ON Cash.tbPayment.TaxCode = App.tbTaxCode.TaxCode
			WHERE     (PaymentCode = @PaymentCode)
		)
		INSERT INTO Invoice.tbInvoice
								 (InvoiceNumber, UserId, SubjectCode, InvoiceTypeCode, InvoiceStatusCode, InvoicedOn, DueOn, ExpectedOn, InvoiceValue, TaxValue, PaidValue, PaidTaxValue, Printed)
		SELECT        @InvoiceNumber AS InvoiceNumber, payment.UserId, payment.SubjectCode, @InvoiceTypeCode AS InvoiceTypeCode, 3 AS InvoiceStatusCode, 
								payment.PaidOn, payment.PaidOn AS DueOn, payment.PaidOn AS ExpectedOn,
								CASE WHEN PaidInValue > 0 THEN PaidInValue - TaxInValue
									WHEN PaidOutValue > 0 THEN PaidOutValue - TaxOutValue
								END AS InvoiceValue, 
								CASE WHEN payment.PaidInValue > 0 THEN payment.TaxInValue 
									WHEN payment.PaidOutValue > 0 THEN payment.TaxOutValue
								END AS TaxValue, 
								CASE WHEN PaidInValue > 0 THEN PaidInValue - TaxInValue
									WHEN PaidOutValue > 0 THEN PaidOutValue - TaxOutValue
								END AS PaidValue, 
								CASE WHEN payment.PaidInValue > 0 THEN payment.TaxInValue 
									WHEN payment.PaidOutValue > 0 THEN payment.TaxOutValue
								END AS PaidTaxValue, 
								1 AS Printed
		FROM payment;

		WITH payment AS
		(
			SELECT CashCode, TaxCode
			FROM Cash.tbPayment
			WHERE (Cash.tbPayment.PaymentCode = @PaymentCode)
		), invoice_header AS
		(
			SELECT InvoiceNumber, InvoiceValue, TaxValue
			FROM Invoice.tbInvoice
			WHERE InvoiceNumber = @InvoiceNumber
		)
		INSERT INTO Invoice.tbItem
							(InvoiceNumber, CashCode, InvoiceValue, TaxValue, TaxCode)
		SELECT TOP 1 InvoiceNumber, CashCode, InvoiceValue, TaxValue, TaxCode
		FROM payment
			CROSS JOIN invoice_header;

		UPDATE  Subject.tbAccount
		SET CurrentBalance = CASE WHEN PaidInValue > 0 THEN Subject.tbAccount.CurrentBalance + PaidInValue ELSE Subject.tbAccount.CurrentBalance - PaidOutValue END
		FROM         Subject.tbAccount INNER JOIN
							  Cash.tbPayment ON Subject.tbAccount.AccountCode = Cash.tbPayment.AccountCode
		WHERE Cash.tbPayment.PaymentCode = @PaymentCode

		UPDATE Cash.tbPayment
		SET PaymentStatusCode = 1
		WHERE (PaymentCode = @PaymentCode)

		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
