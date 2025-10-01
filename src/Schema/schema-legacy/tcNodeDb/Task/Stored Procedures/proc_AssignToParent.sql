CREATE PROCEDURE Task.proc_AssignToParent 
	(
	@ChildTaskCode nvarchar(20),
	@ParentTaskCode nvarchar(20)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@TaskTitle nvarchar(100)
			, @StepNumber smallint

		BEGIN TRANSACTION
		
		IF EXISTS (SELECT ParentTaskCode FROM Task.tbFlow WHERE ChildTaskCode = @ChildTaskCode)
			DELETE FROM Task.tbFlow WHERE ChildTaskCode = @ChildTaskCode

		IF EXISTS(SELECT     TOP 1 StepNumber
				  FROM         Task.tbFlow
				  WHERE     (ParentTaskCode = @ParentTaskCode))
			BEGIN
			SELECT  @StepNumber = MAX(StepNumber) 
			FROM         Task.tbFlow
			WHERE     (ParentTaskCode = @ParentTaskCode)
			SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10		
			END
		ELSE
			SET @StepNumber = 10


		SELECT     @TaskTitle = TaskTitle
		FROM         Task.tbTask
		WHERE     (TaskCode = @ParentTaskCode)		
	
		UPDATE    Task.tbTask
		SET              TaskTitle = @TaskTitle
		WHERE     (TaskCode = @ChildTaskCode) AND ((TaskTitle IS NULL) OR (TaskTitle = ActivityCode))
	
		INSERT INTO Task.tbFlow
							  (ParentTaskCode, StepNumber, ChildTaskCode, UsedOnQuantity)
		VALUES     (@ParentTaskCode, @StepNumber, @ChildTaskCode, 0)
	
		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
