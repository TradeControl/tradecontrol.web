CREATE PROCEDURE App.proc_Template_CO_MICRO_CUR_2026
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
        -- 2. UK MTD TAX TAG SOURCE, TAGS & TEMPLATE MAPPING
        ----------------------------------------------------------------
        INSERT INTO Cash.tbTaxTagSource
            (TaxSourceCode, JurisdictionCode, SourceName, SourceDescription)
        VALUES
            ('UK-MTD', 'UK', 'MTD', 'UK Making Tax Digital (template defaults)');

        INSERT INTO Cash.tbTaxTag
            (TaxSourceCode, TagCode, TagName, TagClassCode, DisplayOrder)
        VALUES
            ('UK-MTD', 'AC12',  'Turnover',           1, 10),
            ('UK-MTD', 'AC405', 'Other Income',       1, 20),
            ('UK-MTD', 'AC410', 'Cost of Sales',      1, 30),
            ('UK-MTD', 'AC415', 'Staff Costs',        1, 40),
            ('UK-MTD', 'AC420', 'Depreciation Total', 1, 50),
            ('UK-MTD', 'AC425', 'Other Charges',      1, 60),
            ('UK-MTD', 'AC34',  'Tax On Profit',      1, 70),
            ('UK-MTD', 'AC435', 'Profit and Loss',    0, 80),
            ('UK-MTD', 'CP28', 'Depreciation charge',     1, 55),
            ('UK-MTD', 'CP46', 'Depreciation adjustment', 1, 56);


        INSERT INTO Cash.tbTaxTagMap
            (TaxSourceCode, TagCode, MapTypeCode, CategoryCode, CashCode, IsEnabled)
        VALUES
            -- Category totals / categories
            ('UK-MTD', 'AC12',  0, 'CT-TURNOV', '', 1),
            ('UK-MTD', 'AC405', 0, 'CT-OTHRIN', '', 1),
            ('UK-MTD', 'AC410', 0, 'CT-CSTSAL', '', 1),
            ('UK-MTD', 'AC415', 0, 'CT-STAFFC', '', 1),
            --('UK-MTD', 'AC420', 0, 'CA-ASSET',  '', 1),
            ('UK-MTD', 'AC425', 0, 'CT-OVERHD', '', 1),
            ('UK-MTD', 'AC34',  0, 'CA-TAXCO',  '', 1),
            ('UK-MTD', 'AC435', 0, 'CT-PANDL',  '', 1),

            -- Depreciation cash-code mapping (to be removed/overridden by STD template)
            ('UK-MTD', 'AC420', 1, '', 'CC-DEPRC', 1),
            ('UK-MTD', 'AC420', 1, '', 'CC-DEPRJ', 1),
            ('UK-MTD', 'CP46', 1, '', 'CC-DEPRJ', 1);

    END TRY
    BEGIN CATCH
        EXEC App.proc_ErrorLog;
    END CATCH;
