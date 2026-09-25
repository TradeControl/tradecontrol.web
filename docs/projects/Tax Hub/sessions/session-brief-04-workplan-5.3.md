# Revise Phase 5.15 — Full MTD Income Tax / Self Assessment target

Update only the MTD Income Tax / Self Assessment planning in:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

The present Phase 5.15 is too narrow. It describes the existing obligations enquiry and Self Employment Business cumulative `PUT` as though that were the intended SA product boundary.

That is not the intended end state.

## Product decision

Trade Control's eventual MTD Income Tax / Self Assessment target is a **full end-to-end Self Assessment product**, not merely an in-year quarterly-update product.

The already implemented Objective 3 obligations and cumulative MIN/STD contracts are the first supported slice of that product. They are not the definition of the completed SA capability.

Revise Phase 5.15 accordingly.

## Required Phase 5.15 scope

Phase 5.15 should begin with a controlled SA/MTD Income Tax contract-and-journey reconciliation.

Using the existing Objective 3 implementation and current authoritative HMRC MTD Income Tax end-to-end/API documentation:

1. establish the complete HMRC journey required for the intended end-to-end Trade Control Self Assessment product;
2. inventory which required Objective 3 contracts already exist;
3. identify which required contracts, payloads and preparation paths remain absent or intentionally deferred;
4. identify the corresponding Objective 4 transport operations;
5. preserve the existing obligations enquiry and cumulative Self Employment Business `PUT` as completed/prepared building blocks;
6. add the missing Objective 3 contract work and Objective 4 transport slices needed for the agreed end-to-end product;
7. include annual/year-end information, calculation and final-declaration journeys where required by the current HMRC product model;
8. retain the existing exact-byte, OAuth, fraud-header, durable-attempt, error, retry and audit principles already established by the VAT REST transport.

Do not revive legacy SA100 XML.

Do not assume that every HMRC MTD Income Tax API must be implemented merely because it exists. Scope the required APIs from the actual end-to-end product journeys and document deliberate exclusions.

## Current HMRC production-access restriction

Record explicitly that HMRC currently states:

> “HMRC is no longer accepting production credential access requests for new 2026–27 quarterly update products, as the market window for these products has now closed.”

This restriction must **not** be interpreted as preventing Trade Control from:

- completing Objective 3 contracts;
- implementing Objective 4 transport;
- using HMRC sandbox facilities;
- completing supported end-to-end sandbox journeys; or
- preparing the product for the next applicable HMRC recognition/production-access process.

However, do not claim that HMRC will recognise the product now and merely withhold credentials until 2027/28. The published statement does not establish that.

Treat production recognition/access for the Trade Control MTD Income Tax product as an **externally gated milestone**.

The Work Plan must not assume a reopening date. Before production enablement, re-check HMRC's then-current product-recognition and production-credential process and, if necessary, confirm it with HMRC's Software Developers Support Team.

## Relationship with Phase 5.14

Retain the existing programme decision gate.

If Phase 5.14 chooses **Proceed to Objective 5**, the complete SA programme remains planned/deferred.

If Phase 5.14 chooses **Continue Objective 4**, Phase 5.15 begins the end-to-end SA reconciliation and implementation described above.

This means SA cannot delay the VAT + CT + Companies House limited-company milestone or Objective 5 integration.

## Phase 5.16

Update Phase 5.16 consequentially.

Full Objective 4 completion must no longer mean merely:

- MTD obligations enquiry; plus
- cumulative Self Employment Business submission.

It means completion of the **agreed end-to-end Trade Control MTD Income Tax / Self Assessment product scope**, including the required Objective 3 contracts, Objective 4 transports, sandbox journey evidence and applicable external recognition/production-access gate.

If HMRC's external production-access window remains closed when the technical product is otherwise complete, distinguish:

- technical/sandbox readiness;
- HMRC recognition/application status; and
- production credential/access status.

Do not falsely mark an externally blocked production milestone as complete.

## Consequential edits

Update only the consequential references required for consistency, including:

- Phase 5.15;
- Phase 5.16;
- cross-phase verification;
- human decisions; and
- Full Objective 4 completion.

Do not alter the VAT, Corporation Tax or Companies House phase design.

Do not implement anything.

Modify only:

`docs\projects\Tax Hub\implementation\tax-hub-workplan-5.md`

Report the changes made and stop.
