CREATE PROCEDURE Cash.proc_CodeDefaults 
	(
	@CashCode nvarchar(50)
	)
  AS
   	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY

		SELECT     Cash.tbCode.CashCode, Cash.tbCode.CashDescription, Cash.tbCode.CategoryCode, Cash.tbCode.TaxCode, 
				App.tbTaxCode.TaxTypeCode, ISNULL( Cash.tbCategory.CashPolarityCode, 0) AS CashPolarityCode, ISNULL(Cash.tbCategory.CashTypeCode, 0) AS CashTypeCode
		FROM         Cash.tbCode INNER JOIN
							  App.tbTaxCode ON Cash.tbCode.TaxCode = App.tbTaxCode.TaxCode LEFT OUTER JOIN
							  Cash.tbCategory ON Cash.tbCode.CategoryCode = Cash.tbCategory.CategoryCode
		WHERE     ( Cash.tbCode.CashCode = @CashCode)
    END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog;
	END CATCH 
