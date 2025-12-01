CREATE   PROCEDURE Subject.proc_DefaultEmailAddress 
	(
	@SubjectCode nvarchar(10),
	@EmailAddress nvarchar(255) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

	SELECT @EmailAddress = COALESCE(EmailAddress, '') FROM Subject.tbSubject WHERE SubjectCode = @SubjectCode;

	IF (LEN(@EmailAddress) = 0)
		SELECT @EmailAddress = EmailAddress
		FROM Subject.tbContact
		WHERE SubjectCode = @SubjectCode AND NOT (EmailAddress IS NULL);

	SET @EmailAddress = COALESCE(@EmailAddress, '');

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
