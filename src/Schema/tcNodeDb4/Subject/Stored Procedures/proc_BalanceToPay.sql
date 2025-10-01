CREATE   PROCEDURE [Subject].[proc_BalanceToPay](@SubjectCode NVARCHAR(10), @Balance DECIMAL(18, 5) = 0 OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @PayBalance BIT

		SELECT @PayBalance = PayBalance FROM Subject.tbSubject WHERE SubjectCode = @SubjectCode

		IF @PayBalance <> 0
			EXEC Subject.proc_BalanceOutstanding @SubjectCode, @Balance OUTPUT
		ELSE
			BEGIN
			SELECT TOP (1)   @Balance = CASE Invoice.tbType.CashPolarityCode 
											WHEN 0 THEN ((InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue)) * - 1 
											WHEN 1 THEN (InvoiceValue + TaxValue) - (PaidValue + PaidTaxValue) END 
			FROM            Invoice.tbInvoice INNER JOIN
									 Invoice.tbType ON Invoice.tbInvoice.InvoiceTypeCode = Invoice.tbType.InvoiceTypeCode
			WHERE  Invoice.tbInvoice.SubjectCode = @SubjectCode AND (Invoice.tbInvoice.InvoiceStatusCode > 0) AND (Invoice.tbInvoice.InvoiceStatusCode < 3) 
			ORDER BY ExpectedOn
			END

		SET @Balance = ISNULL(@Balance, 0)

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

