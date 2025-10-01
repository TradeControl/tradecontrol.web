CREATE   PROCEDURE Object.proc_NetworkUpdated(@SubjectCode nvarchar(10), @AllocationCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE Object.tbMirror
		SET TransmitStatusCode = 3
		WHERE SubjectCode = @SubjectCode AND AllocationCode = @AllocationCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
