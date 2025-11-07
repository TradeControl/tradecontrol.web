import { Page } from "@playwright/test";

export async function loginIfNeeded(page: Page): Promise<void>
{
    await page.goto("/Cash/CategoryTree/Index");

    // If already in, we should see the tree container.
    if (await page.locator("#categoryTree").first().isVisible().catch(() => false))
    {
        return;
    }

    // Heuristic: navigate to Identity login (adjust if your app uses a different path)
    await page.goto("/Identity/Account/Login");

    const user = process.env.TEST_USERNAME || "";
    const pass = process.env.TEST_PASSWORD || "";

    if (!user || !pass)
    {
        throw new Error("Missing TEST_USERNAME/TEST_PASSWORD env vars for login.");
    }

    await page.getByLabel("Email").fill(user).catch(async () =>
    {
        await page.getByLabel("User Name").fill(user);
    });

    await page.getByLabel("Password").fill(pass);

    // Try submit by common selectors
    const btn = page.getByRole("button", { name: /sign in|log in/i });
    if (await btn.count())
    {
        await btn.first().click();
    }
    else
    {
        await page.locator("button[type=submit], input[type=submit]").first().click();
    }

    // Arrive at the tree page once authenticated
    await page.goto("/Cash/CategoryTree/Index");
    await page.locator("#categoryTree").first().waitFor({ state: "visible" });
}