CREATE   PROCEDURE Cash.proc_ChangeNote (@PaymentAddress nvarchar(42), @Note nvarchar(256))
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		UPDATE Cash.tbChange 
		SET Note = @Note
		WHERE PaymentAddress = @PaymentAddress;			
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
