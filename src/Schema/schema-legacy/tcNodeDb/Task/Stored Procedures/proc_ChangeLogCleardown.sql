CREATE   PROCEDURE Task.proc_ChangeLogCleardown (@RetentionDays SMALLINT = 30)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY					
		DECLARE 
			@EventMessage nvarchar(max) = (SELECT [Message] FROM App.tbText WHERE TextId = 1222)
			, @EventTypeCode smallint = 2
			, @LogCode nvarchar(20)

		DELETE FROM Task.tbChangeLog
		WHERE ChangedOn < DATEADD(DAY, @RetentionDays * -1, CAST(CURRENT_TIMESTAMP AS DATE)) 

		EXECUTE App.proc_EventLog @EventMessage, @EventTypeCode, @LogCode OUTPUT

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
