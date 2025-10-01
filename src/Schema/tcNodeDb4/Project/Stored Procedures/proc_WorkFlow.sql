
CREATE   PROCEDURE Project.proc_WorkFlow 
	(
	@ProjectCode nvarchar(20)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Project.tbFlow.ParentProjectCode, Project.tbFlow.StepNumber, Project.tbProject.ProjectCode, Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, 
							  Project.tbProject.ActionOn, Project.vwCashPolarity.CashPolarityCode, Project.tbFlow.OffsetDays
		FROM         Project.tbProject INNER JOIN
							  Project.tbFlow ON Project.tbProject.ProjectCode = Project.tbFlow.ChildProjectCode LEFT OUTER JOIN
							  Project.vwCashPolarity ON Project.tbProject.ProjectCode = Project.vwCashPolarity.ProjectCode
		WHERE     ( Project.tbFlow.ParentProjectCode = @ProjectCode)
		ORDER BY Project.tbFlow.StepNumber, Project.tbFlow.ParentProjectCode
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
