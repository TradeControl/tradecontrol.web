CREATE PROCEDURE App.proc_DatasetSyntheticMIS_Bootstrap
(
	@TemplateName nvarchar(100) = N'Minimal Micro Company Accounts 2026',
	@IsVatRegistered bit = NULL,
    @EnableOpeningBalance bit = 1
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	DECLARE
		@FinancialMonth smallint = NULL,
		@GovAccountName nvarchar(255) = NULL,
		@BankName nvarchar(255) = NULL,
		@BankAddress nvarchar(max) = NULL,
		@DummyAccount nvarchar(50) = NULL,
		@CurrentAccount nvarchar(50) = NULL,
		@CA_SortCode nvarchar(10) = NULL,
		@CA_AccountNumber nvarchar(20) = NULL,
		@ReserveAccount nvarchar(50) = NULL,
		@RA_SortCode nvarchar(10) = NULL,
		@RA_AccountNumber nvarchar(20) = NULL;

	IF @TemplateName IS NULL OR NOT EXISTS (SELECT 1 FROM App.tbTemplate WHERE TemplateName = @TemplateName)
		THROW 51001, 'DatasetSyntheticMIS: @TemplateName not found in App.tbTemplate.', 1;

	IF @IsVatRegistered IS NULL
	BEGIN
		SELECT @IsVatRegistered = IsVatRegistered
		FROM App.tbTemplate
		WHERE TemplateName = @TemplateName;
	END

	DECLARE
		@SubjectCode nvarchar(50) = NULL,
		@BusinessName nvarchar(255) = NULL,
		@FullName nvarchar(100) = NULL,
		@BusinessAddress nvarchar(max) = NULL,
		@BusinessEmailAddress nvarchar(255) = NULL,
		@UserEmailAddress nvarchar(255) = NULL,
		@PhoneNumber nvarchar(50) = NULL,
		@CompanyNumber nvarchar(20) = NULL,
		@VatNumber nvarchar(20) = NULL,
		@CalendarCode nvarchar(10) = NULL,
		@UnitOfCharge nvarchar(5) = NULL;

	DECLARE
		@NodeSubjectCode nvarchar(50) = (SELECT TOP (1) SubjectCode FROM App.tbOptions),
		@CoinTypeCode smallint = (SELECT TOP (1) CoinTypeCode FROM App.tbOptions);

	IF NOT EXISTS (SELECT 1 FROM App.tbOptions)
		THROW 51000, 'DatasetSyntheticMIS: App.tbOptions is empty. Configure a node once via the UI, then rerun.', 1;

	SELECT TOP (1)
		@SubjectCode = opt.SubjectCode,
		@UnitOfCharge = opt.UnitOfCharge
	FROM App.tbOptions opt;

	IF EXISTS (SELECT 1 FROM Usr.vwDoc)
	BEGIN
		SELECT TOP (1)
			@BusinessName = CompanyName,
			@BusinessAddress = CompanyAddress,
			@BusinessEmailAddress = CompanyEmailAddress,
			@PhoneNumber = CompanyPhoneNumber,
			@CompanyNumber = CompanyNumber,
			@VatNumber = VatNumber,
			@BankName = BankName,
			@BankAddress = BankAddress,
			@CurrentAccount = CurrentAccountName,
			@CA_AccountNumber = BankAccountNumber,
			@CA_SortCode = BankSortCode
		FROM Usr.vwDoc;
	END

	IF NOT EXISTS (SELECT 1 FROM Usr.vwCredentials)
		THROW 51001, 'DatasetSyntheticMIS: current user is not registered. Web initialization required.', 1;

	SELECT
		@FullName = u.UserName,
		@UserEmailAddress = u.EmailAddress
	FROM Usr.vwCredentials uc
		JOIN Usr.tbUser u
			ON u.UserId = uc.UserId;

	SELECT TOP (1)
		@CalendarCode = CalendarCode
	FROM Usr.tbUser
	ORDER BY UserId;

	IF @FinancialMonth IS NULL
		SET @FinancialMonth = (SELECT TOP (1) StartMonth FROM App.tbYear ORDER BY YearNumber DESC);

	IF @FinancialMonth IS NULL
		SET @FinancialMonth = 4;

	IF @GovAccountName IS NULL
		SELECT TOP (1) @GovAccountName = s.SubjectName
		FROM Cash.tbTaxType t
			JOIN Subject.tbSubject s ON t.SubjectCode = s.SubjectCode
		ORDER BY t.SubjectCode;

	IF EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountTypeCode = 1)
		SELECT TOP (1) @DummyAccount = AccountCode
		FROM Subject.tbAccount
		WHERE AccountTypeCode = 1
		ORDER BY AccountCode;

	IF EXISTS (SELECT 1 FROM Cash.vwReserveAccount)
		SELECT
			@ReserveAccount = ra.AccountCode,
			@RA_SortCode = ra.SortCode,
			@RA_AccountNumber = ra.AccountNumber
		FROM Cash.vwReserveAccount ra;

	IF @SubjectCode IS NULL OR @BusinessName IS NULL OR @BusinessAddress IS NULL OR @FullName IS NULL OR @CalendarCode IS NULL OR @UnitOfCharge IS NULL
		THROW 51002, 'DatasetSyntheticMIS: unable to reverse engineer required node configuration.', 1;

	DECLARE @IdentityUserId nvarchar(450);

	SELECT TOP (1) @IdentityUserId = au.Id
	FROM dbo.AspNetUsers au
	WHERE au.UserName = @UserEmailAddress
		OR au.Email = @UserEmailAddress
	ORDER BY au.Id;

	IF @IdentityUserId IS NULL
		THROW 51004, 'DatasetSyntheticMIS: current Identity user not found in dbo.AspNetUsers.', 1;

	SELECT * INTO #Keep_AspNetUsers FROM dbo.AspNetUsers WHERE Id = @IdentityUserId;
	SELECT * INTO #Keep_AspNetUserRoles FROM dbo.AspNetUserRoles WHERE UserId = @IdentityUserId;
	SELECT * INTO #Keep_AspNetUserClaims FROM dbo.AspNetUserClaims WHERE UserId = @IdentityUserId;

	EXEC App.proc_NodeDataInit;

	IF NOT EXISTS (SELECT 1 FROM dbo.AspNetUsers WHERE Id = @IdentityUserId)
	BEGIN
		INSERT INTO dbo.AspNetUsers
		(
			Id,
			UserName,
			NormalizedUserName,
			Email,
			NormalizedEmail,
			EmailConfirmed,
			PasswordHash,
			SecurityStamp,
			ConcurrencyStamp,
			PhoneNumber,
			PhoneNumberConfirmed,
			TwoFactorEnabled,
			LockoutEnd,
			LockoutEnabled,
			AccessFailedCount
		)
		SELECT
			Id,
			UserName,
			NormalizedUserName,
			Email,
			NormalizedEmail,
			EmailConfirmed,
			PasswordHash,
			SecurityStamp,
			ConcurrencyStamp,
			PhoneNumber,
			PhoneNumberConfirmed,
			TwoFactorEnabled,
			LockoutEnd,
			LockoutEnabled,
			AccessFailedCount
		FROM #Keep_AspNetUsers;
	END

	INSERT INTO dbo.AspNetUserRoles (UserId, RoleId)
	SELECT k.UserId, k.RoleId
	FROM #Keep_AspNetUserRoles k
	WHERE NOT EXISTS
	(
		SELECT 1
		FROM dbo.AspNetUserRoles r
		WHERE r.UserId = k.UserId
			AND r.RoleId = k.RoleId
	);

	INSERT INTO dbo.AspNetUserClaims (UserId, ClaimType, ClaimValue)
	SELECT k.UserId, k.ClaimType, k.ClaimValue
	FROM #Keep_AspNetUserClaims k
	WHERE NOT EXISTS
	(
		SELECT 1
		FROM dbo.AspNetUserClaims c
		WHERE c.UserId = k.UserId
			AND ISNULL(c.ClaimType, '') = ISNULL(k.ClaimType, '')
			AND ISNULL(c.ClaimValue, '') = ISNULL(k.ClaimValue, '')
	);

	DROP TABLE #Keep_AspNetUsers;
	DROP TABLE #Keep_AspNetUserRoles;
	DROP TABLE #Keep_AspNetUserClaims;

	EXEC App.proc_NodeBusinessInit
		@SubjectCode = @SubjectCode,
		@BusinessName = @BusinessName,
		@FullName = @FullName,
		@BusinessAddress = @BusinessAddress,
		@BusinessEmailAddress = @BusinessEmailAddress,
		@UserEmailAddress = @UserEmailAddress,
		@PhoneNumber = @PhoneNumber,
		@CompanyNumber = @CompanyNumber,
		@VatNumber = @VatNumber,
		@CalendarCode = @CalendarCode,
		@UnitOfCharge = @UnitOfCharge;

	EXEC App.proc_BasicSetup
		@TemplateName = @TemplateName,
		@FinancialMonth = @FinancialMonth,
		@CoinTypeCode = @CoinTypeCode,
		@GovAccountName = @GovAccountName,
		@BankName = @BankName,
		@BankAddress = @BankAddress,
		@DummyAccount = @DummyAccount,
		@CurrentAccount = @CurrentAccount,
		@CA_SortCode = @CA_SortCode,
		@CA_AccountNumber = @CA_AccountNumber,
		@ReserveAccount = @ReserveAccount,
		@RA_SortCode = @RA_SortCode,
		@RA_AccountNumber = @RA_AccountNumber,
		@IsVatRegistered = @IsVatRegistered;

	DECLARE @ExternalRootCode nvarchar(50) = NULL;

	EXEC App.proc_DefaultCodeGenerator
		@Description = N'External',
		@CheckSql = N'SELECT @cnt = COUNT(*) FROM Subject.tbSubject WHERE SubjectCode = @Code;',
		@UseWholeWords = 1,
		@Code = @ExternalRootCode OUTPUT;

	IF @ExternalRootCode IS NULL
		THROW 51005, 'DatasetSyntheticMIS: failed to generate SubjectCode for External root.', 1;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @ExternalRootCode)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode,
			SubjectName,
			SubjectTypeCode,
			SubjectStatusCode,
			TransmitStatusCode,
			TaxCode,
			AddressCode,
			AreaCode,
			PhoneNumber,
			EmailAddress
		)
		VALUES
		(
			@ExternalRootCode,
			N'External',
			10,
			1,
			0,
			N'N/A',
			NULL,
			NULL,
			NULL,
			NULL
		);

		INSERT INTO Subject.tbStructural
		(
			SubjectCode,
			Notes
		)
		VALUES
		(
			@ExternalRootCode,
			N'Dataset root for external trading partners'
		);
	END

	MERGE #DatasetCodes AS t
	USING
	(
		SELECT
			N'SUBJECT' AS CodeType,
			N'ExternalRoot' AS CodeName,
			@ExternalRootCode AS CodeValue,
			NULL AS RelatedName,
			N'External structural root' AS Notes
	) AS s
		ON t.CodeType = s.CodeType
		AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN
		INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes)
		VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN
		UPDATE SET
			CodeValue = s.CodeValue,
			RelatedName = s.RelatedName,
			Notes = s.Notes;

	DECLARE
		@ExistingMinYear smallint = (SELECT MIN(YearNumber) FROM App.tbYear),
		@ExistingStartMonth smallint = (SELECT TOP (1) StartMonth FROM App.tbYear WHERE YearNumber = (SELECT MIN(YearNumber) FROM App.tbYear)),
		@PriorYear smallint;

	IF @ExistingMinYear IS NULL
		THROW 51003, 'DatasetSyntheticMIS: App.tbYear is empty after App.proc_BasicSetup.', 1;

	IF @ExistingStartMonth IS NULL
		SET @ExistingStartMonth = @FinancialMonth;

	SET @PriorYear = @ExistingMinYear - 1;

	IF NOT EXISTS (SELECT 1 FROM App.tbYear WHERE YearNumber = @PriorYear)
	BEGIN
		INSERT INTO App.tbYear (YearNumber, StartMonth, CashStatusCode, Description)
		VALUES
		(
			@PriorYear,
			@ExistingStartMonth,
			2,
			CASE WHEN @ExistingStartMonth > 1
				THEN CONCAT(@PriorYear, '-', @PriorYear - ROUND(@PriorYear, -2) + 1)
				ELSE CONCAT(@PriorYear, '.')
			END
		);

		UPDATE App.tbYear
		SET CashStatusCode = 0
		WHERE YearNumber = @PriorYear;

		EXEC Cash.proc_GeneratePeriods;

		UPDATE App.tbYearPeriod
		SET CashStatusCode = 2
		WHERE YearNumber = @PriorYear;

		UPDATE App.tbYear
		SET CashStatusCode = 2
		WHERE YearNumber = @PriorYear;
	END

    UPDATE App.tbYearPeriod
    SET BusinessTaxRate = 0.19;

	DECLARE
		@CurrentAccountCode nvarchar(10),
		@ReserveAccountCode nvarchar(10);

	SELECT @CurrentAccountCode = AccountCode FROM Cash.vwCurrentAccount;
	SELECT @ReserveAccountCode = AccountCode FROM Cash.vwReserveAccount;

	IF @CurrentAccountCode IS NULL
	BEGIN
		SET @CurrentAccountCode = N'DSCA01';

		IF NOT EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = @CurrentAccountCode)
		BEGIN
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, SortCode, AccountNumber, CashCode, OpeningBalance, CoinTypeCode, AccountTypeCode, LiquidityLevel)
			VALUES (@CurrentAccountCode, @NodeSubjectCode, N'Dataset Current Account', N'00-00-00', N'00000000',
					'CC-BANK', 0, @CoinTypeCode, 0, 0);
		END
	END

	IF @ReserveAccountCode IS NULL
	BEGIN
		SET @ReserveAccountCode = N'DSRA01';

		IF NOT EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = @ReserveAccountCode)
		BEGIN
			INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, SortCode, AccountNumber, CashCode, OpeningBalance, CoinTypeCode, AccountTypeCode, LiquidityLevel)
			VALUES (@ReserveAccountCode, @NodeSubjectCode, N'Dataset Reserve Account', N'00-00-00', N'00000000',
					NULL, 0, @CoinTypeCode, 0, 1);
		END
	END

	DECLARE
		@OpeningCurrentCash decimal(18,5) = (CASE @EnableOpeningBalance WHEN 1 THEN 25000.00000 ELSE 0 END),
		@OpeningReserveCash decimal(18,5) = (CASE @EnableOpeningBalance WHEN 1 THEN 10000.00000 ELSE 0 END);

	UPDATE Subject.tbAccount
	SET OpeningBalance = @OpeningCurrentCash
	WHERE AccountCode = (SELECT AccountCode FROM Cash.vwCurrentAccount);

	UPDATE Subject.tbAccount
	SET OpeningBalance = @OpeningReserveCash
	WHERE AccountCode = (SELECT AccountCode FROM Cash.vwReserveAccount);

	DECLARE
		@BuyerRootCode nvarchar(50) = NULL,
		@SellerRootCode nvarchar(50) = NULL,
		@StateRootCode nvarchar(50) = NULL;

	EXEC Subject.proc_AddNamespace
		@RootSubjectCode = @ExternalRootCode,
		@SubjectName = N'Buyer',
		@SubjectTypeCode = 10,
		@SubjectCode = @BuyerRootCode OUTPUT;

	EXEC Subject.proc_AddNamespace
		@RootSubjectCode = @ExternalRootCode,
		@SubjectName = N'Seller',
		@SubjectTypeCode = 10,
		@SubjectCode = @SellerRootCode OUTPUT;

	EXEC Subject.proc_AddNamespace
		@RootSubjectCode = @ExternalRootCode,
		@SubjectName = N'State',
		@SubjectTypeCode = 10,
		@SubjectCode = @StateRootCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING
	(
		SELECT N'SUBJECT' AS CodeType, N'BuyerRoot' AS CodeName, @BuyerRootCode AS CodeValue, N'ExternalRoot' AS RelatedName, N'Buyer structural root' AS Notes
		UNION ALL
		SELECT N'SUBJECT', N'SellerRoot', @SellerRootCode, N'ExternalRoot', N'Seller structural root'
		UNION ALL
		SELECT N'SUBJECT', N'StateRoot', @StateRootCode, N'ExternalRoot', N'State structural root'
	) AS s
		ON t.CodeType = s.CodeType
		AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN
		INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes)
		VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN
		UPDATE SET
			CodeValue = s.CodeValue,
			RelatedName = s.RelatedName,
			Notes = s.Notes;
GO
