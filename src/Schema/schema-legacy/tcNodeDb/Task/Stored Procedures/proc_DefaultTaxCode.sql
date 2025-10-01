
CREATE   PROCEDURE Task.proc_DefaultTaxCode 
	(
	@AccountCode nvarchar(10),
	@CashCode nvarchar(50),
	@TaxCode nvarchar(10) OUTPUT
	)
AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY		
		IF (NOT @AccountCode IS NULL) and (NOT @CashCode IS NULL)
			BEGIN
			IF EXISTS(SELECT     TaxCode
				  FROM         Org.tbOrg
				  WHERE     (AccountCode = @AccountCode) AND (NOT (TaxCode IS NULL)))
				BEGIN
				SELECT    @TaxCode = TaxCode
				FROM         Org.tbOrg
				WHERE     (AccountCode = @AccountCode) AND (NOT (TaxCode IS NULL))
				END
			ELSE
				BEGIN
				SELECT    @TaxCode =  TaxCode
				FROM         Cash.tbCode
				WHERE     (CashCode = @CashCode)		
				END
			END
		ELSE
			SET @TaxCode = null
		
  	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH
