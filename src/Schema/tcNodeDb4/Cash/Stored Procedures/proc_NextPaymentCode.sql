CREATE   PROCEDURE Cash.proc_NextPaymentCode (@PaymentCode NVARCHAR(20) OUTPUT)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT @PaymentCode = PaymentCode FROM Cash.vwPaymentCode;
		WHILE EXISTS (SELECT * FROM Cash.tbPayment WHERE PaymentCode = @PaymentCode)
			SELECT @PaymentCode = PaymentCode FROM Cash.vwPaymentCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH

