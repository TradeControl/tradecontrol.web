CREATE   PROCEDURE Task.proc_CostSetAdd(@TaskCode nvarchar(20))
AS
	SET XACT_ABORT, NOCOUNT ON;
	BEGIN TRY
		DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
		IF NOT EXISTS (SELECT * FROM Task.tbCostSet WHERE UserId = @UserId AND TaskCode = @TaskCode)
		BEGIN
			INSERT INTO Task.tbCostSet (TaskCode, UserId)
			VALUES (@TaskCode, @UserId);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
