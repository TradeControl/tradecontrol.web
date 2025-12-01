CREATE   PROCEDURE Object.proc_Mirror(@ObjectCode nvarchar(50), @SubjectCode nvarchar(10), @AllocationCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT EXISTS (SELECT * FROM Object.tbMirror WHERE ObjectCode = @ObjectCode AND SubjectCode = @SubjectCode AND AllocationCode = @AllocationCode)
		BEGIN
			INSERT INTO Object.tbMirror (ObjectCode, SubjectCode, AllocationCode)
			VALUES (@ObjectCode, @SubjectCode, @AllocationCode);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
