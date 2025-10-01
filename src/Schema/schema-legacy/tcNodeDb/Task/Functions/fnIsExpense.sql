CREATE   FUNCTION Task.fnIsExpense
	(
	@TaskCode nvarchar(20)
	)
RETURNS bit
AS
	BEGIN
	/* An expense is a task assigned to an outgoing cash code that is not linked to a sale */
	DECLARE @IsExpense bit
	IF EXISTS (SELECT     Task.tbTask.TaskCode
	           FROM         Task.tbTask INNER JOIN
	                                 Cash.tbCode ON Task.tbTask.CashCode = Cash.tbCode.CashCode INNER JOIN
	                                 Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
	           WHERE     ( Cash.tbCategory.CashModeCode = 1) AND ( Task.tbTask.TaskCode = @TaskCode))
		SET @IsExpense = 0			          
	ELSE IF EXISTS(SELECT     ParentTaskCode
	          FROM         Task.tbFlow
	          WHERE     (ChildTaskCode = @TaskCode))
		BEGIN
		DECLARE @ParentTaskCode nvarchar(20)
		SELECT  @ParentTaskCode = ParentTaskCode
		FROM         Task.tbFlow
		WHERE     (ChildTaskCode = @TaskCode)		
		SET @IsExpense = Task.fnIsExpense(@ParentTaskCode)		
		END	              
	ELSE
		SET @IsExpense = 1
			
	RETURN @IsExpense
	END

