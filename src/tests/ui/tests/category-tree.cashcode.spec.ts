import { test, expect } from "@playwright/test";
import { loginIfNeeded } from "./helpers/auth";
import { postForm, fetchChildrenKeys, waitForNodeByKey, activateNode, assertActive } from "../lib/treeTestUtils";

test.describe("CategoryTree - cash codes", () =>
{
  test.beforeEach(async ({ page }) =>
  {
    await loginIfNeeded(page);
    await page.goto("/Cash/CategoryTree/Index");
    await expect(page.locator("#categoryTree")).toBeVisible();
  });

  test("create cash code (no parent) and select", async ({ page }) =>
  {
    const cfg = page.locator("#categoryTreeConfig");
    const rootKey = (await cfg.getAttribute("data-root")) || "";
    const discKey = (await cfg.getAttribute("data-disc")) || "";
    expect(rootKey).not.toEqual("");
    expect(discKey).not.toEqual("");

    const code = `CC_${Date.now().toString().slice(-5)}`;
    const form =
    {
      ParentKey: "",            // blank: appears under root or disc as code:<code>
      CategoryCode: code,
      Category: `Cash ${code}`,
      CashTypeCode: "1",
      CashPolarityCode: "2",
      IsEnabled: "1"
    };

    let resp = await postForm(page, "/Cash/CategoryTree?handler=CreateCategory&embed=1", form);
    if (!resp.ok())
    {
      const alt = await postForm(page, "/Cash/CategoryTree/CreateCategory?embed=1", form);
      if (alt.ok()) resp = alt;
    }
    const raw = await resp.text();
    expect(resp.ok(), `Create cash code failed: ${resp.status()} body:\n${raw.substring(0, 400)}`).toBeTruthy();

    const codeKey = `code:${code}`;
    const rootChildren = await fetchChildrenKeys(page, rootKey);
    const discChildren = await fetchChildrenKeys(page, discKey);
    const underRoot = rootChildren.includes(codeKey) || rootChildren.includes(code);
    const underDisc = discChildren.includes(codeKey) || discChildren.includes(code);
    expect(underRoot || underDisc,
      `New cash code not found. ROOT=${JSON.stringify(rootChildren)} DISC=${JSON.stringify(discChildren)}`
    ).toBeTruthy();

    const branchKey = underRoot ? rootKey : discKey;

    await page.evaluate(([p, c]) =>
    {
      // @ts-ignore
      window.tcTree?.reconcileAndSelect({ parentKey: p, childKey: c });
    }, [branchKey, code]);

    await waitForNodeByKey(page, code, 12000);
    await activateNode(page, code);
    await assertActive(page, code);
  });
});
