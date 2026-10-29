import AxeBuilder from "@axe-core/playwright";
import { expect, Page, test } from "@playwright/test";
import { loginIfNeeded } from "./helpers/auth";

type AccessibilityTarget = {
    name: string;
    path: string;
    heading?: string;
};

const publicTargets: AccessibilityTarget[] = [
    { name: "sign-in", path: "/Identity/Account/Login", heading: "Log in" },
    { name: "registration", path: "/Identity/Account/Register", heading: "Register" }
];

const authenticatedTargets: AccessibilityTarget[] = [
    { name: "dashboard", path: "/" },
    { name: "VAT Tax Hub", path: "/Tax/Hub", heading: "Tax Hub" },
    { name: "Customer Exit", path: "/Admin/Manager/Index?node=CustomerExit", heading: "Sorry to see you go" }
];

const wcagTags = ["wcag2a", "wcag2aa", "wcag21a", "wcag21aa", "wcag22a", "wcag22aa"];

async function assertNoAutomaticallyDetectableWcagViolations(page: Page, target: string): Promise<void>
{
    const results = await new AxeBuilder({ page })
        .withTags(wcagTags)
        .analyze();

    await test.info().attach(`${target}-axe-results.json`, {
        body: Buffer.from(JSON.stringify(results, null, 2)),
        contentType: "application/json"
    });

    expect(results.violations, `${target} has automatically detectable WCAG A/AA violations.`)
        .toEqual([]);
}

async function assertPageMetadata(page: Page, target: AccessibilityTarget): Promise<void>
{
    await expect(page.locator("html")).toHaveAttribute("lang", /^en(?:-|$)/i);
    expect((await page.title()).trim(), `${target.name} has no meaningful page title.`)
        .not.toEqual("");

    if (target.heading)
        await expect(page.getByRole("heading", { name: target.heading }).first()).toBeVisible();
    else
        await expect(page.locator("main, [role=main]").first()).toBeVisible();
}

async function assertNoHorizontalPageOverflow(page: Page, target: string): Promise<void>
{
    const horizontalOverflow = await page.evaluate(() =>
        document.documentElement.scrollWidth > document.documentElement.clientWidth);

    expect(horizontalOverflow, `${target} requires horizontal page scrolling.`)
        .toBe(false);
}

async function applyWcagTextSpacing(page: Page): Promise<void>
{
    await page.addStyleTag({ content: `
        body, body * {
            line-height: 1.5 !important;
            letter-spacing: 0.12em !important;
            word-spacing: 0.16em !important;
        }

        p {
            margin-bottom: 2em !important;
        }
    ` });
}

function parseHexColour(value: string): [number, number, number]
{
    const match = value.trim().match(/^#([0-9a-f]{6})$/i);
    if (!match)
        throw new Error(`Expected a six-digit hexadecimal colour, received '${value}'.`);

    return [0, 2, 4].map(offset => Number.parseInt(match[1].slice(offset, offset + 2), 16)) as [number, number, number];
}

function relativeLuminance(value: string): number
{
    const channels = parseHexColour(value).map(channel =>
    {
        const normalised = channel / 255;
        return normalised <= 0.04045
            ? normalised / 12.92
            : Math.pow((normalised + 0.055) / 1.055, 2.4);
    });

    return (0.2126 * channels[0]) + (0.7152 * channels[1]) + (0.0722 * channels[2]);
}

function contrastRatio(first: string, second: string): number
{
    const firstLuminance = relativeLuminance(first);
    const secondLuminance = relativeLuminance(second);
    const lighter = Math.max(firstLuminance, secondLuminance);
    const darker = Math.min(firstLuminance, secondLuminance);
    return (lighter + 0.05) / (darker + 0.05);
}

test("default Blue header and footer tokens meet WCAG AA normal-text contrast", async ({ page }) =>
{
    await page.goto("/Identity/Account/Login", { waitUntil: "domcontentloaded" });

    const tokens = await page.evaluate(() =>
    {
        const styles = window.getComputedStyle(document.documentElement);
        const read = (name: string): string => styles.getPropertyValue(name).trim();
        return {
            headerStart: read("--tc-header-gradient-start"),
            headerEnd: read("--tc-header-gradient-end"),
            headerText: read("--tc-footer-text"),
            brandText: read("--tc-brand-text"),
            brandHover: read("--tc-brand-hover"),
            authLink: read("--tc-auth-link"),
            authLinkHover: read("--tc-auth-link-hover"),
            footerBackground: read("--tc-footer-bg"),
            footerText: read("--tc-footer-text")
        };
    });

    const headerBackgrounds = [tokens.headerStart, tokens.headerEnd];
    const headerForegrounds = [
        tokens.headerText,
        tokens.brandText,
        tokens.brandHover,
        tokens.authLink,
        tokens.authLinkHover
    ];

    for (const foreground of headerForegrounds)
    {
        for (const background of headerBackgrounds)
        {
            expect(contrastRatio(foreground, background),
                `${foreground} on default Blue header ${background} is below 4.5:1.`)
                .toBeGreaterThanOrEqual(4.5);
        }
    }

    expect(contrastRatio(tokens.footerText, tokens.footerBackground),
        "Default Blue footer text is below 4.5:1.")
        .toBeGreaterThanOrEqual(4.5);
});

for (const target of publicTargets)
{
    test(`${target.name} has no automatically detectable WCAG 2.2 A/AA violations`, async ({ page }) =>
    {
        await page.goto(target.path);
        await page.waitForLoadState("networkidle");
        await assertNoAutomaticallyDetectableWcagViolations(page, target.name);
    });

    test(`${target.name} reflows at a 320 CSS pixel viewport`, async ({ page }) =>
    {
        await page.setViewportSize({ width: 320, height: 720 });
        await page.goto(target.path);
        await page.waitForLoadState("networkidle");

        await assertNoHorizontalPageOverflow(page, `${target.name} at 320 CSS pixels`);
        await assertNoAutomaticallyDetectableWcagViolations(page, `${target.name}-320px`);
    });

    test(`${target.name} exposes language, title, heading and accessible authentication semantics`, async ({ page }) =>
    {
        await page.goto(target.path);
        await page.waitForLoadState("networkidle");
        await assertPageMetadata(page, target);

        const email = page.locator("input[type=email]");
        const password = page.locator("input[type=password]");
        await expect(email).toHaveAttribute("autocomplete", target.name === "sign-in" ? "username" : "email");

        const passwordCount = await password.count();
        expect(passwordCount).toBe(target.name === "sign-in" ? 1 : 2);
        for (let index = 0; index < passwordCount; index++)
        {
            await expect(password.nth(index)).toHaveAttribute("autocomplete",
                target.name === "sign-in" ? "current-password" : "new-password");
            await expect(password.nth(index)).not.toHaveAttribute("onpaste", /.+/);
        }
    });

    test(`${target.name} remains usable with WCAG text spacing`, async ({ page }) =>
    {
        await page.goto(target.path);
        await page.waitForLoadState("networkidle");
        await applyWcagTextSpacing(page);
        await assertPageMetadata(page, target);
        await assertNoHorizontalPageOverflow(page, `${target.name} with WCAG text spacing`);
    });

    test(`${target.name} identifies empty-form errors programmatically`, async ({ page }) =>
    {
        await page.goto(target.path);
        await page.waitForLoadState("networkidle");
        await page.getByRole("button", { name: target.name === "sign-in" ? "Log in" : "Register" }).click();

        const summary = page.locator("[asp-validation-summary], .validation-summary-errors").first();
        await expect(summary).toHaveAttribute("role", "alert");
        await expect(summary).toHaveAttribute("aria-live", "assertive");
        await expect(summary).toContainText(/required/i);
    });
}

test.describe("authenticated customer journeys", () =>
{
    test.skip(!process.env.TEST_USERNAME || !process.env.TEST_PASSWORD,
        "Set TEST_USERNAME and TEST_PASSWORD for a synthetic tenant before collecting authenticated evidence.");

    test("account profile exposes a complete page title and named business link", async ({ page }) =>
    {
        await loginIfNeeded(page);
        await page.goto("/Identity/Account/Manage");
        await page.waitForLoadState("networkidle");

        expect((await page.title()).trim()).toMatch(/^Profile - \S.+/);
        const brand = page.locator("a.tc-brand");
        await expect(brand).toBeVisible();
        expect((await brand.textContent())?.trim()).not.toEqual("");
    });

    test("VAT workspace provides a usable vertical scroll container", async ({ page }) =>
    {
        await page.setViewportSize({ width: 1440, height: 900 });
        await loginIfNeeded(page);
        await page.goto("/Tax/Hub");
        await page.waitForLoadState("networkidle");
        await page.getByRole("button", { name: "VAT", exact: true }).click();

        const panels = page.locator(".tc-tax-hub-vat > .mud-tabs > .mud-tabs-panels");
        await expect(panels).toBeVisible();

        const result = await panels.evaluate(element =>
        {
            const probe = document.createElement("div");
            probe.style.height = "2000px";
            probe.style.width = "1px";
            probe.setAttribute("aria-hidden", "true");
            element.appendChild(probe);

            try
            {
                const overflowY = window.getComputedStyle(element).overflowY;
                element.scrollTop = 64;
                return {
                    overflowY,
                    hasOverflow: element.scrollHeight > element.clientHeight,
                    scrollTop: element.scrollTop
                };
            }
            finally
            {
                probe.remove();
                element.scrollTop = 0;
            }
        });

        expect(result.overflowY).toMatch(/^(auto|scroll)$/);
        expect(result.hasOverflow).toBe(true);
        expect(result.scrollTop).toBeGreaterThan(0);
    });

    test("VAT workspace compact controls meet the 24 pixel target minimum", async ({ page }) =>
    {
        await page.setViewportSize({ width: 1440, height: 900 });
        await loginIfNeeded(page);
        await page.goto("/Tax/Hub");
        await page.waitForLoadState("networkidle");
        await page.getByRole("button", { name: "VAT", exact: true }).click();

        await page.locator(".tc-tax-hub").evaluate(element =>
        {
            const details = document.createElement("details");
            const summary = document.createElement("summary");
            summary.dataset.testid = "tax-hub-summary-target-probe";
            summary.textContent = "Target-size probe";
            details.appendChild(summary);
            element.appendChild(details);
        });

        const targets = [
            page.locator(".tc-tax-hub-sidebar-toggle"),
            page.locator(".tc-tax-hub-hmrc-manage"),
            page.getByTestId("tax-hub-summary-target-probe")
        ];

        for (const target of targets)
        {
            await expect(target).toBeVisible();
            const dimensions = await target.evaluate(element =>
            {
                const rect = element.getBoundingClientRect();
                return { width: rect.width, height: rect.height };
            });

            expect(dimensions.width).toBeGreaterThanOrEqual(24);
            expect(dimensions.height).toBeGreaterThanOrEqual(24);
        }

        const summary = page.getByTestId("tax-hub-summary-target-probe");
        const disclosurePresentation = await summary.evaluate(element =>
        {
            const style = getComputedStyle(element);
            return { display: style.display, cursor: style.cursor };
        });

        expect(disclosurePresentation.display).toBe("list-item");
        expect(disclosurePresentation.cursor).toBe("pointer");
        await summary.press("Space");
        await expect.poll(() => summary.evaluate(element =>
            (element.parentElement as HTMLDetailsElement).open)).toBe(true);
    });

    for (const target of authenticatedTargets)
    {
        test(`${target.name} has no automatically detectable WCAG 2.2 A/AA violations`, async ({ page }) =>
        {
            await loginIfNeeded(page);
            await page.goto(target.path);
            await page.waitForLoadState("networkidle");
            await assertNoAutomaticallyDetectableWcagViolations(page, target.name);
        });

        test(`${target.name} supports keyboard landmarks and visible focus`, async ({ page }) =>
        {
            await loginIfNeeded(page);
            await page.goto(target.path);
            await page.waitForLoadState("networkidle");

            const main = page.locator("main, [role=main]");
            expect(await main.count(), `${target.name} has no main content landmark.`)
                .toBeGreaterThan(0);

            await page.keyboard.press("Tab");
            const focused = page.locator(":focus");
            await expect(focused).toBeVisible();
            const hasVisibleFocusIndicator = await focused.evaluate(element =>
            {
                const style = window.getComputedStyle(element);
                return style.outlineStyle !== "none"
                    || (style.boxShadow !== "none" && style.boxShadow !== "");
            });
            expect(hasVisibleFocusIndicator, `${target.name} has no visible indicator on the first keyboard focus.`)
                .toBe(true);
        });

        test(`${target.name} reflows at a 320 CSS pixel viewport`, async ({ page }) =>
        {
            await page.setViewportSize({ width: 320, height: 720 });
            await loginIfNeeded(page);
            await page.goto(target.path);
            await page.waitForLoadState("networkidle");

            await assertNoHorizontalPageOverflow(page, `${target.name} at 320 CSS pixels`);

            if (target.name === "Customer Exit")
            {
                await expect(page.getByRole("heading", { name: "Sorry to see you go" }))
                    .toBeVisible();
            }

            await assertNoAutomaticallyDetectableWcagViolations(page, `${target.name}-320px`);
        });

        test(`${target.name} exposes language, title and heading metadata`, async ({ page }) =>
        {
            await loginIfNeeded(page);
            await page.goto(target.path);
            await page.waitForLoadState("networkidle");
            await assertPageMetadata(page, target);
        });

        test(`${target.name} remains usable with WCAG text spacing`, async ({ page }) =>
        {
            await loginIfNeeded(page);
            await page.goto(target.path);
            await page.waitForLoadState("networkidle");
            await applyWcagTextSpacing(page);
            await assertPageMetadata(page, target);
            await assertNoHorizontalPageOverflow(page, `${target.name} with WCAG text spacing`);
        });

        test(`${target.name} supports landscape orientation without horizontal page scrolling`, async ({ page }) =>
        {
            await page.setViewportSize({ width: 720, height: 320 });
            await loginIfNeeded(page);
            await page.goto(target.path);
            await page.waitForLoadState("networkidle");
            await assertPageMetadata(page, target);
            await assertNoHorizontalPageOverflow(page, `${target.name} in a 720 by 320 landscape viewport`);
        });
    }
});
