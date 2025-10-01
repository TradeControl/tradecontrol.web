CREATE PROCEDURE App.proc_BasicSetup
(
	@TemplateName NVARCHAR(100),
	@FinancialMonth SMALLINT = 4,
	@CoinTypeCode SMALLINT,
	@GovAccountName NVARCHAR(255),
	@BankName NVARCHAR(255) = null,
	@BankAddress NVARCHAR(MAX) = null,
	@DummyAccount NVARCHAR(50) = null, 
	@CurrentAccount NVARCHAR(50) = null,
	@CA_SortCode NVARCHAR(10) = null,
	@CA_AccountNumber NVARCHAR(20) = null,
	@ReserveAccount NVARCHAR(50) = null, 
	@RA_SortCode NVARCHAR(10) = null,
	@RA_AccountNumber NVARCHAR(20) = null
)
AS
	DECLARE 
		@FinancialYear SMALLINT = DATEPART(YEAR, CURRENT_TIMESTAMP);

		IF EXISTS (SELECT * FROM App.tbOptions WHERE UnitOfCharge <> 'BTC') AND (@CoinTypeCode <> 2)
			SET @CoinTypeCode = 2;

		IF DATEPART(MONTH, CURRENT_TIMESTAMP) < @FinancialMonth
			 SET @FinancialYear -= 1;
		
	DECLARE 
		@Year SMALLINT = @FinancialYear - 1;

	SET NOCOUNT, XACT_ABORT ON;

	BEGIN TRY
		BEGIN TRAN
		
		UPDATE App.tbOptions
		SET CoinTypeCode = @CoinTypeCode;

		DECLARE 
			@ProcName nvarchar(100) = (SELECT StoredProcedure FROM App.tbTemplate WHERE TemplateName = @TemplateName);		

		EXEC @ProcName
				@FinancialMonth = @FinancialMonth,
				@GovAccountName = @GovAccountName, 
				@BankName = @BankName, 
				@BankAddress = @BankAddress, 
				@DummyAccount = @DummyAccount, 
				@CurrentAccount = @CurrentAccount, 
				@CA_SortCode = @CA_SortCode, 
				@CA_AccountNumber = @CA_AccountNumber, 
				@ReserveAccount = @ReserveAccount, 
				@RA_SortCode = @RA_SortCode, 
				@RA_AccountNumber = @RA_AccountNumber;

		--TIME PERIODS
		WHILE (@Year < DATEPART(YEAR, CURRENT_TIMESTAMP) + 2)
		BEGIN
		
			INSERT INTO App.tbYear (YearNumber, StartMonth, CashStatusCode, Description)
			VALUES (@Year, @FinancialMonth, 0, 
						CASE WHEN @FinancialMonth > 1 THEN CONCAT(@Year, '-', @Year - ROUND(@Year, -2) + 1) ELSE CONCAT(@Year, '.') END
					);
			SET @Year += 1;
		END

		EXEC Cash.proc_GeneratePeriods;

		UPDATE App.tbYearPeriod
		SET CorporationTaxRate = 0.19;

		UPDATE App.tbYearPeriod
		SET CashStatusCode = 2
		WHERE StartOn < DATEADD(MONTH, -1, CURRENT_TIMESTAMP)

		IF EXISTS(SELECT * FROM App.tbYearPeriod WHERE CashStatusCode = 3)
			WITH current_month AS
			(
				SELECT MAX(StartOn) AS StartOn
				FROM App.tbYearPeriod
				WHERE CashStatusCode = 2
			)
			UPDATE App.tbYearPeriod
			SET CashStatusCode = 1
			FROM App.tbYearPeriod JOIN current_month ON App.tbYearPeriod.StartOn = current_month.StartOn;	
		ELSE
			WITH current_month AS
			(
				SELECT MIN(StartOn) AS StartOn
				FROM App.tbYearPeriod
				WHERE CashStatusCode = 0
			)
			UPDATE App.tbYearPeriod
			SET CashStatusCode = 1
			FROM App.tbYearPeriod JOIN current_month ON App.tbYearPeriod.StartOn = current_month.StartOn;
	
	
		WITH current_month AS
		(
			SELECT YearNumber
			FROM App.tbYearPeriod
			WHERE CashStatusCode = 1
		)
		UPDATE App.tbYear
		SET CashStatusCode = 1
		FROM App.tbYear JOIN current_month ON App.tbYear.YearNumber = current_month.YearNumber;

		UPDATE App.tbYear
		SET CashStatusCode = 2
		WHERE YearNumber < 	(SELECT YearNumber FROM App.tbYear	WHERE CashStatusCode = 1);

		COMMIT TRAN
	END TRY
	BEGIN CATCH
		EXEC App.proc_ErrorLog
	END CATCH
