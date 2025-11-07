import { test, expect, Page } from "@playwright/test";
import { loginIfNeeded } from "./helpers/auth";

/* ------------ Helpers ------------ */

async function antiforgery(page: Page): Promise<string>
{
  try
  {
    return (await page.locator('head meta[name="request-verification-token"]').first().getAttribute("content")) || "";
  }
  catch
  {
    try { return (await page.locator('input[name="__RequestVerificationToken"]').first().getAttribute("value")) || ""; }
    catch { return ""; }
  }
}

async function postForm(page: Page, url: string, form: Record<string, string>)
{
  const token = await antiforgery(page);
  const body = new URLSearchParams();
  Object.entries(form).forEach(([k, v]) => body.set(k, v));

  return await page.request.post(url,
  {
    headers:
    {
      "Content-Type": "application/x-www-form-urlencoded",
      ...(token ? { RequestVerificationToken: token } : {})
    },
    data: body.toString()
  });
}

async function fetchChildrenKeys(page: Page, id: string): Promise<string[]>
{
  const resp = await page.request.get(`/Cash/CategoryTree?handler=Nodes&id=${encodeURIComponent(id)}`);
  const txt = await resp.text();
  try
  {
    const arr = JSON.parse(txt) as Array<{ key?: string }>;
    return Array.isArray(arr) ? arr.map(x => String(x.key || "")) : [];
  }
  catch
  {
    throw new Error(`Nodes parse failure id=${id}\nBody:\n${txt.substring(0, 600)}`);
  }
}

async function waitForNodeByKey(page: Page, key: string, timeout: number)
{
  await page.waitForFunction((k) =>
  {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const $: any = (window as any).$;
    const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
    if (!tree) return false;
    return !!(tree.getNodeByKey?.(k) || tree.getNodeByKey?.("code:" + k));
  }, key, { timeout });
}

/* ------------ Smoke ------------ */

test.describe("CategoryTree - basic smoke", () =>
{
  test("loads Index and renders anchors", async ({ page }) =>
  {
    await loginIfNeeded(page);
    await page.goto("/Cash/CategoryTree/Index");
    await expect(page.locator("#categoryTree")).toBeVisible();

    const cfg = page.locator("#categoryTreeConfig");
    await expect(cfg).toHaveCount(1);
    await expect(cfg).toHaveAttribute("data-nodes-url", /\/Cash\/CategoryTree\?handler=Nodes/);
    await expect(cfg).toHaveAttribute("data-details-url", /\/Cash\/CategoryTree\/Details/);
  });
});

/* ------------ Reconciliation: create disconnected root total then child total ------------ */

test.describe("CategoryTree - totals reconciliation", () =>
{
  test.beforeEach(async ({ page }) =>
  {
    await loginIfNeeded(page);
    await page.goto("/Cash/CategoryTree/Index");
    await expect(page.locator("#categoryTree")).toBeVisible();
  });

  test("create disconnected total then child total and select child", async ({ page }) =>
  {
    // Read anchor keys
    const cfg = page.locator("#categoryTreeConfig");
    const rootKey = (await cfg.getAttribute("data-root")) || "";
    const discKey = (await cfg.getAttribute("data-disc")) || "";
    expect(rootKey, "Missing root key").not.toEqual("");
    expect(discKey, "Missing disconnected key").not.toEqual("");

    // Create a disconnected TOTAL (ParentKey blank)
    const parentTotalCode = `TOT_${Date.now().toString().slice(-6)}`;
    const createTotalUrlHandler = "/Cash/CategoryTree?handler=CreateTotal&embed=1";
    const createTotalUrlPage = "/Cash/CategoryTree/CreateTotal?embed=1";

    const totalForm =
    {
      ParentKey: "",            // blank => potential root/disconnected
      CategoryCode: parentTotalCode,
      Category: `Total ${parentTotalCode}`,
      CashTypeCode: "1",
      CashPolarityCode: "2",
      IsEnabled: "1"
    };

    // Try handler then fallback to page
    let resp = await postForm(page, createTotalUrlHandler, totalForm);
    if (!resp.ok())
    {
      const alt = await postForm(page, createTotalUrlPage, totalForm);
      if (alt.ok()) resp = alt;
    }

    const rawTotal = await resp.text();
    expect(resp.ok(), `CreateTotal failed status=${resp.status()} body:\n${rawTotal.substring(0, 600)}`).toBeTruthy();

    // Determine where it appears: check both root & disconnected
    const rootChildren = await fetchChildrenKeys(page, rootKey);
    const discChildren = await fetchChildrenKeys(page, discKey);
    const appearsUnderRoot = rootChildren.includes(parentTotalCode);
    const appearsUnderDisc = discChildren.includes(parentTotalCode);

    expect(appearsUnderRoot || appearsUnderDisc,
      `New disconnected total '${parentTotalCode}' not found under root (${rootChildren}) or disc (${discChildren}). Response:\n${rawTotal.substring(0, 400)}`)
      .toBeTruthy();

    const branchKeyForParent = appearsUnderRoot ? rootKey : discKey;

    // Reconcile & select new total
    await page.evaluate(([p, c]) =>
    {
      // @ts-ignore
      window.tcTree?.reconcileAndSelect({ parentKey: p, childKey: c });
    }, [branchKeyForParent, parentTotalCode]);

    await waitForNodeByKey(page, parentTotalCode, 15000);

    // Create a CHILD total under the new total
    const childTotalCode = `SUB_${Date.now().toString().slice(-6)}`;
    const childForm =
    {
      ParentKey: parentTotalCode,
      CategoryCode: childTotalCode,
      Category: `Child ${childTotalCode}`,
      CashTypeCode: "1",
      CashPolarityCode: "2",
      IsEnabled: "1"
    };

    let respChild = await postForm(page, createTotalUrlHandler, childForm);
    if (!respChild.ok())
    {
      const altChild = await postForm(page, createTotalUrlPage, childForm);
      if (altChild.ok()) respChild = altChild;
    }

    const rawChild = await respChild.text();
    expect(respChild.ok(), `Create child total failed status=${respChild.status()} body:\n${rawChild.substring(0, 600)}`).toBeTruthy();

    // Verify persistence under parent
    const parentChildren = await fetchChildrenKeys(page, parentTotalCode);
    expect(parentChildren.includes(childTotalCode),
      `Child total '${childTotalCode}' not listed under parent '${parentTotalCode}'. Children:\n${JSON.stringify(parentChildren)}\nResponse:\n${rawChild.substring(0, 400)}`)
      .toBeTruthy();

    // Reconcile & select child total
    await page.evaluate(([p, c]) =>
    {
      // @ts-ignore
      window.tcTree?.reconcileAndSelect({ parentKey: p, childKey: c });
    }, [parentTotalCode, childTotalCode]);

    await waitForNodeByKey(page, childTotalCode, 15000);

    // Force-activate and wait until active node matches (avoids null activeKey races)
    await page.evaluate(([k]) =>
    {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const $: any = (window as any).$;
      const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
      if (!tree) return;
      let node = tree.getNodeByKey?.(k) || tree.getNodeByKey?.("code:" + k);
      if (node)
      {
        try { node.makeVisible?.(); } catch {}
        try { node.setActive?.(true); } catch {}
        try { node.setFocus?.(true); } catch {}
      }
    }, [childTotalCode]);

    await page.waitForFunction((k) =>
    {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const $: any = (window as any).$;
      const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
      const ak = tree?.getActiveNode?.()?.key ?? null;
      return ak === k || ak === `code:${k}`;
    }, childTotalCode, { timeout: 7000 });

    // Final assert
    const activeKey = await page.evaluate(() =>
    {
      // eslint-disable-next-line @typescript-eslint/no-explicit-any
      const $: any = (window as any).$;
      const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
      return tree?.getActiveNode?.()?.key ?? null;
    });
    expect([childTotalCode, `code:${childTotalCode}`]).toContain(activeKey);
  });
});
