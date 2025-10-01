
CREATE   PROCEDURE Subject.proc_NextAddressCode 
	(
	@SubjectCode nvarchar(10),
	@AddressCode nvarchar(15) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE @AddCount int

		SELECT @AddCount = ISNULL(COUNT(AddressCode), 0) 
		FROM         Subject.tbAddress
		WHERE     (SubjectCode = @SubjectCode)
	
		SET @AddCount += 1
		SET @AddressCode = CONCAT(UPPER(@SubjectCode), '_', FORMAT(@AddCount, '000'))
	
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
