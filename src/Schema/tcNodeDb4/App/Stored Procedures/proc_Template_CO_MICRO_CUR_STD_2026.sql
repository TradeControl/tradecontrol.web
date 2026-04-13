CREATE PROCEDURE [App].[proc_Template_CO_MICRO_CUR_STD_2026]
(
    @FinancialMonth SMALLINT = 4,
    @GovAccountName NVARCHAR(255),
    @BankName NVARCHAR(255) = NULL,
    @BankAddress NVARCHAR(MAX) = NULL,
    @DummyAccount NVARCHAR(50),
    @CurrentAccount NVARCHAR(50) = NULL,
    @CA_SortCode NVARCHAR(10) = NULL,
    @CA_AccountNumber NVARCHAR(20) = NULL,
    @ReserveAccount NVARCHAR(50) = NULL,
    @RA_SortCode NVARCHAR(10) = NULL,
    @RA_AccountNumber NVARCHAR(20) = NULL,
    @IsVATRegistered BIT = 1      -- Micro entity default to VAT Registered
)
AS
    SET NOCOUNT, XACT_ABORT ON;

BEGIN TRY

    BEGIN TRAN MicroStdTemplate;

    ----------------------------------------------------------------
    -- 1. CALL SHARED MICRO CORE
    ----------------------------------------------------------------
    EXEC [App].[proc_Template_CO_MICRO_CUR_2026]
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
    -- 2. STANDARD MICRO CATEGORY TREE (ADDITIVE)
    ----------------------------------------------------------------
    INSERT INTO Cash.tbCategory
        (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
    VALUES
        ('CA-BUILD', 'Building', 0, 0, 0, 411, 1);

    INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
    VALUES
        ('CT-OVERHD', 'CA-BUILD');

    ----------------------------------------------------------------
    -- 3. REPLACE BASE DEPRECIATION ACCOUNTING MODEL
    --    Base creates 'EQUIPMENT' account with CashCode = 'CC-DEPRC'.
    ----------------------------------------------------------------
    DELETE a
    FROM Subject.tbAccount a
    WHERE a.AccountName = 'EQUIPMENT'
      AND a.CashCode = 'CC-DEPRC';

    ----------------------------------------------------------------
    -- 4. STANDARD MICRO CASH CODES (ADDITIVE)
    ----------------------------------------------------------------
    INSERT INTO Cash.tbCode
        (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
    VALUES
        -- Sales splits -> CA-SALES
        ('CC-SALAB', 'Sales – Labour',        'CA-SALES',  'T1', 1),
        ('CC-SAMAT', 'Sales – Materials',     'CA-SALES',  'T1', 1),

        -- Direct cost splits -> CA-DIRECT
        ('CC-MAT',   'Materials',             'CA-DIRECT', 'T1', 1),
        ('CC-SUB',   'Subcontractors',        'CA-DIRECT', 'T1', 1),
        ('CC-FUEL',  'Fuel & Oil',            'CA-DIRECT', 'T1', 1),
        ('CC-MTRAV', 'Motor Travel',          'CA-DIRECT', 'T1', 1),

        -- Staff collapse: CC-WAGE -> CC-SALRY -> CA-WAGES
        ('CC-SALRY', 'Salary',                'CA-WAGES',  'N/A', 1),

        -- Building: CC-RENT, CC-LHEAT, CC-REPA -> CA-BUILD
        ('CC-RENT',  'Rent & Rates',          'CA-BUILD',  'T1', 1),
        ('CC-LHEAT', 'Light, Heat & Power',   'CA-BUILD',  'T1', 1),
        ('CC-REPA',  'Repairs & Maintenance', 'CA-BUILD',  'T1', 1),

        -- Admin collapse -> CA-ADMIN
        ('CC-INS',   'Insurance',             'CA-ADMIN',  'T1', 1),
        ('CC-PHONE', 'Telephone & Internet',  'CA-ADMIN',  'T1', 1),
        ('CC-ADVT',  'Advertising',           'CA-ADMIN',  'T1', 1),
        ('CC-TRAV',  'Travel & Subsistence',  'CA-ADMIN',  'T1', 1),
        ('CC-PROF',  'Professional Fees',     'CA-ADMIN',  'T1', 1),
        ('CC-BANKC', 'Bank Charges',          'CA-ADMIN',  'T1', 1),

        -- Depreciation (STD replacement for CC-DEPRC)
        ('CC-DEPPL', 'Depreciation – Plant & Tools',  'CA-ASSET', 'N/A', 1),
        ('CC-DEPMV', 'Depreciation – Motor Vehicles', 'CA-ASSET', 'N/A', 1),
        ('CC-DEPFX', 'Depreciation – Fixtures',       'CA-ASSET', 'N/A', 1);

    ----------------------------------------------------------------
    -- 4b. Create capital accounts for the STD depreciation model
    ----------------------------------------------------------------
    DECLARE
    @DepAccount NVARCHAR(50)
    , @SubjectCode  NVARCHAR(10) = (SELECT TOP 1 SubjectCode FROM App.tbOptions)
    , @AccountCode  NVARCHAR(10);

    SET @DepAccount = 'PLANT & TOOLS';
    EXEC Subject.proc_DefaultSubjectCode
         @SubjectName = @DepAccount,
         @SubjectCode = @AccountCode OUTPUT;

    INSERT INTO Subject.tbAccount
        (AccountCode, SubjectCode, AccountName, AccountTypeCode, BalanceConstraintCode, LiquidityLevel, CashCode, AccountClosed)
    VALUES
        (@AccountCode, @SubjectCode, @DepAccount, 2, 1, 30, 'CC-DEPPL', 0);

    SET @DepAccount = 'MOTOR VEHICLES';
    EXEC Subject.proc_DefaultSubjectCode
         @SubjectName = @DepAccount,
         @SubjectCode = @AccountCode OUTPUT;

    INSERT INTO Subject.tbAccount
        (AccountCode, SubjectCode, AccountName, AccountTypeCode, BalanceConstraintCode, LiquidityLevel, CashCode, AccountClosed)
    VALUES
        (@AccountCode, @SubjectCode, @DepAccount, 2, 1, 31, 'CC-DEPMV', 0);

    SET @DepAccount = 'FIXTURES';
    EXEC Subject.proc_DefaultSubjectCode
         @SubjectName = @DepAccount,
         @SubjectCode = @AccountCode OUTPUT;

    INSERT INTO Subject.tbAccount
        (AccountCode, SubjectCode, AccountName, AccountTypeCode, BalanceConstraintCode, LiquidityLevel, CashCode, AccountClosed)
    VALUES
        (@AccountCode, @SubjectCode, @DepAccount, 2, 1, 32, 'CC-DEPFX', 0);

    ----------------------------------------------------------------
    -- 5. UK MTD TEMPLATE MAPPING (STD deltas)
    ----------------------------------------------------------------
    INSERT INTO Cash.tbTaxTagMap
        (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled)
    VALUES
        ('UK-MTD', 'CP28', 1, '', 'CC-DEPPL', 1),
        ('UK-MTD', 'CP28', 1, '', 'CC-DEPMV', 1),
        ('UK-MTD', 'CP28', 1, '', 'CC-DEPFX', 1);

    ----------------------------------------------------------------
    -- 6. DELETE CASH CODE INHERITED FROM BASE TEMPLATE (now unused)
    ----------------------------------------------------------------
    DELETE FROM Cash.tbTaxTagMap
    WHERE CashCode = 'CC-DEPRC';

    DELETE FROM Cash.tbCode
    WHERE CashCode = 'CC-DEPRC';

    -- Add STD expression suite
    EXEC App.proc_Template_CO_MICRO_CUR_STD_EXP_2026;

    ----------------------------------------------------------------
    -- 7. VAT handling for non‑registered businesses
    ----------------------------------------------------------------
    IF @IsVATRegistered = 0
        EXEC App.proc_Template_DisableVAT;

    ----------------------------------------------------------------
    -- 8. Check MTD tax mapping
    ----------------------------------------------------------------
    EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = 'UK-MTD';

    COMMIT TRAN MicroStdTemplate;

END TRY
BEGIN CATCH
    EXEC App.proc_ErrorLog;
END CATCH;
GO
