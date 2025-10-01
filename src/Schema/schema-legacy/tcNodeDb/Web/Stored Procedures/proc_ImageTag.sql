CREATE   PROCEDURE Web.proc_ImageTag(@ImageTag nvarchar(50), @NewImageTag nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;
	BEGIN TRY
		UPDATE Web.tbImage
		SET ImageTag = @NewImageTag
		WHERE ImageTag = @ImageTag
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
