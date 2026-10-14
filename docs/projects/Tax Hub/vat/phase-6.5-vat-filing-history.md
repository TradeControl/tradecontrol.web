# Phase 6.5 — VAT filing history, readback and reconciliation

## Implementation checkpoint

Implemented, deployed and accepted by interactive review on 1 October 2026. Acceptance does not approve the development file stores for production hosting.

TCWeb now exposes a Filing History tab beside HMRC Obligations. The ordinary view is available only under the existing Administrator/Manager VAT filing policy and lists the current tenant's VAT submission attempts across filing actors. It does not expose OAuth tokens, fraud facts, secrets, raw request/response bodies or protected-content references.

The visible history is also bounded by statutory context. It includes only evidence whose VAT-registration digest matches the current indirect-tax reporting profile, whose filing period is not wholly before that profile's effective date, and whose filing period overlaps the financial year or month selected in the Tax Hub sidebar. This prevents obsolete sandbox identities from being presented as current filings and keeps a long-lived installation's evidence list aligned with the user's chosen accounting window. Selecting an all-years view deliberately removes only the UI period window; the current-identity and adoption-date boundaries still apply.

Each item links the immutable approval to the write attempt and displays the masked VAT registration, period, actor reference, prepared SHA-256 digest, attempt state, safe HMRC outcome/status, correlation reference and safe receipt fields. The approved nine boxes are read from the retained canonical request. Both the retained approval payload and the separately retained dispatched payload are checked against the prepared digest before they are presented as valid evidence. Missing or altered evidence is shown as an integrity failure rather than silently omitted.

Exact HMRC readback reconciliation is now part of the durable attempt record. A successful submission records the field-by-field `VatReturnReconciliation` result immediately. Older successful attempts and unknown attempts without durable reconciliation expose a controlled **Reconcile with HMRC** action. It captures fresh browser/session fraud facts, verifies the current reporting-subject VAT registration against the approved filing, reconstructs the exact approved request from protected evidence, retrieves HMRC's return and immutably records either the empty exact-match difference set or every field difference. Failure to prove the outcome does not unlock or encourage another submission.

## Persistence boundary

This deployment still uses the explicitly accepted development/reference file implementation. Attempt metadata has the configured seven-year development retention; protected content remains separately bounded by its configured retention. If protected content has expired, is missing, substituted or corrupt, history remains visible but reports an explicit evidence failure and exact reconciliation is unavailable. Production SQL/Blob metadata, object-version/digest, coordinated backup/restore, retention/purge and privileged raw-evidence facilities remain subject to the production hosting review described by Phases 6.0, 6.5 and 6.6.

## Verification evidence

- `TaxHub.slnx` Release build: zero warnings and zero errors.
- Submission adapter foundation: 104 assertions passed, including tenant/principal isolation, bounded history, immutable reconciliation and duplicate-write protection.
- TCWeb Tax Hub boundary suite: 87 assertions passed.
- Remaining offline contract/application/architecture/adapter/WebHarness suites passed. The secret-backed data-provision executable was not run because `TC_NODE_CONTEXT` was not supplied.
- Azure deployment `bf6bb8fd-505a-4dce-aa4e-2957fe8270f8` completed successfully; `tcweb-payg-db96115e` reported `Running` and its HTTPS Tax Hub route redirected an unauthenticated request to TCWeb Identity login.

## Interactive review evidence

The reviewer confirmed:

1. the accepted filing appears with masked identity, receipt, digest and tenant-wide evidence;
2. the previously accepted filing has a durable exact nine-box HMRC readback match;
3. an obsolete filing created under a different sandbox VAT registration is excluded by the current statutory identity boundary;
4. the selected Tax Hub financial period limits the visible filing periods; and
5. the history layout remains usable in the deployed workspace.

Phase 6.5 is accepted for the development/reference sandbox composition. Production retention, privileged raw-evidence and coordinated SQL/Blob restore decisions remain gated for Phase 6.6 and production activation.
