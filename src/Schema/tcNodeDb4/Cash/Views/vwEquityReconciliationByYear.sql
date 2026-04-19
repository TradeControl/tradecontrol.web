CREATE VIEW [Cash].[vwEquityReconciliationByYear]
AS
WITH first_period AS (
    SELECT MIN(yp.StartOn) AS StartOn
    FROM App.tbYearPeriod yp
    JOIN App.tbYear y ON yp.YearNumber = y.YearNumber
    WHERE y.CashStatusCode BETWEEN 1 AND 2
),
opening_position AS (
    SELECT OpeningPosition = -SUM(s.OpeningBalance)
    FROM Subject.tbSubject s
    WHERE s.OpeningBalance <> 0
),
opening_account_position AS (
    SELECT OpeningAccountPosition = SUM(a.OpeningBalance)
    FROM Subject.tbAccount a
    WHERE a.AccountTypeCode = 0
      AND a.AccountClosed = 0
      AND a.OpeningBalance <> 0
),
opening_capital AS (
    SELECT SUM(bs.Balance) AS Capital
    FROM Cash.vwBalanceSheet bs
    JOIN first_period fp ON bs.StartOn = fp.StartOn
),
year_bounds AS (
    SELECT yp.YearNumber, MAX(yp.StartOn) AS YearEndOn
    FROM App.tbYearPeriod yp
    JOIN App.tbYear y ON yp.YearNumber = y.YearNumber
    WHERE y.CashStatusCode BETWEEN 1 AND 2
    GROUP BY yp.YearNumber
),
closing_capital AS (
    SELECT b.YearNumber, b.YearEndOn AS StartOn,
           SUM(bs.Balance) AS Capital
    FROM year_bounds b
    JOIN Cash.vwBalanceSheet bs ON bs.StartOn = b.YearEndOn
    GROUP BY b.YearNumber, b.YearEndOn
),
balances AS (
    SELECT cc.YearNumber,
           OpeningCapital = LAG(cc.Capital) OVER (ORDER BY cc.StartOn),
           ClosingCapital = cc.Capital
    FROM closing_capital cc
),
profit_by_year AS (
    SELECT pl.YearNumber, Profit = SUM(pl.InvoiceValue)
    FROM Cash.vwProfitAndLossByYear pl
    GROUP BY pl.YearNumber
),

-----------------------------------------------------------------
-- Business tax (unified)
-----------------------------------------------------------------
biztax_due_dates AS (
    SELECT PayOn, PayFrom, PayTo
    FROM Cash.fnTaxTypeDueDates(0, 0)
),
biztax_year_end AS (
    SELECT yb.YearNumber, yb.YearEndOn,
           BizTaxPayOn = (
               SELECT TOP (1) dd.PayOn
               FROM biztax_due_dates dd
               WHERE yb.YearEndOn >= dd.PayFrom
                 AND yb.YearEndOn < dd.PayTo
               ORDER BY dd.PayOn
           )
    FROM year_bounds yb
),
biztax_stmt AS (
    SELECT ye.YearNumber, ye.BizTaxPayOn,
           TaxDue = COALESCE(st.TaxDue, 0),
           TaxBalance = COALESCE(st.Balance, 0),
           BusinessTaxRate = COALESCE(yp.BusinessTaxRate, 0)
    FROM biztax_year_end ye
    LEFT JOIN Cash.vwTaxBizStatement st ON st.StartOn = ye.BizTaxPayOn
    LEFT JOIN App.tbYearPeriod yp ON yp.StartOn = ye.BizTaxPayOn
),
biztax_by_year AS (
    SELECT YearNumber,
           BusinessTaxExpense = SUM(CASE WHEN TaxDue > 0 THEN TaxDue ELSE 0 END),
           TaxCarry = SUM(CASE WHEN TaxDue < 0 THEN TaxDue ELSE 0 END),
           TaxBalance = SUM(TaxBalance),
           BusinessTaxRate = MAX(BusinessTaxRate)
    FROM biztax_stmt
    GROUP BY YearNumber
),

loss_cf_delta AS (
    SELECT YearNumber,
           OpeningLossesCarriedForward = LAG(LossesCarriedForward) OVER (ORDER BY YearNumber),
           ClosingLossesCarriedForward = LossesCarriedForward
    FROM (
        SELECT YearNumber,
               LossesCarriedForward =
                   CASE WHEN BusinessTaxRate = 0 THEN 0
                        WHEN (TaxBalance / BusinessTaxRate) < 0
                             THEN ABS(TaxBalance / BusinessTaxRate)
                        ELSE 0 END
        FROM biztax_by_year
    ) x
),

-----------------------------------------------------------------
-- CapitalMovement (balance-based, invariant-preserving)
-----------------------------------------------------------------
capital_position_by_year AS (
    SELECT pe.YearNumber,
           CapitalPosition = SUM(ba.Balance)
    FROM (
        SELECT yb.YearNumber, yb.YearEndOn AS StartOn
        FROM year_bounds yb
    ) pe
    JOIN Cash.vwBalanceSheetAssets ba ON ba.StartOn = pe.StartOn
    JOIN Subject.tbAccount acc ON acc.AccountCode = ba.AssetCode
    WHERE acc.BalanceConstraintCode = 2
    GROUP BY pe.YearNumber
),
capital_movement_by_year AS (
    SELECT YearNumber,
           CapitalMovement = COALESCE(CapitalPosition, 0)
                           - COALESCE(LAG(CapitalPosition) OVER (ORDER BY YearNumber), 0)
    FROM capital_position_by_year
),

-----------------------------------------------------------------
-- Main reconciliation
-----------------------------------------------------------------
recon AS (
    SELECT
        y.YearNumber,
        y.Description,

        OpeningCapital =
            CASE WHEN b.OpeningCapital IS NULL THEN 0
                 ELSE ROUND(b.OpeningCapital, 2)
            END,

        ClosingCapital = ROUND(b.ClosingCapital, 2),
        Profit = ROUND(p.Profit, 2),
        BusinessTax = ROUND(COALESCE(ct.BusinessTaxExpense, 0), 2),
        ProfitAfterTax = ROUND(COALESCE(p.Profit, 0)
                               - COALESCE(ct.BusinessTaxExpense, 0), 2),
        TaxCarry = ROUND(COALESCE(ct.TaxCarry, 0), 2),

        OpeningPosition =
            CASE WHEN b.OpeningCapital IS NULL
                 THEN COALESCE((SELECT OpeningPosition FROM opening_position), 0)
                 ELSE 0 END,

        OpeningAccountPosition =
            CASE WHEN b.OpeningCapital IS NULL
                 THEN COALESCE((SELECT OpeningAccountPosition FROM opening_account_position), 0)
                 ELSE 0 END,

        OpeningLossesCarriedForward =
            ROUND(COALESCE(lcfd.OpeningLossesCarriedForward, 0), 2),

        ClosingLossesCarriedForward =
            ROUND(COALESCE(lcfd.ClosingLossesCarriedForward, 0), 2),

        LossesCarriedForwardDelta =
            ROUND(COALESCE(lcfd.ClosingLossesCarriedForward, 0)
                - COALESCE(lcfd.OpeningLossesCarriedForward, 0), 2),

        CapitalDelta =
            ROUND(b.ClosingCapital
                - CASE WHEN b.OpeningCapital IS NULL THEN 0
                       ELSE b.OpeningCapital END, 2),

        CapitalMovement = COALESCE(cm.CapitalMovement, 0)
    FROM App.tbYear y
    JOIN balances b ON y.YearNumber = b.YearNumber
    LEFT JOIN profit_by_year p ON y.YearNumber = p.YearNumber
    LEFT JOIN biztax_by_year ct ON y.YearNumber = ct.YearNumber
    LEFT JOIN loss_cf_delta lcfd ON y.YearNumber = lcfd.YearNumber
    LEFT JOIN capital_movement_by_year cm ON y.YearNumber = cm.YearNumber
    WHERE y.CashStatusCode BETWEEN 1 AND 2
)

SELECT
    r.YearNumber,
    r.Description,
    OpeningCapital = CONVERT(decimal(38,5), r.OpeningCapital),
    ClosingCapital = CONVERT(decimal(38,5), r.ClosingCapital),
    r.Profit,
    r.BusinessTax,
    r.ProfitAfterTax,
    r.TaxCarry,
    CapitalMovement = CONVERT(decimal(38,5), r.CapitalMovement),
    r.OpeningPosition,
    r.OpeningAccountPosition,
    r.OpeningLossesCarriedForward,
    r.ClosingLossesCarriedForward,
    r.LossesCarriedForwardDelta,
    CapitalDelta = CONVERT(decimal(38,5), r.CapitalDelta),

    Variance = CONVERT(decimal(38,5),
        r.CapitalDelta
      - (r.ProfitAfterTax
       +  r.CapitalMovement
       +  r.OpeningPosition
       +  r.OpeningAccountPosition)
    )
FROM recon r;
