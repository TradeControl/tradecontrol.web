CREATE PROCEDURE [App].[proc_Template_ST_SOLE_CUR_2026]
(
    @FinancialMonth      SMALLINT      = 4,
    @GovAccountName      NVARCHAR(255),
    @BankName            NVARCHAR(255) = NULL,
    @BankAddress         NVARCHAR(MAX) = NULL,
    @DummyAccount        NVARCHAR(50),
    @CurrentAccount      NVARCHAR(50)  = NULL,
    @CA_SortCode         NVARCHAR(10)  = NULL,
    @CA_AccountNumber    NVARCHAR(20)  = NULL,
    @ReserveAccount      NVARCHAR(50)  = NULL,
    @RA_SortCode         NVARCHAR(10)  = NULL,
    @RA_AccountNumber    NVARCHAR(20)  = NULL,
    @IsVATRegistered     BIT           = 0      -- Sole traders default to non‑VAT
)
AS
    SET NOCOUNT, XACT_ABORT ON;
BEGIN TRY

    BEGIN TRAN SoleTraderTemplate;

    ----------------------------------------------------------------
    -- 1. Base template: Minimal Micro Company (current schema)
    ----------------------------------------------------------------
    EXEC [App].[proc_Template_CO_MICRO_CUR_MIN_2026]
        @FinancialMonth   = @FinancialMonth,
        @GovAccountName   = @GovAccountName,
        @BankName         = @BankName,
        @BankAddress      = @BankAddress,
        @DummyAccount     = @DummyAccount,
        @CurrentAccount   = @CurrentAccount,
        @CA_SortCode      = @CA_SortCode,
        @CA_AccountNumber = @CA_AccountNumber,
        @ReserveAccount   = @ReserveAccount,
        @RA_SortCode      = @RA_SortCode,
        @RA_AccountNumber = @RA_AccountNumber;

    ----------------------------------------------------------------
    -- 2. Disable Corporation Tax categories (do NOT delete)
    --    Sole traders may incorporate later; CT is monthly‑scoped
    ----------------------------------------------------------------
    UPDATE Cash.tbCategory
    SET IsEnabled = 0
    WHERE CategoryCode IN ('CT', 'CT_PAY', 'CT_ADJ', 'AC34', 'TC-TAXCO', 'TC-DI');

    UPDATE Cash.tbCode
    SET IsEnabled = 0
    WHERE CashCode IN ('TC500', 'TC501', 'TC601', 'TC602', 'TC701', 'TC900');

    UPDATE App.tbYearPeriod
    SET BusinessTaxRate = 0;

    ----------------------------------------------------------------
    -- 2b. Personal Tax settings for Sole Traders
    ----------------------------------------------------------------
    UPDATE Cash.tbTaxType
    SET MonthNumber = 4,
        RecurrenceCode = 4,
        OffsetDays = 300,
        IsEnabled = 0
    WHERE TaxTypeCode = 0;

    ----------------------------------------------------------------
    -- 3. Sole‑trader specific categories: Drawings, Capital Introduced
    ----------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = 'DRAW')
    BEGIN
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('DRAW', 'Drawings', 0, 1, 0, 900, 1);
    END;

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = 'CAPIN')
    BEGIN
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('CAPIN', 'Capital Introduced', 0, 2, 0, 901, 1);
    END;

    ----------------------------------------------------------------
    -- 4. Cash Codes for Drawings and Capital Introduced
    ----------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'DRAW01')
    BEGIN
        INSERT INTO Cash.tbCode
            (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES
            ('DRAW01', 'Owner Drawings', 'DRAW', 'N/A', 1);
    END;

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CAPIN01')
    BEGIN
        INSERT INTO Cash.tbCode
            (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES
            ('CAPIN01', 'Capital Introduced', 'CAPIN', 'N/A', 1);
    END;

    ----------------------------------------------------------------
    -- 5. VAT handling for non‑registered businesses
    ----------------------------------------------------------------
    IF @IsVATRegistered = 0
        EXEC App.proc_Template_DisableVAT;

    ----------------------------------------------------------------
    -- 6. MTD Tax Type: Sole Trader MTD Quarterly Update
    ----------------------------------------------------------------
    IF NOT EXISTS (
        SELECT 1 FROM Cash.tbTaxType WHERE TaxTypeCode = 4
    )
    BEGIN
        INSERT INTO Cash.tbTaxType
            (TaxTypeCode, TaxType, CashCode, MonthNumber, RecurrenceCode, SubjectCode, OffsetDays)
        VALUES
            (4, 'Sole Trader MTD',
             'TC603', 1, 2,
             (SELECT TOP 1 SubjectCode FROM Cash.tbTaxType), 0);
    END;

    ----------------------------------------------------------------
    -- 7. Tax year alignment: financial year starts on April 6
    ----------------------------------------------------------------
    WITH year_start AS
    (
	    SELECT YearNumber, MIN(StartOn) StartOn
	    FROM App.tbYearPeriod
	    GROUP BY YearNumber
    )
    UPDATE yp
    SET 
	    StartOn = DATEADD(DAY, 5, yp.StartOn)
    FROM year_start ys
	    JOIN App.tbYearPeriod yp
		    ON ys.YearNumber = yp.YearNumber AND ys.StartOn = yp.StartOn
			    AND DATEPART(DAY, yp.StartOn) = 1;

    COMMIT TRAN SoleTraderTemplate;

END TRY
BEGIN CATCH
    EXEC App.proc_ErrorLog;
END CATCH;
GO
