
CREATE   PROCEDURE Object.proc_Mode
	(
	@ObjectCode nvarchar(50)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SELECT     Object.tbObject.ObjectCode, Object.tbObject.UnitOfMeasure, Project.tbStatus.ProjectStatus, ISNULL(Cash.tbCategory.CashPolarityCode, 2) AS CashPolarityCode
		FROM         Object.tbObject INNER JOIN
							  Project.tbStatus ON Object.tbObject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode LEFT OUTER JOIN
							  Cash.tbCode ON Object.tbObject.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
							  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE     ( Object.tbObject.ObjectCode = @ObjectCode)
		 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
