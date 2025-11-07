import { test, expect } from "@playwright/test";
import { loginIfNeeded } from "./helpers/auth";
import { postForm, fetchChildrenKeys, waitForNodeByKey, reloadBranch } from "../lib/treeTestUtils";

async function nodeLists(page: any, rootKey: string, discKey: string)
{
  const [rootChildren, discChildren] = await Promise.all([
    fetchChildrenKeys(page, rootKey),
    fetchChildrenKeys(page, discKey)
  ]);
  return { rootChildren, discChildren };
}

test.describe("CategoryTree - delete totals (soft behavior)", () =>
{
  test.beforeEach(async ({ page }) =>
  {
    await loginIfNeeded(page);
    await page.goto("/Cash/CategoryTree/Index");
    await expect(page.locator("#categoryTree")).toBeVisible();
  });

  test("create then attempt delete (accept disabled or removed)", async ({ page }) =>
  {
    const cfg = page.locator("#categoryTreeConfig");
    const rootKey = (await cfg.getAttribute("data-root")) || "";
    const discKey = (await cfg.getAttribute("data-disc")) || "";

    const code = `DEL_${Date.now().toString().slice(-6)}`;
    const createForm =
    {
      ParentKey: "",
      CategoryCode: code,
      Category: `Del ${code}`,
      CashTypeCode: "1",
      CashPolarityCode: "2",
      IsEnabled: "true"
    };

    let resp = await postForm(page, "/Cash/CategoryTree?handler=CreateTotal&embed=1", createForm);
    if (!resp.ok())
    {
      const alt = await postForm(page, "/Cash/CategoryTree/CreateTotal?embed=1", createForm);
      if (alt.ok()) resp = alt;
    }
    expect(resp.ok()).toBeTruthy();

    const { rootChildren, discChildren } = await nodeLists(page, rootKey, discKey);
    const branchKey = rootChildren.includes(code) ? rootKey :
                      discChildren.includes(code) ? discKey : discKey;

    await page.evaluate(([p, c]) =>
    {
      // @ts-ignore
      window.tcTree?.reconcileAndSelect({ parentKey: p, childKey: c });
    }, [branchKey, code]);

    await waitForNodeByKey(page, code, 12000);

    const delParams = { key: code, CategoryCode: code, categoryCode: code, ChildCode: code };
    let del = await postForm(page, "/Cash/CategoryTree?handler=Delete&embed=1", delParams);
    if (!del.ok())
    {
      const altDel = await postForm(page, "/Cash/CategoryTree/Delete?embed=1", delParams);
      if (altDel.ok()) del = altDel;
    }
    const delRaw = await del.text();
    expect(del.ok(), `Delete failed HTTP ${del.status()} body:\n${delRaw.substring(0, 400)}`).toBeTruthy();

    // Reload both anchors
    await Promise.all([reloadBranch(page, rootKey), reloadBranch(page, discKey)]);
    const { rootChildren: rootAfter, discChildren: discAfter } = await nodeLists(page, rootKey, discKey);
    const stillPresent = rootAfter.includes(code) || discAfter.includes(code);

    if (!stillPresent)
    {
      expect(stillPresent).toBeFalsy(); // removed
      return;
    }

    // Check disabled flag if not removed
    const state = await page.evaluate((k) =>
    {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const $: any = (window as any).$;
      const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
      const n = tree?.getNodeByKey?.(k) || tree?.getNodeByKey?.("code:" + k);
      if (!n) return { exists: false, isEnabled: 1 };
      return { exists: true, isEnabled: Number(n.data?.isEnabled ?? 1) };
    }, code);

    expect(
      state.exists && state.isEnabled === 0,
      `Node still enabled (soft delete not applied).
RootAfter=${JSON.stringify(rootAfter)}
DiscAfter=${JSON.stringify(discAfter)}`
    ).toBeTruthy();
  });
});
