
CREATE   PROCEDURE Project.proc_Mode 
	(
	@ProjectCode nvarchar(20)
	)
AS
  	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Project.tbProject.SubjectCode, Project.tbProject.ObjectCode, Project.tbProject.ProjectStatusCode, Project.tbProject.ActionOn, Project.vwCashPolarity.CashPolarityCode
		FROM         Project.tbProject LEFT OUTER JOIN
							  Project.vwCashPolarity ON Project.tbProject.ProjectCode = Project.vwCashPolarity.ProjectCode
		WHERE     ( Project.tbProject.ProjectCode = @ProjectCode)
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
