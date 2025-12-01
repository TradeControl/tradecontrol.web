CREATE   PROCEDURE App.proc_DocDespoolAll
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRAN
		UPDATE Project.tbProject
		SET Spooled = 0, Printed = 1;

		UPDATE  Invoice.tbInvoice
		SET  Spooled = 0, Printed = 1;
		COMMIT TRAN
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
