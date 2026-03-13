SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    IF OBJECT_ID('tempdb..#DatasetCodes') IS NOT NULL
        DROP TABLE #DatasetCodes;

    CREATE TABLE #DatasetCodes
    (
        CodeType nvarchar(20) NOT NULL,        -- 'SUBJECT' | 'OBJECT' | 'LINK'
        CodeName nvarchar(100) NOT NULL,       -- logical name (unique within CodeType)
        CodeValue nvarchar(50) NOT NULL,       -- SubjectCode (10) or ObjectCode (50) or link value
        RelatedName nvarchar(100) NULL,        -- for LINK rows: points at another CodeName
        Notes nvarchar(255) NULL,
        PRIMARY KEY (CodeType, CodeName)
    );

    -------------------------------------------------------------------------
    -- Chunk 1: Bootstrap + periods + bank + opening balances
    -------------------------------------------------------------------------

    -------------------------------------------------------------------------
    -- Step 1: Capture existing configuration (MUST be before proc_NodeDataInit)
    -------------------------------------------------------------------------

    DECLARE
        @TemplateName nvarchar(100) = N'Minimal Micro Company Accounts 2026',
        --@TemplateName nvarchar(100) = N'Sole Trader Accounts 2026',
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
        THROW 51001, 'SyntheticDataset: @TemplateName not found in App.tbTemplate.', 1;

    -------------------------------------------------------------------------
    -- Overwrite template default option
    -------------------------------------------------------------------------
    DECLARE @IsVatRegistered bit =
        (
            SELECT IsVatRegistered
            FROM App.tbTemplate
            WHERE TemplateName = @TemplateName
        );
    -------------------------------------------------------------------------

    DECLARE
        @SubjectCode nvarchar(10) = NULL,
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
        @NodeSubjectCode nvarchar(10) = (SELECT TOP (1) SubjectCode FROM App.tbOptions),
        @CoinTypeCode smallint = (SELECT TOP (1) CoinTypeCode FROM App.tbOptions);

    -- Must exist to reverse engineer
    IF NOT EXISTS (SELECT 1 FROM App.tbOptions)
        THROW 51000, 'SyntheticDataset: App.tbOptions is empty. Configure a node once via the UI, then rerun.', 1;

    SELECT TOP (1)
        @SubjectCode = opt.SubjectCode,
        @UnitOfCharge = opt.UnitOfCharge
    FROM App.tbOptions opt;

    -- Business / bank details (Usr.vwDoc)
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

    -- User identity (Usr.vwCredentials: current login)
    IF NOT EXISTS (SELECT 1 FROM Usr.vwCredentials)
        THROW 51001, 'Current user is not registered. Web intialisation required.', 1;

    SELECT
        @FullName = u.UserName,
        @UserEmailAddress = u.EmailAddress
    FROM Usr.vwCredentials uc
        JOIN Usr.tbUser u
            ON u.UserId = uc.UserId;

    -- Calendar code
    SELECT TOP (1)
        @CalendarCode = CalendarCode
    FROM Usr.tbUser
    ORDER BY UserId;

    -- Financial start month (latest year record is acceptable for re-seeding)
    IF @FinancialMonth IS NULL
        SET @FinancialMonth = (SELECT TOP (1) StartMonth FROM App.tbYear ORDER BY YearNumber DESC);

    IF @FinancialMonth IS NULL
        SET @FinancialMonth = 4;

    -- Government subject name
    IF @GovAccountName IS NULL
        SELECT TOP (1) @GovAccountName = s.SubjectName
        FROM Cash.tbTaxType t
        JOIN Subject.tbSubject s ON t.SubjectCode = s.SubjectCode
        ORDER BY t.SubjectCode;

    -- Dummy account code (AccountTypeCode=1)
    IF EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountTypeCode = 1)
        SELECT TOP (1) @DummyAccount = AccountCode
        FROM Subject.tbAccount
        WHERE AccountTypeCode = 1
        ORDER BY AccountCode;

    -- Reserve account (may be null)
    IF EXISTS (SELECT 1 FROM Cash.vwReserveAccount)
        SELECT
            @ReserveAccount = ra.AccountCode,
            @RA_SortCode = ra.SortCode,
            @RA_AccountNumber = ra.AccountNumber
        FROM Cash.vwReserveAccount ra;

    -- Validate captured mandatory fields for business init
    IF @SubjectCode IS NULL OR @BusinessName IS NULL OR @BusinessAddress IS NULL OR @FullName IS NULL OR @CalendarCode IS NULL OR @UnitOfCharge IS NULL
        THROW 51002, 'SyntheticDataset: unable to reverse engineer required node configuration (Subject/Business/User/Calendar/UoC).', 1;

    -------------------------------------------------------------------------
    -- Preserve Identity (current user + roles/claims) before NodeDataInit wipes it
    -------------------------------------------------------------------------
    DECLARE @IdentityUserId nvarchar(450);

    SELECT TOP (1) @IdentityUserId = au.Id
    FROM dbo.AspNetUsers au
    WHERE au.UserName = @UserEmailAddress
       OR au.Email = @UserEmailAddress
    ORDER BY au.Id;

    IF @IdentityUserId IS NULL
        THROW 51004, 'SyntheticDataset: current Identity user not found in dbo.AspNetUsers.', 1;

    SELECT *
    INTO #Keep_AspNetUsers
    FROM dbo.AspNetUsers
    WHERE Id = @IdentityUserId;

    SELECT *
    INTO #Keep_AspNetUserRoles
    FROM dbo.AspNetUserRoles
    WHERE UserId = @IdentityUserId;

    SELECT *
    INTO #Keep_AspNetUserClaims
    FROM dbo.AspNetUserClaims
    WHERE UserId = @IdentityUserId;

    -------------------------------------------------------------------------
    -- Step 2: Reset node
    -------------------------------------------------------------------------
    EXEC App.proc_NodeDataInit;

    -------------------------------------------------------------------------
    -- Restore Identity (user first, then roles/claims)
    -------------------------------------------------------------------------
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

    -------------------------------------------------------------------------
    -- Step 3: Recreate business node (same as Config.OnPostAsync -> ConfigureNode)
    -------------------------------------------------------------------------
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

    -------------------------------------------------------------------------
    -- Step 4: Install template (same as Config.OnPostAsync -> InstallBasicSetup)
    -------------------------------------------------------------------------
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

    -------------------------------------------------------------------------
    -- Continue with existing Chunk 1 logic (prior year + opening balances)
    -------------------------------------------------------------------------

    DECLARE
        @ExistingMinYear smallint = (SELECT MIN(YearNumber) FROM App.tbYear),
        @ExistingStartMonth smallint = (SELECT TOP (1) StartMonth FROM App.tbYear WHERE YearNumber = (SELECT MIN(YearNumber) FROM App.tbYear)),
        @PriorYear smallint;

    IF @ExistingMinYear IS NULL
        THROW 51003, 'SyntheticDataset: App.tbYear is empty after App.proc_BasicSetup.', 1;

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

	-------------------------------------------------------------------------
	-- Ensure current + reserve accounts exist (AccountTypeCode=0)
	-------------------------------------------------------------------------
	DECLARE
		@CurrentAccountCode nvarchar(10),
		@ReserveAccountCode nvarchar(10);

	SELECT
		@CurrentAccountCode = AccountCode
	FROM Cash.vwCurrentAccount;

	SELECT
		@ReserveAccountCode = AccountCode
	FROM Cash.vwReserveAccount;

    IF @CurrentAccountCode IS NULL
    BEGIN
        SET @CurrentAccountCode = N'DSCA01';

        IF NOT EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = @CurrentAccountCode)
        BEGIN
            INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, SortCode, AccountNumber, CashCode, OpeningBalance, CoinTypeCode, AccountTypeCode, LiquidityLevel)
            VALUES (@CurrentAccountCode, @NodeSubjectCode, N'Dataset Current Account', N'00-00-00', N'00000000',
                    (SELECT TOP (1) CashCode FROM Cash.tbCode ORDER BY CashCode), 0, @CoinTypeCode, 0, 0);
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

    -- Refresh after potential creates
	SELECT
		@CurrentAccountCode = AccountCode
	FROM Cash.vwCurrentAccount;

	SELECT
		@ReserveAccountCode = AccountCode
	FROM Cash.vwReserveAccount;

    -------------------------------------------------------------------------
    -- Ensure dummy account exists (AccountTypeCode=1) - no view available
    -------------------------------------------------------------------------
    DECLARE @DummyAccountCode nvarchar(10);

    SELECT TOP (1)
        @DummyAccountCode = AccountCode
    FROM Subject.tbAccount
    WHERE AccountTypeCode = 1
        AND AccountClosed = 0
    ORDER BY AccountCode;

    IF @DummyAccountCode IS NULL
    BEGIN
        SET @DummyAccountCode = N'DSADJ1';

        IF NOT EXISTS (SELECT 1 FROM Subject.tbAccount WHERE AccountCode = @DummyAccountCode)
        BEGIN
            INSERT INTO Subject.tbAccount (AccountCode, SubjectCode, AccountName, SortCode, AccountNumber, CashCode, OpeningBalance, CoinTypeCode, AccountTypeCode, LiquidityLevel)
            VALUES (@DummyAccountCode, @NodeSubjectCode, N'Dataset Adjustments', NULL, NULL,
                    NULL, 0, @CoinTypeCode, 1, 0);
        END
    END

    -------------------------------------------------------------------------
    -- Seed opening balances
    -------------------------------------------------------------------------
    DECLARE
        @OpeningCurrentCash decimal(18,5) = 25000.00000,
        @OpeningReserveCash decimal(18,5) = 10000.00000;

    UPDATE Subject.tbAccount
    SET OpeningBalance = @OpeningCurrentCash
    WHERE AccountCode = @CurrentAccountCode;

    UPDATE Subject.tbAccount
    SET OpeningBalance = @OpeningReserveCash
    WHERE AccountCode = @ReserveAccountCode;

    COMMIT TRAN;

    -------------------------------------------------------------------------
    -- Verification output (non-mutating)
    -------------------------------------------------------------------------
    SELECT
        @NodeSubjectCode AS NodeSubjectCode,
        @CurrentAccountCode AS CurrentAccountCode,
        (SELECT OpeningBalance FROM Subject.tbAccount WHERE AccountCode = @CurrentAccountCode) AS CurrentOpeningBalance,
        @ReserveAccountCode AS ReserveAccountCode,
        (SELECT OpeningBalance FROM Subject.tbAccount WHERE AccountCode = @ReserveAccountCode) AS ReserveOpeningBalance,
        @DummyAccountCode AS DummyAccountCode;

    SELECT TOP (5) YearNumber, StartMonth, CashStatusCode, Description
    FROM App.tbYear
    ORDER BY YearNumber;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;
    THROW;
END CATCH;

-----------------------------------------------------------------------------
-- Chunk 2: MIS master data (subjects + objects)
-----------------------------------------------------------------------------
SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
	BEGIN TRAN;

	---------------------------------------------------------------------
	-- Helper vars
	---------------------------------------------------------------------
	DECLARE @Code nvarchar(10);

	---------------------------------------------------------------------
	-- Customers (UK + EU + misc)
	---------------------------------------------------------------------
	-- Moulding customer UK (T1, not EU)
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Moulding Customer UK', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Moulding Customer UK', 1, 1,
			N'T1', 0,
			N'30 days', 0, 30, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT' AS CodeType, N'MouldingCustomerUK' AS CodeName, @Code AS CodeValue, NULL AS RelatedName, N'Dataset Moulding Customer UK' AS Notes) AS s
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Moulding customer EU (T0, EUJurisdiction=1)
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Moulding Customer EU', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Moulding Customer EU', 1, 1,
			N'T0', 1,
			N'30 days', 0, 30, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'MouldingCustomerEU', @Code, NULL, N'Dataset Moulding Customer EU') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Print customer UK (T1)
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Print Customer UK', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Print Customer UK', 1, 1,
			N'T1', 0,
			N'30 days', 0, 30, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'PrintCustomerUK', @Code, NULL, N'Dataset Print Customer UK') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Print customer EU (T0)
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Print Customer EU', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Print Customer EU', 1, 1,
			N'T0', 1,
			N'30 days', 0, 30, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'PrintCustomerEU', @Code, NULL, N'Dataset Print Customer EU') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Misc customers
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Walk-in Customer', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Walk-in Customer', 1, 1,
			N'T1', 0,
			N'Immediate', 0, 0, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'MiscCustomer1', @Code, NULL, N'Dataset Walk-in Customer') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'Dataset Online Customer', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'Dataset Online Customer', 1, 1,
			N'T1', 0,
			N'14 days', 0, 14, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'MiscCustomer2', @Code, NULL, N'Dataset Online Customer') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	---------------------------------------------------------------------
	-- Ensure mandatory addresses exist (delivery cannot be to nowhere)
	-- We use #DatasetCodes because the per-subject @...Code variables are not retained.
	---------------------------------------------------------------------
	DECLARE
		@AddrSubjectCode nvarchar(10),
		@AddrSubjectName nvarchar(100);

	DECLARE curAddresses CURSOR LOCAL FAST_FORWARD FOR
		SELECT
			CAST(dc.CodeValue AS nvarchar(10)) AS SubjectCode,
			CAST(dc.Notes AS nvarchar(100)) AS SubjectName
		FROM #DatasetCodes dc
		WHERE dc.CodeType = N'SUBJECT'
		  AND dc.CodeName IN
		  (
			N'MouldingCustomerUK',
			N'MouldingCustomerEU',
			N'PrintCustomerUK',
			N'PrintCustomerEU',
			N'MiscCustomer1',
			N'MiscCustomer2',
			N'PlasticSupplier',
			N'InsertSupplier',
			N'BoxSupplier',
			N'MouldingHaulier',
			N'Printer',
			N'PrintHaulier',
			N'ProvisionsSupplier',
			N'EntertainmentSupplier',
			N'VehicleMaintenanceSupplier',
			N'Employee'
		  );

	OPEN curAddresses;

	FETCH NEXT FROM curAddresses INTO @AddrSubjectCode, @AddrSubjectName;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		IF NOT EXISTS
		(
			SELECT 1
			FROM Subject.tbSubject s
			JOIN Subject.tbAddress a
				ON a.AddressCode = s.AddressCode
			WHERE s.SubjectCode = @AddrSubjectCode
		)
		BEGIN
			DECLARE @Address nvarchar(max);
			SET @Address = N'Residence of ' + @AddrSubjectName;

			EXEC Subject.proc_AddAddress
				@SubjectCode = @AddrSubjectCode,
				@Address = @Address;
		END

		FETCH NEXT FROM curAddresses INTO @AddrSubjectCode, @AddrSubjectName;
	END

	CLOSE curAddresses;
	DEALLOCATE curAddresses;

	---------------------------------------------------------------------
	-- Suppliers / services (all Supplier type=0)
	---------------------------------------------------------------------
	DECLARE @Supplier AS TABLE
	(
		CodeName nvarchar(100) NOT NULL,
		SubjectName nvarchar(100) NOT NULL,
		PaymentTerms nvarchar(100) NOT NULL,
		PayDaysFromMonthEnd bit NOT NULL
	);

	INSERT INTO @Supplier (CodeName, SubjectName, PaymentTerms, PayDaysFromMonthEnd)
	VALUES
		(N'PlasticSupplier', N'Dataset Plastic Supplier', N'30 days', 0),
		(N'InsertSupplier', N'Dataset Inserts Supplier', N'30 days', 0),
		(N'BoxSupplier', N'Dataset Boxes & Pallets Supplier', N'30 days', 0),
		(N'MouldingHaulier', N'Dataset Haulier (Moulding)', N'30 days end of month', 1),
		(N'Printer', N'Dataset Printer', N'30 days', 0),
		(N'PrintHaulier', N'Dataset Haulier (Print)', N'30 days end of month', 1),
		(N'ProvisionsSupplier', N'Dataset Provisions Supplier', N'30 days', 0),
		(N'EntertainmentSupplier', N'Dataset Entertainment Supplier', N'14 days', 0),
		(N'VehicleMaintenanceSupplier', N'Dataset Vehicle Maintenance Supplier', N'30 days', 0);

	DECLARE
		@SuppCodeName nvarchar(100),
		@SuppName nvarchar(100),
		@SuppTerms nvarchar(100),
		@SuppME bit;

	DECLARE c CURSOR LOCAL FAST_FORWARD FOR
		SELECT CodeName, SubjectName, PaymentTerms, PayDaysFromMonthEnd
		FROM @Supplier;

	OPEN c;

	FETCH NEXT FROM c INTO @SuppCodeName, @SuppName, @SuppTerms, @SuppME;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @Code = NULL;
		EXEC Subject.proc_DefaultSubjectCode @SubjectName = @SuppName, @SubjectCode = @Code OUTPUT;

		IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
		BEGIN
			INSERT INTO Subject.tbSubject
			(
				SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
				TaxCode, EUJurisdiction,
				PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
			)
			VALUES
			(
				@Code, @SuppName, 0, 1,
				N'T1', 0,
				@SuppTerms, 0, CASE WHEN @SuppTerms LIKE N'%14%' THEN 14 ELSE 30 END, @SuppME, 1
			);
		END

		MERGE #DatasetCodes AS t
		USING (SELECT N'SUBJECT', @SuppCodeName, @Code, NULL, @SuppName) AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
			ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
		WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
		WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

		FETCH NEXT FROM c INTO @SuppCodeName, @SuppName, @SuppTerms, @SuppME;
	END

	CLOSE c;
	DEALLOCATE c;

	---------------------------------------------------------------------
	-- Employee (SubjectTypeCode=9)
	---------------------------------------------------------------------
	SET @Code = NULL;
	EXEC Subject.proc_DefaultSubjectCode @SubjectName = N'John Smith', @SubjectCode = @Code OUTPUT;

	IF NOT EXISTS (SELECT 1 FROM Subject.tbSubject WHERE SubjectCode = @Code)
	BEGIN
		INSERT INTO Subject.tbSubject
		(
			SubjectCode, SubjectName, SubjectTypeCode, SubjectStatusCode,
			TaxCode, EUJurisdiction,
			PaymentTerms, ExpectedDays, PaymentDays, PayDaysFromMonthEnd, PayBalance
		)
		VALUES
		(
			@Code, N'John Smith', 9, 1,
			N'N/A', 0,
			N'Immediate', 0, 0, 0, 1
		);
	END

	MERGE #DatasetCodes AS t
	USING (SELECT N'SUBJECT', N'Employee', @Code, NULL, N'John Smith') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	---------------------------------------------------------------------
	-- Objects (3 colors + 2 services)
	---------------------------------------------------------------------
	DECLARE
		@WidgetClear nvarchar(50),
		@WidgetRed nvarchar(50),
		@WidgetBlue nvarchar(50),
		@ServiceFlyer nvarchar(50),
		@ServiceBrochure nvarchar(50);

	EXEC App.proc_DatasetCreateProduct @MaterialType = N'CLEAR', @ObjectCode = @WidgetClear OUTPUT;
	EXEC App.proc_DatasetCreateProduct @MaterialType = N'RED', @ObjectCode = @WidgetRed OUTPUT;
	EXEC App.proc_DatasetCreateProduct @MaterialType = N'BLUE', @ObjectCode = @WidgetBlue OUTPUT;

	EXEC App.proc_DatasetCreateService @ServiceName = N'Flyer', @UnitCharge = 0.5, @ObjectCode = @ServiceFlyer OUTPUT;
	EXEC App.proc_DatasetCreateService @ServiceName = N'Brochure', @UnitCharge = 0.10, @ObjectCode = @ServiceBrochure OUTPUT;

	MERGE #DatasetCodes AS t
	USING
	(
		SELECT N'OBJECT' AS CodeType, N'Widget_CLEAR' AS CodeName, @WidgetClear AS CodeValue, NULL AS RelatedName, N'' AS Notes
		UNION ALL SELECT N'OBJECT', N'Widget_RED', @WidgetRed, NULL, N''
		UNION ALL SELECT N'OBJECT', N'Widget_BLUE', @WidgetBlue, NULL, N''
		UNION ALL SELECT N'OBJECT', N'Service_Flyer', @ServiceFlyer, NULL, N''
		UNION ALL SELECT N'OBJECT', N'Service_Brochure', @ServiceBrochure, NULL, N''
		UNION ALL SELECT N'LINK', N'Service_Brochure_Printer', (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'Printer'), N'Service_Brochure', N'Primary printer supplier'
		UNION ALL SELECT N'LINK', N'PO_Transport_Haulier', (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintHaulier'), N'PO Transport', N'Primary haulier for PO Transport'
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

	---------------------------------------------------------------------
	-- Seed opening AR/AP against real dataset counterparties
	---------------------------------------------------------------------
	DECLARE
		@OpeningAR decimal(18,5) = 1200.00000,
		@OpeningAP decimal(18,5) = -800.00000;

	DECLARE
		@OpeningCustomerCode nvarchar(10) =
			(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerUK'),
		@OpeningSupplierCode nvarchar(10) =
			(SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PlasticSupplier');

	IF @OpeningCustomerCode IS NULL
		THROW 51020, 'SyntheticDataset: missing #DatasetCodes entry for SUBJECT/MouldingCustomerUK.', 1;

	IF @OpeningSupplierCode IS NULL
		THROW 51021, 'SyntheticDataset: missing #DatasetCodes entry for SUBJECT/PlasticSupplier.', 1;

	UPDATE Subject.tbSubject
	SET OpeningBalance = @OpeningAR
	WHERE SubjectCode = @OpeningCustomerCode;

	UPDATE Subject.tbSubject
	SET OpeningBalance = @OpeningAP
	WHERE SubjectCode = @OpeningSupplierCode;

	COMMIT TRAN;
END TRY
BEGIN CATCH
	IF @@TRANCOUNT > 0
		ROLLBACK TRAN;
	THROW;
END CATCH;

---------------------------------------------------------------------
-- Dead-year template projects (containers + master projects)
---------------------------------------------------------------------

BEGIN TRY
    BEGIN TRAN

	DECLARE @DeadYearStartOn date =
	(
		SELECT MIN(yp.StartOn)
		FROM App.tbYear y
		JOIN App.tbYearPeriod yp ON yp.YearNumber = y.YearNumber
		WHERE y.YearNumber = (SELECT MIN(YearNumber) FROM App.tbYear)
	);

	DECLARE
		@MouldingCustomerUK nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerUK'),
		@MouldingCustomerEU nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerEU'),
		@PrintCustomerUK nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintCustomerUK'),
		@PrintCustomerEU nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintCustomerEU'),

		@PlasticSupplier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PlasticSupplier'),
		@InsertSupplier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'InsertSupplier'),
		@BoxSupplier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'BoxSupplier'),
		@MouldingHaulier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingHaulier'),
		@Printer nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'Printer'),
		@PrintHaulier nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintHaulier'),

		@WidgetClearObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Widget_CLEAR'),
		@WidgetRedObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Widget_RED'),
		@WidgetBlueObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Widget_BLUE'),
		@ServiceFlyerObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Service_Flyer'),
		@ServiceBrochureObj nvarchar(50) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'OBJECT' AND CodeName = N'Service_Brochure');

	DECLARE
		@ContainerProjectCode nvarchar(20),
		@TemplateProjectCode nvarchar(20);

	-- Moulding UK container + 3 templates
	SET @ContainerProjectCode = NULL;

    INSERT INTO Object.tbObject (ObjectCode, ProjectStatusCode, ObjectDescription, UnitOfMeasure, CashCode, UnitCharge, Printed, RegisterName)
    VALUES ('PROJECT', 0, NULL, 'each', NULL, 0, 0, 'Works Order');

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Moulding UK',
		@CustomerSubjectCode = @MouldingCustomerUK,
		@ObjectCode = @WidgetClearObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = 100,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT' AS CodeType, N'TPL_MouldingUK_CLEAR' AS CodeName, @TemplateProjectCode AS CodeValue, N'MouldingCustomerUK' AS RelatedName, N'' AS Notes) s
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Moulding UK',
		@CustomerSubjectCode = @MouldingCustomerUK,
		@ObjectCode = @WidgetRedObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = 100,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT', N'TPL_MouldingUK_RED', @TemplateProjectCode, N'MouldingCustomerUK', N'') s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Moulding UK',
		@CustomerSubjectCode = @MouldingCustomerUK,
		@ObjectCode = @WidgetBlueObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = 100,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT', N'TPL_MouldingUK_BLUE', @TemplateProjectCode, N'MouldingCustomerUK', N'') s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	-- Print UK container + 2 templates
	SET @ContainerProjectCode = NULL;

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Print UK',
		@CustomerSubjectCode = @PrintCustomerUK,
		@ObjectCode = @ServiceFlyerObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = 100,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT', N'TPL_PrintUK_Flyer', @TemplateProjectCode, N'PrintCustomerUK', N'') s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	EXEC App.proc_DatasetCreateProjectTemplate
		@ParentProjectCode = @ContainerProjectCode OUTPUT,
		@ParentProjectTitle = N'DS Templates - Print UK',
		@CustomerSubjectCode = @PrintCustomerUK,
		@ObjectCode = @ServiceBrochureObj,
		@ActionOn = @DeadYearStartOn,
		@Quantity = 100,
		@BoxSupplierSubjectCode = @BoxSupplier,
		@PlasticSupplierSubjectCode = @PlasticSupplier,
		@InsertSupplierSubjectCode = @InsertSupplier,
		@MouldingHaulierSubjectCode = @MouldingHaulier,
		@PrinterSubjectCode = @Printer,
		@PrintHaulierSubjectCode = @PrintHaulier,
		@ProjectCode = @TemplateProjectCode OUTPUT;

	MERGE #DatasetCodes AS t
	USING (SELECT N'PROJECT', N'TPL_PrintUK_Brochure', @TemplateProjectCode, N'PrintCustomerUK', N'') s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue, RelatedName = s.RelatedName, Notes = s.Notes;

	---------------------------------------------------------------------
	-- Initialise delivery/carriage legs (manual quantity because UsedOnQuantity = 0)
	---------------------------------------------------------------------
	UPDATE p
	SET
		Quantity = 1,
		UnitCharge = o.UnitCharge,
		TotalCharge = o.UnitCharge * 1
	FROM Project.tbFlow f
		JOIN Project.tbProject p
			ON p.ProjectCode = f.ChildProjectCode
			AND p.CashCode = N'TC200'
		JOIN Object.tbObject o
			ON p.ObjectCode = o.ObjectCode
	WHERE f.UsedOnQuantity = 0;

	COMMIT TRAN;
END TRY
BEGIN CATCH
	IF @@TRANCOUNT > 0
		ROLLBACK TRAN;
	THROW;
END CATCH;

-------------------------------------------------------------------------
-- Chunk 3: 24 month transactions
-------------------------------------------------------------------------

BEGIN TRY
	BEGIN TRAN;

	---------------------------------------------------------------------
	-- Chunk 3 settings (sliders/flags)
	---------------------------------------------------------------------
	DECLARE
		@EnableLayer1_Mis bit = 1,
        @EnableLayer1_Invoicing bit = 0,
        @EnableLayer1_Settlement bit = 0,
		@EnableLayer2_Accounts bit = 0,
		@EnableLayer3_Assets bit = 0,

		@MisOrdersPerMonth int = 2,
		@MonthsForward int = 3;

	---------------------------------------------------------------------
	-- Period anchors
	-- Create MIS projects from Month 2 of earliest year, through current + 1
	-- Invoice projects only up to last closed period
	---------------------------------------------------------------------
	DECLARE
		@CurrentPeriodStartOn date =
		(
			SELECT MIN(CAST(StartOn AS date))
			FROM App.tbYearPeriod
			WHERE CashStatusCode = 1
		),
		@LastClosedStartOn date =
		(
			SELECT MAX(CAST(StartOn AS date))
			FROM App.tbYearPeriod
			WHERE CashStatusCode = 2
		),
		@FirstYearStartOn date =
		(
			SELECT MIN(CAST(StartOn AS date))
			FROM App.tbYearPeriod
			WHERE YearNumber = (SELECT MIN(YearNumber) FROM App.tbYear)
		)

    DECLARE
		@StartOn date = DATEADD(month, 1, @FirstYearStartOn),
		@EndOn date = DATEADD(month, @MonthsForward, @CurrentPeriodStartOn);

	IF @CurrentPeriodStartOn IS NULL OR @LastClosedStartOn IS NULL OR @FirstYearStartOn IS NULL
		THROW 51050, 'SyntheticDataset: unable to resolve period anchors from App.tbYearPeriod.', 1;

	---------------------------------------------------------------------
	-- Current account for settling invoices
	---------------------------------------------------------------------
	DECLARE @SettlementAccountCode nvarchar(10) =
	(
		SELECT AccountCode
		FROM Cash.vwCurrentAccount
	);

	IF @SettlementAccountCode IS NULL
		THROW 51051, 'SyntheticDataset: Cash.vwCurrentAccount returned no AccountCode.', 1;

	---------------------------------------------------------------------
	-- Template projects to copy each month (from #DatasetCodes)
	---------------------------------------------------------------------
	DECLARE
		@Tpl_Moulding_Clear nvarchar(20) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'PROJECT' AND CodeName = N'TPL_MouldingUK_CLEAR'),
		@Tpl_Moulding_Red nvarchar(20) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'PROJECT' AND CodeName = N'TPL_MouldingUK_RED'),
		@Tpl_Moulding_Blue nvarchar(20) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'PROJECT' AND CodeName = N'TPL_MouldingUK_BLUE'),
		@Tpl_Print_Flyer nvarchar(20) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'PROJECT' AND CodeName = N'TPL_PrintUK_Flyer'),
		@Tpl_Print_Brochure nvarchar(20) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'PROJECT' AND CodeName = N'TPL_PrintUK_Brochure');

	IF @Tpl_Moulding_Clear IS NULL OR @Tpl_Print_Brochure IS NULL
		THROW 51052, 'SyntheticDataset: missing template project codes in #DatasetCodes (PROJECT/*).', 1;

	---------------------------------------------------------------------
	-- Month table (MonthStartOn, MonthEndOn)
	---------------------------------------------------------------------
	IF OBJECT_ID('tempdb..#Months') IS NOT NULL DROP TABLE #Months;

	;WITH m AS
	(
		SELECT @StartOn AS MonthStartOn
		UNION ALL
		SELECT DATEADD(month, 1, MonthStartOn)
		FROM m
		WHERE DATEADD(month, 1, MonthStartOn) <= @EndOn
	)
	SELECT
		CAST(MonthStartOn AS date) AS MonthStartOn,
		CAST(DATEADD(day, -1, DATEADD(month, 1, MonthStartOn)) AS date) AS MonthEndOn,
		ROW_NUMBER() OVER (ORDER BY MonthStartOn) AS MonthIndex
	INTO #Months
	FROM m
	OPTION (MAXRECURSION 1000);

	---------------------------------------------------------------------
	-- Track created project roots to invoice/pay deterministically
	---------------------------------------------------------------------
	IF OBJECT_ID('tempdb..#MisRoots') IS NOT NULL DROP TABLE #MisRoots;

	CREATE TABLE #MisRoots
	(
		MonthStartOn date NOT NULL,
		ProjectCode nvarchar(20) COLLATE Latin1_General_CI_AS NOT NULL,
		PRIMARY KEY (ProjectCode)
	);

	---------------------------------------------------------------------
	-- Phase 1: Create projects (copy -> update -> schedule)
	---------------------------------------------------------------------
	IF @EnableLayer1_Mis <> 0
	BEGIN
		DECLARE
			@MonthStartOn date,
			@MonthEndOn date,
			@MonthIndex int,
			@FromProject nvarchar(20),
			@ToProject nvarchar(20),
			@ActionOn date,
			@Qty decimal(18,4),
			@Selector int,
			@SubjectCodeForOrder nvarchar(10),
			@IsProduct bit;

		-- Customer subjects for UK/EU mix (from #DatasetCodes)
		DECLARE
			@MouldingCustomerUK_Code nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerUK'),
			@MouldingCustomerEU_Code nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'MouldingCustomerEU'),
			@PrintCustomerUK_Code nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintCustomerUK'),
			@PrintCustomerEU_Code nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'PrintCustomerEU');

		IF @MouldingCustomerUK_Code IS NULL OR @MouldingCustomerEU_Code IS NULL OR @PrintCustomerUK_Code IS NULL OR @PrintCustomerEU_Code IS NULL
			THROW 51053, 'SyntheticDataset: missing SUBJECT codes in #DatasetCodes required for UK/EU mix.', 1;

		DECLARE curMonths CURSOR LOCAL FAST_FORWARD FOR
			SELECT MonthStartOn, MonthEndOn, MonthIndex
			FROM #Months
			ORDER BY MonthStartOn;

		OPEN curMonths;
		FETCH NEXT FROM curMonths INTO @MonthStartOn, @MonthEndOn, @MonthIndex;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			-- Action date: mid-month (deterministic, avoids month-end edge cases)
			SET @ActionOn = DATEADD(day, 14, @MonthStartOn);
			IF @ActionOn > @MonthEndOn SET @ActionOn = @MonthEndOn;

			-----------------------------------------------------------------
			-- Product order (moulding) - qty 100..5000
			-----------------------------------------------------------------
			SET @IsProduct = 1;

			SET @FromProject =
				CASE (@MonthIndex % 3)
					WHEN 1 THEN @Tpl_Moulding_Clear
					WHEN 2 THEN @Tpl_Moulding_Red
					ELSE @Tpl_Moulding_Blue
				END;

			-- 80/20 mix: 20% EU
			SET @Selector = ABS(CHECKSUM(CONCAT(N'DS:', @MonthIndex, N':', N'PRODUCT', N':', N'SUBJECT'))) % 100;
			SET @SubjectCodeForOrder = CASE WHEN @Selector < 20 THEN @MouldingCustomerEU_Code ELSE @MouldingCustomerUK_Code END;

			-- qty 100..5000 inclusive
			SET @Qty = 100 + (ABS(CHECKSUM(CONCAT(N'DS:', @MonthIndex, N':', N'PRODUCT', N':', N'QTY'))) % 4901);

			SET @ToProject = NULL;
			EXEC Project.proc_Copy @FromProjectCode = @FromProject, @ParentProjectCode = NULL, @ToProjectCode = @ToProject OUTPUT;

			UPDATE Project.tbProject
			SET
				SubjectCode = @SubjectCodeForOrder,
				ActionOn = @ActionOn,
				Quantity = @Qty
			WHERE ProjectCode = @ToProject;

			EXEC Project.proc_Schedule @ToProject;

			INSERT INTO #MisRoots (MonthStartOn, ProjectCode) VALUES (@MonthStartOn, @ToProject);

			-----------------------------------------------------------------
			-- Service order (print) - qty 5000..20000
			-----------------------------------------------------------------
			SET @IsProduct = 0;

			SET @FromProject =
				CASE WHEN (@MonthIndex % 2) = 1 THEN @Tpl_Print_Flyer ELSE @Tpl_Print_Brochure END;

			-- 80/20 mix: 20% EU
			SET @Selector = ABS(CHECKSUM(CONCAT(N'DS:', @MonthIndex, N':', N'SERVICE', N':', N'SUBJECT'))) % 100;
			SET @SubjectCodeForOrder = CASE WHEN @Selector < 20 THEN @PrintCustomerEU_Code ELSE @PrintCustomerUK_Code END;

			-- qty 5000..20000 inclusive
			SET @Qty = 5000 + (ABS(CHECKSUM(CONCAT(N'DS:', @MonthIndex, N':', N'SERVICE', N':', N'QTY'))) % 15001);

			SET @ToProject = NULL;
			EXEC Project.proc_Copy @FromProjectCode = @FromProject, @ParentProjectCode = NULL, @ToProjectCode = @ToProject OUTPUT;

			UPDATE Project.tbProject
			SET
				SubjectCode = @SubjectCodeForOrder,
				ActionOn = @ActionOn,
				Quantity = @Qty
			WHERE ProjectCode = @ToProject;

			EXEC Project.proc_Schedule @ToProject;

			INSERT INTO #MisRoots (MonthStartOn, ProjectCode) VALUES (@MonthStartOn, @ToProject);

			FETCH NEXT FROM curMonths INTO @MonthStartOn, @MonthEndOn, @MonthIndex;
		END

		CLOSE curMonths;
		DEALLOCATE curMonths;

		-----------------------------------------------------------------
		-- Expand #MisRoots to include all descendant projects (supply chain)
		-----------------------------------------------------------------
		IF OBJECT_ID('tempdb..#MisAllProjects') IS NOT NULL
			DROP TABLE #MisAllProjects;

		;WITH flow_cte AS
		(
			SELECT
				r.MonthStartOn,
				r.ProjectCode AS RootProjectCode,
				r.ProjectCode AS ProjectCode
			FROM #MisRoots r

			UNION ALL

			SELECT
				c.MonthStartOn,
				c.RootProjectCode,
				f.ChildProjectCode AS ProjectCode
			FROM flow_cte c
				JOIN Project.tbFlow f
					ON f.ParentProjectCode = c.ProjectCode
		)
		SELECT DISTINCT
			MonthStartOn,
			RootProjectCode,
			ProjectCode
		INTO #MisAllProjects
		FROM flow_cte
		OPTION (MAXRECURSION 32767);

		-----------------------------------------------------------------
		-- Diagnostics 1: Projected quantity by month for each object (ALL legs)
		-- Quantity uses same polarity logic as money (opposite sign by CashPolarity)
		-----------------------------------------------------------------
		SELECT
			m.MonthStartOn,
			prj.ObjectCode,
			SUM(
				CASE cat.CashPolarityCode
					WHEN 1 THEN prj.Quantity * -1
					ELSE prj.Quantity
				END
			) AS Quantity
		FROM #MisAllProjects ap
			JOIN #Months m
				ON m.MonthStartOn = ap.MonthStartOn
			JOIN Project.tbProject prj
				ON prj.ProjectCode = ap.ProjectCode
			JOIN Cash.tbCode cc
				ON cc.CashCode = prj.CashCode
			JOIN Cash.tbCategory cat
				ON cc.CategoryCode = cat.CategoryCode
		GROUP BY m.MonthStartOn, prj.ObjectCode
		ORDER BY m.MonthStartOn, prj.ObjectCode;

		-----------------------------------------------------------------
		-- Diagnostics 2: Projected turnover by month for each subject (ALL legs)
		-----------------------------------------------------------------
		SELECT
			m.MonthStartOn,
			prj.SubjectCode,
			SUM(
				CASE cat.CashPolarityCode
					WHEN 0 THEN prj.TotalCharge * -1
					ELSE prj.TotalCharge
				END
			) AS Turnover
		FROM #MisAllProjects ap
			JOIN #Months m
				ON m.MonthStartOn = ap.MonthStartOn
			JOIN Project.tbProject prj
				ON prj.ProjectCode = ap.ProjectCode
			JOIN Cash.tbCode cc
				ON cc.CashCode = prj.CashCode
			JOIN Cash.tbCategory cat
				ON cc.CategoryCode = cat.CategoryCode
		GROUP BY m.MonthStartOn, prj.SubjectCode
		ORDER BY m.MonthStartOn, prj.SubjectCode;
    END;

	---------------------------------------------------------------------
	-- Phase 2: Invoice them on ActionOn (up to last closed period)
	---------------------------------------------------------------------
	IF @EnableLayer1_Invoicing <> 0
	BEGIN
		DECLARE
			@InvoiceProjectCode nvarchar(20),
			@InvoiceTypeCode smallint,
			@InvoiceNumber nvarchar(20),
			@ProjectActionOn date;

		DECLARE curInv CURSOR LOCAL FAST_FORWARD FOR
			SELECT r.ProjectCode
			FROM #MisRoots r
				JOIN Project.tbProject p ON r.ProjectCode = p.ProjectCode
			WHERE CAST(p.ActionOn AS date) <= EOMONTH(@LastClosedStartOn)
			  AND p.CashCode IS NOT NULL
			ORDER BY p.ActionOn, r.ProjectCode;

		OPEN curInv;
		FETCH NEXT FROM curInv INTO @InvoiceProjectCode;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			SELECT @ProjectActionOn = CAST(ActionOn AS date)
			FROM Project.tbProject
			WHERE ProjectCode = @InvoiceProjectCode;

			SET @InvoiceTypeCode = NULL;
			EXEC Project.proc_DefaultInvoiceType @ProjectCode = @InvoiceProjectCode, @InvoiceTypeCode = @InvoiceTypeCode OUTPUT;

			SET @InvoiceNumber = NULL;
			EXEC Invoice.proc_Raise
				@ProjectCode = @InvoiceProjectCode,
				@InvoiceTypeCode = @InvoiceTypeCode,
				@InvoicedOn = @ProjectActionOn,
				@InvoiceNumber = @InvoiceNumber OUTPUT;

			UPDATE Invoice.tbInvoice
			SET InvoiceStatusCode = 1
			WHERE InvoiceNumber = @InvoiceNumber
			  AND InvoiceStatusCode = 0;

			FETCH NEXT FROM curInv INTO @InvoiceProjectCode;
		END

		CLOSE curInv;
		DEALLOCATE curInv;
	END

	---------------------------------------------------------------------
	-- Phase 3: Pay off month-end balances (invoice settlements only)
	-- Uses Subject.vwStatement balances at month end.
	---------------------------------------------------------------------
	IF @EnableLayer1_Settlement <> 0
	BEGIN
		IF OBJECT_ID('tempdb..#Payables') IS NOT NULL DROP TABLE #Payables;

		SELECT DISTINCT p.SubjectCode
		INTO #Payables
		FROM Project.tbProject p
			JOIN #MisRoots r ON p.ProjectCode = r.ProjectCode;

		-- For each month, compute the latest statement balance <= month end and pay it off.
		DECLARE
			@PayMonthStart date,
			@PayMonthEnd date,
			@PaySubjectCode nvarchar(10),
			@Balance float,
			@PaidOn date,
			@PaymentCode nvarchar(20);

		DECLARE curPayMonths CURSOR LOCAL FAST_FORWARD FOR
			SELECT MonthStartOn, MonthEndOn
			FROM #Months
			WHERE MonthStartOn <= EOMONTH(@LastClosedStartOn)
			ORDER BY MonthStartOn;

		OPEN curPayMonths;
		FETCH NEXT FROM curPayMonths INTO @PayMonthStart, @PayMonthEnd;

		WHILE @@FETCH_STATUS = 0
		BEGIN
			-- Pay on month end for determinism
			SET @PaidOn = @PayMonthEnd;

			DECLARE curSubjects CURSOR LOCAL FAST_FORWARD FOR
				SELECT SubjectCode
				FROM #Payables
				ORDER BY SubjectCode;

			OPEN curSubjects;
			FETCH NEXT FROM curSubjects INTO @PaySubjectCode;

			WHILE @@FETCH_STATUS = 0
			BEGIN
				SELECT TOP (1) @Balance = Balance
				FROM Subject.vwStatement
				WHERE SubjectCode = @PaySubjectCode
				  AND CAST(TransactedOn AS date) <= @PayMonthEnd
				ORDER BY TransactedOn DESC, RowNumber DESC;

				SET @Balance = ISNULL(@Balance, 0);

				IF @Balance <> 0
				BEGIN
					SET @PaymentCode = NULL;
					EXEC Cash.proc_NextPaymentCode @PaymentCode = @PaymentCode OUTPUT;

					INSERT INTO Cash.tbPayment
					(
						PaymentCode,
						UserId,
						PaymentStatusCode,
						SubjectCode,
						AccountCode,
						CashCode,
						TaxCode,
						PaidOn,
						PaidInValue,
						PaidOutValue,
						PaymentReference
					)
					SELECT
						@PaymentCode,
						(SELECT UserId FROM Usr.vwCredentials),
						0,
						@PaySubjectCode,
						@SettlementAccountCode,
						NULL,
						NULL,
						@PaidOn,
						CASE WHEN @Balance < 0 THEN CAST(@Balance * -1 AS decimal(18,5)) ELSE CAST(0 AS decimal(18,5)) END,
						CASE WHEN @Balance > 0 THEN CAST(@Balance AS decimal(18,5)) ELSE CAST(0 AS decimal(18,5)) END,
						CONCAT(N'DS SETTLE ', FORMAT(@PaidOn, 'yyyy-MM'));

					-- Payment(s) remain status 0 until posted
				END

				FETCH NEXT FROM curSubjects INTO @PaySubjectCode;
			END

			CLOSE curSubjects;
			DEALLOCATE curSubjects;

			FETCH NEXT FROM curPayMonths INTO @PayMonthStart, @PayMonthEnd;
		END

		CLOSE curPayMonths;
		DEALLOCATE curPayMonths;

		EXEC Cash.proc_PaymentPost;
	END

	COMMIT TRAN;
END TRY
BEGIN CATCH
	IF @@TRANCOUNT > 0
		ROLLBACK TRAN;
	THROW;
END CATCH;
