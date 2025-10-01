
CREATE   PROCEDURE Task.proc_Delete 
	(
	@TaskCode nvarchar(20)
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @ChildTaskCode nvarchar(20)

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION

		DELETE FROM Task.tbFlow
		WHERE     (ChildTaskCode = @TaskCode)

		DECLARE curFlow cursor local for
			SELECT     ChildTaskCode
			FROM         Task.tbFlow
			WHERE     (ParentTaskCode = @TaskCode)
	
		OPEN curFlow		
		FETCH NEXT FROM curFlow INTO @ChildTaskCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Task.proc_Delete @ChildTaskCode
			FETCH NEXT FROM curFlow INTO @ChildTaskCode		
			END
	
		CLOSE curFlow
		DEALLOCATE curFlow
	
		DELETE FROM Task.tbTask
		WHERE (TaskCode = @TaskCode)
	
		IF @@NESTLEVEL = 1
			COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
