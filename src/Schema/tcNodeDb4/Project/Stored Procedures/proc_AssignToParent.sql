CREATE PROCEDURE Project.proc_AssignToParent 
	(
	@ChildProjectCode nvarchar(20),
	@ParentProjectCode nvarchar(20)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE 
			@ProjectTitle nvarchar(100)
			, @StepNumber smallint

		BEGIN TRANSACTION
		
		IF EXISTS (SELECT ParentProjectCode FROM Project.tbFlow WHERE ChildProjectCode = @ChildProjectCode)
			DELETE FROM Project.tbFlow WHERE ChildProjectCode = @ChildProjectCode

		IF EXISTS(SELECT     TOP 1 StepNumber
				  FROM         Project.tbFlow
				  WHERE     (ParentProjectCode = @ParentProjectCode))
			BEGIN
			SELECT  @StepNumber = MAX(StepNumber) 
			FROM         Project.tbFlow
			WHERE     (ParentProjectCode = @ParentProjectCode)
			SET @StepNumber = @StepNumber - (@StepNumber % 10) + 10		
			END
		ELSE
			SET @StepNumber = 10


		SELECT     @ProjectTitle = ProjectTitle
		FROM         Project.tbProject
		WHERE     (ProjectCode = @ParentProjectCode)		
	
		UPDATE    Project.tbProject
		SET              ProjectTitle = @ProjectTitle
		WHERE     (ProjectCode = @ChildProjectCode) AND ((ProjectTitle IS NULL) OR (ProjectTitle = ObjectCode))
	
		INSERT INTO Project.tbFlow
							  (ParentProjectCode, StepNumber, ChildProjectCode, UsedOnQuantity)
		VALUES     (@ParentProjectCode, @StepNumber, @ChildProjectCode, 0)
	
		COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
