# Self Assessment SQL Node — Phase 0 Session Brief

## Purpose

This is the operational brief for the first Codex session on Self Assessment SQL Node integration.

The governing specification is `docs/tmp/self-assessment-sql-node-spec.md`. Read it before beginning. This session is limited to **Phase 0 — Live-State Verification** as defined by that specification.

## Authority and scope

The live `tradecontrol.web` checkout, including its populated Git submodules, is authoritative. Codex may inspect the complete checkout and follow any dependencies, references, validation procedures, category hierarchy objects, builders, models, tests, or other relevant repository trails necessary to establish the current state.

The Appendix paths below are starting points for reconnaissance, not limits on inspection.

This is a read-only source-code session. No source-code modifications, implementation work, commits, pushes, submodule pointer changes, or repository integration actions are authorised.

The sole authorised write is the Phase 0 report at `docs/tmp/findings.md`.

## Assignment

1. Read `docs/tmp/self-assessment-sql-node-spec.md` in full.
2. Begin reconnaissance from the files listed in the Appendix.
3. Perform every Phase 0 live-state verification activity required by the governing specification, following the repository trail wherever necessary.
4. Record the evidence, confirmed facts, discrepancies, open questions, and recommendations in `docs/tmp/findings.md`.
5. Stop when `docs/tmp/findings.md` is complete. Do not begin Phase 1 or make implementation changes.

## Required report

`docs/tmp/findings.md` must be sufficiently precise for review and should:

- identify inspected evidence by repository path;
- distinguish confirmed live facts from inference or unresolved questions;
- report discrepancies against the governing specification without correcting them;
- document relevant signatures, wrapper call graphs, tax-source and tag inventories, `hmrc_mtd` consumer alignment, validation behaviour, category and cash-code hierarchy findings, and available repeatable tests;
- state whether the evidence supports proceeding to Phase 1 and list any decisions or specification amendments required first.

Completion of the report is the session boundary. Do not continue into implementation.

## Appendix — reconnaissance entry map

src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsAdjustments.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsAllowances.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsLosses.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsMetadata.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Eops/EopsResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationDeductions.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationIncomeSummary.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationMetadata.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/FinalDeclaration/FinalDeclarationTaxCalculation.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaBalanceDetails.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaChargeDetail.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaLiabilitiesEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaLiabilitiesRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Liabilities/SaLiabilitiesResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Obligations/SaObligation.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Obligations/SaObligationsEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Obligations/SaObligationsRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Obligations/SaObligationsResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Payments/SaPayment.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Payments/SaPaymentsEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Payments/SaPaymentsRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Payments/SaPaymentsResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateAdjustments.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateEndpoint.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateExpenses.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateIncome.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateRequest.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/QuarterlyUpdate/QuarterlyUpdateResponse.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdAdjustment.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdBusiness.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdError.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdExpenseCategory.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdIncomeCategory.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdMetadata.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/MTDITSA/Shared/MtdPeriod.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100BasisPeriodSummary.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100CapitalAllowanceSummary.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100LossSummary.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa100Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa102.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa102Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa103F.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa103FSerializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa105.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa105Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa106.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa106Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa108.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa108Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa110.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Schedules/Sa110Serializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaCanonicaliser.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaEnvelope.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaEnvelopeBuilder.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaEnvelopeHeader.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaEnvelopeSerializer.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaIdAuthentication.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaIrmarkGenerator.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaMessageDetails.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaScheduleDocument.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaSenderDetails.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Sa/v1_0/Submissions/SA100/Submission/SaSubmissionBuilder.cs  
src/hmrc_mtd/src/HMRC_MTD/Hmrc/Shared/JsonExtract.cs  
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_NodeDataInit.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_BASE_MIN_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_MTD_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_MIN_SA_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_MTD_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_STD_SA_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_TAX_MTD_2026.sql
src/sqlnode/src/tcNodeDb4/App/Stored Procedures/proc_Template_ST_SOLE_CUR_TAX_SA_2026.sql
