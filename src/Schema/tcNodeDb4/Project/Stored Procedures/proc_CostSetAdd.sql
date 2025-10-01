CREATE   PROCEDURE Project.proc_CostSetAdd(@ProjectCode nvarchar(20))
AS
	SET XACT_ABORT, NOCOUNT ON;
	BEGIN TRY
		DECLARE @UserId nvarchar(10) = (SELECT UserId FROM Usr.vwCredentials);
		IF NOT EXISTS (SELECT * FROM Project.tbCostSet WHERE UserId = @UserId AND ProjectCode = @ProjectCode)
		BEGIN
			INSERT INTO Project.tbCostSet (ProjectCode, UserId)
			VALUES (@ProjectCode, @UserId);
		END
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
