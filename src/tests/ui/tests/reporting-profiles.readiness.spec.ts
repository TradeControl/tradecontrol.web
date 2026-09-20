import { test, expect } from "@playwright/test";
import { loginIfNeeded } from "./helpers/auth";

test.describe("Reporting profiles and Tax Hub readiness", () =>
{
  test.beforeEach(async ({ page }) =>
  {
    await loginIfNeeded(page);
  });

  test("sensitive statutory identifiers are not browser password fields", async ({ page }) =>
  {
    await page.goto("/Admin/Manager/Index?node=ReportingProfiles");
    await page.getByRole("button", { name: "Reporting Profiles", exact: true }).click();
    await expect(page.getByText(/Maintain durable authority identifiers/i)).toBeVisible();
    await page.getByRole("tab", { name: /Company tax reporting/i }).click();

    const utrContainer = page.getByText("Unique Taxpayer Reference", { exact: true }).locator("..");
    const utr = utrContainer.locator("input");

    await expect(utr).toBeVisible();
    await expect(utr).toHaveAttribute("type", "text");
    await expect(utr).toHaveAttribute("autocomplete", "off");
    await expect(utrContainer.getByRole("button", { name: /show unique taxpayer reference/i })).toBeVisible();
  });

  test("an incomplete statutory profile opens Tax Hub with an administrator repair link", async ({ page }) =>
  {
    test.skip(process.env.EXPECT_MISSING_COMPANY_UTR !== "1",
      "Run against the company missing-UTR fixture with EXPECT_MISSING_COMPANY_UTR=1.");

    await page.goto("/Tax/Hub/Index");

    await expect(page.getByText("Statutory reporting configuration is incomplete", { exact: true })).toBeVisible();
    await expect(page.getByText(/required Unique Taxpayer Reference is missing/i)).toBeVisible();

    const repairLink = page.getByRole("link", { name: "Open Reporting Profiles in Admin Manager" });
    await expect(repairLink).toBeVisible();
    await expect(repairLink).toHaveAttribute("href", "/Admin/Manager/Index?node=ReportingProfiles");
  });
});
