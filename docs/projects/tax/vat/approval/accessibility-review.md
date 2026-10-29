# Accessibility review — Pass 1

## Standard and scope

HMRC's current production-access terms require web software to meet WCAG Level AA but do not name a WCAG release in the question. This review uses **WCAG 2.2 Level A and AA**, the current GOV.UK service benchmark, rather than treating an older version as sufficient.

The review scope is sign-in, registration, authenticated navigation, dashboard, Customer Exit and the VAT preparation, review, approval, submission-control and filing-history journey. It combines repeatable Playwright/axe checks with a criterion matrix and targeted manual assessment. Automated results alone are not treated as conformance evidence.

## Implemented test

`src/tests/ui/tests/vat-compliance.accessibility.spec.ts` adds:

- axe-core checks tagged for WCAG 2.0, 2.1 and 2.2 Level A/AA rules;
- 320 CSS-pixel reflow checks for public and authenticated pages;
- authenticated dashboard, Tax Hub and Customer Exit checks when `TEST_USERNAME` and `TEST_PASSWORD` are supplied; and
- landmark, first-keyboard-focus and visible-focus checks for authenticated pages.

Run from `src/tests/ui`:

```powershell
npm.cmd install
$env:BASE_URL='https://business5.tradecontrol.co.uk'
$env:TEST_USERNAME='<tenant application user>'
$env:TEST_PASSWORD='<read from the approved private source>'
npx.cmd playwright test tests/vat-compliance.accessibility.spec.ts --project=edge
```

Do not use an HMRC sandbox identity as the Trade Control application login and do not put passwords in the repository or report.

## Result on 28 October 2026

The first public-page run detected a repeated blue-theme contrast defect: primary buttons used white text on `#1e88e5`, a 3.68:1 contrast ratio. The initial source correction changed the button normal state to `#1565c0` and hover/focus to `#0d47a1`. A deployed rerun then exposed the same lighter blue in the footer and in small headings at the 320 CSS-pixel viewport. Those shared tokens now also use `#1565c0`, taking the affected foreground/background combinations above the 4.5:1 Level AA threshold.

The first authenticated B5 run then detected insufficient contrast in the orange user theme, an unnamed Tax Hub loading indicator and a missing visible first-focus indicator. The theme tokens were darkened, the loading indicator was named and a two-colour focus treatment was applied. Extending the suite to Customer Exit exposed three Admin Manager expanders whose effective safe target was 23.6 pixels rather than the required 24 pixels; those controls now have a 28-pixel minimum. Independent B6 testing then exposed the equivalent contrast defect in its green user theme, which received the same bounded correction.

On 29 October the owner confirmed that Blue is the default presentation and that the established bright Orange palette must remain available unchanged because it is used throughout the published product screenshots. The pending source therefore restores the original Orange palette. The earlier B5 result proves the temporarily darkened palette only; it is not evidence that the restored optional Orange presentation meets the contrast criterion. The final assessment will exercise the authenticated VAT journey in the default Blue presentation and verify that the accessible user-theme selector provides a clear route back to that presentation. No claim is made that every optional colour palette independently conforms.

The final correction was deployed to the B5/B6 Integration topology on 28 October 2026 as deployment `ec3799cf-6a1c-4e3b-9ba0-2fca729fdc49`. Separate Edge runs against B5 and B6 each passed four public checks and nine authenticated checks. The authenticated cases cover Dashboard, Tax Hub and Customer Exit with axe WCAG 2.2 A/AA analysis, a main-landmark/visible-first-focus assertion and 320 CSS-pixel reflow. No automatically detectable WCAG 2.2 A/AA violation remained on the assessed pages. Protected test artifacts are retained only under the ignored local evidence area. Earlier restricted-runner and superseded discovery failures are not counted as final product evidence.

A subsequent live B5 keyboard and zoom review confirmed the primary navigation's logical focus order, visible focus treatment and keyboard opening of System and Admin Manager. The first 200% Customer Exit check exposed a gap that the horizontal-overflow assertion had not detected: the desktop Admin Manager panes crossed the CSS responsive breakpoint without restacking, leaving the selected detail below a clipped layout. The panes now stack vertically below that breakpoint, the automated Customer Exit reflow case explicitly requires its heading to remain visible, and the deployed correction passed live B5 checks at 200% and 400% without observed horizontal page scrolling.

The owner then completed a bounded Windows Narrator check on the deployed B5 Customer Exit page. Heading navigation announced the page and content headings; control navigation announced the complete confirmation-checkbox label and unchecked state, the download button and its unavailable state, and the guide link. No confirmation was selected and no download was invoked.

On 29 October a fresh B7 Standard Micro Entity tenant was commissioned specifically for the remaining full VAT-journey evidence. Its default Blue public checks passed, and an owner-authenticated Tax Hub inspection confirmed the main landmark and heading structure, named HMRC connection region, intelligible disconnected guidance, labelled VAT tabs/tables and retained focus when Filing History was selected. A missing B7 broker return-host entry was found during the first connection attempt and corrected as Integration configuration without product-code or database changes. The owner then completed Elvin Lee's sandbox OAuth. The live journey retrieved obligations, prepared a named immutable nine-box return with PASS reconciliation, gated approval behind an explicit declaration, announced that approval did not submit, retained the approved return across reload and exposed an explicit non-destructive submission-disabled state. No VAT submission has been authorised or made.

The same session verified that the labelled Theme selector had Default selected. It also exposed an incomplete account title and unnamed tenant-brand link. The shared layout correction was built, deployed and verified as `Profile - Business 7 Ltd` with a named Business 7 Ltd link. The deployment's Linux App Service restart retained B7's Connected grant and approved/not-submitted return while the tenant filing gate remained disabled.

The long VAT obligations/review view then exposed a missing vertical scrollbar: the active tab component could not own scrolling in the current rendered structure while its parent hid overflow. The bounded parent panels container now owns vertical scrolling and reserves a stable gutter. The deployed correction displayed the scrollbar, accepted wheel scrolling through the complete review and again retained the Connected, approved-but-not-submitted state across the application restart.

The final live B7 review traversed the complete no-submission journey by keyboard, including arrow-key and Space selection of the obligations and history tabs, with visible focus retained. At 400% Edge zoom the responsive page had no horizontal overflow and the complete nine-box review, declaration, approval state and disabled-submission explanation remained reachable. A safe repeated refresh exposed a recent-refresh status without moving focus. Three compact VAT controls found below the 24 CSS-pixel target minimum were corrected and measured after deployment at 32 × 32, 60.45 × 24 and 1,058 × 24. The final 32-case deployed run passed all 11 public/default-Blue cases and explicitly skipped 21 credential-gated cases because no application password was supplied; the skipped cases are not presented as passes. The executable Tax Hub boundary suite passed all 333 assertions.

The owner then completed the B7 Narrator sequence. Headings, the prepared-return table and its columns/values, the disabled legal declaration, Filing History selected state and empty-history message were intelligible. This exposed one final presentation defect: the audit disclosure appeared as plain text because its flex layout suppressed the native marker. The corrected deployment restored a visible disclosure triangle and keyboard operation; Narrator announced its collapsed and expanded states. The final public/default-Blue rerun again passed 11/11 and B5/B6/B7 health remained Healthy.

The criterion matrix records multimedia criteria as not applicable to the assessed journeys, the detected contrast, focus, target-size and reflow criteria as passing for the automated scope after deployed verification, and criteria needing complete manual or screen-reader evidence as not tested or partial. Prior Phase 6.6 observations remain regression context rather than substitutes for the current manual record.

## Disposition

**READY FOR OWNER ATTESTATION for the assessed no-submission scope.** B5/B6 authenticated automation, B7's public/default-Blue checks, and B7's connected keyboard, Narrator, 400% zoom, target-size, safe-error-focus, preparation, approval, restart and kill-switch checks pass. Isolated-runner B7 authenticated axe/reflow/text-spacing cases remain transparently skipped because no application password was supplied; a literal 60-minute elapsed session was not performed. One separately authorised B7 sandbox filing may later prove terminal acceptance/readback/history but is not required for this accessibility assessment. The owner must review the criterion matrix before answering HMRC; no independent certification is claimed.

## Sources

- [HMRC Developer Hub terms of use](https://developer.service.hmrc.gov.uk/api-documentation/docs/terms-of-use)
- [GOV.UK — Understanding WCAG 2.2](https://www.gov.uk/service-manual/helping-people-to-use-your-service/understanding-wcag)
- [GOV.UK — Testing for accessibility](https://www.gov.uk/service-manual/helping-people-to-use-your-service/testing-for-accessibility)

---

## Document status

Public, non-secret Pass 1 evidence for owner review. Detailed local results and the criterion matrix remain in the ignored evidence area until the review is complete.
