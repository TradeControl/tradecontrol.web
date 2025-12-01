CREATE   PROCEDURE App.proc_EventLogCleardown (@RetentionDays SMALLINT = 30)
AS
	SET NOCOUNT, XACT_ABORT OFF;

	BEGIN TRY
		DECLARE 
			@EventMessage nvarchar(max) = (SELECT [Message] FROM App.tbText WHERE TextId = 1221)
			, @EventTypeCode smallint = 2
			, @LogCode nvarchar(20)

		DELETE FROM App.tbEventLog
		WHERE LoggedOn < DATEADD(DAY, @RetentionDays * -1, CAST(CURRENT_TIMESTAMP AS DATE));
		
		EXECUTE App.proc_EventLog @EventMessage, @EventTypeCode, @LogCode OUTPUT

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
