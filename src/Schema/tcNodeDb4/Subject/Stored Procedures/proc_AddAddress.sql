
CREATE   PROCEDURE Subject.proc_AddAddress 
	(
	@SubjectCode nvarchar(10),
	@Address ntext
	)
AS
 	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @AddressCode nvarchar(15)
	
		EXECUTE Subject.proc_NextAddressCode @SubjectCode, @AddressCode OUTPUT
	
		INSERT INTO Subject.tbAddress
							  (AddressCode, SubjectCode, Address)
		VALUES     (@AddressCode, @SubjectCode, @Address)
	
		IF NOT EXISTS (SELECT * FROM Subject.tbSubject Subject JOIN Subject.tbAddress Subject_addr ON Subject.AddressCode = Subject_addr.AddressCode WHERE Subject.SubjectCode = @SubjectCode)
		BEGIN
			UPDATE Subject.tbSubject
			SET AddressCode = @AddressCode
			WHERE Subject.tbSubject.SubjectCode = @SubjectCode
		END

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
