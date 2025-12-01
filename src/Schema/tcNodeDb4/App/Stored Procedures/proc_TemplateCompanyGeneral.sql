CREATE PROCEDURE App.proc_TemplateCompanyGeneral
(
	@FinancialMonth SMALLINT = 4,
	@GovAccountName NVARCHAR(255),
	@BankName NVARCHAR(255) = null,
	@BankAddress NVARCHAR(MAX) = null,
	@DummyAccount NVARCHAR(50), 
	@CurrentAccount NVARCHAR(50) = null,
	@CA_SortCode NVARCHAR(10) = null,
	@CA_AccountNumber NVARCHAR(20) = null,
	@ReserveAccount NVARCHAR(50) = null, 
	@RA_SortCode NVARCHAR(10) = null,
	@RA_AccountNumber NVARCHAR(20) = null
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		DECLARE
			@CoinTypeCode SMALLINT = (SELECT TOP (1) CoinTypeCode FROM App.tbOptions),
			@SubjectCode NVARCHAR(10),
			@AccountCode NVARCHAR(10);

		INSERT INTO [App].[tbBucket] ([Period], [BucketId], [BucketDescription], [AllowForecasts])
		VALUES (0, 'Overdue', 'Overdue Orders', 0)
		, (1, 'Current', 'Current Week', 0)
		, (2, 'Week 2', 'Week Two', 0)
		, (3, 'Week 3', 'Week Three', 0)
		, (4, 'Week 4', 'Week Four', 0)
		, (8, 'Next Month', 'Next Month', 0)
		, (16, '2 Months', '2 Months', 1)
		, (52, 'Forward', 'Forward Orders', 1)
		;
		INSERT INTO [App].[tbUom] ([UnitOfMeasure])
		VALUES ('each')
		, ('days')
		, ('hrs')
		, ('kilo')
		, ('miles')
		, ('mins')
		, ('units')
		;

		DECLARE @Decimals smallint = CASE @CoinTypeCode WHEN 2 THEN 2 ELSE 3 END

		INSERT INTO [App].[tbTaxCode] ([TaxCode], [TaxRate], [TaxDescription], [TaxTypeCode], [RoundingCode], [Decimals])
		VALUES ('INT', 0, 'Interest Tax', 3, 0, @Decimals)
		, ('N/A', 0, 'Untaxed', 3, 0, @Decimals)
		, ('NI1', 0, 'Directors National Insurance', 2, 0, @Decimals)
		, ('NI2', 0.121, 'Employees National Insurance', 2, 0, @Decimals)
		, ('T0', 0, 'Zero Rated VAT', 1, 0, @Decimals)
		, ('T1', 0.2, 'Standard VAT Rate', 1, 0, @Decimals)
		, ('T9', 0, 'TBC', 1, 0, @Decimals)

		INSERT INTO Cash.tbCategory (CategoryCode, Category, CategoryTypeCode, CashPolarityCode, CashTypeCode, DisplayOrder, IsEnabled)
		VALUES ('AL', 'Assets and Liabilities', 1, 2, 0, 20, 1)
		, ('AS', 'Assets', 0, 1, 2, 70, 1)
		, ('BA', 'Bank Accounts', 0, 2, 2, 80, 1)
		, ('BP', 'Bank Payments', 0, 0, 0, 90, 1)
		, ('BR', 'Bank Receipts', 0, 1, 0, 100, 1)
		, ('DB', 'Directors Bank Account', 0, 1, 0, 0, 1)
		, ('DBA', 'Director Account', 1, 2, 0, 11, 1)
		, ('DC', 'Direct Cost', 0, 0, 0, 20, 1)
		, ('DI', 'Dividends', 0, 0, 0, 110, -1)
		, ('DR', 'Drawings', 0, 2, 0, 150, 0)
		, ('EX', 'Expenses', 1, 2, 0, 10, 1)
		, ('FY', 'Profit for Financial Year', 1, 2, 0, 50, 1)
		, ('IC', 'Indirect Cost', 0, 0, 0, 30, 1)
		, ('IP', 'Intercompany Payment', 0, 0, 2, 120, 1)
		, ('IR', 'Intercompany Receipt', 0, 1, 2, 130, 1)
		, ('IV', 'Investment', 0, 2, 0, 160, 1)
		, ('LI', 'Liabilities', 0, 0, 2, 71, 1)
		, ('PL', 'Profit Before Taxation', 1, 2, 0, 30, 1)
		, ('SA', 'Sales', 0, 1, 0, 10, 1)
		, ('TA', 'Tax on Company', 0, 0, 1, 60, 1)
		, ('TO', 'Turnover', 1, 2, 0, 0, 1)
		, ('TP', 'Tax on Profit', 1, 2, 0, 40, 1)
		, ('TR', 'Trading Profit', 1, 2, 0, 12, 1)
		, ('TV', 'Tax on Goods', 0, 0, 1, 61, -1)
		, ('VAT', 'Vat Cash Codes', 1, 2, 0, 100, 1)
		, ('WA', 'Wages', 0, 0, 0, 50, 1)
		;

		INSERT INTO Cash.tbCode (CashCode, CashDescription, CategoryCode, TaxCode, IsEnabled)
		VALUES ('ACCOUNTS', 'Professional Fees', 'IC', 'T1', 1)
		, ('ADMIN', 'Company Administration', 'IC', 'T1', 1)
		, ('BANKINTR', 'Bank Interest', 'BR', 'N/A', 1)
		, ('BC', 'Bank Charges', 'BP', 'N/A', 1)
		, ('CAPITAL', 'Share Capital', 'LI', 'N/A', 1)
		, ('CASH', 'Company Cash', 'BA', 'N/A', 1)
		, ('COMS', 'Communications', 'IC', 'T1', 1)
		, ('DEBTWRITEOFF', 'Capital Debt Write-off', 'DB', 'N/A', 1)
		, ('DEPR', 'Depreciation', 'AS', 'N/A', 1)
		, ('DIVIDEND', 'Dividends', 'DI', 'N/A', 1)
		, ('DLAP', 'Directors Personal Bank', 'DB', 'N/A', 1)
		, ('EQUIP', 'Equipment Expensed', 'IC', 'T1', 1)
		, ('EXPENSES', 'Directors Expenses reimbursement', 'IC', 'N/A', 1)
		, ('IT', 'IT and Software', 'IC', 'T1', 1)
		, ('LOANCOM', 'Company Loan', 'IV', 'N/A', -1)
		, ('LOANDIR', 'Directors Loan', 'IV', 'N/A', 1)
		, ('LOANREPAY', 'Dept Repayment', 'LI', 'N/A', 1)
		, ('MAT', 'Material Purchases', 'DC', 'T1', 1)
		, ('MILEAGE', 'Travel - Car Mileage', 'IC', 'T0', 1)
		, ('NI', 'Employers NI', 'TA', 'N/A', 1)
		, ('OFFICERENT', 'Office Rent', 'IC', 'T0', 1)
		, ('PAYIN', 'Transfer Receipt', 'IR', 'N/A', 1)
		, ('PAYOUT', 'Account Payment', 'IP', 'N/A', 1)
		, ('POST', 'Post and Stationary', 'IC', 'T1', 1)
		, ('PURCHASES', 'Direct Purchase', 'DC', 'T1', 1)
		, ('SALARY', 'Salaries', 'WA', 'NI1', 1)
		, ('SALES', 'Sales', 'SA', 'T1', 1)
		, ('SUNDRYCOST', 'Sundry Costs', 'IC', 'T1', 1)
		, ('TAXCOMPANY', 'Taxes (Corporation)', 'TV', 'N/A', 1)
		, ('TAXGENERAL', 'Taxes (General)', 'TA', 'N/A', 1)
		, ('TAXVAT', 'VAT', 'TV', 'N/A', 1)
		, ('TRAVEL', 'Travel - General', 'IC', 'T1', 1)
		;
		INSERT INTO Cash.tbCategoryTotal (ParentCode, ChildCode)
		VALUES ('AL', 'AS')
		, ('AL', 'LI')
		, ('DBA', 'DB')
		, ('EX', 'BP')
		, ('EX', 'DC')
		, ('EX', 'IC')
		, ('EX', 'WA')
		, ('FY', 'PL')
		, ('FY', 'TP')
		, ('PL', 'AL')
		, ('PL', 'TR')
		, ('TO', 'BR')
		, ('TO', 'IV')
		, ('TO', 'SA')
		, ('TP', 'TA')
		, ('TR', 'DB')
		, ('TR', 'EX')
		, ('TR', 'TO')
		, ('VAT', 'DC')
		, ('VAT', 'IC')
		, ('VAT', 'SA')
		;

		IF @CoinTypeCode < 2
		BEGIN
			INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
			VALUES ('MINERFEE', 'Miner Fees', 'IC', 'N/A', 1);
		
			UPDATE App.tbOptions
			SET MinerFeeCode = 'MINERFEE';
		END

		--ASSIGN NET PROFIT CALCULATION
		UPDATE App.tbOptions
		SET NetProfitCode = 'FY', VatCategoryCode = 'VAT';

		--SET HOME TAX CODE
		UPDATE Subject.tbSubject
		SET TaxCode = 'T1'
		WHERE SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		--CREATE GOV
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @GovAccountName, @SubjectCode = @SubjectCode OUTPUT
		INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, @GovAccountName, 1, 7, 'N/A');

		--ASSIGN CASH CODES AND GOV TO TAX TYPES
		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TAXCOMPANY', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 0;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TAXVAT', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 1;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'NI', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 2;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = 'TAXGENERAL', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 3;
		
		IF @CoinTypeCode = 2
		BEGIN
			--fiat
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @BankName, @SubjectCode = @SubjectCode OUTPUT	
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, @BankName, 1, 5, 'T0');

			EXEC Subject.proc_AddAddress @SubjectCode = @SubjectCode, @Address = @BankAddress;
		END
		ELSE
		BEGIN
			--crypto
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = 'BITCOIN MINER', @SubjectCode = @SubjectCode OUTPUT
			INSERT INTO Subject.tbSubject (SubjectCode, SubjectName, SubjectStatusCode, SubjectTypeCode, TaxCode)
			VALUES (@SubjectCode, 'BITCOIN MINER', 1, 7, 'N/A');

			UPDATE App.tbOptions
			SET MinerAccountCode = @SubjectCode;

			SELECT @SubjectCode = SubjectCode FROM App.tbOptions 
		END

		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CurrentAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber, CashCode)
		VALUES        (@AccountCode, @SubjectCode, @CurrentAccount, 0, @CA_SortCode, @CA_AccountNumber, 'CASH')

		IF (LEN(COALESCE(@ReserveAccount, '')) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @ReserveAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber)
			VALUES        (@AccountCode, @SubjectCode, @ReserveAccount, 0, @RA_SortCode, @RA_AccountNumber)
		END

		SELECT @SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		IF (LEN(COALESCE(@DummyAccount, '')) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @DummyAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode)
			VALUES        (@AccountCode, @SubjectCode, @DummyAccount, 1);
		END

		--CAPITAL 
		DECLARE @CapitalAccount NVARCHAR(50);

		SET @CapitalAccount = 'LONGTERM LIABILITIES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 50, 'LOANREPAY', 0);

		SET @CapitalAccount = 'CALLED UP SHARE CAPITAL';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 60, 'CAPITAL', 0);

		SET @CapitalAccount = 'PLANT AND MACHINERY';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 30, 'DEPR', 1);

	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
