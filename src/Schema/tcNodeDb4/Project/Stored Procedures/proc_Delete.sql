
CREATE   PROCEDURE Project.proc_Delete 
	(
	@ProjectCode nvarchar(20)
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @ChildProjectCode nvarchar(20)

		IF @@NESTLEVEL = 1
			BEGIN TRANSACTION

		DELETE FROM Project.tbFlow
		WHERE     (ChildProjectCode = @ProjectCode)

		DECLARE curFlow cursor local for
			SELECT     ChildProjectCode
			FROM         Project.tbFlow
			WHERE     (ParentProjectCode = @ProjectCode)
	
		OPEN curFlow		
		FETCH NEXT FROM curFlow INTO @ChildProjectCode
		WHILE @@FETCH_STATUS = 0
			BEGIN
			EXEC Project.proc_Delete @ChildProjectCode
			FETCH NEXT FROM curFlow INTO @ChildProjectCode		
			END
	
		CLOSE curFlow
		DEALLOCATE curFlow
	
		DELETE FROM Project.tbProject
		WHERE (ProjectCode = @ProjectCode)
	
		IF @@NESTLEVEL = 1
			COMMIT TRANSACTION

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
