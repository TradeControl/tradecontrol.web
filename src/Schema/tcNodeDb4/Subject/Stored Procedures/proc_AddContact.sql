
CREATE   PROCEDURE Subject.proc_AddContact 
	(
	@SubjectCode nvarchar(10),
	@ContactName nvarchar(100)	 
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
	
		INSERT INTO Subject.tbContact
								(SubjectCode, ContactName, PhoneNumber, EmailAddress)
		SELECT     SubjectCode, @ContactName AS ContactName, PhoneNumber, EmailAddress
		FROM         Subject.tbSubject
		WHERE SubjectCode = @SubjectCode
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
