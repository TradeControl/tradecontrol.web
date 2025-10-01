
CREATE   PROCEDURE Cash.proc_NetworkUpdated(@AccountCode nvarchar(10), @ChargeCode nvarchar(50))
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		UPDATE Cash.tbMirror
		SET TransmitStatusCode = 3
		WHERE AccountCode = @AccountCode AND ChargeCode = @ChargeCode;
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
