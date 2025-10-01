CREATE   PROCEDURE Project.proc_NetworkUpdated (@ProjectCode nvarchar(20))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		UPDATE Project.tbChangeLog
		SET TransmitStatusCode = 3
		WHERE ProjectCode = @ProjectCode AND TransmitStatusCode < 3;

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
