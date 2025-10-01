CREATE   PROCEDURE Activity.proc_Mirror(@ActivityCode nvarchar(50), @AccountCode nvarchar(10), @AllocationCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT EXISTS (SELECT * FROM Activity.tbMirror WHERE ActivityCode = @ActivityCode AND AccountCode = @AccountCode AND AllocationCode = @AllocationCode)
		BEGIN
			INSERT INTO Activity.tbMirror (ActivityCode, AccountCode, AllocationCode)
			VALUES (@ActivityCode, @AccountCode, @AllocationCode);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
