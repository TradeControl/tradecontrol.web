CREATE   PROCEDURE Org.proc_DefaultEmailAddress 
	(
	@AccountCode nvarchar(10),
	@EmailAddress nvarchar(255) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

	SELECT @EmailAddress = COALESCE(EmailAddress, '') FROM Org.tbOrg WHERE AccountCode = @AccountCode;

	IF (LEN(@EmailAddress) = 0)
		SELECT @EmailAddress = EmailAddress
		FROM Org.tbContact
		WHERE AccountCode = @AccountCode AND NOT (EmailAddress IS NULL);

	SET @EmailAddress = COALESCE(@EmailAddress, '');

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
