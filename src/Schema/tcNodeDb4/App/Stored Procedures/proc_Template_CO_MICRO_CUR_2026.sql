CREATE PROCEDURE [App].[proc_Template_CO_MICRO_CUR_2026]
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
        VALUES ('INT', 0,      'Interest Tax',       3, 0, @Decimals),  -- General (TC603)
               ('N/A', 0,      'Untaxed',            3, 0, @Decimals),  -- General (TC603)
               ('T0',  0,      'Zero Rated VAT',     1, 0, @Decimals),  -- VAT (TC600)
               ('T1',  0.2000, 'Standard VAT Rate',  1, 0, @Decimals);  -- VAT (TC600)

        ----------------------------------------------------------------
        -- CATEGORIES (HMRC schema + internal TC structure)
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            -- HMRC Micro-entity P&L (schema codes)
            ('AC12',  'Turnover',          1, 2, 0,  10, 1),
            ('AC405', 'Other Income',      1, 2, 0,  20, 1),
            ('AC410', 'Cost of Sales',     1, 2, 0,  30, 1),
            ('AC415', 'Staff Costs',       1, 2, 0,  40, 1),

            -- Depreciation schema categories
            ('CP28',  'Depreciation Charge',      1, 2, 0,  51, 1),
            ('CP46',  'Depreciation Adjustment',  1, 2, 0,  52, 1),

            ('AC420', 'Depreciation (Total)',     1, 2, 0,  53, 1),
            ('AC425', 'Other Charges',            1, 2, 0,  60, 1),
            ('AC34',  'Tax on Profit',            1, 2, 0,  70, 1),

            -- Profit root (after tax)
            ('AC435', 'Profit and Loss',          1, 2, 0,  80, 1),

            -- Internal P&L categories
            ('TC-SALES',   'Sales',                       0, 1, 0, 100, 1),
            ('TC-INCOME',  'Other Income (Internal)',     0, 1, 0, 110, 1),
            ('TC-DIRECT',  'Direct Costs',                0, 0, 0, 120, 1),
            ('TC-WAGES',   'Wages',                       0, 0, 0, 130, 1),
            ('TC-ADMIN',   'Admin Expenses',              0, 0, 0, 140, 1),

            -- Minimal asset movement bucket
            ('TC-ASSET',   'Asset Movements',             0, 2, 2, 150, 1),
            ('TC-ASADJ',   'Asset Adjustments',           0, 2, 2, 160, 1),

            -- Tax categories
            ('TC-TAXGD',   'Tax on Goods (VAT / General)',0, 0, 1, 170, 1),
            ('TC-TAXCO',   'Tax on Company (Corp)',       0, 0, 1, 180, 1),

            -- VAT root
            ('TC-VAT',     'VAT Control Root',            1, 2, 1, 900, 1),

            -- Internal Balance Sheet-ish groupings
            ('TC-BANK',    'Bank Accounts',               0, 2, 2, 910, 1),
            ('TC-INVEST',  'Investments / Capital',       0, 2, 2, 920, 1),
            ('TC-LIAB',    'Liabilities',                 0, 0, 2, 930, 1),

			-- Interbank Transfers
			('TC-IP',	   'Intercompany Payment',	      0, 0, 2, 200, 1),
			('TC-IR',      'Intercompany Receipt',	      0, 1, 2, 210, 1),

			-- Dividend Payment
			('TC-DI',      'Dividends',                   0, 0, 0, 300, 1);

        ----------------------------------------------------------------
        -- CATEGORY TOTALS (roll-up structure)
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
        VALUES
            -- Profit before tax
            ('AC435', 'AC12'),
            ('AC435', 'AC405'),
            ('AC435', 'AC410'),
            ('AC435', 'AC415'),
            ('AC435', 'AC420'),
            ('AC435', 'AC425'),

            -- Turnover
            ('AC12',  'TC-SALES'),

            -- Other income
            ('AC405', 'TC-INCOME'),

            -- Cost of sales
            ('AC410', 'TC-DIRECT'),

            -- Staff costs
            ('AC415', 'TC-WAGES'),

            -- Depreciation roll-up
            ('AC420', 'CP28'),
            ('AC420', 'CP46'),

            -- Internal asset bucket feeding schema categories
            ('CP28',  'TC-ASSET'),
            ('CP46',  'TC-ASADJ'),

            -- Other charges
            ('AC425', 'TC-ADMIN'),

            -- Tax on profit
            ('AC34',  'TC-TAXCO'),

            -- VAT root
            ('TC-VAT', 'TC-SALES'),
            ('TC-VAT', 'TC-DIRECT'),
            ('TC-VAT', 'TC-ADMIN');

        ----------------------------------------------------------------
        -- CASH CODES (minimal set for MICRO_CUR_MIN)
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCode
            (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
        VALUES
            -- Income
            ('TC100', 'Sales',             'TC-SALES',   'T1',  1),
            ('TC101', 'Other Income',      'TC-INCOME',  'T1',  1),

            -- Direct Costs
            ('TC200', 'Direct Costs',  'TC-DIRECT',  'T1',  1),

            -- Wages
            ('TC300', 'Wages',             'TC-WAGES',   'N/A', 1),
			('TC301', 'Pensions',          'TC-WAGES',   'N/A', 1),

            -- Admin
            ('TC400', 'Admin Expenses',    'TC-ADMIN',   'T1',  1),

            -- Depreciation (internal)
            ('TC500', 'Depreciation',            'TC-ASSET', 'N/A', 1),
            ('TC501', 'Depreciation Adjustment', 'TC-ASADJ', 'N/A', 1),

            -- Tax (aligned to TaxTypeCode 0–3)
            ('TC600', 'VAT Control',       'TC-TAXGD',   'N/A', 1),            
            ('TC602', 'Corporation Tax',   'TC-TAXCO',   'N/A', 1),
            ('TC603', 'General Taxes',     'TC-TAXGD',   'N/A', 1),

			('TC601', 'Employers NI',      'TC-WAGES',   'N/A', 1),

            -- Operational accounts
            ('TC700', 'Bank',              'TC-BANK',    'N/A', 1),
            ('TC701', 'Share Capital',     'TC-INVEST',  'N/A', 1),
            ('TC702', 'Loan / Liability',  'TC-LIAB',    'N/A', 1),

			-- Bank Transfers
			('TC800', 'Transfer Payment',   'TC-IP',     'N/A', 1),
            ('TC801', 'Transfer Receipt',   'TC-IR',     'N/A', 1),
			
			-- Dividends
			('TC900', 'Dividends',          'TC-DI',     'N/A', 1);

        ----------------------------------------------------------------
        -- EXPRESSION CATEGORIES (CategoryTypeCode = 2)
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('GP', 'Gross Margin %',              2, 2, 0,  1, 1),
            ('NP', 'Net Profit %',                2, 2, 0,  2, 1),
            ('WP', 'Wages Ratio',                 2, 2, 0,  3, 1),
            ('AP', 'Admin Cost Ratio',            2, 2, 0,  4, 1),
            ('DP', 'Direct Cost Ratio',           2, 2, 0,  5, 1),
            ('DE', 'Depreciation Ratio',          2, 2, 0,  6, 1),
            ('TR', 'Effective Tax Rate',          2, 2, 0,  7, 1),
            ('OC', 'Overhead Coverage Ratio',     2, 2, 0,  8, 1),
            ('RS', 'Revenue-to-Wages Ratio',      2, 2, 0,  9, 1);

        ----------------------------------------------------------------
        -- EXPRESSION DEFINITIONS
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategoryExp
            (CategoryCode, Expression, Format, SyntaxTypeCode, IsError, ErrorMessage)
        VALUES
            -- Gross Margin %
            ('GP', 'IF([Sales]=0,0,([Gross Profit]/[Sales]))', 'Pct0', 0, 0, NULL),

            -- Net Profit %
            ('NP', 'IF([Sales]=0,0,([Net Profit]/[Sales]))', 'Pct0', 0, 0, NULL),

            -- Wages %
            ('WP', 'IF([Sales]=0,0,(ABS([Wages])/[Sales]))', 'Num2', 0, 0, NULL),

            -- Admin Cost %
            ('AP', 'IF([Sales]=0,0,(ABS([Admin Expenses])/[Sales]))', 'Num2', 0, 0, NULL),

            -- Direct Cost %
            ('DP', 'IF([Sales]=0,0,(ABS([Direct Costs])/[Sales]))', 'Num2', 0, 0, NULL),

            -- Depreciation %
            ('DE', 'IF([Sales]=0,0,(ABS([Depreciation (Total)])/[Sales]))', 'Num2', 0, 0, NULL),

            -- Effective Tax Rate
            ('TR', 'IF([Profit Before Tax]=0,0,(ABS([Tax on Profit])/[Profit Before Tax]))', 'Pct0', 0, 0, NULL),

            -- Overhead Coverage Ratio
            ('OC', 'IF([Admin Expenses]=0,0,([Gross Profit]/ABS([Admin Expenses])))', 'Num2', 0, 0, NULL),

            -- Revenue per £ Staff Cost
            ('RS', 'IF([Wages]=0,0,([Sales]/ABS([Wages])))', 'Num2', 0, 0, NULL);

        ----------------------------------------------------------------
        -- CRYPTO-ONLY MINER FEE CASH CODE
        ----------------------------------------------------------------
        IF @CoinTypeCode < 2
        BEGIN
            INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
            VALUES ('TC212', 'Miner Fees', 'TC-DIRECT', 'N/A', 1);

            UPDATE App.tbOptions
            SET MinerFeeCode = 'TC212';
        END

        ----------------------------------------------------------------
        -- NET PROFIT & VAT CATEGORY
        ----------------------------------------------------------------
        UPDATE App.tbOptions
        SET NetProfitCode   = 'AC435',
            VatCategoryCode = 'TC-VAT';

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
            CashCode    = 'TC602',
            MonthNumber = @FinancialMonth
        WHERE TaxTypeCode = 0;  -- Corporation Tax

        UPDATE Cash.tbTaxType
        SET SubjectCode = @SubjectCode,
            CashCode    = 'TC600',
            MonthNumber = @FinancialMonth
        WHERE TaxTypeCode = 1;  -- VAT

        UPDATE Cash.tbTaxType
        SET SubjectCode = @SubjectCode,
            CashCode    = 'TC601',
            MonthNumber = @FinancialMonth
        WHERE TaxTypeCode = 2;  -- N.I.

        UPDATE Cash.tbTaxType
        SET SubjectCode = @SubjectCode,
            CashCode    = 'TC603',
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
            (@AccountCode, @SubjectCode, @CurrentAccount, 0, @CA_SortCode, @CA_AccountNumber, 'TC700');

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
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@AccountCode, @SubjectCode, @CapitalAccount, 2, 50, 'TC702', 0);

        -- CALLED UP SHARE CAPITAL
        SET @CapitalAccount = 'CALLED UP SHARE CAPITAL';
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @CapitalAccount,
             @SubjectCode = @AccountCode OUTPUT;

        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@AccountCode, @SubjectCode, @CapitalAccount, 2, 60, 'TC701', 0);

        -- EQUIPMENT (replaces P&M, Vehicles, Stock)
        SET @CapitalAccount = 'EQUIPMENT';
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @CapitalAccount,
             @SubjectCode = @AccountCode OUTPUT;

        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@AccountCode, @SubjectCode, @CapitalAccount, 2, 30, 'TC500', 1);

        -- EQUIPMENT ADJUSTMENTS (replaces Depreciation Adjustments)
        SET @CapitalAccount = 'EQUIPMENT ADJUSTMENTS';
        EXEC Subject.proc_DefaultSubjectCode
             @SubjectName = @CapitalAccount,
             @SubjectCode = @AccountCode OUTPUT;

        INSERT INTO Subject.tbAccount
            (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
        VALUES
            (@AccountCode, @SubjectCode, @CapitalAccount, 2, 40, 'TC501', 1);

    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH;
