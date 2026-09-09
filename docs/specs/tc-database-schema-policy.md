# Trade Control Database Schema Policy

## Purpose

This document defines the persistent relational-design rules shared by Trade Control projects. These rules protect database portability, node synchronisation and consistency with the established SQL design language.

Read this policy before designing or changing domain tables, keys, catalogues, initialization or database versioning.

## 1. Schema ownership

- Every domain object belongs to its established schema, such as `App`, `Subject`, `Cash`, `Invoice`, `Project`, `Object`, `Usr` or `Web`.
- Do not place Trade Control domain objects in `dbo`.
- `dbo` is reserved for the standard ASP.NET Core authentication framework and other explicitly approved infrastructure.
- Locate a table with the entity that owns its lifecycle. Locate a projection with the domain that owns the resulting read model.

## 2. Portable domain identity

Trade Control databases may be created, copied, operated offline and synchronised with another database instance. Domain identity must therefore survive movement between instances.

- Do not use `IDENTITY` columns or other database-local surrogate numbers as primary keys for synchronisable domain data.
- Root a child key in the stable key of its owning entity.
- Prefer meaningful composite keys where the business fields form stable identity.
- Where a compact code is required, generate a durable code using the established procedure pattern based on `App.proc_DefaultCodeGenerator`.
- A generated code must be unique within its declared owner and stored as part of the row's primary key.
- Effective-dated rows normally include their effective start in the key when the same semantic value may recur.
- Foreign keys and diagnostic references must use the durable domain key, not an arbitrary local row number.

Examples:

```text
Subject registration
  PRIMARY KEY (SubjectCode, RegistrationCode)

Reporting profile
  PRIMARY KEY (SubjectCode, ReportingProfileCode)

Effective profile setting
  PRIMARY KEY (SubjectCode, ReportingProfileCode, SettingCode, EffectiveFrom)
```

An identity or surrogate key is permitted only when the data is explicitly instance-local or infrastructure-owned and the exception has been agreed.

## 3. Controlled values and enumerations

- Represent reusable enumerated values with a controlled table and foreign key.
- Follow the established numeric or textual code convention of the owning domain.
- Do not introduce a free-text `...Code` column whose allowed values exist only in application code or a hard-coded `CHECK` constraint.
- A check constraint may enforce structural validity, ranges or cross-column consistency; it must not replace a shared enumeration catalogue.
- Do not duplicate an existing controlled classification under a new name. Derive it through the authoritative relationship.
- Introduce a many-to-many applicability table only when an actual workflow requires that relationship; do not add speculative classification metadata.

## 4. Scope-qualified definitions

- When the meaning or uniqueness of a definition depends on an owning scope, include that scope in the definition's primary key.
- A scope may be geographical, legal, administrative, organisational or another controlled domain context.
- Make the scope mandatory when the definition has no valid meaning outside it.
- Child foreign keys must carry the complete scope-qualified key; do not reference only the locally unique portion.
- Give conceptually distinct scopes explicit names, even when their current values happen to match.
- Do not silently infer that one scope determines another unless that dependency is an established domain invariant.
- Reuse the existing authoritative scope catalogue where it represents the required concept.
- Do not introduce overlapping catalogues merely to rename or duplicate an existing scope. Add a distinct catalogue only when it owns genuinely different semantics or behaviour.

## 5. Authoritative ownership and derivation

- Give each semantic value one authoritative owner.
- Do not persist a value in a second table when it can be reliably obtained through an existing foreign-key path.
- A projection may expose derived values, but it must retain the authoritative source and must not become a competing write model.
- Do not infer legal or statutory facts from convenient accounting classifications unless the specification explicitly establishes that relationship.
- Preserve distinct concepts even when their current values happen to match.

## 6. Effective dating and integrity

- Effective-dated data must have explicit start and optional end fields.
- Enforce a valid date interval with a constraint.
- Prevent overlapping active rows where the business cardinality is one.
- Status, provenance and review state must use controlled foreign keys or established domain conventions.
- Relational constraints are authoritative. Application validation may improve feedback but must not be the only protection.
- Row versions and audit fields must follow the conventions of the owning schema.

## 7. Projections and sensitive values

- Use database views and table-valued functions for authoritative, reusable read models and joins.
- Accept an explicit date or reporting period when resolving effective data.
- Carry sufficient row-version or update provenance to identify the source state of a prepared artifact.
- Readiness and diagnostic projections must return stable domain keys.
- Do not expose sensitive registration values in finding messages, logs or diagnostic identifiers.
- Missing or unavailable structured data must remain explicit; do not guess or parse it during statutory request population.

## 8. Initialization and synthetic regeneration

- `App.proc_NodeDataInit` is the sole owner of Trade Control catalogue initialization and reset ordering.
- Do not duplicate catalogue or domain initialization in pre-deployment or post-deployment scripts.
- `Script.PostDeployment` may record the initially installed SQL version/build; it is not a second data-initialization path.
- Synthetic regeneration must remain valid when run inside an existing node, including correct deletion order for foreign keys and re-creation of controlled definitions.
- Never seed fabricated subject registrations or authority references as general catalogue data. Synthetic fixtures must be conspicuous and limited to test datasets.

## 9. Database versioning

- `App.tbInstall` records released version components and the development `SQLBuild`.
- Development work advances `SQLBuild` when a coherent tested database boundary is reached.
- Do not advance `SQLRelease` merely for development changes. A release represents software released into the world.
- The application setting `Settings:SqlNodeVersion` may identify a compatible release line such as `4.1.*`; the latest install row identifies the exact database build.
- Keep application, EF models, SQL project and active development databases synchronised.

## 10. Change procedure

Before implementing a schema change:

1. Inspect existing tables, keys, catalogues, procedures and naming conventions in the owning schema.
2. Identify the authoritative owner and synchronisation requirements of every new value.
3. State proposed tables, primary keys, foreign keys and enumerations before coding when the design introduces a new domain concept.
4. Trace all SQL, EF, initialization, configuration UI and projection consumers.
5. Preserve unrelated data and existing behaviour.

Before considering the change complete:

1. Build the SQL project and every affected application project.
2. Verify destructive replacements against the exact target tables and data before deployment.
3. Test constraints, effective-date resolution and rollback cleanup.
4. Test the supported active sandbox profiles where database integration is required.
5. Confirm the installed SQL build without changing the released version unintentionally.

## 11. Design review checklist

- Is every object in the correct domain schema?
- Is every primary key durable across database instances?
- Is each child key rooted in its owner?
- Are all enumerations relationally controlled?
- Does any new column duplicate a value already derivable from the subject or another owner?
- Does every jurisdiction-owned definition have a jurisdiction-qualified key?
- Are distinct jurisdiction meanings modelled separately?
- Is every foreign key complete and synchronisable?
- Is initialization owned only by `App.proc_NodeDataInit`?
- Have SQL, EF, UI and active sandbox databases been kept aligned?
