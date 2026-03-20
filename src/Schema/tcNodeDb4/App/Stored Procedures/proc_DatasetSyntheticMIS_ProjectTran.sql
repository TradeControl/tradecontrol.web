CREATE PROCEDURE App.proc_DatasetSyntheticMIS_ProjectTran
(
	@IsCompany bit,
	@IsVatRegistered bit,
	@MisOrdersPerMonth int,
	@MonthsForward int
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51212, 'DatasetSyntheticMIS_ProjectTran: #DatasetCodes was not found. Run via App.proc_DatasetSyntheticMIS.', 1;

	---------------------------------------------------------------------
	-- Period anchors
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
		);

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
	-- Template projects to copy each month
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
	-- Track created project roots
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
		SET @ActionOn = DATEADD(day, 14, @MonthStartOn);
		IF @ActionOn > @MonthEndOn SET @ActionOn = @MonthEndOn;

		-- Product order
		SET @IsProduct = 1;

		SET @FromProject =
			CASE (@MonthIndex % 3)
				WHEN 1 THEN @Tpl_Moulding_Clear
				WHEN 2 THEN @Tpl_Moulding_Red
				ELSE @Tpl_Moulding_Blue
			END;

		SET @Selector = ABS(CHECKSUM(CONCAT(N'DS:', @MonthIndex, N':', N'PRODUCT', N':', N'SUBJECT'))) % 100;
		SET @SubjectCodeForOrder = CASE WHEN @Selector < 20 THEN @MouldingCustomerEU_Code ELSE @MouldingCustomerUK_Code END;

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

		-- Service order
		SET @IsProduct = 0;

		SET @FromProject = CASE WHEN (@MonthIndex % 2) = 1 THEN @Tpl_Print_Flyer ELSE @Tpl_Print_Brochure END;

		SET @Selector = ABS(CHECKSUM(CONCAT(N'DS:', @MonthIndex, N':', N'SERVICE', N':', N'SUBJECT'))) % 100;
		SET @SubjectCodeForOrder = CASE WHEN @Selector < 20 THEN @PrintCustomerEU_Code ELSE @PrintCustomerUK_Code END;

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
	-- Expand #MisRoots to include all descendant projects
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
	-- Diagnostics 1
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
	-- Diagnostics 2
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

	-----------------------------------------------------------------
	-- Diagnostics 3
	-----------------------------------------------------------------
	WITH periods AS
	(
		SELECT yp.YearNumber, yp.StartOn
		FROM App.tbYearPeriod yp
	),
	projects AS
	(
		SELECT
			p.ProjectCode,
			p.CashCode,
			(SELECT TOP (1) StartOn
				FROM App.tbYearPeriod
				WHERE StartOn <= p.ActionOn
				ORDER BY StartOn DESC) AS StartOn,
			CASE cat.CashPolarityCode WHEN 0 THEN p.TotalCharge * -1 ELSE p.TotalCharge END TotalCharge,
			ISNULL(tax.TaxRate, 0) AS TaxRate
		FROM Project.tbProject p
		JOIN App.tbTaxCode tax
			ON p.TaxCode = tax.TaxCode
		JOIN Cash.tbCode cc
			ON p.CashCode = cc.CashCode
		JOIN Cash.tbCategory cat
			ON cc.CategoryCode = cat.CategoryCode
		WHERE p.ProjectStatusCode IN (1, 2)
			AND p.CashCode IS NOT NULL
	),
	projects_foryear AS
	(
		SELECT per.YearNumber, prj.*
		FROM projects prj
		JOIN periods per
			ON prj.StartOn = per.StartOn
	),
	orders AS
	(
		SELECT
			pfy.CashCode,
			pfy.YearNumber,
			pfy.StartOn,
			pfy.TotalCharge  AS InvoiceValue,
			(pfy.TotalCharge * pfy.TaxRate) AS InvoiceTax
		FROM projects_foryear pfy
	),
	order_summary AS
	(
		SELECT
			CashCode,
			YearNumber,
			StartOn,
			SUM(InvoiceValue) AS InvoiceValue,
			SUM(InvoiceTax) AS InvoiceTax
		FROM orders
		GROUP BY CashCode,YearNumber, StartOn
	)
	SELECT
		CashCode,
		YearNumber,
		CAST(StartOn AS date) AS StartOn,
		InvoiceValue,
		InvoiceTax
	INTO #T
	FROM order_summary
	WHERE InvoiceValue <> 0 OR InvoiceTax <> 0;

	SELECT YearNumber, SUM(InvoiceValue) TotalProfit, SUM(os.InvoiceTax) TotalVAT
	FROM #T os
	GROUP BY YearNumber
	ORDER BY YearNumber;

	SELECT
		CashCode,
		YearNumber,
		CAST(StartOn AS date) AS StartOn,
		InvoiceValue,
		InvoiceTax
	FROM #T order_summary
	WHERE InvoiceValue <> 0 OR InvoiceTax <> 0
	ORDER BY CashCode, YearNumber, StartOn;

	DROP TABLE #T;

	-- persist these for later phases via #DatasetCodes (avoids re-query drift)
	MERGE #DatasetCodes AS t
	USING (SELECT N'LINK' AS CodeType, N'LastClosedStartOn' AS CodeName, CONVERT(nvarchar(50), @LastClosedStartOn, 23) AS CodeValue, NULL AS RelatedName, N'' AS Notes) AS s
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue;

	MERGE #DatasetCodes AS t
	USING (SELECT N'LINK', N'SettlementAccountCode', @SettlementAccountCode, NULL, N'') AS s (CodeType, CodeName, CodeValue, RelatedName, Notes)
		ON t.CodeType = s.CodeType AND t.CodeName = s.CodeName
	WHEN NOT MATCHED THEN INSERT (CodeType, CodeName, CodeValue, RelatedName, Notes) VALUES (s.CodeType, s.CodeName, s.CodeValue, s.RelatedName, s.Notes)
	WHEN MATCHED THEN UPDATE SET CodeValue = s.CodeValue;
