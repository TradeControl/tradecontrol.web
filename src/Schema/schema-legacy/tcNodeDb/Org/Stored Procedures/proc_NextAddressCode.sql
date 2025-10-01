
CREATE   PROCEDURE Org.proc_NextAddressCode 
	(
	@AccountCode nvarchar(10),
	@AddressCode nvarchar(15) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @AddCount int

		SELECT @AddCount = ISNULL(COUNT(AddressCode), 0) 
		FROM         Org.tbAddress
		WHERE     (AccountCode = @AccountCode)
	
		SET @AddCount += 1
		SET @AddressCode = CONCAT(UPPER(@AccountCode), '_', FORMAT(@AddCount, '000'))
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
