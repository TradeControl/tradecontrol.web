
CREATE   PROCEDURE Object.proc_WorkFlow
	(
	@ParentObjectCode nvarchar(50),
	@ObjectCode nvarchar(50)
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS(SELECT * FROM Object.tbFlow WHERE (ParentCode = @ParentObjectCode))
			AND NOT EXISTS(SELECT COUNT(*) FROM Object.tbFlow WHERE ChildCode = @ParentObjectCode GROUP BY ChildCode HAVING COUNT(*) > 1)			
		BEGIN
			SELECT     Object.tbObject.ObjectCode, Project.tbStatus.ProjectStatus, ISNULL(Cash.tbCategory.CashPolarityCode, 2) AS CashPolarityCode, Object.tbObject.UnitOfMeasure, Object.tbFlow.OffsetDays, Object.tbFlow.UsedOnQuantity
			FROM         Object.tbObject INNER JOIN
								  Project.tbStatus ON Object.tbObject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
								  Object.tbFlow ON Object.tbObject.ObjectCode = Object.tbFlow.ChildCode LEFT OUTER JOIN
								  Cash.tbCode ON Object.tbObject.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
								  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE     ( Object.tbFlow.ParentCode = @ObjectCode)
			ORDER BY Object.tbFlow.StepNumber	
		END
		ELSE
		BEGIN
			SELECT     Object.tbObject.ObjectCode, Project.tbStatus.ProjectStatus, ISNULL(Cash.tbCategory.CashPolarityCode, 2) AS CashPolarityCode, Object.tbObject.UnitOfMeasure, Object.tbFlow.OffsetDays, Object.tbFlow.UsedOnQuantity
			FROM         Object.tbObject INNER JOIN
								  Project.tbStatus ON Object.tbObject.ProjectStatusCode = Project.tbStatus.ProjectStatusCode INNER JOIN
								  Object.tbFlow ON Object.tbObject.ObjectCode = Object.tbFlow.ParentCode LEFT OUTER JOIN
								  Cash.tbCode ON Object.tbObject.CashCode = Cash.tbCode.CashCode LEFT OUTER JOIN
								  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
			WHERE     ( Object.tbFlow.ChildCode = @ObjectCode)
			ORDER BY Object.tbFlow.StepNumber	
		END
			 
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
