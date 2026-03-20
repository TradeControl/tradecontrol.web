CREATE PROCEDURE App.proc_DatasetSyntheticMIS_PayInit
(
	@IsCompany bit,
	@IsVatRegistered bit
)
AS
	SET NOCOUNT, XACT_ABORT ON;

	IF OBJECT_ID('tempdb..#DatasetCodes') IS NULL
		THROW 51220, 'DatasetSyntheticMIS_PayInit: missing temp table #DatasetCodes.', 1;

	DECLARE @LastClosedStartOn date =
		TRY_CONVERT(date, (SELECT CodeValue FROM #DatasetCodes WHERE CodeType = N'LINK' AND CodeName = N'LastClosedStartOn'));

	IF @LastClosedStartOn IS NULL
		THROW 51223, 'DatasetSyntheticMIS_PayInit: missing LINK/LastClosedStartOn in #DatasetCodes. Ensure ProjectTran ran.', 1;

	IF OBJECT_ID('tempdb..#Months') IS NULL
	BEGIN
		DECLARE @FirstYearStartOn date =
		(
			SELECT MIN(CAST(StartOn AS date))
			FROM App.tbYearPeriod
			WHERE YearNumber = (SELECT MIN(YearNumber) FROM App.tbYear)
		);

		IF @FirstYearStartOn IS NULL
			THROW 51224, 'DatasetSyntheticMIS_PayInit: unable to resolve @FirstYearStartOn from App.tbYearPeriod.', 1;

		DECLARE
			@StartOn date = DATEADD(month, 1, @FirstYearStartOn),
			@EndOn date = EOMONTH(@LastClosedStartOn);

		IF @StartOn IS NULL OR @EndOn IS NULL OR @StartOn > @EndOn
			THROW 51225, 'DatasetSyntheticMIS_PayInit: invalid month range (StartOn > EndOn).', 1;

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
	END
