CREATE PROCEDURE App.proc_DatasetSyntheticMIS_ProjectInvoice
(
	@IsCompany bit,
	@IsVatRegistered bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51213, 'DatasetSyntheticMIS_ProjectInvoice: #DatasetCodes was not found. Run via App.proc_DatasetSyntheticMIS.', 1;

	DECLARE @LastClosedStartOn date =
		TRY_CONVERT(date, (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'LastClosedStartOn'));

	IF @LastClosedStartOn IS NULL
		THROW 51214, 'DatasetSyntheticMIS_ProjectInvoice: missing LINK/LastClosedStartOn in #DatasetCodes. Ensure ProjectTran ran.', 1;

	DECLARE
		@InvoiceProjectCode nvarchar(20),
		@InvoiceTypeCode smallint,
		@InvoiceNumber nvarchar(20),
		@ProjectActionOn date;

	DECLARE curInv CURSOR LOCAL FAST_FORWARD FOR
		SELECT DISTINCT p.ProjectCode
		FROM Project.tbProject p
			JOIN Cash.tbCode cc
				ON p.CashCode = cc.CashCode
		WHERE CAST(p.ActionOn AS date) <= EOMONTH(@LastClosedStartOn)
		ORDER BY p.ProjectCode;

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

	---------------------------------------------------------------------
	-- Returns (Credit/Debit Notes)
	---------------------------------------------------------------------
	DECLARE
		@CreditNoteTypeCode smallint = 1,
		@DebitNoteTypeCode smallint = 3;

	IF NOT EXISTS (SELECT 1 FROM Invoice.tbType WHERE InvoiceTypeCode = @CreditNoteTypeCode)
		THROW 51070, 'SyntheticDataset: missing Invoice.tbType row for CreditNote (InvoiceTypeCode=1).', 1;

	IF NOT EXISTS (SELECT 1 FROM Invoice.tbType WHERE InvoiceTypeCode = @DebitNoteTypeCode)
		THROW 51071, 'SyntheticDataset: missing Invoice.tbType row for DebitNote (InvoiceTypeCode=3).', 1;

	IF OBJECT_ID('tempdb..#ReturnCandidates') IS NOT NULL DROP TABLE #ReturnCandidates;

	;WITH candidates AS
	(
		SELECT
			yp.YearNumber,
			p.ProjectCode,
			CAST(p.ActionOn AS date) AS ActionOn,
			subj.SubjectTypeCode,
			obj.UnitOfMeasure,
			ROW_NUMBER() OVER
			(
				PARTITION BY yp.YearNumber,
					CASE
						WHEN subj.SubjectTypeCode = 1 AND obj.UnitOfMeasure = N'each' THEN N'CR_PRODUCT'
						WHEN subj.SubjectTypeCode = 1 AND obj.UnitOfMeasure <> N'each' THEN N'CR_SERVICE'
						WHEN subj.SubjectTypeCode = 0 AND obj.UnitOfMeasure = N'each' THEN N'DR_PRODUCT'
						WHEN subj.SubjectTypeCode = 0 AND obj.UnitOfMeasure <> N'each' THEN N'DR_SERVICE'
						ELSE N'OTHER'
					END
				ORDER BY p.ProjectCode
			) AS RN
		FROM Project.tbProject p
			JOIN Subject.tbSubject subj
				ON p.SubjectCode = subj.SubjectCode
			JOIN Object.tbObject obj
				ON p.ObjectCode = obj.ObjectCode
			JOIN App.tbYearPeriod yp
				ON yp.StartOn =
					(
						SELECT TOP (1) StartOn
						FROM App.tbYearPeriod
						WHERE StartOn <= p.ActionOn
						ORDER BY StartOn DESC
					)
		WHERE p.CashCode IS NOT NULL
			AND CAST(p.ActionOn AS date) <= EOMONTH(@LastClosedStartOn)
			AND subj.SubjectTypeCode IN (0, 1)
	)
	SELECT
		YearNumber,
		ProjectCode,
		ActionOn,
		CASE
			WHEN SubjectTypeCode = 1 AND UnitOfMeasure = N'each' THEN N'CR_PRODUCT'
			WHEN SubjectTypeCode = 1 AND UnitOfMeasure <> N'each' THEN N'CR_SERVICE'
			WHEN SubjectTypeCode = 0 AND UnitOfMeasure = N'each' THEN N'DR_PRODUCT'
			WHEN SubjectTypeCode = 0 AND UnitOfMeasure <> N'each' THEN N'DR_SERVICE'
			ELSE N'OTHER'
		END AS ReturnKind
	INTO #ReturnCandidates
	FROM candidates
	WHERE RN = 1
		AND
		(
			(SubjectTypeCode = 1 AND (UnitOfMeasure = N'each' OR UnitOfMeasure <> N'each'))
			OR (SubjectTypeCode = 0 AND (UnitOfMeasure = N'each' OR UnitOfMeasure <> N'each'))
		)
		AND
		(
			CASE
				WHEN SubjectTypeCode = 1 AND UnitOfMeasure = N'each' THEN N'CR_PRODUCT'
				WHEN SubjectTypeCode = 1 AND UnitOfMeasure <> N'each' THEN N'CR_SERVICE'
				WHEN SubjectTypeCode = 0 AND UnitOfMeasure = N'each' THEN N'DR_PRODUCT'
				WHEN SubjectTypeCode = 0 AND UnitOfMeasure <> N'each' THEN N'DR_SERVICE'
				ELSE N'OTHER'
			END <> N'OTHER'
		);

	DECLARE
		@RetYear smallint,
		@RetProjectCode nvarchar(20),
		@RetActionOn date,
		@RetKind nvarchar(20),
		@RetInvoiceNumber nvarchar(20),
		@IsHalf bit;

	DECLARE curReturns CURSOR LOCAL FAST_FORWARD FOR
		SELECT YearNumber, ProjectCode, ActionOn, ReturnKind
		FROM #ReturnCandidates
		ORDER BY YearNumber, ReturnKind;

	OPEN curReturns;
	FETCH NEXT FROM curReturns INTO @RetYear, @RetProjectCode, @RetActionOn, @RetKind;

	WHILE @@FETCH_STATUS = 0
	BEGIN
		SET @IsHalf = CASE WHEN @RetKind = N'CR_PRODUCT' THEN 1 ELSE 0 END;
		SET @RetInvoiceNumber = NULL;

		IF @RetKind IN (N'CR_PRODUCT', N'CR_SERVICE')
		BEGIN
			EXEC Invoice.proc_Raise
				@ProjectCode = @RetProjectCode,
				@InvoiceTypeCode = @CreditNoteTypeCode,
				@InvoicedOn = @RetActionOn,
				@InvoiceNumber = @RetInvoiceNumber OUTPUT;
		END
		ELSE
		BEGIN
			EXEC Invoice.proc_Raise
				@ProjectCode = @RetProjectCode,
				@InvoiceTypeCode = @DebitNoteTypeCode,
				@InvoicedOn = @RetActionOn,
				@InvoiceNumber = @RetInvoiceNumber OUTPUT;
		END

		IF @RetInvoiceNumber IS NOT NULL
		BEGIN
			UPDATE Invoice.tbInvoice
			SET InvoiceStatusCode = 1
			WHERE InvoiceNumber = @RetInvoiceNumber
				AND InvoiceStatusCode = 0;

			IF @IsHalf <> 0
			BEGIN
				UPDATE ip
				SET
					Quantity = CAST(ip.Quantity / 2.0 AS decimal(18,4)),
					InvoiceValue = CAST(ip.InvoiceValue / 2.0 AS decimal(18,5)),
					TotalValue = 0
				FROM Invoice.tbProject ip
				WHERE ip.InvoiceNumber = @RetInvoiceNumber;

				UPDATE ii
				SET
					InvoiceValue = CAST(ii.InvoiceValue / 2.0 AS decimal(18,5)),
					TotalValue = 0
				FROM Invoice.tbItem ii
				WHERE ii.InvoiceNumber = @RetInvoiceNumber;
			END
		END

		FETCH NEXT FROM curReturns INTO @RetYear, @RetProjectCode, @RetActionOn, @RetKind;
	END

	CLOSE curReturns;
	DEALLOCATE curReturns;

	DROP TABLE #ReturnCandidates;

	-- Pull invoice due date into the past
	UPDATE i
	SET
		DueOn = App.fnAdjustToCalendar
		(
			CASE
				WHEN s.PayDaysFromMonthEnd <> 0
				THEN DATEADD(d, -1, DATEADD(m, 1, CONCAT(FORMAT(DATEADD(d, s.PaymentDays, i.InvoicedOn), 'yyyyMM'), '01')))
				ELSE DATEADD(d, s.PaymentDays, i.InvoicedOn)
			END
			, 0
		)
	FROM Invoice.tbInvoice i
		JOIN Subject.tbSubject s
			ON i.SubjectCode = s.SubjectCode;

	UPDATE Invoice.tbInvoice
	SET ExpectedOn = DueOn;

