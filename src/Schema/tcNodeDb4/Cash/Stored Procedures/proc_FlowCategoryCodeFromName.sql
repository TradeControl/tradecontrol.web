
CREATE   PROCEDURE Cash.proc_FlowCategoryCodeFromName
	(
		@Category nvarchar(50),
		@CategoryCode nvarchar(10) output
	)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		IF EXISTS (SELECT CategoryCode
					FROM         Cash.tbCategory
					WHERE     (Category = @Category))
			SELECT @CategoryCode = CategoryCode
			FROM         Cash.tbCategory
			WHERE     (Category = @Category)
		ELSE
			SET @CategoryCode = 0 
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH  
