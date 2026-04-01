CREATE VIEW Cash.vwSoleTraderReconciliationByYear
AS
	WITH bank_accounts AS
	(
		SELECT
			CurrentAccountCode = (SELECT AccountCode FROM Cash.vwCurrentAccount),
			ReserveAccountCode = (SELECT AccountCode FROM Cash.vwReserveAccount)
	),
	first_period AS
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
	snap_net_assets AS
	(
		SELECT
			b.YearNumber,
			b.YearEndOn AS StartOn,
			NetAssets =
				SUM(CASE WHEN bs.CashPolarityCode = 1 THEN bs.Balance ELSE 0 END)
				- SUM(CASE WHEN bs.CashPolarityCode = 0 THEN bs.Balance ELSE 0 END)
		FROM year_bounds b
		JOIN Cash.vwBalanceSheet bs ON bs.StartOn = b.YearEndOn
		GROUP BY b.YearNumber, b.YearEndOn
	),
	snap_cash_at_bank AS
	(
		SELECT
			b.YearNumber,
			b.YearEndOn AS StartOn,
			CashAtBank =
				SUM(
					CASE
						WHEN bs.CashPolarityCode = 1
						 AND (bs.AssetCode = ba.CurrentAccountCode OR bs.AssetCode = ba.ReserveAccountCode)
						THEN bs.Balance
						ELSE 0
					END
				)
		FROM year_bounds b
		JOIN Cash.vwBalanceSheet bs ON bs.StartOn = b.YearEndOn
		CROSS JOIN bank_accounts ba
		GROUP BY b.YearNumber, b.YearEndOn
	),
	opening_net_assets AS
	(
		SELECT
			NetAssets =
				SUM(CASE WHEN bs.CashPolarityCode = 1 THEN bs.Balance ELSE 0 END)
				- SUM(CASE WHEN bs.CashPolarityCode = 0 THEN bs.Balance ELSE 0 END)
		FROM Cash.vwBalanceSheet bs
		JOIN first_period fp ON bs.StartOn = fp.StartOn
	),
	opening_cash_at_bank AS
	(
		SELECT
			CashAtBank =
				SUM(
					CASE
						WHEN bs.CashPolarityCode = 1
						 AND (bs.AssetCode = ba.CurrentAccountCode OR bs.AssetCode = ba.ReserveAccountCode)
						THEN bs.Balance
						ELSE 0
					END
				)
		FROM Cash.vwBalanceSheet bs
		JOIN first_period fp ON bs.StartOn = fp.StartOn
		CROSS JOIN bank_accounts ba
	),
	balances AS
	(
		SELECT
			na.YearNumber,
			OpeningCapital = LAG(na.NetAssets) OVER (ORDER BY na.StartOn),
			ClosingCapital = na.NetAssets,

			OpeningCashAtBank = LAG(cb.CashAtBank) OVER (ORDER BY cb.StartOn),
			ClosingCashAtBank = cb.CashAtBank
		FROM snap_net_assets na
		JOIN snap_cash_at_bank cb
			ON na.YearNumber = cb.YearNumber
		   AND na.StartOn = cb.StartOn
	),
	profit_by_year AS
	(
		SELECT
			pl.YearNumber,
			Profit = SUM(pl.InvoiceValue)
		FROM Cash.vwProfitAndLossByYear pl
		GROUP BY pl.YearNumber
	),
	owner_movements_by_year AS
	(
		SELECT
			yp.YearNumber,
			OwnerIntroduced = SUM(CASE WHEN p.CashCode = N'CAPIN01' THEN (COALESCE(p.PaidInValue, 0) - COALESCE(p.PaidOutValue, 0)) ELSE 0 END),
			OwnerDrawings = SUM(CASE WHEN p.CashCode = N'DRAW01' THEN (COALESCE(p.PaidOutValue, 0) - COALESCE(p.PaidInValue, 0)) ELSE 0 END)
		FROM Cash.tbPayment p
			JOIN App.tbYearPeriod yp
				ON yp.StartOn =
					(SELECT TOP (1) StartOn
					 FROM App.tbYearPeriod
					 WHERE StartOn <= p.PaidOn
					 ORDER BY StartOn DESC)
			JOIN App.tbYear y ON yp.YearNumber = y.YearNumber
		WHERE y.CashStatusCode BETWEEN 1 AND 2
		  AND p.PaymentStatusCode = 1
		  AND p.CashCode IN (N'CAPIN01', N'DRAW01')
		GROUP BY yp.YearNumber
	),
	raw AS
	(
		SELECT
			y.YearNumber,
			y.Description,

			OpeningCapital = ROUND(COALESCE(b.OpeningCapital, (SELECT NetAssets FROM opening_net_assets)), 2),
			ClosingCapital = ROUND(b.ClosingCapital, 2),

			OpeningCashAtBank = ROUND(COALESCE(b.OpeningCashAtBank, (SELECT CashAtBank FROM opening_cash_at_bank)), 2),
			ClosingCashAtBank = ROUND(b.ClosingCashAtBank, 2),

			Profit = ROUND(COALESCE(p.Profit, 0), 2),
			OwnerIntroduced = ROUND(COALESCE(om.OwnerIntroduced, 0), 2),
			OwnerDrawings = ROUND(COALESCE(om.OwnerDrawings, 0), 2),

			OpeningPosition =
				CASE WHEN b.OpeningCapital IS NULL THEN COALESCE((SELECT OpeningPosition FROM opening_position), 0) ELSE 0 END
		FROM App.tbYear y
			JOIN balances b ON y.YearNumber = b.YearNumber
			LEFT JOIN profit_by_year p ON y.YearNumber = p.YearNumber
			LEFT JOIN owner_movements_by_year om ON y.YearNumber = om.YearNumber
		WHERE y.CashStatusCode BETWEEN 1 AND 2
	),
	calc AS
	(
		SELECT
			r.*,
			CapitalDelta = ROUND(r.ClosingCapital - r.OpeningCapital, 2),
			CashAtBankDelta = ROUND(r.ClosingCashAtBank - r.OpeningCashAtBank, 2),

			Bridge =
				ROUND(
					r.Profit
					+ r.OwnerIntroduced
					- r.OwnerDrawings
					+ r.OpeningPosition,
					2
				)
		FROM raw r
	)
	SELECT
		c.YearNumber,
		c.Description,

		c.OpeningCapital,
		c.ClosingCapital,
		c.CapitalDelta,

		c.Profit,
		c.OwnerIntroduced,
		c.OwnerDrawings,
		c.OpeningPosition,

		c.OpeningCashAtBank,
		c.ClosingCashAtBank,
		c.CashAtBankDelta,

		-- Balancing item: change in net assets not explained by profit/owner movements.
		WorkingCapitalDelta = ROUND(c.CapitalDelta - c.Bridge, 2),

		-- Residual after rounding each term to 2dp (should be pennies).
		Difference =
			ROUND(
				c.CapitalDelta
				- (
					c.Bridge
					+ ROUND(c.CapitalDelta - c.Bridge, 2)
				),
				2
			)
	FROM calc c;
