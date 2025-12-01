CREATE PROCEDURE Subject.proc_DefaultTaxCode 
	(
	@SubjectCode nvarchar(10),
	@TaxCode nvarchar(10) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS (SELECT * FROM Subject.tbSubject o JOIN App.tbTaxCode t ON o.TaxCode = t.TaxCode WHERE SubjectCode = @SubjectCode)
			SELECT @TaxCode = TaxCode FROM Subject.tbSubject WHERE SubjectCode = @SubjectCode
		ELSE IF EXISTS(SELECT * FROM  Subject.tbSubject JOIN App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode)
			SELECT @TaxCode = Subject.tbSubject.TaxCode FROM  Subject.tbSubject JOIN App.tbOptions ON Subject.tbSubject.SubjectCode = App.tbOptions.SubjectCode		
		ELSE
			SET @TaxCode = ''

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
