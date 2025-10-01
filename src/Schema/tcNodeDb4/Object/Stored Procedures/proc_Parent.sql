
CREATE   PROCEDURE Object.proc_Parent
	(
	@ObjectCode nvarchar(50),
	@ParentCode nvarchar(50) = null output
	)
AS
    SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		SET @ParentCode = @ObjectCode;
		
		IF EXISTS(SELECT ParentCode FROM Object.tbFlow WHERE (ParentCode = @ObjectCode))
			OR NOT EXISTS(SELECT COUNT(*) FROM Object.tbFlow WHERE ChildCode = @ObjectCode GROUP BY ChildCode HAVING COUNT(*) > 1)
		BEGIN		
			WHILE EXISTS (SELECT COUNT(*) FROM Object.tbFlow WHERE ChildCode = @ParentCode GROUP BY ChildCode HAVING COUNT(*) = 1)
				SELECT @ParentCode = ParentCode, @ObjectCode = ParentCode 
				FROM Object.tbFlow		
				WHERE ChildCode = @ObjectCode;	 
		END
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
