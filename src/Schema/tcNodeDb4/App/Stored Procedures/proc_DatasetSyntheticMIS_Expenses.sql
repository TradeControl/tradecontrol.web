CREATE PROCEDURE App.proc_DatasetSyntheticMIS_Expenses
(
	@IsCompany bit,
	@IsVatRegistered bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51215, 'DatasetSyntheticMIS_ProjectPay: missing temp table #DatasetCodes. Ensure ProjectTran ran.', 1;

	DECLARE @LastClosedStartOn date =
		TRY_CONVERT(date, (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'LastClosedStartOn'));

	IF @LastClosedStartOn IS NULL
		THROW 51241, 'DatasetSyntheticMIS_Expenses: missing LINK/LastClosedStartOn.', 1;

	-----------------------------------------------------------------
	-- Ensure Category + CashCode for Employee Expenses
	-----------------------------------------------------------------

	IF NOT EXISTS (SELECT 1 FROM Cash.tbCode WHERE CashCode = N'CC-EXPENSE')
	BEGIN
		INSERT INTO Cash.tbCode
		(
			CashCode,
			CashDescription,
			CategoryCode,
			TaxCode,
			IsEnabled
		)
		VALUES
		(
			N'CC-EXPENSE',
			N'Employee Expenses',
			N'CA-ADMIN',
			N'T0',
			1
		);
	END

	IF NOT EXISTS (SELECT 1 FROM Object.tbObject WHERE ObjectCode = 'EXPENSE')
	BEGIN
		INSERT INTO Object.tbObject
		(
			ObjectCode,
			ProjectStatusCode,
			UnitOfMeasure,
			CashCode,
			Printed,
			RegisterName,
			ObjectDescription,
			UnitCharge
		)
		VALUES
		(
			'EXPENSE',
			1,
			'each',
			'CC-EXPENSE',
			1,
			'Purchase Order',
			'Employee Expense Claim',
			0
		);
	END

	-----------------------------------------------------------------
	-- Resolve subjects
	-----------------------------------------------------------------
	DECLARE
		@L2_UserId nvarchar(10) = (SELECT TOP (1) UserId FROM Usr.vwCredentials),
		@L2_EmployeeSubjectCode nvarchar(10) = (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'SUBJECT' AND CodeName = N'Employee');

	IF @L2_EmployeeSubjectCode IS NULL
		THROW 51100, 'SyntheticDataset Layer2: missing SUBJECT/Employee in #DatasetCodes.', 1;

	-----------------------------------------------------------------
	-- Employee Expenses: container project + monthly claim child projects
	-----------------------------------------------------------------
	DECLARE
		@L2_ClaimsContainerProjectCode nvarchar(20) = NULL,
		@L2_ClaimsChildProjectCode nvarchar(20) = NULL,
		@L2_ClaimPaymentCode nvarchar(20) = NULL,
		@L2_ClaimsMonthEnd date,
		@L2_ClaimsMonthIndex int,
		@ObjectCode nvarchar(50);

	-- Container
	SET @ObjectCode = 'PROJECT';
	EXEC Project.proc_NextCode @ObjectCode = @ObjectCode, @ProjectCode = @L2_ClaimsContainerProjectCode OUTPUT;

	IF @L2_ClaimsContainerProjectCode IS NULL
		THROW 51110, 'SyntheticDataset Layer2: Project.proc_NextCode returned NULL.', 1;

	IF NOT EXISTS (SELECT 1 FROM Project.tbProject WHERE ProjectCode = @L2_ClaimsContainerProjectCode)
	BEGIN
		INSERT INTO Project.tbProject
		(
			ProjectCode,
			UserId,
			SubjectCode,
			ProjectTitle,
			ObjectCode,
			ProjectStatusCode,
			ActionById,
			ActionOn,
			Quantity,
			CashCode,
			TaxCode,
			UnitCharge,
			TotalCharge
		)
		VALUES
		(
			@L2_ClaimsContainerProjectCode,
			@L2_UserId,
			@L2_EmployeeSubjectCode,
			N'John Smith''s Expense Claims',
			@ObjectCode,
			0,
			@L2_UserId,
			EOMONTH(@LastClosedStartOn),
			0,
			NULL,
			NULL,
			0,
			0
		);
	END

	-- One claim per closed month
	DECLARE curClaims CURSOR LOCAL FAST_FORWARD FOR
		SELECT MonthEndOn, MonthIndex
		FROM App.fnDatasetMonths(@LastClosedStartOn)
		ORDER BY MonthStartOn;

	OPEN curClaims;
	FETCH NEXT FROM curClaims INTO @L2_ClaimsMonthEnd, @L2_ClaimsMonthIndex;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @ObjectCode = 'EXPENSE';
		EXEC Project.proc_NextCode @ObjectCode = @ObjectCode, @ProjectCode = @L2_ClaimsChildProjectCode OUTPUT;

		IF @L2_ClaimsChildProjectCode IS NULL
			THROW 51111, 'SyntheticDataset Layer2: Project.proc_NextCode returned NULL for claim child project.', 1;

		INSERT INTO Project.tbProject
		(
			ProjectCode,
			UserId,
			SubjectCode,
			ProjectTitle,
			ObjectCode,
			ProjectStatusCode,
			ActionById,
			ActionOn,
			Quantity,
			CashCode,
			TaxCode,
			UnitCharge,
			TotalCharge
		)
		VALUES
		(
			@L2_ClaimsChildProjectCode,
			@L2_UserId,
			@L2_EmployeeSubjectCode,
			CASE WHEN (@L2_ClaimsMonthIndex % 2) = 0 THEN N'Travel Costs' ELSE N'Entertainment' END,
			@ObjectCode,
			0,
			@L2_UserId,
			@L2_ClaimsMonthEnd,
			1,
			N'CC-EXPENSE',
			N'T0',
			CAST(25 + (ABS(CHECKSUM(CONCAT(N'DS:L2:CLAIM:', @L2_ClaimsMonthIndex))) % 175) AS decimal(18,7)),
			0
		);

		EXEC Project.proc_AssignToParent
			@ChildProjectCode = @L2_ClaimsChildProjectCode,
			@ParentProjectCode = @L2_ClaimsContainerProjectCode;

		SET @L2_ClaimPaymentCode = NULL;
		EXEC Project.proc_Pay
			@ProjectCode = @L2_ClaimsChildProjectCode,
			@Post = 1,
			@PaymentCode = @L2_ClaimPaymentCode OUTPUT;

		FETCH NEXT FROM curClaims INTO @L2_ClaimsMonthEnd, @L2_ClaimsMonthIndex;
	END

	CLOSE curClaims;
	DEALLOCATE curClaims;
