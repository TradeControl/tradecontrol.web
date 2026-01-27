CREATE PROCEDURE Cash.proc_CategoryExprStatusSet
(
	@CategoryCode nvarchar(10), 
	@IsError bit,
	@ErrorMessage nvarchar(max) = null
)
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY

        IF NOT EXISTS (SELECT * FROM Cash.tbCategoryExp WHERE CategoryCode = @CategoryCode AND IsError = @IsError)
		    IF @IsError != 0
		    BEGIN
			    UPDATE tbCategoryExp
			    SET 
				    IsError = 1,
				    ErrorMessage = @ErrorMessage
			    WHERE
				    CategoryCode = @CategoryCode
		    END
		    ELSE
		    BEGIN
			    UPDATE tbCategoryExp
			    SET 
				    IsError = 0,
				    ErrorMessage = NULL
			    WHERE
				    CategoryCode = @CategoryCode
		    END

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
