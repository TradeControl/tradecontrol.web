CREATE   PROCEDURE Activity.proc_NetworkUpdated(@AccountCode nvarchar(10), @AllocationCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE Activity.tbMirror
		SET TransmitStatusCode = 3
		WHERE AccountCode = @AccountCode AND AllocationCode = @AllocationCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
