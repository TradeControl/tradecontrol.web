CREATE PROCEDURE App.proc_TemplateTutorials
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
		VALUES ('copies')
		, ('days')
		, ('each')
		, ('hrs')
		, ('kilo')
		, ('miles')
		, ('mins')
		, ('pallets')
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
		;

		INSERT INTO [Cash].[tbCategory] ([CategoryCode], [Category], [CategoryTypeCode], [CashPolarityCode], [CashTypeCode], [DisplayOrder], [IsEnabled])
		VALUES ('AS', 'Assets', 0, 1, 2, 70, 1)
		, ('BA', 'Bank Accounts', 0, 2, 2, 80, 1)
		, ('BP', 'Bank Payments', 0, 0, 0, 90, 1)
		, ('BR', 'Bank Receipts', 0, 1, 0, 100, 1)
		, ('DC', 'Direct Cost', 0, 0, 0, 20, 1)
		, ('DI', 'Dividends', 0, 0, 0, 110, -1)
		, ('DR', 'Drawings', 0, 2, 0, 150, 0)
		, ('IC', 'Indirect Cost', 0, 0, 0, 30, 1)
		, ('IP', 'Intercompany Payment', 0, 0, 2, 120, 1)
		, ('IR', 'Intercompany Receipt', 0, 1, 2, 130, 1)
		, ('IV', 'Investment', 0, 2, 0, 160, 1)
		, ('LI', 'Liabilities', 0, 0, 2, 71, 1)
		, ('SA', 'Sales', 0, 1, 0, 10, 1)
		, ('TA1', 'Taxes on Company', 0, 0, 1, 60, 1)
		, ('TA2', 'Taxes on Trade', 0, 0, 1, 60, 1)
		, ('WA', 'Wages', 0, 0, 0, 50, 1)
		;

		INSERT INTO [Cash].[tbCategory] ([CategoryCode], [Category], [CategoryTypeCode], [CashPolarityCode], [CashTypeCode], [DisplayOrder], [IsEnabled])
		VALUES 
			('TO', 'Turnover', 1, 2, 0, 0, 1)			
			, ('EX', 'Expenses', 1, 2, 0, 1, 1)
			, ('AL', 'Assets and Liabilities', 1, 2, 0, 2, 1)
			, ('PL', 'Profit Before Taxation', 1, 2, 0, 3, 1)			
			, ('TP', 'Tax on Profit', 1, 2, 0, 4, 1)
			, ('FY', 'Profit for Financial Year', 1, 2, 0, 5, 1)
			, ('VAT', 'Vat Cash Codes', 1, 2, 0, 100, 1)
			, ('WR', 'Wages Ratio', 2, 2, 0, 0, 1)
			;

		INSERT INTO [Cash].[tbCategoryTotal] ([ParentCode], [ChildCode])
		VALUES ('EX', 'BP')
		, ('EX', 'DC')
		, ('EX', 'IC')
		, ('EX', 'WA')
		, ('FY', 'PL')
		, ('FY', 'TP')
		, ('PL', 'EX')
		, ('PL', 'TO')
		, ('PL', 'AL')
		, ('TO', 'BR')
		, ('TO', 'SA')
		, ('TO', 'IV')
		, ('TP', 'TA1')
		, ('VAT', 'DC')
		, ('VAT', 'IC')
		, ('VAT', 'SA')
		, ('AL', 'AS')
		, ('AL', 'LI')
		;

		INSERT INTO [Cash].[tbCategoryExp] ([CategoryCode], [Expression], [Format])
		VALUES ('WR', 'IF([Sales]=0,0,(ABS([Wages])/[Sales]))', '0%');

		INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
		VALUES ('101', 'Sales - Carriage', 'SA', 'T1', 1)
		, ('102', 'Sales - Export', 'SA', 'T1', 1)
		, ('103', 'Sales - Home', 'SA', 'T1', 1)
		, ('104', 'Sales - Consultancy', 'SA', 'T1', 1)
		, ('200', 'Direct Purchase', 'DC', 'T1', 1)
		, ('201', 'Company Administration', 'IC', 'T1', 1)
		, ('202', 'Communications', 'IC', 'T1', 1)
		, ('203', 'Entertaining', 'IC', 'N/A', 1)
		, ('204', 'Office Equipment', 'IC', 'T1', 1)
		, ('205', 'Office Rent', 'IC', 'T0', 1)
		, ('206', 'Professional Fees', 'IC', 'T1', 1)
		, ('207', 'Postage', 'IC', 'T1', 1)
		, ('208', 'Sundry', 'IC', 'T1', 1)
		, ('209', 'Stationery', 'IC', 'T1', 1)
		, ('210', 'Subcontracting', 'IC', 'T1', 1)
		, ('211', 'Systems', 'IC', 'T9', 1)
		, ('212', 'Travel - Car Mileage', 'IC', 'N/A', 1)
		, ('213', 'Travel - General', 'IC', 'N/A', 1)
		, ('214', 'Company Loan', 'IV', 'N/A', 1)
		, ('215', 'Directors Loan', 'IV', 'N/A', 1)
		, ('216', 'Directors Expenses reimbursement', 'IC', 'N/A', 1)
		, ('217', 'Office Expenses (General)', 'IC', 'N/A', 1)
		, ('218', 'Subsistence', 'IC', 'N/A', 1)
		, ('250', 'Commission', 'DC', 'T1', 1)
		, ('301', 'Company Cash', 'BA', 'N/A', 1)
		, ('302', 'Bank Charges', 'BP', 'N/A', 1)
		, ('303', 'Account Payment', 'IP', 'N/A', 1)
		, ('304', 'Bank Interest', 'BR', 'N/A', 1)
		, ('305', 'Transfer Receipt', 'IR', 'N/A', 1)
		, ('401', 'Dividends', 'DI', 'N/A', -1)
		, ('402', 'Salaries', 'WA', 'NI1', 1)
		, ('403', 'Pensions', 'WA', 'N/A', 1)
		, ('501', 'Charitable Donation', 'IC', 'N/A', 1)
		, ('601', 'VAT', 'TA2', 'N/A', 1)
		, ('602', 'Taxes (General)', 'TA1', 'N/A', 1)
		, ('603', 'Taxes (Corporation)', 'TA2', 'N/A', 1)
		, ('604', 'Employers NI', 'TA1', 'N/A', 1)
		, ('700', 'Stock Movement', 'AS', 'N/A', 0)
		, ('701', 'Depreciation', 'AS', 'N/A', 1)
		, ('702', 'Dept Repayment', 'LI', 'N/A', 1)
		, ('703', 'Share Capital', 'LI', 'N/A', 1)
		;

		IF @CoinTypeCode < 2
		BEGIN
			INSERT INTO [Cash].[tbCode] ([CashCode], [CashDescription], [CategoryCode], [TaxCode], [IsEnabled])
			VALUES ('219', 'Miner Fees', 'IC', 'N/A', 1);
		
			UPDATE App.tbOptions
			SET MinerFeeCode = '219';
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
		SET SubjectCode = @SubjectCode, CashCode = '603', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 0;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = '601', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 1;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = '604', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 2;

		UPDATE Cash.tbTaxType
		SET SubjectCode = @SubjectCode, CashCode = '602', MonthNumber = @FinancialMonth
		WHERE TaxTypeCode = 3;

		--BANK ACCOUNTS / WALLETS

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
		VALUES        (@AccountCode, @SubjectCode, @CurrentAccount, 0, @CA_SortCode, @CA_AccountNumber, '301')

		IF (LEN(@ReserveAccount) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @ReserveAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, OpeningBalance, SortCode, AccountNumber)
			VALUES        (@AccountCode, @SubjectCode, @ReserveAccount, 0, @RA_SortCode, @RA_AccountNumber)
		END

		SELECT @SubjectCode = (SELECT SubjectCode FROM App.tbOptions)

		IF (LEN(@DummyAccount) > 0)
		BEGIN
			EXEC Subject.proc_DefaultSubjectCode @SubjectName = @DummyAccount, @SubjectCode = @AccountCode OUTPUT
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode)
			VALUES        (@AccountCode, @SubjectCode, @DummyAccount, 1);
		END

		--CAPITAL 
		DECLARE @CapitalAccount NVARCHAR(50);

		SET @CapitalAccount = 'PREMISES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 50, '701', 1);

		SET @CapitalAccount = 'FIXTURES AND FITTINGS';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 40, '701', 1);

		SET @CapitalAccount = 'PLANT AND MACHINERY';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 30, '701', 1);

		SET @CapitalAccount = 'VEHICLES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 20, '701', 1);

		SET @CapitalAccount = 'STOCK';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 10, '700', 1)

		SET @CapitalAccount = 'LONGTERM LIABILITIES';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 50, '702', 0);

		SET @CapitalAccount = 'CALLED UP SHARE CAPITAL';
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @CapitalAccount, @SubjectCode = @AccountCode OUTPUT
		INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, AccountTypeCode, LiquidityLevel, CashCode, AccountClosed)
		VALUES        (@AccountCode, @SubjectCode, @CapitalAccount, 2, 60, '703', 0);


	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
