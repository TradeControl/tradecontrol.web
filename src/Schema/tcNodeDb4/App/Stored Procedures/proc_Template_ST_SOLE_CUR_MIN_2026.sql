CREATE PROCEDURE App.proc_Template_ST_SOLE_CUR_MIN_2026
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
    -- 1. Base template: Minimal Micro Business (current schema)
    ----------------------------------------------------------------
    EXEC App.proc_Template_BASE_MIN_2026
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
    -- 2. Disable company-only categories/codes (do NOT delete)
    ----------------------------------------------------------------
    UPDATE Cash.tbCategory
    SET IsEnabled = 0
    WHERE CategoryCode IN ('CA-DIVID');

    UPDATE Cash.tbCode
    SET IsEnabled = 0
    WHERE CashCode IN ('CC-DEPRC', 'CC-DEPRJ', 'CC-EMPNI', 'CC-SHCAP', 'CC-DIVID');

    UPDATE Subject.tbAccount
    SET AccountClosed = 1
    WHERE AccountCode IN ('CALUP');

    UPDATE App.tbYearPeriod
    SET BusinessTaxRate = 0;

    ----------------------------------------------------------------
    -- 3. Business Tax settings for Sole Traders
    ----------------------------------------------------------------
    UPDATE Cash.tbTaxType
    SET MonthNumber = 4,
        RecurrenceCode = 4,
        OffsetDays = 300,
        IsEnabled = 0
    WHERE TaxTypeCode = 0;

    ----------------------------------------------------------------
    -- 4. Sole trader owner movements (single CASH CODE, polarity driven)
    ----------------------------------------------------------------

    -- Ensure the MONEY nominal category exists to hold the owner cash code.
    IF NOT EXISTS (SELECT 1 FROM Cash.tbCategory WHERE CategoryCode = 'CA-OWNER')
    BEGIN
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('CA-OWNER', 'Owner Capital Account', 0, 2, 2, 895, 1);
    END;

    -- Single owner capital asset cash code (polarity indicates introduced vs drawings).
    IF EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = 'CC-OWNCAP')
    BEGIN
        UPDATE Cash.tbCode
        SET CategoryCode = 'CA-OWNER',
            TaxCode = 'N/A',
            IsEnabled = 1
        WHERE CashCode = 'CC-OWNCAP';
    END
    ELSE
    BEGIN
        INSERT INTO Cash.tbCode
            (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES
            ('CC-OWNCAP', 'Owner Capital', 'CA-OWNER', 'N/A', 1);
    END;

    ----------------------------------------------------------------
    -- 5. Dedicated cash account (ASSET-type) for owner capital balance
    ----------------------------------------------------------------
    DECLARE
        @SubjectCode    NVARCHAR(10) = (SELECT SubjectCode FROM App.tbOptions),
        @OwnerAccount   NVARCHAR(50) = N'OWNER CAPITAL ACCOUNT',
        @OwnerAccountCode NVARCHAR(10);

    EXEC Subject.proc_DefaultSubjectCode
         @SubjectName = @OwnerAccount,
         @SubjectCode = @OwnerAccountCode OUTPUT;

    IF NOT EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = @OwnerAccountCode)
    BEGIN
        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, BalanceConstraintCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@OwnerAccountCode, @SubjectCode, @OwnerAccount, 2, 2, 45, 'CC-OWNCAP', 0);
    END
    ELSE
    BEGIN
        UPDATE Subject.tbAccount
        SET CashCode = 'CC-OWNCAP'
        WHERE AccountCode = @OwnerAccountCode;
    END;

    ----------------------------------------------------------------
    -- 7. VAT handling for non-registered businesses
    ----------------------------------------------------------------
    IF @IsVATRegistered = 0
        EXEC App.proc_Template_DisableVAT;

    ----------------------------------------------------------------
    -- 8. Tax year alignment: financial year starts on April 6
    ----------------------------------------------------------------
    WITH year_start AS
    (
        SELECT YearNumber, MIN(StartOn) StartOn
        FROM App.tbYearPeriod
        GROUP BY YearNumber
    )
    UPDATE yp
    SET StartOn = DATEADD(DAY, 5, yp.StartOn)
    FROM year_start ys
        JOIN App.tbYearPeriod yp
            ON ys.YearNumber = yp.YearNumber
           AND ys.StartOn = yp.StartOn
           AND DATEPART(DAY, yp.StartOn) = 1;

    ----------------------------------------------------------------
    -- 9. UK-ITSA-* MTD MAPPINGS
    ----------------------------------------------------------------
    IF NOT EXISTS (SELECT 1 FROM Cash.tbTaxTagSource WHERE TaxSourceCode = 'UK-ITSA-SE-QU')
    BEGIN
        INSERT INTO Cash.tbTaxTagSource
            (TaxSourceCode, JurisdictionCode, SourceName, SourceDescription, IsEnabled)
        VALUES
            ('UK-ITSA-SE-QU', 'UK', 'ITSA',
             'MTD ITSA Self-Employment (SA103F-aligned) Quarterly Update field set', 1);
    END;

    IF NOT EXISTS (SELECT 1 FROM Cash.tbTaxTagSource WHERE TaxSourceCode = 'UK-ITSA-SE-EOPS')
    BEGIN
        INSERT INTO Cash.tbTaxTagSource
            (TaxSourceCode, JurisdictionCode, SourceName, SourceDescription, IsEnabled)
        VALUES
            ('UK-ITSA-SE-EOPS', 'UK', 'ITSA',
             'MTD ITSA Self-Employment (SA103F-aligned) annual business return field set (EOPS)', 1);
    END;

    -- QU tags (existing)
    ;WITH TagSeed AS
    (
        SELECT * FROM (VALUES
            ('turnover',            'Turnover',                       CONVERT(TINYINT, 1), CONVERT(SMALLINT, 10)),
            ('otherIncome',         'Other income',                   CONVERT(TINYINT, 1), CONVERT(SMALLINT, 20)),

            ('costOfGoods',         'Cost of goods',                  CONVERT(TINYINT, 1), CONVERT(SMALLINT, 110)),
            ('constructionCosts',   'Construction costs',             CONVERT(TINYINT, 1), CONVERT(SMALLINT, 120)),
            ('wagesSalaries',       'Wages and salaries',             CONVERT(TINYINT, 1), CONVERT(SMALLINT, 130)),
            ('carVanExpenses',      'Car/van expenses',               CONVERT(TINYINT, 1), CONVERT(SMALLINT, 140)),
            ('travelExpenses',      'Travel expenses',                CONVERT(TINYINT, 1), CONVERT(SMALLINT, 150)),
            ('premisesRunningCosts','Premises running costs',         CONVERT(TINYINT, 1), CONVERT(SMALLINT, 160)),
            ('maintenanceCosts',    'Maintenance costs',              CONVERT(TINYINT, 1), CONVERT(SMALLINT, 170)),
            ('adminCosts',          'Admin costs',                    CONVERT(TINYINT, 1), CONVERT(SMALLINT, 180)),
            ('advertisingMarketing','Advertising/marketing',          CONVERT(TINYINT, 1), CONVERT(SMALLINT, 190)),
            ('interestOnLoans',     'Interest on loans',              CONVERT(TINYINT, 1), CONVERT(SMALLINT, 200)),
            ('financialCharges',    'Financial charges',              CONVERT(TINYINT, 1), CONVERT(SMALLINT, 210)),
            ('badDebts',            'Bad debts',                      CONVERT(TINYINT, 1), CONVERT(SMALLINT, 220)),
            ('professionalFees',    'Professional fees',              CONVERT(TINYINT, 1), CONVERT(SMALLINT, 230)),
            ('depreciation',        'Depreciation',                   CONVERT(TINYINT, 2), CONVERT(SMALLINT, 240)),
            ('otherExpenses',       'Other expenses',                 CONVERT(TINYINT, 1), CONVERT(SMALLINT, 250))
        ) v(TagCode, TagName, TagClassCode, DisplayOrder)
    )
    INSERT INTO Cash.tbTaxTag
        (TaxSourceCode, TagCode, TagName, TagClassCode, DisplayOrder, IsEnabled)
    SELECT
        'UK-ITSA-SE-QU',
        s.TagCode,
        s.TagName,
        s.TagClassCode,
        s.DisplayOrder,
        1
    FROM TagSeed s
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM Cash.tbTaxTag t
        WHERE t.TaxSourceCode = 'UK-ITSA-SE-QU'
          AND t.TagCode = s.TagCode
    );

    -- EOPS tags (vendor-canonical; derived items marked TagClassCode=2)
    ;WITH EopsSeed AS
    (
        SELECT * FROM (VALUES
            -- Chunk 1: totals + adjustments
            ('turnover',                 'Turnover',                              CONVERT(TINYINT, 1), CONVERT(SMALLINT, 10)),
            ('otherIncome',              'Other income',                          CONVERT(TINYINT, 1), CONVERT(SMALLINT, 20)),
            ('costOfGoods',              'Cost of goods',                         CONVERT(TINYINT, 1), CONVERT(SMALLINT, 110)),
            ('constructionCosts',        'Construction costs',                    CONVERT(TINYINT, 1), CONVERT(SMALLINT, 120)),
            ('wagesSalaries',            'Wages and salaries',                    CONVERT(TINYINT, 1), CONVERT(SMALLINT, 130)),
            ('carVanExpenses',           'Car/van expenses',                      CONVERT(TINYINT, 1), CONVERT(SMALLINT, 140)),
            ('travelExpenses',           'Travel expenses',                       CONVERT(TINYINT, 1), CONVERT(SMALLINT, 150)),
            ('premisesRunningCosts',     'Premises running costs',                CONVERT(TINYINT, 1), CONVERT(SMALLINT, 160)),
            ('maintenanceCosts',         'Maintenance costs',                     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 170)),
            ('adminCosts',               'Admin costs',                           CONVERT(TINYINT, 1), CONVERT(SMALLINT, 180)),
            ('advertisingMarketing',     'Advertising/marketing',                 CONVERT(TINYINT, 1), CONVERT(SMALLINT, 190)),
            ('interestOnLoans',          'Interest on loans',                     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 200)),
            ('financialCharges',         'Financial charges',                     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 210)),
            ('badDebts',                 'Bad debts',                             CONVERT(TINYINT, 1), CONVERT(SMALLINT, 220)),
            ('professionalFees',         'Professional fees',                     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 230)),
            ('depreciation',             'Depreciation',                          CONVERT(TINYINT, 2), CONVERT(SMALLINT, 240)),
            ('otherExpenses',            'Other expenses',                        CONVERT(TINYINT, 1), CONVERT(SMALLINT, 250)),

            ('goodsForOwnUse',           'Goods for own use',                     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 310)),

            ('disallowableCostOfGoods',  'Disallowable cost of goods',            CONVERT(TINYINT, 1), CONVERT(SMALLINT, 320)),
            ('disallowableWages',        'Disallowable wages',                    CONVERT(TINYINT, 1), CONVERT(SMALLINT, 330)),
            ('disallowableMotor',        'Disallowable motor expenses',           CONVERT(TINYINT, 1), CONVERT(SMALLINT, 340)),
            ('disallowableTravel',       'Disallowable travel expenses',          CONVERT(TINYINT, 1), CONVERT(SMALLINT, 350)),
            ('disallowablePremises',     'Disallowable premises expenses',        CONVERT(TINYINT, 1), CONVERT(SMALLINT, 360)),
            ('disallowableMaintenance',  'Disallowable maintenance expenses',     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 370)),
            ('disallowableAdmin',        'Disallowable admin expenses',           CONVERT(TINYINT, 1), CONVERT(SMALLINT, 380)),
            ('disallowableAdvertising',  'Disallowable advertising/marketing',    CONVERT(TINYINT, 1), CONVERT(SMALLINT, 390)),
            ('disallowableInterest',     'Disallowable interest',                 CONVERT(TINYINT, 1), CONVERT(SMALLINT, 400)),
            ('disallowableFinancial',    'Disallowable financial charges',        CONVERT(TINYINT, 1), CONVERT(SMALLINT, 410)),
            ('disallowableBadDebts',     'Disallowable bad debts',                CONVERT(TINYINT, 1), CONVERT(SMALLINT, 420)),
            ('disallowableProfessional', 'Disallowable professional fees',        CONVERT(TINYINT, 1), CONVERT(SMALLINT, 430)),
            ('disallowableOther',        'Disallowable other expenses',           CONVERT(TINYINT, 1), CONVERT(SMALLINT, 440)),

            ('accountingProfit',         'Accounting profit',                     CONVERT(TINYINT, 2), CONVERT(SMALLINT, 500)),
            ('totalDisallowables',       'Total disallowables',                   CONVERT(TINYINT, 2), CONVERT(SMALLINT, 510)),
            ('adjustedProfit',           'Adjusted profit',                       CONVERT(TINYINT, 2), CONVERT(SMALLINT, 520)),

            -- Chunk 2: losses / basis period
            ('lossBroughtForward',       'Loss brought forward',                  CONVERT(TINYINT, 1), CONVERT(SMALLINT, 610)),
            ('lossUsedAgainstProfit',    'Loss used against profit',              CONVERT(TINYINT, 1), CONVERT(SMALLINT, 620)),
            ('lossCarriedForward',       'Loss carried forward',                  CONVERT(TINYINT, 1), CONVERT(SMALLINT, 630)),
            ('lossUsedAgainstOtherIncome','Loss used against other income',       CONVERT(TINYINT, 1), CONVERT(SMALLINT, 640)),
            ('lossUsedAgainstCapitalGains','Loss used against capital gains',     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 650)),
            ('postCessationReceipts',    'Post-cessation receipts',               CONVERT(TINYINT, 1), CONVERT(SMALLINT, 660)),
            ('postCessationExpenses',    'Post-cessation expenses',               CONVERT(TINYINT, 1), CONVERT(SMALLINT, 670)),

            ('basisPeriodStart',         'Basis period start',                    CONVERT(TINYINT, 1), CONVERT(SMALLINT, 710)),
            ('basisPeriodEnd',           'Basis period end',                      CONVERT(TINYINT, 1), CONVERT(SMALLINT, 720)),
            ('basisPeriodAdjustedProfit','Basis period adjusted profit',          CONVERT(TINYINT, 2), CONVERT(SMALLINT, 730)),
            ('basisPeriodDisallowables', 'Basis period disallowables',            CONVERT(TINYINT, 2), CONVERT(SMALLINT, 740)),

            ('overlapProfit',            'Overlap profit',                        CONVERT(TINYINT, 1), CONVERT(SMALLINT, 810)),
            ('overlapReliefUsed',        'Overlap relief used',                   CONVERT(TINYINT, 1), CONVERT(SMALLINT, 820)),
            ('transitionalProfit',       'Transitional profit',                   CONVERT(TINYINT, 1), CONVERT(SMALLINT, 830)),
            ('transitionalRelief',       'Transitional relief',                   CONVERT(TINYINT, 1), CONVERT(SMALLINT, 840)),
            ('transitionalProfitSpread', 'Transitional profit spread',            CONVERT(TINYINT, 1), CONVERT(SMALLINT, 850)),
            ('adjustedProfitForTax',     'Adjusted profit for tax',               CONVERT(TINYINT, 2), CONVERT(SMALLINT, 860)),

            -- Chunk 3: capital allowances
            ('capitalAllowancesClaimed',        'Capital allowances claimed',     CONVERT(TINYINT, 2), CONVERT(SMALLINT, 910)),
            ('annualInvestmentAllowance',       'Annual Investment Allowance',   CONVERT(TINYINT, 1), CONVERT(SMALLINT, 920)),
            ('writingDownAllowanceMainPool',    'Writing Down Allowance (Main pool)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 930)),
            ('writingDownAllowanceSpecialRate', 'Writing Down Allowance (Special rate)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 940)),
            ('writingDownAllowanceSingleAsset', 'Writing Down Allowance (Single asset)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 950)),
            ('smallPoolsAllowance',             'Small pools allowance',         CONVERT(TINYINT, 1), CONVERT(SMALLINT, 960)),

            ('balancingChargeMainPool',         'Balancing charge (Main pool)',  CONVERT(TINYINT, 1), CONVERT(SMALLINT, 970)),
            ('balancingChargeSpecialRate',      'Balancing charge (Special rate)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 980)),
            ('balancingChargeSingleAsset',      'Balancing charge (Single asset)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 990)),
            ('balancingAllowanceMainPool',      'Balancing allowance (Main pool)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1000)),
            ('balancingAllowanceSpecialRate',   'Balancing allowance (Special rate)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1010)),
            ('balancingAllowanceSingleAsset',   'Balancing allowance (Single asset)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1020)),

            ('privateUseAdjustment',            'Private use adjustment',        CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1030)),

            ('carMainRateAllowance',            'Car allowance (Main rate)',     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1040)),
            ('carSpecialRateAllowance',         'Car allowance (Special rate)',  CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1050)),
            ('carBalancingCharge',              'Car balancing charge',          CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1060)),
            ('carBalancingAllowance',           'Car balancing allowance',       CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1070)),

            ('enhancedCapitalAllowance',        'Enhanced Capital Allowance',    CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1080)),
            ('superDeductionAllowance',         'Super-deduction allowance',     CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1090)),
            ('fullExpensingAllowance',          'Full expensing allowance',      CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1100)),
            ('specialRateFirstYearAllowance',   'Special rate first-year allowance', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1110)),

            ('poolOpeningValueMainPool',        'Pool opening value (Main pool)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1120)),
            ('poolOpeningValueSpecialRate',     'Pool opening value (Special rate)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1130)),
            ('poolOpeningValueSingleAsset',     'Pool opening value (Single asset)', CONVERT(TINYINT, 1), CONVERT(SMALLINT, 1140)),
            ('poolClosingValueMainPool',        'Pool closing value (Main pool)', CONVERT(TINYINT, 2), CONVERT(SMALLINT, 1150)),
            ('poolClosingValueSpecialRate',     'Pool closing value (Special rate)', CONVERT(TINYINT, 2), CONVERT(SMALLINT, 1160)),
            ('poolClosingValueSingleAsset',     'Pool closing value (Single asset)', CONVERT(TINYINT, 2), CONVERT(SMALLINT, 1170)),
            ('capitalAllowancesTotal',          'Capital allowances total',      CONVERT(TINYINT, 2), CONVERT(SMALLINT, 1180))
        ) v(TagCode, TagName, TagClassCode, DisplayOrder)
    )
    INSERT INTO Cash.tbTaxTag
        (TaxSourceCode, TagCode, TagName, TagClassCode, DisplayOrder, IsEnabled)
    SELECT
        'UK-ITSA-SE-EOPS',
        s.TagCode,
        s.TagName,
        s.TagClassCode,
        s.DisplayOrder,
        1
    FROM EopsSeed s
    WHERE NOT EXISTS
    (
        SELECT 1
        FROM Cash.tbTaxTag t
        WHERE t.TaxSourceCode = 'UK-ITSA-SE-EOPS'
          AND t.TagCode = s.TagCode
    );

    COMMIT TRAN SoleTraderTemplate;

END TRY
BEGIN CATCH
    EXEC App.proc_ErrorLog;
END CATCH;
GO
