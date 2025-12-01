import { test, expect } from "@playwright/test";
import { loginIfNeeded } from "./helpers/auth";
import { postForm, fetchChildrenKeys, waitForNodeByKey, reloadBranch } from "../lib/treeTestUtils";

test.describe("CategoryTree - disabled create (server ignores flag)", () =>
{
  test.beforeEach(async ({ page }) =>
  {
    await loginIfNeeded(page);
    await page.goto("/Cash/CategoryTree/Index");
  });

  test("create cash code with IsEnabled=false (verify creation only)", async ({ page }) =>
  {
    const cfg = page.locator("#categoryTreeConfig");
    const rootKey = (await cfg.getAttribute("data-root")) || "";
    const discKey = (await cfg.getAttribute("data-disc")) || "";

    const code = `DIS_${Date.now().toString().slice(-5)}`;
    const form =
    {
      ParentKey: "",
      CategoryCode: code,
      Category: `DisabledIntent ${code}`,
      CashTypeCode: "1",
      CashPolarityCode: "2",
      IsEnabled: "false"
    };

    let resp = await postForm(page, "/Cash/CategoryTree?handler=CreateCategory&embed=1", form);
    if (!resp.ok())
    {
      const alt = await postForm(page, "/Cash/CategoryTree/CreateCategory?embed=1", form);
      if (alt.ok()) resp = alt;
    }
    expect(resp.ok()).toBeTruthy();

    const codeKey = `code:${code}`;
    const rootChildren = await fetchChildrenKeys(page, rootKey);
    const discChildren = await fetchChildrenKeys(page, discKey);
    const branchKey = rootChildren.includes(codeKey) || rootChildren.includes(code)
      ? rootKey
      : discChildren.includes(codeKey) || discChildren.includes(code)
        ? discKey
        : discKey;

    await page.evaluate(([p, c]) =>
    {
      // @ts-ignore
      window.tcTree?.reconcileAndSelect({ parentKey: p, childKey: c });
    }, [branchKey, code]);

    await waitForNodeByKey(page, code, 12000);
    await Promise.all([reloadBranch(page, rootKey), reloadBranch(page, discKey)]);
    await waitForNodeByKey(page, code, 6000);

    const exists = await page.evaluate((k) =>
    {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const $: any = (window as any).$;
      const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
      return !!(tree?.getNodeByKey?.(k) || tree?.getNodeByKey?.("code:" + k));
    }, code);

    expect(exists).toBeTruthy();

    // Log actual persisted state for future toggle test
    const detailsResp = await page.request.get(`/Cash/CategoryTree/Details?key=${encodeURIComponent(code)}&embed=1`);
    console.log("Details snippet:", (await detailsResp.text()).substring(0, 250));
  });
});
