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
        -- Turnover splits
        ('SALAB', 'Sales – Labour',        0, 1, 0, 111, 1),
        ('SAMAT', 'Sales – Materials',     0, 1, 0, 112, 1),

        -- Direct cost splits
        ('MAT',   'Materials',             0, 0, 0, 211, 1),
        ('SUB',   'Subcontractors',        0, 0, 0, 212, 1),
        ('FUEL',  'Fuel & Oil',            0, 0, 0, 213, 1),
        ('MTRAV', 'Motor Travel',          0, 0, 0, 214, 1),

        -- Staff cost splits
        ('WAGE',  'Wages & Salaries',      0, 0, 0, 311, 1),
        ('ERNI',  'Employer NI',           0, 0, 0, 312, 1),
        ('PENS',  'Employer Pension',      0, 0, 0, 313, 1),

        -- Admin splits
        ('RENT',  'Rent & Rates',          0, 0, 0, 411, 1),
        ('LHEAT', 'Light, Heat & Power',   0, 0, 0, 412, 1),
        ('INS',   'Insurance',             0, 0, 0, 413, 1),
        ('REPA',  'Repairs & Maintenance', 0, 0, 0, 414, 1),
        ('PHONE', 'Telephone & Internet',  0, 0, 0, 415, 1),
        ('ADVT',  'Advertising',           0, 0, 0, 416, 1),
        ('TRAV',  'Travel & Subsistence',  0, 0, 0, 417, 1),
        ('PROF',  'Professional Fees',     0, 0, 0, 418, 1),
        ('BANK',  'Bank Charges',          0, 0, 0, 419, 1),

        -- Depreciation splits
        ('DEPPL', 'Depreciation – Plant & Tools', 0, 0, 0, 611, 1),
        ('DEPMV', 'Depreciation – Motor Vehicles',0, 0, 0, 612, 1),
        ('DEPFX', 'Depreciation – Fixtures',      0, 0, 0, 613, 1);

    ----------------------------------------------------------------
    -- 3. STANDARD MICRO ROLL-UPS (ADDITIVE)
    ----------------------------------------------------------------
    INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
    VALUES
        -- Turnover
        ('AC12', 'SALAB'),
        ('AC12', 'SAMAT'),

        -- Cost of Sales
        ('AC410', 'MAT'),
        ('AC410', 'SUB'),
        ('AC410', 'FUEL'),
        ('AC410', 'MTRAV'),

        -- Staff Costs
        ('AC415', 'WAGE'),
        ('AC415', 'ERNI'),
        ('AC415', 'PENS'),

        -- Admin Expenses
        ('AC425', 'RENT'),
        ('AC425', 'LHEAT'),
        ('AC425', 'INS'),
        ('AC425', 'REPA'),
        ('AC425', 'PHONE'),
        ('AC425', 'ADVT'),
        ('AC425', 'TRAV'),
        ('AC425', 'PROF'),
        ('AC425', 'BANK'),

        -- Depreciation
        ('AC420', 'DEPPL'),
        ('AC420', 'DEPMV'),
        ('AC420', 'DEPFX'),

        -- VAT root
        ('TC-VAT', 'SALAB'),
        ('TC-VAT', 'SAMAT'),
        ('TC-VAT', 'MAT'),
        ('TC-VAT', 'SUB'),
        ('TC-VAT', 'FUEL'),
        ('TC-VAT', 'MTRAV'),
        ('TC-VAT', 'RENT'),
        ('TC-VAT', 'LHEAT'),
        ('TC-VAT', 'INS'),
        ('TC-VAT', 'REPA'),
        ('TC-VAT', 'PHONE'),
        ('TC-VAT', 'ADVT'),
        ('TC-VAT', 'TRAV'),
        ('TC-VAT', 'PROF'),
        ('TC-VAT', 'BANK');

    ----------------------------------------------------------------
    -- 4. STANDARD MICRO CASH CODES (ADDITIVE)
    ----------------------------------------------------------------
    INSERT INTO Cash.tbCode
        (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
    VALUES
        -- Turnover
        ('TC110', 'Sales – Labour',        'SALAB', 'T1', 1),
        ('TC111', 'Sales – Materials',     'SAMAT', 'T1', 1),

        -- Direct Costs
        ('TC210', 'Materials',             'MAT',   'T1', 1),
        ('TC211', 'Subcontractors',        'SUB',   'T1', 1),
        ('TC212', 'Fuel & Oil',            'FUEL',  'T1', 1),
        ('TC213', 'Motor Travel',          'MTRAV', 'T1', 1),

        -- Staff Costs
        ('TC310', 'Wages & Salaries',      'WAGE',  'N/A', 1),
        ('TC311', 'Employer NI',           'ERNI',  'N/A', 1),
        ('TC312', 'Employer Pension',      'PENS',  'N/A', 1),

        -- Admin
        ('TC410', 'Rent & Rates',          'RENT',  'T1', 1),
        ('TC411', 'Light, Heat & Power',   'LHEAT', 'T1', 1),
        ('TC412', 'Insurance',             'INS',   'T1', 1),
        ('TC413', 'Repairs & Maintenance', 'REPA',  'T1', 1),
        ('TC414', 'Telephone & Internet',  'PHONE', 'T1', 1),
        ('TC415', 'Advertising',           'ADVT',  'T1', 1),
        ('TC416', 'Travel & Subsistence',  'TRAV',  'T1', 1),
        ('TC417', 'Professional Fees',     'PROF',  'T1', 1),
        ('TC418', 'Bank Charges',          'BANK',  'T1', 1),

        -- Depreciation
        ('TC510', 'Depreciation – Plant & Tools', 'DEPPL', 'N/A', 1),
        ('TC511', 'Depreciation – Motor Vehicles','DEPMV', 'N/A', 1),
        ('TC512', 'Depreciation – Fixtures',      'DEPFX', 'N/A', 1);

    -- Add STD expression suite
    EXEC App.proc_Template_CO_MICRO_CUR_STD_EXP_2026;

    ----------------------------------------------------------------
    -- 5. VAT handling for non‑registered businesses
    ----------------------------------------------------------------
    IF @IsVATRegistered = 0
        EXEC App.proc_Template_DisableVAT;

    COMMIT TRAN MicroStdTemplate;

END TRY
BEGIN CATCH
    EXEC App.proc_ErrorLog;
END CATCH;
GO
