import { Page, APIResponse } from "@playwright/test";

export async function antiforgery(page: Page): Promise<string>
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

export async function postForm(page: Page, url: string, form: Record<string, string>): Promise<APIResponse>
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

export async function fetchChildrenKeys(page: Page, id: string): Promise<string[]>
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

export async function waitForNodeByKey(page: Page, key: string, timeout: number)
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

export async function activateNode(page: Page, key: string)
{
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
  }, [key]);
}

export async function assertActive(page: Page, key: string)
{
  const activeKey = await page.evaluate(() =>
  {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const $: any = (window as any).$;
    const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
    return tree?.getActiveNode?.()?.key ?? null;
  });
  if (![key, `code:${key}`].includes(activeKey))
    throw new Error(`Active key mismatch. Expected ${key} or code:${key}, got ${activeKey}`);
}

export async function reloadBranch(page: Page, key: string)
{
  await page.evaluate((k) =>
  {
    // eslint-disable-next-line @typescript-eslint/no-explicit-any
    const $: any = (window as any).$;
    const tree = $?.ui?.fancytree?.getTree?.("#categoryTree");
    const node = tree?.getNodeByKey?.(k);
    if (node?.reloadChildren)
    {
      try { node.reloadChildren(); } catch {}
    }
  }, key);
}
