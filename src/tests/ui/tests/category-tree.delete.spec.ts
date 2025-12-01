import { test, expect, Page } from "@playwright/test";
import { loginIfNeeded } from "./helpers/auth";
import { postForm, fetchChildrenKeys, waitForNodeByKey, reloadBranch } from "../lib/treeTestUtils";

function isSynthetic(key: string | null | undefined): boolean
{
  if (!key) return true;
  return key === "__DISCONNECTED__"
      || key === "__ROOT__"
      || /^root_\d+$/i.test(key)
      || key.startsWith("type:");
}

async function nodeLists(page: Page, rootKey: string, discKey: string)
{
  const [rootChildren, discChildren] = await Promise.all([
    fetchChildrenKeys(page, rootKey),
    fetchChildrenKeys(page, discKey)
  ]);
  return { rootChildren, discChildren };
}

test.describe("CategoryTree - delete totals/categories (disconnected vs parented)", () =>
{
  test.beforeEach(async ({ page }) =>
  {
    await loginIfNeeded(page);
    await page.goto("/Cash/CategoryTree/Index");
    await expect(page.locator("#categoryTree")).toBeVisible();
  });

  test("create disconnected total then delete appropriately", async ({ page }) =>
  {
    // Anchors
    const cfg = page.locator("#categoryTreeConfig");
    const rootKey = (await cfg.getAttribute("data-root")) || "";
    const discKey = (await cfg.getAttribute("data-disc")) || "";
    expect(rootKey).not.toEqual("");
    expect(discKey).not.toEqual("");

    // Create disconnected TOTAL
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
    expect(resp.ok(), "CreateTotal failed").toBeTruthy();

    // Which branch lists it?
    const { rootChildren, discChildren } = await nodeLists(page, rootKey, discKey);
    const inRoot = rootChildren.includes(code);
    const inDisc = discChildren.includes(code);
    const branchKey = inRoot ? rootKey : discKey;

    // Select
    await page.evaluate(([p, c]) =>
    {
      // @ts-ignore
      window.tcTree?.reconcileAndSelect({ parentKey: p, childKey: c });
    }, [branchKey, code]);

    await waitForNodeByKey(page, code, 12000);

    // Derive the real parent key from the tree (not the synthetic anchors)
    const realParentKey = await page.evaluate((k) =>
    {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const $: any = (window as any).$;
      const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
      if (!tree) return "";
      let n = tree.getNodeByKey?.(k) || tree.getNodeByKey?.("code:" + k);
      if (!n) return "";
      const p = n.getParent?.();
      return p && p.key ? String(p.key) : "";
    }, code);

    // Determine delete strategy:
    // - If parent is synthetic/empty (disconnected root), delete CATEGORY.
    // - Otherwise (child under a real parent), delete TOTAL using that parent.
    type Attempt = { url: string, payload: Record<string, string>, note: string };

    const attempts: Attempt[] = [];

    if (isSynthetic(realParentKey) || inDisc)
    {
      // Disconnected: Delete the category itself
      attempts.push(
      {
        url: "/Cash/CategoryTree?handler=DeleteCategory&embed=1",
        payload: { key: code, CategoryCode: code, categoryCode: code },
        note: "handler-DeleteCategory"
      });
      attempts.push(
      {
        url: "/Cash/CategoryTree/DeleteCategory?embed=1",
        payload: { key: code, CategoryCode: code, categoryCode: code },
        note: "page-DeleteCategory"
      });
    }
    else
    {
      // Parented: remove the total edge under real parent
      attempts.push(
      {
        url: "/Cash/CategoryTree?handler=DeleteTotal&embed=1",
        payload: { parentKey: realParentKey, childKey: code },
        note: "handler-DeleteTotal"
      });
      attempts.push(
      {
        url: "/Cash/CategoryTree/DeleteTotal?embed=1",
        payload: { parentKey: realParentKey, childKey: code },
        note: "page-DeleteTotal"
      });
    }

    // Execute attempts with diagnostics
    let ok = false;
    let lastDiag = "";

    for (const a of attempts)
    {
      const res = await postForm(page, a.url, a.payload);
      const body = await res.text();
      lastDiag = `Attempt=${a.note} status=${res.status()} url=${a.url}\nPayload=${JSON.stringify(a.payload)}\nBody=${body.substring(0, 600)}`;

      if (!res.ok())
      {
        continue;
      }

      if (body.trim().startsWith("{"))
      {
        try
        {
          const j = JSON.parse(body);
          if ("success" in j && j.success === true)
          {
            ok = true;
            break;
          }
        }
        catch { /* fall through */ }
      }

      // Accept 200; will verify by absence after reload
      ok = true;
      break;
    }

    expect(ok, `Delete request(s) did not indicate success.\n${lastDiag}`).toBeTruthy();

    // Reload both anchors and verify absence across both lists
    await Promise.all([reloadBranch(page, rootKey), reloadBranch(page, discKey)]);
    const { rootChildren: rootAfter, discChildren: discAfter } = await nodeLists(page, rootKey, discKey);
    const stillPresent = rootAfter.includes(code) || discAfter.includes(code);

    expect(stillPresent,
`Node still present after delete.
ParentUsed=${realParentKey || "(none)"}
InRootBefore=${inRoot} InDiscBefore=${inDisc}
${lastDiag}
RootAfter=${JSON.stringify(rootAfter)}
DiscAfter=${JSON.stringify(discAfter)}`
    ).toBeFalsy();
  });
});
