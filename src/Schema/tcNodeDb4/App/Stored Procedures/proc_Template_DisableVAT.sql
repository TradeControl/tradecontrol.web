CREATE PROCEDURE [App].[proc_Template_DisableVAT]
AS
SET NOCOUNT, XACT_ABORT ON;

BEGIN TRY

    BEGIN TRAN DisableVAT;

    ----------------------------------------------------------------
    -- 1. Disable VAT categories
    ----------------------------------------------------------------
    UPDATE Cash.tbCategory
    SET IsEnabled = 0
    WHERE CategoryCode IN ('TC-VAT', 'TC-TAXGD');

    ----------------------------------------------------------------
    -- 2. Disable VAT control Cash Codes
    ----------------------------------------------------------------
    UPDATE Cash.tbCode
    SET IsEnabled = 0
    WHERE CashCode IN ('TC600', 'TC501', 'TC602');

    ----------------------------------------------------------------
    -- 3. Default all VAT-bearing Cash Codes to zero-rated (T0)
    --    Preserve special codes such as 'N/A'
    ----------------------------------------------------------------
    UPDATE C
    SET C.TaxCode = 'T0'
    FROM Cash.tbCode C
    WHERE C.TaxCode <> 'T0'
      AND C.TaxCode NOT IN ('N/A');

    COMMIT TRAN DisableVAT;

END TRY
BEGIN CATCH
    EXEC App.proc_ErrorLog;
END CATCH;
GO
