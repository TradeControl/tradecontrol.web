CREATE PROCEDURE Project.proc_Op (@ProjectCode nvarchar(20))
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS (SELECT     ProjectCode
				   FROM         Project.tbOp
				   WHERE     (ProjectCode = @ProjectCode))
			BEGIN
			SELECT     Project.tbOp.*
				   FROM         Project.tbOp
				   WHERE     (ProjectCode = @ProjectCode)
			END
		ELSE
			BEGIN
			SELECT     Project.tbOp.*
				   FROM         Project.tbFlow INNER JOIN
										 Project.tbOp ON Project.tbFlow.ParentProjectCode = Project.tbOp.ProjectCode
				   WHERE     ( Project.tbFlow.ChildProjectCode = @ProjectCode)
			END
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
