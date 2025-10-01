
CREATE   PROCEDURE Task.proc_NextOperationNumber 
	(
	@TaskCode nvarchar(20),
	@OperationNumber smallint = 10 output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 OperationNumber
				  FROM         Task.tbOp
				  WHERE     (TaskCode = @TaskCode))
			BEGIN
			SELECT  @OperationNumber = MAX(OperationNumber) 
			FROM         Task.tbOp
			WHERE     (TaskCode = @TaskCode)
			SET @OperationNumber = @OperationNumber - (@OperationNumber % 10) + 10		
			END
		ELSE
			SET @OperationNumber = 10
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
