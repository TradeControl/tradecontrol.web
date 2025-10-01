CREATE   PROCEDURE App.proc_EventLog (@EventMessage NVARCHAR(MAX), @EventTypeCode SMALLINT = 0, @LogCode NVARCHAR(20) = NULL OUTPUT)
AS
	SET XACT_ABORT, NOCOUNT ON;

	BEGIN TRY

		DECLARE 
			@UserId nvarchar(10)
			, @LogNumber INT
			, @RegisterName nvarchar(50) = (SELECT RegisterName FROM App.tbOptions);
	
		SET @UserId = (SELECT TOP 1 Usr.tbUser.UserId FROM Usr.vwCredentials c INNER JOIN
								Usr.tbUser ON c.UserId = Usr.tbUser.UserId);

		BEGIN TRANSACTION;
		
		WHILE (1 = 1)
			BEGIN
			SET @LogNumber = FORMAT((SELECT TOP 1 r.NextNumber
						FROM App.tbRegister r
						WHERE r.RegisterName = @RegisterName), '00000');
				
			UPDATE App.tbRegister
			SET NextNumber += 1
			WHERE RegisterName = @RegisterName;

			SET @LogCode = CONCAT(@UserId, @LogNumber);

			IF NOT EXISTS (SELECT * FROM App.tbEventLog WHERE LogCode = @LogCode)
				BREAK;
			END

		INSERT INTO App.tbEventLog (LogCode, EventTypeCode, EventMessage)
		VALUES (@LogCode, @EventTypeCode, @EventMessage);

		COMMIT TRANSACTION;

		RETURN;
					
	END TRY
	BEGIN CATCH
		IF @@TRANCOUNT > 0 
			ROLLBACK TRANSACTION;
		THROW;
	END CATCH

