
CREATE   PROCEDURE Project.proc_NextOperationNumber 
	(
	@ProjectCode nvarchar(20),
	@OperationNumber smallint = 10 output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS(SELECT     TOP 1 OperationNumber
				  FROM         Project.tbOp
				  WHERE     (ProjectCode = @ProjectCode))
			BEGIN
			SELECT  @OperationNumber = MAX(OperationNumber) 
			FROM         Project.tbOp
			WHERE     (ProjectCode = @ProjectCode)
			SET @OperationNumber = @OperationNumber - (@OperationNumber % 10) + 10		
			END
		ELSE
			SET @OperationNumber = 10
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
