SET NOCOUNT ON;
SET XACT_ABORT ON;

------------------------------------------------------------------------------
-- PROOF_CashStatementReconciliation.sql
--
-- Purpose:
--   Provide machine-checkable proofs (invariants) demonstrating that the Cash
--   Statement reconciliation is mathematically consistent with the annual
--   equity bridge and that residuals are rounding-only.
--
-- Systems note:
--   Trade Control is a production/flow system; accounting is derived from
--   genesis transactions and cash polarity. This script validates the DEBK-
--   style invariants presented by the reporting layer.
------------------------------------------------------------------------------

DECLARE @Tolerance decimal(18, 4) = 0.10; -- acceptable rounding tolerance in base currency units
DECLARE @ShowTop int = 25;

------------------------------------------------------------------------------
-- Base dataset
------------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#Reconciliation') IS NOT NULL DROP TABLE #Reconciliation;

SELECT
    r.YearNumber,
    r.[Description],
    r.OpeningCapital,
    r.ClosingCapital,
    r.Profit,
    r.CorporationTax,
    r.ProfitAfterTax,
    r.TaxCarry,
    r.CapitalInjection,
    r.OpeningPosition,
    r.OpeningLossesCarriedForward,
    r.ClosingLossesCarriedForward,
    r.LossesCarriedForwardDelta,
    r.CapitalDelta,
    r.Difference
INTO #Reconciliation
FROM Cash.vwEquityReconciliationByYear r;

CREATE UNIQUE CLUSTERED INDEX IX_Reconciliation_YearNumber ON #Reconciliation(YearNumber);

------------------------------------------------------------------------------
-- Invariant checks (annual)
------------------------------------------------------------------------------

;WITH calc AS
(
    SELECT
        YearNumber,
        [Description],

        -- Identity 1: CapitalDelta must equal Closing - Opening (pre-round)
        CapitalDelta_Definition = (ClosingCapital - OpeningCapital),
        CapitalDelta_Reported = CapitalDelta,
        CapitalDelta_Error = (CapitalDelta - (ClosingCapital - OpeningCapital)),

        -- Identity 2: ProfitAfterTax must equal Profit - CorporationTax
        ProfitAfterTax_Definition = (Profit - CorporationTax),
        ProfitAfterTax_Reported = ProfitAfterTax,
        ProfitAfterTax_Error = (ProfitAfterTax - (Profit - CorporationTax)),

        -- Identity 3: DEBK-style equity bridge residual
        BridgeTotal = (ProfitAfterTax + CapitalInjection + OpeningPosition),
        Residual_Definition = (CapitalDelta - (ProfitAfterTax + CapitalInjection + OpeningPosition)),
        Residual_Reported = Difference,
        Residual_Error = (Difference - (CapitalDelta - (ProfitAfterTax + CapitalInjection + OpeningPosition))),

        -- Loss pool shape checks (not DEBK identities; proof of consistency/communication)
        LossCF_Sign_Bad = CASE WHEN ClosingLossesCarriedForward < 0 THEN 1 ELSE 0 END,
        LossCF_Delta_Definition = (ClosingLossesCarriedForward - OpeningLossesCarriedForward),
        LossCF_Delta_Error = (LossesCarriedForwardDelta - (ClosingLossesCarriedForward - OpeningLossesCarriedForward))
    FROM #Reconciliation
),
summary AS
(
    SELECT
        MaxAbs_CapitalDelta_Error = MAX(ABS(CapitalDelta_Error)),
        MaxAbs_ProfitAfterTax_Error = MAX(ABS(ProfitAfterTax_Error)),
        MaxAbs_Residual_Error = MAX(ABS(Residual_Error)),
        MaxAbs_Difference = MAX(ABS(Residual_Definition)),
        MaxAbs_LossCF_Delta_Error = MAX(ABS(LossCF_Delta_Error)),
        LossCF_Negative_Count = SUM(LossCF_Sign_Bad),
        YearCount = COUNT(*)
    FROM calc
)
SELECT
    ProofName = 'Cash Statement / Equity Reconciliation Proofs',
    Tolerance = @Tolerance,
    YearCount = s.YearCount,

    CapitalDelta_Definition_MaxAbsError = s.MaxAbs_CapitalDelta_Error,
    ProfitAfterTax_Definition_MaxAbsError = s.MaxAbs_ProfitAfterTax_Error,
    Difference_Definition_MaxAbsError = s.MaxAbs_Residual_Error,
    Difference_MaxAbs = s.MaxAbs_Difference,

    LossesCarriedForwardDelta_Definition_MaxAbsError = s.MaxAbs_LossCF_Delta_Error,
    LossesCarriedForward_NegativeCount = s.LossCF_Negative_Count,

    Status =
        CASE
            WHEN s.MaxAbs_CapitalDelta_Error > @Tolerance THEN 'FAIL'
            WHEN s.MaxAbs_ProfitAfterTax_Error > @Tolerance THEN 'FAIL'
            WHEN s.MaxAbs_Residual_Error > @Tolerance THEN 'FAIL'
            WHEN s.MaxAbs_Difference > @Tolerance THEN 'WARN' -- identity holds, residual too big for rounding
            WHEN s.MaxAbs_LossCF_Delta_Error > @Tolerance THEN 'FAIL'
            WHEN s.LossCF_Negative_Count > 0 THEN 'FAIL'
            ELSE 'PASS'
        END
FROM summary s;

------------------------------------------------------------------------------
-- Detailed offenders (top N), only when something is out-of-tolerance
------------------------------------------------------------------------------

;WITH calc AS
(
    SELECT
        YearNumber,
        [Description],

        OpeningCapital,
        ClosingCapital,
        Profit,
        CorporationTax,
        ProfitAfterTax,
        CapitalInjection,
        OpeningPosition,
        CapitalDelta,
        Difference,

        CapitalDelta_Definition = (ClosingCapital - OpeningCapital),
        ProfitAfterTax_Definition = (Profit - CorporationTax),
        BridgeTotal = (ProfitAfterTax + CapitalInjection + OpeningPosition),
        Residual_Definition = (CapitalDelta - (ProfitAfterTax + CapitalInjection + OpeningPosition)),

        CapitalDelta_Error = (CapitalDelta - (ClosingCapital - OpeningCapital)),
        ProfitAfterTax_Error = (ProfitAfterTax - (Profit - CorporationTax)),
        Residual_Error = (Difference - (CapitalDelta - (ProfitAfterTax + CapitalInjection + OpeningPosition))),

        OpeningLossesCarriedForward,
        ClosingLossesCarriedForward,
        LossesCarriedForwardDelta,
        LossCF_Delta_Definition = (ClosingLossesCarriedForward - OpeningLossesCarriedForward),
        LossCF_Delta_Error = (LossesCarriedForwardDelta - (ClosingLossesCarriedForward - OpeningLossesCarriedForward))
    FROM #Reconciliation
),
offenders AS
(
    SELECT
        *,
        WorstAbs =
            (SELECT MAX(v)
             FROM (VALUES
                 (ABS(CapitalDelta_Error)),
                 (ABS(ProfitAfterTax_Error)),
                 (ABS(Residual_Error)),
                 (ABS(Residual_Definition)),
                 (ABS(LossCF_Delta_Error))
             ) x(v))
    FROM calc
    WHERE ABS(CapitalDelta_Error) > @Tolerance
       OR ABS(ProfitAfterTax_Error) > @Tolerance
       OR ABS(Residual_Error) > @Tolerance
       OR ABS(Residual_Definition) > @Tolerance
       OR ABS(LossCF_Delta_Error) > @Tolerance
       OR ClosingLossesCarriedForward < 0
)
SELECT TOP (@ShowTop)
    YearNumber,
    [Description],

    OpeningCapital,
    ClosingCapital,
    CapitalDelta,
    CapitalDelta_Definition,
    CapitalDelta_Error,

    Profit,
    CorporationTax,
    ProfitAfterTax,
    ProfitAfterTax_Definition,
    ProfitAfterTax_Error,

    CapitalInjection,
    OpeningPosition,
    BridgeTotal,
    Difference,
    Residual_Definition,
    Residual_Error,

    OpeningLossesCarriedForward,
    ClosingLossesCarriedForward,
    LossesCarriedForwardDelta,
    LossCF_Delta_Definition,
    LossCF_Delta_Error,

    WorstAbs
FROM offenders
ORDER BY WorstAbs DESC, YearNumber;

------------------------------------------------------------------------------
-- Proof: Flow view is a pure projection of equity reconciliation
-- (sanity check of row set shape per year)
------------------------------------------------------------------------------

;WITH per_year AS
(
    SELECT YearNumber, LineCount = COUNT(*)
    FROM Cash.vwFlowReconciliationByYear
    GROUP BY YearNumber
)
SELECT
    ProofName = 'FlowReconciliation row-shape check',
    ExpectedLinesPerYear = 13,
    YearsWithWrongLineCount = SUM(CASE WHEN LineCount <> 13 THEN 1 ELSE 0 END),
    MinLinesPerYear = MIN(LineCount),
    MaxLinesPerYear = MAX(LineCount),
    Status = CASE WHEN SUM(CASE WHEN LineCount <> 13 THEN 1 ELSE 0 END) = 0 THEN 'PASS' ELSE 'FAIL' END
FROM per_year;
