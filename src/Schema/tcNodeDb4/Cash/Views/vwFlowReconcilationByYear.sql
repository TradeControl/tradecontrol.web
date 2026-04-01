CREATE VIEW Cash.vwFlowReconciliationByYear
AS
	WITH base AS
	(
		SELECT
			YearNumber,
			Description,
			OpeningCapital,
			ClosingCapital,
			Profit,
			BusinessTax,
			ProfitAfterTax,
			TaxCarry,
			CapitalInjection,
			OpeningPosition,
			OpeningLossesCarriedForward,
			ClosingLossesCarriedForward,
			LossesCarriedForwardDelta,
			CapitalDelta,
			Difference
		FROM Cash.vwEquityReconciliationByYear
	)
	SELECT
		YearNumber,
		Description,
		LineNumber = 10,
		LineCode = N'OPEN_CAP',
		LineName = N'Opening capital',
		Amount = OpeningCapital
	FROM base

	UNION ALL SELECT YearNumber, Description, 20, N'PROFIT',    N'Profit',                 Profit FROM base
	UNION ALL SELECT YearNumber, Description, 30, N'TAX_EXP',   N'Business tax (P&L)',  BusinessTax FROM base
	UNION ALL SELECT YearNumber, Description, 40, N'PROF_AT',   N'Profit after tax',       ProfitAfterTax FROM base
	UNION ALL SELECT YearNumber, Description, 50, N'CAP_INJ',   N'Capital injection',      CapitalInjection FROM base
	UNION ALL SELECT YearNumber, Description, 60, N'OPEN_POS',  N'Opening position',       OpeningPosition FROM base

	UNION ALL SELECT
		YearNumber,
		Description,
		70,
		N'BRIDGE',
		N'Bridge total (Profit after tax + Capital injection + Opening position)',
		(ProfitAfterTax + CapitalInjection + OpeningPosition)
	FROM base

	UNION ALL SELECT YearNumber, Description, 80, N'CAP_DELTA', N'Capital delta (Close - Open)', CapitalDelta FROM base
	UNION ALL SELECT YearNumber, Description, 90, N'DIFF',      N'Difference',                  Difference FROM base

	UNION ALL SELECT YearNumber, Description, 100, N'TAX_CARRY', N'Tax carry (statement)', TaxCarry FROM base
	UNION ALL SELECT YearNumber, Description, 110, N'LOSS_CF_O', N'Losses carried forward (opening)', OpeningLossesCarriedForward FROM base
	UNION ALL SELECT YearNumber, Description, 120, N'LOSS_CF_C', N'Losses carried forward (closing)', ClosingLossesCarriedForward FROM base
	UNION ALL SELECT YearNumber, Description, 130, N'LOSS_CF_D', N'Losses carried forward (delta)',   LossesCarriedForwardDelta FROM base;
