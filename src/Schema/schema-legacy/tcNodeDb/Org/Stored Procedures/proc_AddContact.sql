
CREATE   PROCEDURE Org.proc_AddContact 
	(
	@AccountCode nvarchar(10),
	@ContactName nvarchar(100)	 
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	
		INSERT INTO Org.tbContact
								(AccountCode, ContactName, PhoneNumber, EmailAddress)
		SELECT     AccountCode, @ContactName AS ContactName, PhoneNumber, EmailAddress
		FROM         Org.tbOrg
		WHERE AccountCode = @AccountCode
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
