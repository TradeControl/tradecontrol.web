import { test, expect } from "@playwright/test";
import { loginIfNeeded } from "./helpers/auth";
import { postForm, fetchChildrenKeys } from "../lib/treeTestUtils";

test.describe("CategoryTree - validation", () =>
{
  test.beforeEach(async ({ page }) =>
  {
    await loginIfNeeded(page);
    await page.goto("/Cash/CategoryTree/Index");
  });

  test("duplicate category code rejected", async ({ page }) =>
  {
    const baseCode = `DU_${Date.now().toString().slice(-5)}`;

    // First create (should pass)
    const form =
    {
      ParentKey: "",
      CategoryCode: baseCode,
      Category: `Dup Test ${baseCode}`,
      CashTypeCode: "1",
      CashPolarityCode: "2",
      IsEnabled: "1"
    };
    let r1 = await postForm(page, "/Cash/CategoryTree?handler=CreateCategory&embed=1", form);
    if (!r1.ok())
    {
      const alt = await postForm(page, "/Cash/CategoryTree/CreateCategory?embed=1", form);
      if (alt.ok()) r1 = alt;
    }
    expect(r1.ok(), "Initial create failed unexpectedly").toBeTruthy();

    // Second create with same code
    const r2 = await postForm(page, "/Cash/CategoryTree?handler=CreateCategory&embed=1", form);
    const raw2 = await r2.text();

    // Expect 200 with validation page OR JSON with success:false
    expect(r2.ok(), "Duplicate create did not return 200 (expected validation page)").toBeTruthy();
    const isJson = raw2.trim().startsWith("{");
    if (isJson)
    {
      let parsed: any;
      try { parsed = JSON.parse(raw2); } catch {}
      expect(parsed && parsed.success === false).toBeTruthy();
    }
    else
    {
      // HTML: look for validation message
      expect(/already exists/i.test(raw2)).toBeTruthy();
    }
  });
});
