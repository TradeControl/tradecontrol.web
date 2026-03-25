CREATE VIEW Cash.vwEquityReconciliationByYear
AS
	WITH first_period AS
	(
		SELECT MIN(yp.StartOn) AS StartOn
		FROM App.tbYearPeriod yp
		JOIN App.tbYear y ON yp.YearNumber = y.YearNumber
		WHERE y.CashStatusCode BETWEEN 1 AND 2
	),
	opening_position AS
	(
		SELECT OpeningPosition = -SUM(s.OpeningBalance)
		FROM Subject.tbSubject s
		WHERE s.OpeningBalance <> 0
	),
	opening_capital AS
	(
		SELECT SUM(bs.Balance) AS Capital
		FROM Cash.vwBalanceSheet bs
		JOIN first_period fp ON bs.StartOn = fp.StartOn
	),
	year_bounds AS
	(
		SELECT
			yp.YearNumber,
			MAX(yp.StartOn) AS YearEndOn
		FROM App.tbYearPeriod yp
		JOIN App.tbYear y ON yp.YearNumber = y.YearNumber
		WHERE y.CashStatusCode BETWEEN 1 AND 2
		GROUP BY yp.YearNumber
	),
	closing_capital AS
	(
		SELECT
			b.YearNumber,
			b.YearEndOn AS StartOn,
			SUM(bs.Balance) AS Capital
		FROM year_bounds b
		JOIN Cash.vwBalanceSheet bs ON bs.StartOn = b.YearEndOn
		GROUP BY b.YearNumber, b.YearEndOn
	),
	balances AS
	(
		SELECT
			cc.YearNumber,
			OpeningCapital = LAG(cc.Capital) OVER (ORDER BY cc.StartOn),
			ClosingCapital = cc.Capital
		FROM closing_capital cc
	),
	profit_by_year AS
	(
		SELECT
			pl.YearNumber,
			Profit = SUM(pl.InvoiceValue)
		FROM Cash.vwProfitAndLossByYear pl
		GROUP BY pl.YearNumber
	),

	-----------------------------------------------------------------
	-- Corporation tax: bind each accounting year-end to the corp tax
	-- statement bucket (PayOn), then split expense vs carry by TaxDue
	-- sign (statement truth source).
	-----------------------------------------------------------------
	corptax_due_dates AS
	(
		SELECT PayOn, PayFrom, PayTo
		FROM Cash.fnTaxTypeDueDates(0, 0)
	),
	corptax_year_end AS
	(
		SELECT
			yb.YearNumber,
			yb.YearEndOn,
			CorpTaxPayOn =
				(
					SELECT TOP (1) dd.PayOn
					FROM corptax_due_dates dd
					WHERE yb.YearEndOn >= dd.PayFrom
					  AND yb.YearEndOn < dd.PayTo
					ORDER BY dd.PayOn
				)
		FROM year_bounds yb
	),
	corptax_stmt AS
	(
		SELECT
			ye.YearNumber,
			ye.CorpTaxPayOn,
			TaxDue = COALESCE(st.TaxDue, 0),
			TaxBalance = COALESCE(st.Balance, 0),
			CorporationTaxRate = COALESCE(yp.CorporationTaxRate, 0)
		FROM corptax_year_end ye
			LEFT JOIN Cash.vwTaxCorpStatement st
				ON st.StartOn = ye.CorpTaxPayOn
			LEFT JOIN App.tbYearPeriod yp
				ON yp.StartOn = ye.CorpTaxPayOn
	),
	corptax_by_year AS
	(
		SELECT
			YearNumber,
			CorporationTaxExpense = SUM(CASE WHEN TaxDue > 0 THEN TaxDue ELSE 0 END),
			TaxCarry = SUM(CASE WHEN TaxDue < 0 THEN TaxDue ELSE 0 END),
			TaxBalance = SUM(TaxBalance),
			CorporationTaxRate = MAX(CorporationTaxRate)
		FROM corptax_stmt
		GROUP BY YearNumber
	),
	loss_cf_delta AS
	(
		SELECT
			YearNumber,
			OpeningLossesCarriedForward = LAG(LossesCarriedForward) OVER (ORDER BY YearNumber),
			ClosingLossesCarriedForward = LossesCarriedForward
		FROM
		(
			SELECT
				YearNumber,
				LossesCarriedForward =
					CASE
						WHEN CorporationTaxRate = 0 THEN 0
						ELSE
							CASE
								WHEN (TaxBalance / CorporationTaxRate) < 0 THEN ABS(TaxBalance / CorporationTaxRate)
								ELSE 0
							END
					END
			FROM corptax_by_year
		) x
	),

	-----------------------------------------------------------------
	-- Capital injection: derive applicable cash codes dynamically by
	-- inspecting the first posted transaction direction.
	-----------------------------------------------------------------
	capital_cash_codes AS
	(
		SELECT DISTINCT cc.CashCode
		FROM Cash.tbCode cc
			JOIN Cash.tbCategory cat
				ON cat.CategoryCode = cc.CategoryCode
			JOIN Subject.tbAccount acc
				ON acc.CashCode = cc.CashCode
		WHERE cat.CashTypeCode = 2
		  AND acc.AccountTypeCode = 2
		  AND acc.AccountClosed = 0
	),
	capital_injection_cash_codes AS
	(
		SELECT CashCode
		FROM
		(
			SELECT
				p.CashCode,
				FirstPaidOutValue =
					FIRST_VALUE(COALESCE(p.PaidOutValue, 0))
					OVER (PARTITION BY p.CashCode ORDER BY p.PaidOn, p.PaymentCode)
			FROM Cash.tbPayment p
				JOIN capital_cash_codes cc
					ON cc.CashCode = p.CashCode
			WHERE p.PaymentStatusCode = 1
		) first_payments
		GROUP BY CashCode
		HAVING SUM(FirstPaidOutValue) > 0
	),
	capital_injection_by_year AS
	(
		SELECT
			yp.YearNumber,
			CapitalInjection = SUM(COALESCE(p.PaidInValue, 0) - COALESCE(p.PaidOutValue, 0))
		FROM Cash.tbPayment p
			JOIN capital_injection_cash_codes cicc
				ON cicc.CashCode = p.CashCode
			JOIN App.tbYearPeriod yp
				ON yp.StartOn =
					(SELECT TOP (1) StartOn
					 FROM App.tbYearPeriod
					 WHERE StartOn <= p.PaidOn
					 ORDER BY StartOn DESC)
			JOIN App.tbYear y ON yp.YearNumber = y.YearNumber
		WHERE y.CashStatusCode BETWEEN 1 AND 2
		  AND p.PaymentStatusCode = 1
		GROUP BY yp.YearNumber
	)
	SELECT
		y.YearNumber,
		y.Description,
		OpeningCapital = ROUND(COALESCE(b.OpeningCapital, (SELECT Capital FROM opening_capital)), 2),
		ClosingCapital = ROUND(b.ClosingCapital, 2),
		Profit = ROUND(p.Profit, 2),

		CorporationTax = ROUND(COALESCE(ct.CorporationTaxExpense, 0), 2),
		ProfitAfterTax = ROUND(COALESCE(p.Profit, 0) - COALESCE(ct.CorporationTaxExpense, 0), 2),

		TaxCarry = ROUND(COALESCE(ct.TaxCarry, 0), 2),

		CapitalInjection = COALESCE(ci.CapitalInjection, 0),
		OpeningPosition =
			CASE WHEN b.OpeningCapital IS NULL THEN COALESCE((SELECT OpeningPosition FROM opening_position), 0) ELSE 0 END,

		OpeningLossesCarriedForward = ROUND(COALESCE(lcfd.OpeningLossesCarriedForward, 0), 2),
		ClosingLossesCarriedForward = ROUND(COALESCE(lcfd.ClosingLossesCarriedForward, 0), 2),
		LossesCarriedForwardDelta =
			ROUND(COALESCE(lcfd.ClosingLossesCarriedForward, 0) - COALESCE(lcfd.OpeningLossesCarriedForward, 0), 2),

		CapitalDelta = ROUND(b.ClosingCapital - COALESCE(b.OpeningCapital, (SELECT Capital FROM opening_capital)), 2),
		Difference =
			ROUND(
				(b.ClosingCapital - COALESCE(b.OpeningCapital, (SELECT Capital FROM opening_capital)))
				- (
					(COALESCE(p.Profit, 0) - COALESCE(ct.CorporationTaxExpense, 0))
					+ COALESCE(ci.CapitalInjection, 0)
					+ CASE WHEN b.OpeningCapital IS NULL THEN COALESCE((SELECT OpeningPosition FROM opening_position), 0) ELSE 0 END
				),
				2
			)
	FROM App.tbYear y
		JOIN balances b ON y.YearNumber = b.YearNumber
		LEFT JOIN profit_by_year p ON y.YearNumber = p.YearNumber
		LEFT JOIN corptax_by_year ct ON y.YearNumber = ct.YearNumber
		LEFT JOIN capital_injection_by_year ci ON y.YearNumber = ci.YearNumber
		LEFT JOIN loss_cf_delta lcfd ON y.YearNumber = lcfd.YearNumber
	WHERE y.CashStatusCode BETWEEN 1 AND 2;
