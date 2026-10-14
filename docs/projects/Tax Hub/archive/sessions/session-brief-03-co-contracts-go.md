# Tax Hub — Company Objective 3 Implementation

4 August 2026

Proceed with implementation of the approved Company Statutory Contract Design.

Decisions:

- Approve Open Questions 1–9 as written.
- Defer Open Question 10, Acceptance Threshold, to Objective 4.
- Treat `docs/projects/Tax Hub/implementation/company-statutory-contract-design.md` as authoritative for this implementation.

Implement Objective 3 in the sequence defined in the design document, within:

- `TradeControl.Tax.UK.Company.Contracts`
- `TradeControl.Tax.UK.Company.ContractTests`

Scope includes:

- company statutory semantic types
- XBRL/iXBRL contract infrastructure
- pinned/versioned contract descriptors and taxonomy/schema provenance
- reproducible generated wire contracts where appropriate
- Companies House TIS 5.9 contracts
- current HMRC Corporation Tax / CT600 contract family
- statutory accounts and computation iXBRL projections
- package/envelope/acknowledgement/status contracts
- deterministic serialization
- validation and reconciliation rules
- offline fixtures and contract tests

Out of scope:

- Trade Control SQL/data access
- Tax Sources / Tax Tags / category mappings
- Trade Control population logic
- application orchestration
- live Companies House or HMRC transport
- authentication/credentials
- official test-service integration
- Objective 4 acceptance criteria

Use official pinned schemas/taxonomies where licensing permits. If redistribution is not permitted, preserve provenance/checksums and use derived catalogs or separately provisioned validation assets as designed.

Implement the smallest complete supported ordinary UK private micro-company profile defined by the design. Unsupported statutory scenarios must fail explicitly rather than being silently omitted.

Run the relevant builds and offline contract tests throughout and report the final implementation, evidence, any authoritative-source gaps encountered, and any questions that genuinely block correctness.

Do not stage, commit, push, amend, reset, revert, stash, or otherwise alter Git history or index state. Leave all changes unstaged in the working tree for user review. The user will perform Git operations separately.
