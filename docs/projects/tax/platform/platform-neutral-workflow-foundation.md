# Platform-neutral statutory workflow foundation

**Status:** Architecture objective agreed; detailed design pending

## 1. Aim

Trade Control's statutory filing journeys must remain independent of any single hosting platform,
infrastructure provider or deployment topology. The same reviewed calculation, immutable document,
approval, submission and authority-outcome semantics should be available to independently operated
installations and hosted services.

Managed services may be used where they are useful. They are adapters, not product prerequisites.
Statutory calculations, serializers, workflow rules and user-interface meaning must not change when
the persistence or hosting provider changes.

This objective supports practical software freedom, operational resilience and the ability to move
the service when technical, economic or strategic circumstances change.

## 2. Governing principles

### 2.1 One application contract

Application services depend on capability interfaces, never on provider SDK types, proprietary
connection mechanisms or a particular database location. The shared contract covers:

- workflow metadata and state transitions;
- immutable protected content;
- tenant and reporting-subject identity;
- attributable reviews and approvals;
- exact-byte reads with SHA-256 verification;
- append-only evidence and bounded corrections;
- one-send claims, polling and authority outcomes; and
- schema and capability health checks.

Provider names must not leak into calculation, preparation, approval or transport semantics.

### 2.2 Equivalent behaviour

Every supported provider must preserve the same invariants:

- tenant isolation;
- authenticated actor attribution;
- appropriate concurrency control;
- immutable approved and submitted bytes;
- refusal of stale evidence;
- duplicate-send prevention;
- digest verification on every protected-content read;
- restart-safe pending work; and
- recoverable audit history.

A portable implementation that silently weakens evidence, concurrency or recovery guarantees is not
acceptable.

### 2.3 Replaceable hosting adapters

A hosted service may use managed databases, protected object storage, managed identities and other
platform facilities. These remain implementations of the application contract. Replacing them must
not require changes to statutory calculations, serializers or product workflow rules.

### 2.4 Independently operated installations are a product mode

Independent operation is not merely a developer simulator. It requires durable, backed-up storage
suitable for statutory records. A file-backed development mode may remain useful for diagnostics,
but it does not by itself satisfy the independently operated product requirement.

### 2.5 Practical Free Software operation

Source availability should be accompanied by a genuine ability to operate the useful product. A
commercial hosted service and professional support are compatible with this principle; users retain
the option to deploy, inspect, adapt and operate the software independently.

## 3. Provider model

The intended provider matrix is:

| Mode | Purpose | Required capability |
|---|---|---|
| **Installation-local durable** | Independently operated installations | Durable workflow metadata and protected statutory artifacts under the operator's control |
| **Hosted shared** | Multi-tenant hosted services | Tenant-partitioned durable metadata and protected content using reviewed hosted infrastructure |
| **Development files** | Diagnostics, fixtures and destructive testing | Clearly labelled non-production persistence with the same logical contract where practical |
| **Disabled** | Explicitly unavailable statutory workflow | Fail-closed services and an accurate readiness explanation |

The local durable backing remains a detailed design decision. Candidates may include storage in the
installation's business database or a separately administered workflow database and protected
content store. The decision must consider backup coherence, encryption, evidence size, least
privilege, upgrades, disaster recovery and ease of installation.

The hosted implementation may use shared infrastructure after its schema and permission model have
been reviewed. Its existence must not make those facilities mandatory for another mode.

## 4. Database ownership and conventions

No database schema or table is authorised by this document.

Before implementing a durable provider, agree and record:

1. which repository owns each database model and migration artifact;
2. the schemas and naming conventions;
3. schema versioning and application compatibility rules;
4. additive migration, forward-fix and restore procedures;
5. installation-local upgrade behaviour; and
6. role boundaries for each statutory product, background processing and support.

Database changes should be delivered through a versioned database project or an approved migration
system. Product roles must receive only the capabilities they require. Sharing an infrastructure
resource must not silently grant one statutory product access to another product's evidence.

Existing workflow objects may contain retained statutory evidence. Any correction begins with an
inventory and migration plan. Deployed evidence must not be renamed, transferred or deleted merely
to make a database appear consistent.

## 5. Application boundaries

### Workflow metadata store

Owns bounded records, state transitions, concurrency claims, references, timestamps and safe
diagnostic fields. It does not contain clear-text credentials or unnecessarily duplicate complete
statutory documents.

### Protected artifact store

Owns exact accounts, computations, returns, request and response bodies, and receipts. References
are opaque and tenant-scoped. Writes are immutable and reads verify the expected digest. Encryption
and backup requirements apply to local and hosted providers.

### Secret and authority credential source

Supplies credentials at dispatch time without writing them into prepared artifacts or workflow
metadata. Provider implementations may differ without changing the submission contract.

### Background work scheduler

Executes restart-safe polling and recovery work. It consumes durable claims through the application
contract and does not assume a particular web process, database service or hosted scheduler.

### Product services

Companies House, Corporation Tax, VAT and later workflows own their statutory meanings. They consume
the shared capabilities and remain unaware of the selected provider.

Shared interfaces should not expose accidental dependencies through product-specific or
provider-specific names.

## 6. Delivery sequence

### PF-1 — inventory and design decision

- Inventory workflow metadata, protected artifact classes and application stores.
- Identify deployed evidence, retention obligations and current readers and writers.
- Agree database ownership, schema, naming, versioning and permission boundaries.
- Select the installation-local durable backing.
- Record migration and compatibility policy before changing deployed objects.

**Review gate:** approve the provider contract and database design. No migration is implied.

### PF-2 — provider-independent contracts

- Generalise metadata, protected-content, identity and health interfaces.
- Remove provider and accidental product terminology from shared capabilities.
- Keep statutory product records typed rather than replacing them with an unbounded generic store.
- Define transaction and concurrency requirements for each operation.

**Review gate:** prove that the Application layer has no provider dependency.

### PF-3 — installation-local durable provider

- Implement durable local metadata and protected-content stores.
- Add schema installation, upgrade and compatibility checks.
- Prove backup and restore, immutable reads, digest failure, restart recovery and least privilege.
- Exercise a complete accepted-accounts to Corporation Tax preview journey using only locally
  controlled facilities.

**Review gate:** an independently operated installation completes the required evidence journey.

### PF-4 — hosted shared provider

- Adapt the hosted implementation to the same contracts.
- Apply the approved schema, permissions and migration design.
- Preserve existing evidence without altering its bytes or digests.
- Prove cross-tenant isolation and scaled-out concurrency.

**Review gate:** hosted behaviour is contract-equivalent to the local provider.

### PF-5 — composition and parity

- Select providers through capabilities and configuration rather than provider-branded business
  logic.
- Add startup validation with actionable readiness findings.
- Run a shared provider contract suite against local, hosted and development implementations.
- Add application composition tests for every supported mode.
- Prove that prepared bytes and logical outcomes are provider-independent.

**Review gate:** no supported product path silently falls back to disabled services.

### PF-6 — statutory journey adoption

- Move each existing statutory workflow onto the neutral foundation.
- Complete immutable preparation and stale-source acceptance on both durable providers.
- Continue approval, submission and polling only after parity is demonstrated.

## 7. Acceptance criteria

The foundation is accepted only when all of the following are true:

1. An independently operated installation completes the supported statutory preparation journey
   without a hosted workflow dependency.
2. A hosted installation completes the same journey through its hosted provider.
3. The same source produces byte-identical statutory artifacts before provider retention.
4. Shared contract tests prove isolation, immutability, digest verification, stale refusal,
   concurrency and restart recovery.
5. Local backup and restore recover workflow metadata and protected content as one coherent evidence
   set.
6. Hosted backup, restore and evidence recovery are documented and exercised independently.
7. Startup rejects incomplete compositions instead of presenting a superficially available but
   disabled filing journey.
8. Application and statutory-contract assemblies do not reference provider SDKs or concrete store
   classes.
9. Database objects follow an explicitly approved ownership, naming and versioning convention.
10. Product roles cannot read or mutate another product's evidence unless an explicitly reviewed
    shared capability requires it.
11. Completing the foundation does not implicitly enable any production submission gate.

## 8. Redundancy boundary

Platform neutrality provides strategic redundancy: a hosted service can move provider and independent
users can continue operating their own installations. It also makes provider-independent backup,
export and recovery tooling possible.

The first implementation will not dual-write live workflow evidence to two providers or perform
automatic cross-provider failover. Dual writes introduce split-brain, ordering and evidence-identity
risks precisely where statutory records must remain unambiguous. Operational replication or a
controlled provider migration must be designed separately after the independent providers and their
recovery contracts are proven.
