CREATE PROCEDURE [App].[proc_Template_ST_SOLE_CUR_STD_2026]
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
    @IsVATRegistered     BIT           = 0
)
AS
    SET NOCOUNT, XACT_ABORT ON;
BEGIN TRY

    BEGIN TRAN SoleTraderStdTemplate;

    ----------------------------------------------------------------
    -- 1. Base: Sole Trader MIN (includes ITSA sources/tags)
    ----------------------------------------------------------------
    EXEC App.proc_Template_ST_SOLE_CUR_MIN_2026
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
        @RA_AccountNumber = @RA_AccountNumber,
        @IsVATRegistered  = @IsVATRegistered;

    ----------------------------------------------------------------
    -- 2. Additive cash codes (no 1:1 nominal categories)
    --    Overhead-type costs -> CA-ADMIN
    ----------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-PHONE')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-PHONE', 'Phone & Internet', 'CA-ADMIN', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-INSUR')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-INSUR', 'Insurance', 'CA-ADMIN', 'N/A', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-BANKC')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-BANKC', 'Bank Charges', 'CA-ADMIN', 'N/A', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-PROF')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-PROF', 'Professional Fees', 'CA-ADMIN', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-ADVT')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-ADVT', 'Advertising & Marketing', 'CA-ADMIN', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-REPA')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-REPA', 'Repairs & Maintenance', 'CA-ADMIN', 'T1', 1);

    ----------------------------------------------------------------
    -- 3. Example extension: Transport / Travel nominal grouping
    ----------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = 'CA-TRAVEL')
    BEGIN
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('CA-TRAVEL', 'Travel & Transport', 0, 0, 0, 149, 1);

        INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
        VALUES ('CT-OVERHD', 'CA-TRAVEL');
    END;

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-PARK')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-PARK', 'Parking & Tolls', 'CA-TRAVEL', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-PUBTR')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-PUBTR', 'Public Transport', 'CA-TRAVEL', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-HOTEL')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-HOTEL', 'Accommodation', 'CA-TRAVEL', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-MEALS')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-MEALS', 'Subsistence / Meals', 'CA-TRAVEL', 'N/A', 1);

    ----------------------------------------------------------------
    -- 4. Example extension: Motor nominal grouping (granular breakdown)
    ----------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = 'CA-MOTOR')
    BEGIN
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('CA-MOTOR', 'Motor Expenses', 0, 0, 0, 150, 1);

        INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
        VALUES ('CT-OVERHD', 'CA-MOTOR');
    END;

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-MFUEL')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-MFUEL', 'Motor Fuel', 'CA-MOTOR', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-MREPA')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-MREPA', 'Motor Repairs & Servicing', 'CA-MOTOR', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-MINSR')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-MINSR', 'Motor Insurance', 'CA-MOTOR', 'N/A', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-MLICN')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-MLICN', 'Road Tax / Licences', 'CA-MOTOR', 'N/A', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-MLEASE')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-MLEASE', 'Vehicle Lease / Hire', 'CA-MOTOR', 'T1', 1);

    ----------------------------------------------------------------
    -- 5. Finance
    ----------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = 'CA-FINANCE')
    BEGIN
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('CA-FINANCE', 'Finance Costs', 0, 0, 0, 151, 1);

        INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
        VALUES ('CT-OVERHD', 'CA-FINANCE');
    END;

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-LOINT')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-LOINT', 'Loan Interest', 'CA-FINANCE', 'INT', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-FINCH')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-FINCH', 'Financial Charges', 'CA-FINANCE', 'N/A', 1);

    ----------------------------------------------------------------
    -- 6. Premises
    ----------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = 'CA-PREMS')
    BEGIN
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('CA-PREMS', 'Premises Running Costs', 0, 0, 0, 152, 1);

        INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
        VALUES ('CT-OVERHD', 'CA-PREMS');
    END;

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-RENT')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-RENT', 'Rent', 'CA-PREMS', 'N/A', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-UTILS')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-UTILS', 'Utilities', 'CA-PREMS', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-CLEAN')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-CLEAN', 'Cleaning', 'CA-PREMS', 'T1', 1);

    IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-PREMS')
        INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES ('CC-PREMS', 'Premises Costs', 'CA-PREMS', 'T1', 1);

    ----------------------------------------------------------------
    -- 7. UK-ITSA-* Slice 2 mappings (STD-owned additions only)
    ----------------------------------------------------------------
    INSERT INTO Cash.tbTaxTagMap
        (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled)
    SELECT v.TaxSourceCode, v.TagCode, v.MapTypeCode, v.CategoryCode, v.CashCode, 1
    FROM (VALUES
        -- QU: overhead categories created by STD
        ('UK-ITSA-SE-QU', 'carVanExpenses',       CONVERT(TINYINT, 0), 'CA-MOTOR',  CAST('' AS NVARCHAR(50))),
        ('UK-ITSA-SE-QU', 'travelExpenses',       CONVERT(TINYINT, 0), 'CA-TRAVEL', CAST('' AS NVARCHAR(50))),
        ('UK-ITSA-SE-QU', 'premisesRunningCosts', CONVERT(TINYINT, 0), 'CA-PREMS',  CAST('' AS NVARCHAR(50))),
        ('UK-ITSA-SE-QU', 'adminCosts',           CONVERT(TINYINT, 0), 'CA-ADMIN',  CAST('' AS NVARCHAR(50))),

        -- QU: finance + selected overhead headings by CashCode (created/enabled by STD)
        ('UK-ITSA-SE-QU', 'interestOnLoans',      CONVERT(TINYINT, 1), CAST('' AS NVARCHAR(10)), 'CC-LOINT'),
        ('UK-ITSA-SE-QU', 'financialCharges',     CONVERT(TINYINT, 1), CAST('' AS NVARCHAR(10)), 'CC-FINCH'),
        ('UK-ITSA-SE-QU', 'professionalFees',     CONVERT(TINYINT, 1), CAST('' AS NVARCHAR(10)), 'CC-PROF'),
        ('UK-ITSA-SE-QU', 'advertisingMarketing', CONVERT(TINYINT, 1), CAST('' AS NVARCHAR(10)), 'CC-ADVT'),

        -- EOPS: overhead categories created by STD
        ('UK-ITSA-SE-EOPS', 'carVanExpenses',       CONVERT(TINYINT, 0), 'CA-MOTOR',  CAST('' AS NVARCHAR(50))),
        ('UK-ITSA-SE-EOPS', 'travelExpenses',       CONVERT(TINYINT, 0), 'CA-TRAVEL', CAST('' AS NVARCHAR(50))),
        ('UK-ITSA-SE-EOPS', 'premisesRunningCosts', CONVERT(TINYINT, 0), 'CA-PREMS',  CAST('' AS NVARCHAR(50))),
        ('UK-ITSA-SE-EOPS', 'adminCosts',           CONVERT(TINYINT, 0), 'CA-ADMIN',  CAST('' AS NVARCHAR(50))),

        -- EOPS: finance + selected overhead headings by CashCode
        ('UK-ITSA-SE-EOPS', 'interestOnLoans',      CONVERT(TINYINT, 1), CAST('' AS NVARCHAR(10)), 'CC-LOINT'),
        ('UK-ITSA-SE-EOPS', 'financialCharges',     CONVERT(TINYINT, 1), CAST('' AS NVARCHAR(10)), 'CC-FINCH'),
        ('UK-ITSA-SE-EOPS', 'professionalFees',     CONVERT(TINYINT, 1), CAST('' AS NVARCHAR(10)), 'CC-PROF'),
        ('UK-ITSA-SE-EOPS', 'advertisingMarketing', CONVERT(TINYINT, 1), CAST('' AS NVARCHAR(10)), 'CC-ADVT')
    ) v(TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode)
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM Cash.tbTaxTagMap tm
        WHERE tm.TaxSourceCode = v.TaxSourceCode
          AND tm.TagCode = v.TagCode
          AND tm.MapTypeCode = v.MapTypeCode
          AND tm.CategoryCode = v.CategoryCode
          AND tm.CashCode = v.CashCode
    );

    EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = 'UK-ITSA-SE-QU';
    EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = 'UK-ITSA-SE-EOPS';

    COMMIT TRAN SoleTraderStdTemplate;

END TRY
BEGIN CATCH
    EXEC App.proc_ErrorLog;
END CATCH;
GO
