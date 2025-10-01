CREATE PROCEDURE Org.proc_DefaultTaxCode 
	(
	@AccountCode nvarchar(10),
	@TaxCode nvarchar(10) OUTPUT
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		IF EXISTS (SELECT * FROM Org.tbOrg o JOIN App.tbTaxCode t ON o.TaxCode = t.TaxCode WHERE AccountCode = @AccountCode)
			SELECT @TaxCode = TaxCode FROM Org.tbOrg WHERE AccountCode = @AccountCode
		ELSE IF EXISTS(SELECT * FROM  Org.tbOrg JOIN App.tbOptions ON Org.tbOrg.AccountCode = App.tbOptions.AccountCode)
			SELECT @TaxCode = Org.tbOrg.TaxCode FROM  Org.tbOrg JOIN App.tbOptions ON Org.tbOrg.AccountCode = App.tbOptions.AccountCode		
		ELSE
			SET @TaxCode = ''

  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
