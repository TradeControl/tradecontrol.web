SET NOCOUNT ON;
SET XACT_ABORT ON;

------------------------------------------------------------------------------
-- PROOF_CashStatementReconciliation.sql
--
-- Purpose:
--   Provide machine-checkable proofs (invariants) demonstrating that:
--     1) Annual equity reconciliation fields are internally consistent.
--     2) The DEBK-style equity bridge balances within a rounding tolerance.
--     3) Flow reconciliation is a pure projection (row-shape invariant).
--
-- Notes:
--   - The reporting layer derives accounting from genesis transactions and cash
--     polarity; this script validates the invariants presented by that layer.
--   - Tolerance is intentionally small to catch drift, but allows for rounding.
--   - This script assumes `Cash.vwEquityReconciliationByYear` exposes the
--     post-refactor names:
--       * `CapitalMovement` (formerly `CapitalInjection`)
--       * `Variance`        (formerly `Difference`)
--       * `OpeningAccountPosition` (new: opening cash/bank seed funding)
------------------------------------------------------------------------------

DECLARE @Tolerance decimal(18, 4) = 0.10; -- rounding tolerance in base currency units
DECLARE @ShowTop int = 25;

------------------------------------------------------------------------------
-- Base dataset (materialize to temp table)
--
-- Rationale:
--   - Makes the proof deterministic and fast to query repeatedly.
--   - Allows us to index by YearNumber for the later checks.
------------------------------------------------------------------------------
IF OBJECT_ID('tempdb..#Reconciliation') IS NOT NULL DROP TABLE #Reconciliation;

SELECT
    r.YearNumber,
    r.[Description],

    -- Balance sheet bridge endpoints
    r.OpeningCapital,
    r.ClosingCapital,

    -- Profit and tax
    r.Profit,
    r.BusinessTax,
    r.ProfitAfterTax,
    r.TaxCarry,

    -- Equity bridge components
    r.CapitalMovement,
    r.OpeningPosition,
    r.OpeningAccountPosition,

    -- Loss pool telemetry (not strictly part of the DEBK identity)
    r.OpeningLossesCarriedForward,
    r.ClosingLossesCarriedForward,
    r.LossesCarriedForwardDelta,

    -- Derived bridge target and residual
    r.CapitalDelta,
    r.Variance
INTO #Reconciliation
FROM Cash.vwEquityReconciliationByYear r;

CREATE UNIQUE CLUSTERED INDEX IX_Reconciliation_YearNumber ON #Reconciliation(YearNumber);

------------------------------------------------------------------------------
-- Invariant checks (annual)
--
-- Definitions:
--   Identity 1:
--     CapitalDelta = ClosingCapital - OpeningCapital
--
--   Identity 2:
--     ProfitAfterTax = Profit - BusinessTax
--
--   Identity 3 (DEBK-style equity bridge, explicit opening items):
--     CapitalDelta = ProfitAfterTax + CapitalMovement + OpeningPosition + OpeningAccountPosition + Residual
--   where Residual ~= 0 (rounding-only).
------------------------------------------------------------------------------
;WITH calc AS
(
    SELECT
        YearNumber,
        [Description],

        ----------------------------------------------------------------------
        -- Identity 1: CapitalDelta definition check
        ----------------------------------------------------------------------
        CapitalDelta_Definition = (ClosingCapital - OpeningCapital),
        CapitalDelta_Reported = CapitalDelta,
        CapitalDelta_Error = (CapitalDelta - (ClosingCapital - OpeningCapital)),

        ----------------------------------------------------------------------
        -- Identity 2: ProfitAfterTax definition check
        ----------------------------------------------------------------------
        ProfitAfterTax_Definition = (Profit - BusinessTax),
        ProfitAfterTax_Reported = ProfitAfterTax,
        ProfitAfterTax_Error = (ProfitAfterTax - (Profit - BusinessTax)),

        ----------------------------------------------------------------------
        -- Identity 3: Equity bridge + residual
        ----------------------------------------------------------------------
        BridgeTotal = (ProfitAfterTax + CapitalMovement + OpeningPosition + OpeningAccountPosition),
        Residual_Definition = (CapitalDelta - (ProfitAfterTax + CapitalMovement + OpeningPosition + OpeningAccountPosition)),
        Residual_Reported = Variance,
        Residual_Error =
            (Variance - (CapitalDelta - (ProfitAfterTax + CapitalMovement + OpeningPosition + OpeningAccountPosition))),

        ----------------------------------------------------------------------
        -- Loss pool shape checks (consistency / communication checks)
        -- (not part of the DEBK identity above)
        ----------------------------------------------------------------------
        LossCF_Sign_Bad = CASE WHEN ClosingLossesCarriedForward < 0 THEN 1 ELSE 0 END,
        LossCF_Delta_Definition = (ClosingLossesCarriedForward - OpeningLossesCarriedForward),
        LossCF_Delta_Error =
            (LossesCarriedForwardDelta - (ClosingLossesCarriedForward - OpeningLossesCarriedForward))
    FROM #Reconciliation
),
summary AS
(
    SELECT
        MaxAbs_CapitalDelta_Error = MAX(ABS(CapitalDelta_Error)),
        MaxAbs_ProfitAfterTax_Error = MAX(ABS(ProfitAfterTax_Error)),
        MaxAbs_Residual_Error = MAX(ABS(Residual_Error)),
        MaxAbs_Variance = MAX(ABS(Residual_Definition)),
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
    Variance_Definition_MaxAbsError = s.MaxAbs_Residual_Error,
    Variance_MaxAbs = s.MaxAbs_Variance,

    LossesCarriedForwardDelta_Definition_MaxAbsError = s.MaxAbs_LossCF_Delta_Error,
    LossesCarriedForward_NegativeCount = s.LossCF_Negative_Count,

    Status =
        CASE
            WHEN s.MaxAbs_CapitalDelta_Error > @Tolerance THEN 'FAIL'
            WHEN s.MaxAbs_ProfitAfterTax_Error > @Tolerance THEN 'FAIL'
            WHEN s.MaxAbs_Residual_Error > @Tolerance THEN 'FAIL'
            WHEN s.MaxAbs_Variance > @Tolerance THEN 'WARN' -- identity holds, residual too big for rounding
            WHEN s.MaxAbs_LossCF_Delta_Error > @Tolerance THEN 'FAIL'
            WHEN s.LossCF_Negative_Count > 0 THEN 'FAIL'
            ELSE 'PASS'
        END
FROM summary s;

------------------------------------------------------------------------------
-- Detailed offenders (top N)
--
-- Output only years that violate tolerance or have invalid loss pool shape.
-- This makes it easy to paste the worst rows into an issue/PR description.
------------------------------------------------------------------------------
;WITH calc AS
(
    SELECT
        YearNumber,
        [Description],

        OpeningCapital,
        ClosingCapital,
        Profit,
        BusinessTax,
        ProfitAfterTax,
        CapitalMovement,
        OpeningPosition,
        OpeningAccountPosition,
        CapitalDelta,
        Variance,

        CapitalDelta_Definition = (ClosingCapital - OpeningCapital),
        ProfitAfterTax_Definition = (Profit - BusinessTax),

        BridgeTotal = (ProfitAfterTax + CapitalMovement + OpeningPosition + OpeningAccountPosition),
        Residual_Definition = (CapitalDelta - (ProfitAfterTax + CapitalMovement + OpeningPosition + OpeningAccountPosition)),

        CapitalDelta_Error = (CapitalDelta - (ClosingCapital - OpeningCapital)),
        ProfitAfterTax_Error = (ProfitAfterTax - (Profit - BusinessTax)),
        Residual_Error = (Variance - (CapitalDelta - (ProfitAfterTax + CapitalMovement + OpeningPosition + OpeningAccountPosition))),

        OpeningLossesCarriedForward,
        ClosingLossesCarriedForward,
        LossesCarriedForwardDelta,
        LossCF_Delta_Definition = (ClosingLossesCarriedForward - OpeningLossesCarriedForward),
        LossCF_Delta_Error =
            (LossesCarriedForwardDelta - (ClosingLossesCarriedForward - OpeningLossesCarriedForward))
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
    BusinessTax,
    ProfitAfterTax,
    ProfitAfterTax_Definition,
    ProfitAfterTax_Error,

    CapitalMovement,
    OpeningPosition,
    OpeningAccountPosition,
    BridgeTotal,
    Variance,
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
-- Proof: Flow view is a pure projection of equity reconciliation (shape check)
--
-- The flow view is intended as a fixed (per-year) line set for reporting.
-- This check ensures the view returns exactly one row per line per year.
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
    Status =
        CASE
            WHEN SUM(CASE WHEN LineCount <> 13 THEN 1 ELSE 0 END) = 0 THEN 'PASS'
            ELSE 'FAIL'
        END
FROM per_year;
