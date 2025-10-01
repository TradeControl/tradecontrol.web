CREATE   PROCEDURE Cash.proc_TxPayOutInvoice 
(
	@AccountCode nvarchar(10),
	@CashCode nvarchar(50),
	@TaxCode nvarchar(10),
	@ItemReference nvarchar(50),
	@PaidOutValue decimal(18,5)
)
AS
  SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		DECLARE @InvoiceNumber nvarchar(20)
		
		BEGIN TRANSACTION
		
		EXEC Invoice.proc_RaiseBlank @AccountCode, 2, @InvoiceNumber OUTPUT;
		
		INSERT INTO Invoice.tbItem (InvoiceNumber, CashCode, TaxCode, ItemReference, TotalValue)
		VALUES (@InvoiceNumber, @CashCode, @TaxCode, @ItemReference, @PaidOutValue);
		
		EXEC Invoice.proc_Accept @InvoiceNumber;

		COMMIT TRANSACTION
	
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
