CREATE PROCEDURE [App].[proc_Template_CO_MICRO_CUR_MIN_2026]
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

        BEGIN TRAN MicroMinTemplate;

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

        -- MIN-specific deltas

        ----------------------------------------------------------------
        -- 1. EXPRESSION CATEGORIES (CategoryTypeCode = 2)
        ----------------------------------------------------------------
        INSERT INTO Cash.tbCategory
            (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
        VALUES
            ('CE-CO', 'Cash Operating Surplus %', 2, 2, 0, 10, 1);

        INSERT INTO Cash.tbCategoryExp
            (CategoryCode, Expression, Format, SyntaxTypeCode, IsError, ErrorMessage)
        VALUES
            ('CE-CO', 'IF([Sales]=0,0,(([Sales]+[Other Income]-[Direct Purchases]-[Wages]-[Admin Expenses])/[Sales]))', 'Pct0', 0, 0, NULL);

        ----------------------------------------------------------------
        -- 2. VAT handling for non‑registered businesses
        ----------------------------------------------------------------
        IF @IsVATRegistered = 0
            EXEC App.proc_Template_DisableVAT;

        ----------------------------------------------------------------
        -- 3. Enable Asset Accounts
        ----------------------------------------------------------------
        UPDATE Subject.tbAccount
        SET AccountClosed = 0
        WHERE AccountClosed = 1;

        ----------------------------------------------------------------
        -- 4. MTD tax mapping
        ----------------------------------------------------------------
        INSERT INTO Cash.tbTaxTagMap
            (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled)
        VALUES
            ('UK-MTD', 'CP28', 1, '', 'CC-DEPRC', 1);

        EXEC Cash.proc_TaxTagMapValidate @TaxSourceCode = 'UK-MTD';

        COMMIT TRAN MicroMinTemplate;

    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH;
GO
