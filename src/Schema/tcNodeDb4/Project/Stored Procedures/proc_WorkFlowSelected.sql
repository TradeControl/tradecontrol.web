
CREATE   PROCEDURE Project.proc_WorkFlowSelected 
	(
	@ChildProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20) = NULL
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF NOT (@ParentProjectCode IS NULL)
			SELECT        Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionOn, Project.vwCashPolarity.CashPolarityCode, Project.tbFlow.OffsetDays
			FROM            Project.tbProject INNER JOIN
									 Project.tbFlow ON Project.tbProject.ProjectCode = Project.tbFlow.ChildProjectCode LEFT OUTER JOIN
									 Project.vwCashPolarity ON Project.tbProject.ProjectCode = Project.vwCashPolarity.ProjectCode
			WHERE        (Project.tbFlow.ParentProjectCode = @ParentProjectCode) AND (Project.tbFlow.ChildProjectCode = @ChildProjectCode)
		ELSE
			SELECT        Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionOn, Project.vwCashPolarity.CashPolarityCode, 0 AS OffsetDays
			FROM            Project.tbProject LEFT OUTER JOIN
									 Project.vwCashPolarity ON Project.tbProject.ProjectCode = Project.vwCashPolarity.ProjectCode
			WHERE        (Project.tbProject.ProjectCode = @ChildProjectCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
