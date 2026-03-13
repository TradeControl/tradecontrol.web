CREATE PROCEDURE App.proc_Template_CO_MICRO_CUR_STD_EXP_2026
AS
BEGIN

    ----------------------------------------------------------------
    -- STD-SPECIFIC EXPRESSION CATEGORIES (CategoryTypeCode = 2)
    ----------------------------------------------------------------
    INSERT INTO Cash.tbCategory
        (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
    VALUES
        -- Enabled by default (the “big four”)
        ('COS',  'Cash Operating Surplus % (Structured)', 2, 2, 0, 20, 1),
        ('LM',   'Labour Margin %',                      2, 2, 0, 21, 1),
        ('MM',   'Materials Margin %',                   2, 2, 0, 22, 1),
        ('SDR',  'Subcontractor Dependency Ratio',       2, 2, 0, 23, 1),

        -- Disabled by default (progressive disclosure)
        ('VCR',  'Vehicle Cost Ratio',                   2, 2, 0, 24, 0),

        ('WPR',  'Wages % of Staff Costs',               2, 2, 0, 25, 0),
        ('NPR',  'Employer NI % of Staff Costs',         2, 2, 0, 26, 0),
        ('PPR',  'Pension % of Staff Costs',             2, 2, 0, 27, 0),

        ('RPR',  'Rent % of Admin Expenses',             2, 2, 0, 28, 0),
        ('UPR',  'Utilities % of Admin Expenses',        2, 2, 0, 29, 0),
        ('IPR',  'Insurance % of Admin Expenses',        2, 2, 0, 30, 0),
        ('MPR',  'Repairs % of Admin Expenses',          2, 2, 0, 31, 0),
        ('TPR',  'Telephone % of Admin Expenses',        2, 2, 0, 32, 0),
        ('APR',  'Advertising % of Admin Expenses',      2, 2, 0, 33, 0),
        ('TRR',  'Travel % of Admin Expenses',           2, 2, 0, 34, 0),
        ('PFR',  'Professional Fees % of Admin Expenses',2, 2, 0, 35, 0),
        ('BCR',  'Bank Charges % of Admin Expenses',     2, 2, 0, 36, 0),

        ('DPL',  'Plant Depreciation % of Depreciation', 2, 2, 0, 37, 0),
        ('DMV',  'Motor Depreciation % of Depreciation', 2, 2, 0, 38, 0),
        ('DFX',  'Fixtures Depreciation % of Depreciation', 2, 2, 0, 39, 0);


    ----------------------------------------------------------------
    -- STD-SPECIFIC EXPRESSIONS (DESCRIPTION-BASED)
    ----------------------------------------------------------------
    INSERT INTO Cash.tbCategoryExp
        (CategoryCode, Expression, Format, SyntaxTypeCode, IsError, ErrorMessage)
    VALUES
        -- Structured Cash Operating Surplus %
        ('COS', 'IF([Turnover]=0,0,([Turnover]-[Cost of Sales]-[Staff Costs]-[Admin Expenses])/[Turnover])', 'Pct0', 0, 0, NULL),

        -- Labour Margin %
        ('LM', 'IF([Sales – Labour]=0,0,([Sales – Labour]-[Wages & Salaries]-[Employer NI]-[Employer Pension])/[Sales – Labour])', 'Pct0', 0, 0, NULL),

        -- Materials Margin %
        ('MM', 'IF([Sales – Materials]=0,0,([Sales – Materials]-[Materials])/[Sales – Materials])', 'Pct0', 0, 0, NULL),

        -- Subcontractor Dependency Ratio
        ('SDR', 'IF([Cost of Sales]=0,0,[Subcontractors]/[Cost of Sales])', 'Pct0', 0, 0, NULL),

        -- Vehicle Cost Ratio
        ('VCR', 'IF([Turnover]=0,0,([Fuel & Oil]+[Motor Travel])/[Turnover])', 'Pct0', 0, 0, NULL),

        -- Staff Cost Structure
        ('WPR', 'IF([Staff Costs]=0,0,[Wages & Salaries]/[Staff Costs])', 'Pct0', 0, 0, NULL),
        ('NPR', 'IF([Staff Costs]=0,0,[Employer NI]/[Staff Costs])', 'Pct0', 0, 0, NULL),
        ('PPR', 'IF([Staff Costs]=0,0,[Employer Pension]/[Staff Costs])', 'Pct0', 0, 0, NULL),

        -- Admin Cost Structure
        ('RPR', 'IF([Admin Expenses]=0,0,[Rent & Rates]/[Admin Expenses])', 'Pct0', 0, 0, NULL),
        ('UPR', 'IF([Admin Expenses]=0,0,[Light, Heat & Power]/[Admin Expenses])', 'Pct0', 0, 0, NULL),
        ('IPR', 'IF([Admin Expenses]=0,0,[Insurance]/[Admin Expenses])', 'Pct0', 0, 0, NULL),
        ('MPR', 'IF([Admin Expenses]=0,0,[Repairs & Maintenance]/[Admin Expenses])', 'Pct0', 0, 0, NULL),
        ('TPR', 'IF([Admin Expenses]=0,0,[Telephone & Internet]/[Admin Expenses])', 'Pct0', 0, 0, NULL),
        ('APR', 'IF([Admin Expenses]=0,0,[Advertising]/[Admin Expenses])', 'Pct0', 0, 0, NULL),
        ('TRR', 'IF([Admin Expenses]=0,0,[Travel & Subsistence]/[Admin Expenses])', 'Pct0', 0, 0, NULL),
        ('PFR', 'IF([Admin Expenses]=0,0,[Professional Fees]/[Admin Expenses])', 'Pct0', 0, 0, NULL),
        ('BCR', 'IF([Admin Expenses]=0,0,[Bank Charges]/[Admin Expenses])', 'Pct0', 0, 0, NULL),

        -- Depreciation Structure
        ('DPL', 'IF([Depreciation]=0,0,[Depreciation – Plant & Tools]/[Depreciation])', 'Pct0', 0, 0, NULL),
        ('DMV', 'IF([Depreciation]=0,0,[Depreciation – Motor Vehicles]/[Depreciation])', 'Pct0', 0, 0, NULL),
        ('DFX', 'IF([Depreciation]=0,0,[Depreciation – Fixtures]/[Depreciation])', 'Pct0', 0, 0, NULL);

END;
GO
