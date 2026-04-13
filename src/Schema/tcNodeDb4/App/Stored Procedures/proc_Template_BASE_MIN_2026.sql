CREATE PROCEDURE App.proc_Template_BASE_MIN_2026
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
    @RA_AccountNumber NVARCHAR(20) = NULL
)
AS
    SET NOCOUNT, XACT_ABORT ON;

    BEGIN TRY

        DECLARE
            @CoinTypeCode SMALLINT = (SELECT TOP (1) CoinTypeCode FROM App.tbOptions),
            @SubjectCode  NVARCHAR(10),
            @AccountCode  NVARCHAR(10),
            @Decimals     SMALLINT;

        ----------------------------------------------------------------
        -- BUCKETS
        ----------------------------------------------------------------
        INSERT INTO [App].[tbBucket] ([Period], [BucketId], [BucketDescription], [AllowForecasts])
        VALUES (0,  'Overdue',    'Overdue Orders', 0),
               (1,  'Current',    'Current Week',   0),
               (2,  'Week 2',     'Week Two',       0),
               (3,  'Week 3',     'Week Three',     0),
               (4,  'Week 4',     'Week Four',      0),
               (8,  'Next Month', 'Next Month',     0),
               (16, '2 Months',   '2 Months',       1),
               (52, 'Forward',    'Forward Orders', 1);

        ----------------------------------------------------------------
        -- UNITS OF MEASURE
        ----------------------------------------------------------------
        INSERT INTO [App].[tbUom] ([UnitOfMeasure])
        VALUES ('each'),
               ('days'),
               ('hrs'),
               ('kilo'),
               ('miles'),
               ('mins'),
               ('units');

		----------------------------------------------------------------
        -- TAX CODES (minimalist, CoinType-aware)
        ----------------------------------------------------------------
        SET @Decimals = CASE @CoinTypeCode WHEN 2 THEN 2 ELSE 3 END;

        INSERT INTO [App].[tbTaxCode]
            ([TaxCode], [TaxRate], [TaxDescription], [TaxTypeCode], [RoundingCode], [Decimals])
        VALUES ('INT', 0,      'Interest Tax',       3, 0, @Decimals),
               ('N/A', 0,      'Untaxed',            3, 0, @Decimals),
               ('T0',  0,      'Zero Rated VAT',     1, 0, @Decimals),
               ('T1',  0.2000, 'Standard VAT Rate',  1, 0, @Decimals);

        ----------------------------------------------------------------
        -- CATEGORIES (purged of MTD tags; semantic prefixes)
        --   CT-* = Total (CategoryTypeCode=1)
        --   CA-* = Nominal (CategoryTypeCode=0)
        --   CE-* = Expression (CategoryTypeCode=2)
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            -- Totals / reporting groups
            ('CT-TURNOV', 'Turnover',                 1, 2, 0,  10, 1),
            ('CT-OTHRIN', 'Other Income',             1, 2, 0,  20, 1),
            ('CT-CSTSAL', 'Cost of Sales',            1, 2, 0,  30, 1),
            ('CT-STAFFC', 'Staff Costs',              1, 2, 0,  40, 1),

            ('CT-OVERHD', 'Overheads',                1, 2, 0,  60, 1),

            -- Profit root
            ('CT-PANDL',  'Profit and Loss',          1, 2, 0,  80, 1),

            -- Internal nominal categories
            ('CA-SALES',  'Sales',                       0, 1, 0, 100, 1),
            ('CA-INCOME', 'Other Income (Internal)',     0, 1, 0, 110, 1),
            ('CA-DIRECT', 'Direct Costs',                0, 0, 0, 120, 1),
            ('CA-WAGES',  'Wages',                       0, 0, 0, 130, 1),
            ('CA-ADMIN',  'Admin Expenses',              0, 0, 0, 140, 1),

            -- Minimal asset movement bucket (moved to P&L)
            ('CA-ASSET',  'Asset Movements',             0, 2, 2, 150, 1),

            -- Tax categories
            ('CA-TAXGD',  'Tax on Goods (VAT / General)',0, 0, 1, 170, 1),
            ('CA-TAXCO',  'Tax on Company (Biz)',        0, 0, 1, 180, 1),

            -- VAT root
            ('CT-VAT',    'VAT Control Root',            1, 2, 1, 900, 1),

            -- Internal Balance Sheet-ish groupings
            ('CA-BANK',   'Bank Accounts',               0, 2, 2, 910, 1),
            ('CA-INVEST', 'Investments / Capital',       0, 2, 2, 920, 1),
            ('CA-LIAB',   'Liabilities',                 0, 0, 2, 930, 1),

			-- Interbank Transfers
			('CA-IP',	   'Intercompany Payment',	      0, 0, 2, 200, 1),
			('CA-IR',      'Intercompany Receipt',	      0, 1, 2, 210, 1),

			-- Dividend Payment
			('CA-DIVID',   'Dividends',                   0, 0, 0, 300, 1);

        ----------------------------------------------------------------
        -- CATEGORY TOTALS (roll-up structure)
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
        VALUES
            -- Profit and Loss (CT-PANDL)
            ('CT-PANDL', 'CT-TURNOV'),
            ('CT-PANDL', 'CT-OTHRIN'),
            ('CT-PANDL', 'CT-CSTSAL'),
            ('CT-PANDL', 'CT-STAFFC'),
            ('CT-PANDL', 'CT-OVERHD'),
            ('CT-PANDL', 'CA-ASSET'),

            -- Turnover
            ('CT-TURNOV', 'CA-SALES'),

            -- Other income
            ('CT-OTHRIN', 'CA-INCOME'),

            -- Cost of sales
            ('CT-CSTSAL', 'CA-DIRECT'),

            -- Staff costs
            ('CT-STAFFC', 'CA-WAGES'),

            -- Overheads
            ('CT-OVERHD', 'CA-ADMIN'),

            -- VAT root
            ('CT-VAT', 'CA-SALES'),
            ('CT-VAT', 'CA-INCOME'),
            ('CT-VAT', 'CA-DIRECT'),
            ('CT-VAT', 'CT-OVERHD');

        ----------------------------------------------------------------
        -- CASH CODES 
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCode
            (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES
            -- Income
            ('CC-SALES', 'Sales',             'CA-SALES',  'T1',  1),
            ('CC-INCME', 'Other Income',      'CA-INCOME', 'T1',  1),

            -- Direct Costs
            ('CC-DIRCT', 'Direct Costs',      'CA-DIRECT', 'T1',  1),

            -- Wages
            ('CC-WAGES', 'Wages',             'CA-WAGES',  'N/A', 1),
			('CC-PENSN', 'Pensions',          'CA-WAGES',  'N/A', 1),

            -- Admin
            ('CC-ADMIN', 'Admin Expenses',    'CA-ADMIN',  'T1',  1),

            -- Depreciation (internal)
            ('CC-DEPRC', 'Depreciation',            'CA-ASSET', 'N/A', 1),
            ('CC-DEPRJ', 'Depreciation Adjustment', 'CA-ASSET', 'N/A', 1),

            -- Tax (aligned to TaxTypeCode 0–3)
            ('CC-VAT',   'VAT Control',       'CA-TAXGD',  'N/A', 1),
            ('CC-BIZTX', 'Business Tax',      'CA-TAXCO',  'N/A', 1),
            ('CC-GENTX', 'General Taxes',     'CA-TAXGD',  'N/A', 1),

			('CC-EMPNI', 'Employers NI',      'CA-WAGES',  'N/A', 1),

            -- Operational accounts
            ('CC-BANK',  'Bank',              'CA-BANK',   'N/A', 1),
            ('CC-SHCAP', 'Share Capital',     'CA-INVEST', 'N/A', 1),
            ('CC-LOAN',  'Loan / Liability',  'CA-LIAB',   'N/A', 1),

			-- Bank Transfers
			('CC-TRNPY', 'Transfer Payment',  'CA-IP',     'N/A', 1),
            ('CC-TRNRC', 'Transfer Receipt',  'CA-IR',     'N/A', 1),
			
			-- Dividends
			('CC-DIVID', 'Dividends',         'CA-DIVID',  'N/A', 1);

        ----------------------------------------------------------------
        -- EXPRESSION CATEGORIES (CategoryTypeCode = 2)
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('CE-GP', 'Gross Margin %',              2, 2, 0,  1, 1),
            ('CE-NP', 'Net Profit %',                2, 2, 0,  2, 1),
            ('CE-WP', 'Wages Ratio',                 2, 2, 0,  3, 1),
            ('CE-AP', 'Admin Cost Ratio',            2, 2, 0,  4, 1),
            ('CE-DP', 'Direct Cost Ratio',           2, 2, 0,  5, 1),
            ('CE-DE', 'Depreciation Ratio',          2, 2, 0,  6, 1),
            ('CE-TR', 'Effective Tax Rate',          2, 2, 0,  7, 1),
            ('CE-OC', 'Overhead Coverage Ratio',     2, 2, 0,  8, 1),
            ('CE-RS', 'Revenue-to-Wages Ratio',      2, 2, 0,  9, 1);

        ----------------------------------------------------------------
        -- EXPRESSION DEFINITIONS
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategoryExp
            (CategoryCode, Expression, Format, SyntaxTypeCode, IsError, ErrorMessage)
        VALUES
            -- Gross Margin %
            ('CE-GP', 'IF([Sales]=0,0,([Gross Profit]/[Sales]))', 'Pct0', 0, 0, NULL),

            -- Net Profit %
            ('CE-NP', 'IF([Sales]=0,0,([Net Profit]/[Sales]))', 'Pct0', 0, 0, NULL),

            -- Wages %
            ('CE-WP', 'IF([Sales]=0,0,(ABS([Wages])/[Sales]))', 'Num2', 0, 0, NULL),

            -- Admin Cost %
            ('CE-AP', 'IF([Sales]=0,0,(ABS([Admin Expenses])/[Sales]))', 'Num2', 0, 0, NULL),

            -- Direct Cost %
            ('CE-DP', 'IF([Sales]=0,0,(ABS([Direct Costs])/[Sales]))', 'Num2', 0, 0, NULL),

            -- Depreciation %
            ('CE-DE', 'IF([Sales]=0,0,(ABS([Depreciation (Total)])/[Sales]))', 'Num2', 0, 0, NULL),

            -- Effective Tax Rate
            ('CE-TR', 'IF([Profit Before Tax]=0,0,(ABS([Tax on Profit])/[Profit Before Tax]))', 'Pct0', 0, 0, NULL),

            -- Overhead Coverage Ratio
            ('CE-OC', 'IF([Admin Expenses]=0,0,([Gross Profit]/ABS([Admin Expenses])))', 'Num2', 0, 0, NULL),

            -- Revenue per £ Staff Cost
            ('CE-RS', 'IF([Wages]=0,0,([Sales]/ABS([Wages])))', 'Num2', 0, 0, NULL);

        ----------------------------------------------------------------
        -- CRYPTO-ONLY MINER FEE CASH CODE
        ----------------------------------------------------------------
        IF @CoinTypeCode < 2
        BEGIN
            INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
            VALUES ('CC-MINER', 'Miner Fees', 'CA-DIRECT', 'N/A', 1);

            UPDATE App.tbOptions
            SET MinerFeeCode = 'CC-MINER';
        END

        ----------------------------------------------------------------
        -- NET PROFIT & VAT CATEGORY
        ----------------------------------------------------------------
        UPDATE App.tbOptions
        SET NetProfitCode   = 'CT-PANDL',
            VatCategoryCode = 'CT-VAT';

        ----------------------------------------------------------------
        -- HOME SUBJECT TAX CODE (STANDARD VAT)
        ----------------------------------------------------------------
        UPDATE Subject.tbSubject
        SET TaxCode = 'T1'
        WHERE SubjectCode = (SELECT SubjectCode FROM App.tbOptions);

        ----------------------------------------------------------------
        -- GOV / HMRC SUBJECT
        ----------------------------------------------------------------
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @GovAccountName,
             @SubjectCode = @SubjectCode OUTPUT;

        INSERT INTO Subject.tbSubject
            (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
        VALUES
            (@SubjectCode, @GovAccountName, 1, 7, 'N/A');

        ----------------------------------------------------------------
        -- ASSIGN CASH CODES AND GOV TO TAX TYPES
        ----------------------------------------------------------------
        UPDATE Cash.tbTaxType
        SET SubjectCode = @SubjectCode,
            CashCode    = 'CC-BIZTX',
            MonthNumber = @FinancialMonth
        WHERE TaxTypeCode = 0;  -- Business Tax

        UPDATE Cash.tbTaxType
        SET SubjectCode = @SubjectCode,
            CashCode    = 'CC-VAT',
            MonthNumber = @FinancialMonth
        WHERE TaxTypeCode = 1;  -- VAT

        UPDATE Cash.tbTaxType
        SET SubjectCode = @SubjectCode,
            CashCode    = 'CC-EMPNI',
            MonthNumber = @FinancialMonth
        WHERE TaxTypeCode = 2;  -- N.I.

        UPDATE Cash.tbTaxType
        SET SubjectCode = @SubjectCode,
            CashCode    = 'CC-GENTX',
            MonthNumber = @FinancialMonth
        WHERE TaxTypeCode = 3;  -- General

        ----------------------------------------------------------------
        -- BANK / CRYPTO SUBJECT
        ----------------------------------------------------------------
        IF @CoinTypeCode = 2
        BEGIN
            -- FIAT BANK
            EXEC Subject.proc_DefaultSubjectCode
                 @SubjectName = @BankName,
                 @SubjectCode = @SubjectCode OUTPUT;

            INSERT INTO Subject.tbSubject
                (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
            VALUES
                (@SubjectCode, @BankName, 1, 5, 'T0');

            EXEC Subject.proc_AddAddress
                 @SubjectCode = @SubjectCode,
                 @Address     = @BankAddress;
        END
        ELSE
        BEGIN
            -- CRYPTO MINER SUBJECT
            EXEC Subject.proc_DefaultSubjectCode
                 @SubjectName = 'BITCOIN MINER',
                 @SubjectCode = @SubjectCode OUTPUT;

            INSERT INTO Subject.tbSubject
                (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
            VALUES
                (@SubjectCode, 'BITCOIN MINER', 1, 7, 'N/A');

            UPDATE App.tbOptions
            SET MinerAccountCode = @SubjectCode;

            -- Reset @SubjectCode to HOME subject
            SELECT @SubjectCode = SubjectCode
            FROM App.tbOptions;
        END

        ----------------------------------------------------------------
        -- CURRENT ACCOUNT
        ----------------------------------------------------------------
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @CurrentAccount,
             @SubjectCode = @AccountCode OUTPUT;

        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber, CashCode)
        VALUES
            (@AccountCode, @SubjectCode, @CurrentAccount, 0, @CA_SortCode, @CA_AccountNumber, 'CC-BANK');

        ----------------------------------------------------------------
        -- RESERVE ACCOUNT (OPTIONAL)
        ----------------------------------------------------------------
        IF (LEN(COALESCE(@ReserveAccount, '')) > 0)
        BEGIN
            EXEC Subject.proc_DefaultSubjectCode
                 @SubjectName = @ReserveAccount,
                 @SubjectCode = @AccountCode OUTPUT;

            INSERT INTO Subject.tbAccount
                (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber)
            VALUES
                (@AccountCode, @SubjectCode, @ReserveAccount, 0, @RA_SortCode, @RA_AccountNumber);
        END

        ----------------------------------------------------------------
        -- RESET SUBJECT TO HOME SUBJECT
        ----------------------------------------------------------------
        SELECT @SubjectCode = (SELECT SubjectCode FROM App.tbOptions);

        ----------------------------------------------------------------
        -- DUMMY ACCOUNT (OPTIONAL)
        ----------------------------------------------------------------
        IF (LEN(COALESCE(@DummyAccount, '')) > 0)
        BEGIN
            EXEC Subject.proc_DefaultSubjectCode
                 @SubjectName = @DummyAccount,
                 @SubjectCode = @AccountCode OUTPUT;

            INSERT INTO Subject.tbAccount
                (AccountCode, SubjectCode, AccountName, AccountTypeCode, CashCode)
            VALUES
                (@AccountCode, @SubjectCode, @DummyAccount, 1, NULL);
        END

        ----------------------------------------------------------------
        -- CAPITAL (MINIMALIST: EQUIPMENT + ADJUSTMENTS)
        ----------------------------------------------------------------
        DECLARE @CapitalAccount NVARCHAR(50);

        -- LONGTERM LIABILITIES
        SET @CapitalAccount = 'LONGTERM LIABILITIES';
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @CapitalAccount,
             @SubjectCode = @AccountCode OUTPUT;

        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, BalanceConstraintCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@AccountCode, @SubjectCode, @CapitalAccount, 2, 2, 50, 'CC-LOAN', 0);

        -- CALLED UP SHARE CAPITAL
        SET @CapitalAccount = 'CALLED UP SHARE CAPITAL';
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @CapitalAccount,
             @SubjectCode = @AccountCode OUTPUT;

        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, BalanceConstraintCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@AccountCode, @SubjectCode, @CapitalAccount, 2, 2, 60, 'CC-SHCAP', 0);

        -- EQUIPMENT (replaces P&M, Vehicles, Stock)
        SET @CapitalAccount = 'EQUIPMENT';
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @CapitalAccount,
             @SubjectCode = @AccountCode OUTPUT;

        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, BalanceConstraintCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@AccountCode, @SubjectCode, @CapitalAccount, 2, 1, 30, 'CC-DEPRC', 1);

        -- EQUIPMENT ADJUSTMENTS (replaces Depreciation Adjustments)
        SET @CapitalAccount = 'EQUIPMENT ADJUSTMENTS';
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @CapitalAccount,
             @SubjectCode = @AccountCode OUTPUT;

        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, BalanceConstraintCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@AccountCode, @SubjectCode, @CapitalAccount, 2, 0, 40, 'CC-DEPRJ', 1);

    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH;
