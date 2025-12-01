CREATE   PROCEDURE Invoice.proc_NetworkUpdated (@InvoiceNumber nvarchar(20))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		UPDATE Invoice.tbChangeLog
		SET TransmitStatusCode = 3
		WHERE InvoiceNumber = @InvoiceNumber AND TransmitStatusCode < 3;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
