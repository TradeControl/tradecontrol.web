# Customer data export and exit control

## Implemented first-release control

Trade Control now contains a bounded customer-exit archive intended for a business changing provider. It is separate from financial-report downloads and the Identity account's personal-data download.

An authenticated user in the tenant's `Administrators` role selects **Admin Manager → Company → Customer Exit**. The panel explains the archive, exclusions, security precautions and limitations, and requires a per-download acknowledgement before requesting `GET /Admin/CustomerExit/Download?confirmed=true`. The endpoint enforces that confirmation; a signed-in administrator reaching the download URL without it is returned to the explanatory panel. The existing hostname-derived tenant context supplies that tenant's database and file namespace; the caller cannot select a tenant identifier in the request.

The application creates a ZIP archive containing:

- `manifest.json`, describing the format, scope, exclusions, table paths and columns;
- one UTF-8 JSON array for each supported physical tenant table; and
- tenant-owned documents, images and templates in their original formats.

The export deliberately excludes infrastructure configuration and operational event logs; host passwords; encryption keys and initialisation vectors; OAuth grants, access and refresh tokens; vendor credentials; central identity credentials; and information belonging to any other tenant. Mapped database views are not exported because they duplicate or derive the underlying physical records.

The archive is generated in memory and returned directly over the authenticated HTTPS response. No server-side temporary export file or reusable public download URL is created, so there is no server-side expiry or cleanup object. The response prohibits caching, carries a unique export reference and SHA-256 digest, and records an attributable generation event in the tenant event log. The acknowledgement does not close the account, cancel service, delete data, authorise a filing or perform an automatic migration.

## Evidence

The implementation is in:

- `src/TCWeb/AppServices/CustomerExitArchiveService.cs`;
- `src/TCWeb/Controllers/CustomerExitController.cs`; and
- `src/TCWeb.TaxHub.Tests/CustomerExitArchiveTests.cs`.

On 28 October 2026 the complete Tax Hub boundary executable passed 333 assertions. The added assertions cover the manifest, JSON business record, tenant-owned document, protected-value exclusion, digest, administrator role and identity, no-cache response, attributable event, per-download confirmation and the physical-column boundary of the actual EF model. The application build completed with no warnings or errors.

## Deployment acceptance — 28 October 2026

The control was deployed to the shared Integration topology and exercised independently through authenticated B5 and B6 administrators. The first B5 request exposed two convention-created EF shadow properties that were not physical SQL columns. After that correction, inspection of the first B5/B6 archives identified a private cryptographic-key column and infrastructure host binding that contradicted the documented exclusions. Those captures were rejected and retained only as protected failed evidence. The real-model regression now excludes all four fields.

Fresh B5 and B6 archives then passed inspection. Each contains a manifest, 131 supported physical tables and 13 tenant files, with a distinct export reference and digest. The B5 subject data contains the expected B5 synthetic business name and not B6's; B6 contains its own name and not B5's. No configured forbidden secret or host-binding column remains in either manifest, and each tenant's own Event Viewer records the matching attributable generation event. The archives and exact digests remain under ignored `.local` and are not part of this public package.

The owner subsequently exercised the deployed explanatory page, confirmation and download control and accepted the result on 28 October 2026. The HMRC switching-provider answer is therefore **READY** within the stated scope.

This first release supplies business data in documented, durable formats; it does not promise automated import into another product, automatic account closure, automatic deletion, or export of infrastructure and authority credentials. Retention, account closure and lawful deletion remain governed separately.

---

## Document status

Pre-launch implementation evidence for owner review. It contains no customer identifiers, credentials or unredacted export data.
