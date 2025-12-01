CREATE   PROCEDURE Project.proc_Project 
	(
	@ProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20) output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentProjectCode = @ProjectCode
		WHILE EXISTS(SELECT     ParentProjectCode
					 FROM         Project.tbFlow
					 WHERE     (ChildProjectCode = @ParentProjectCode))
			SELECT @ParentProjectCode = ParentProjectCode
					 FROM         Project.tbFlow
					 WHERE     (ChildProjectCode = @ParentProjectCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
