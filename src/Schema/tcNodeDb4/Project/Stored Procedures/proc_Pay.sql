CREATE PROCEDURE Project.proc_Pay (@ProjectCode NVARCHAR(20), @Post BIT = 0,	@PaymentCode nvarchar(20) NULL OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		
		DECLARE 
			@InvoiceTypeCode smallint
			, @InvoiceNumber NVARCHAR(20)
			, @InvoicedOn DATETIME = CURRENT_TIMESTAMP

		SELECT @InvoiceTypeCode = CASE CashPolarityCode WHEN 0 THEN 2 ELSE 0 END, @InvoicedOn = Project.tbProject.PaymentOn
		FROM  Project.tbProject INNER JOIN
				Cash.tbCode ON Project.tbProject.CashCode = Cash.tbCode.CashCode INNER JOIN
				Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode AND 
				Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE Project.tbProject.ProjectCode = @ProjectCode
		
		EXEC Invoice.proc_Raise @ProjectCode = @ProjectCode, @InvoiceTypeCode = @InvoiceTypeCode, @InvoicedOn = @InvoicedOn, @InvoiceNumber = @InvoiceNumber OUTPUT
		EXEC Invoice.proc_Accept @InvoiceNumber
		EXEC Invoice.proc_Pay @InvoiceNumber = @InvoiceNumber, @PaidOn = @InvoicedOn, @Post = @Post, @PaymentCode = @PaymentCode OUTPUT

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
