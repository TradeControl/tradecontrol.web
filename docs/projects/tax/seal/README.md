# Tax seal and calculation continuity

This area owns the provider-neutral statutory seal and the tax calculation-continuity work that builds on it. These are open-source accounting and tax capabilities, not features of the hosted deployment.

The Tax Hub is one user interface over this subsystem. It does not own the accounting contract, the statutory evidence format or the calculation root.

## Documents

- [Statutory seal contract](statutory-seal-contract.md) — portable evidence, approval, dispatch and correction contract.
- [Tax balance and optimisation work plan](tax-balance-and-optimisation-workplan.md) — the staged delivery plan for statement balance checkpoints and later real-time calculation optimisation.

## Delivery boundary

Stage 1 is required before VAT and Corporation Tax accreditation evidence is finalised or production submissions are enabled. It extends the existing seal with statement continuity and a durable tax-balance checkpoint while retaining full-history reconstruction as the correctness oracle.

Stage 2 is a later MIS Mode optimisation. It may use a validated checkpoint to avoid repeatedly traversing the complete transaction history, but only after exact equivalence, invalidation and recovery have been proved. Stage 2 is not an accreditation prerequisite.

Hosting documentation may specify evidence-store, retention, key-management and operational controls for a deployment. Those controls protect the public protocol but do not define it.
