
CREATE   PROCEDURE Activity.proc_Parent
	(
	@ActivityCode nvarchar(50),
	@ParentCode nvarchar(50) = null output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentCode = @ActivityCode;
		
		IF EXISTS(SELECT ParentCode FROM Activity.tbFlow WHERE (ParentCode = @ActivityCode))
			OR NOT EXISTS(SELECT COUNT(*) FROM Activity.tbFlow WHERE ChildCode = @ActivityCode GROUP BY ChildCode HAVING COUNT(*) > 1)
		BEGIN		
			WHILE EXISTS (SELECT COUNT(*) FROM Activity.tbFlow WHERE ChildCode = @ParentCode GROUP BY ChildCode HAVING COUNT(*) = 1)
				SELECT @ParentCode = ParentCode, @ActivityCode = ParentCode 
				FROM Activity.tbFlow		
				WHERE ChildCode = @ActivityCode;	 
		END
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
