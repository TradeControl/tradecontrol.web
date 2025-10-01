
CREATE   PROCEDURE App.proc_ErrorLog 
AS
DECLARE 
	@ErrorMessage NVARCHAR(MAX)
	, @ErrorSeverity TINYINT
	, @ErrorState TINYINT
	, @MessagePrefix nvarchar(4) = '*** ';
	
	IF @@TRANCOUNT > 0 
		ROLLBACK TRANSACTION;

	SET @ErrorSeverity = ERROR_SEVERITY();
	SET @ErrorState = ERROR_STATE();
	SET @ErrorMessage = ERROR_MESSAGE();

	IF @ErrorMessage NOT LIKE CONCAT(@MessagePrefix, '%')
		BEGIN
		SET @ErrorMessage = CONCAT(@MessagePrefix, ERROR_NUMBER(), ': ', QUOTENAME(ERROR_PROCEDURE()) + '.' + FORMAT(ERROR_LINE(), '0'),
			' Severity ', @ErrorSeverity, ', State ', @ErrorState, ' => ' + LEFT(ERROR_MESSAGE(), 1500));		

		EXEC App.proc_EventLog @ErrorMessage;
		END

	RAISERROR ('%s', @ErrorSeverity, @ErrorState, @ErrorMessage);
