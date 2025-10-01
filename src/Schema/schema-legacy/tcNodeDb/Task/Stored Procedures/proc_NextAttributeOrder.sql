
CREATE   PROCEDURE Task.proc_NextAttributeOrder 
	(
	@TaskCode nvarchar(20),
	@PrintOrder smallint = 10 output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT     TOP 1 PrintOrder
				  FROM         Task.tbAttribute
				  WHERE     (TaskCode = @TaskCode))
			BEGIN
			SELECT  @PrintOrder = MAX(PrintOrder) 
			FROM         Task.tbAttribute
			WHERE     (TaskCode = @TaskCode)
			SET @PrintOrder = @PrintOrder - (@PrintOrder % 10) + 10		
			END
		ELSE
			SET @PrintOrder = 10
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
