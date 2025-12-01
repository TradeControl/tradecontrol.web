
 CREATE   PROCEDURE Project.proc_Parent 
	(
	@ProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20) output
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentProjectCode = @ProjectCode
		IF EXISTS(SELECT     ParentProjectCode
					 FROM         Project.tbFlow
					 WHERE     (ChildProjectCode = @ParentProjectCode))
			SELECT @ParentProjectCode = ParentProjectCode
					 FROM         Project.tbFlow
					 WHERE     (ChildProjectCode = @ParentProjectCode)
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH


