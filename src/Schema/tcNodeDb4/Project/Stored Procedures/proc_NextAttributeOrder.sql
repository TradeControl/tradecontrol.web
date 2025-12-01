
CREATE   PROCEDURE Project.proc_NextAttributeOrder 
	(
	@ProjectCode nvarchar(20),
	@PrintOrder smallint = 10 output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT     TOP 1 PrintOrder
				  FROM         Project.tbAttribute
				  WHERE     (ProjectCode = @ProjectCode))
			BEGIN
			SELECT  @PrintOrder = MAX(PrintOrder) 
			FROM         Project.tbAttribute
			WHERE     (ProjectCode = @ProjectCode)
			SET @PrintOrder = @PrintOrder - (@PrintOrder % 10) + 10		
			END
		ELSE
			SET @PrintOrder = 10
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
