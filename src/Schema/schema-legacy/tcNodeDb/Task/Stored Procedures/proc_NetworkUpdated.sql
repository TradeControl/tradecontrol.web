CREATE   PROCEDURE Task.proc_NetworkUpdated (@TaskCode nvarchar(20))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		UPDATE Task.tbChangeLog
		SET TransmitStatusCode = 3
		WHERE TaskCode = @TaskCode AND TransmitStatusCode < 3;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
