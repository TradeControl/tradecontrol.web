(function ()
{
    // Page config (used to force a fresh nodes load when reloading a parent)
    var _cfgEl = document.getElementById("categoryTreeConfig");
    var _nodesUrl = _cfgEl ? _cfgEl.dataset.nodesUrl : null;

    function _appendQuery(url, key, value)
    {
        var sep = url.indexOf('?') === -1 ? '?' : '&';
        return url + sep + encodeURIComponent(key) + '=' + encodeURIComponent(value);
    }

    function _nocache(url)
    {
        if (!url)
        {
            return url;
        }

        return _appendQuery(url, '_', Date.now());
    }

    // lightweight global-safe helpers (avoid deprecated plugin call)
    function getTreeGlobal()
    {
        try
        {
            if (!window.$ || !window.$.ui || !window.$.ui.fancytree)
            {
                return null;
            }

            var el = document.querySelector("#categoryTree");
            if (!el)
            {
                return null;
            }

            return $.ui.fancytree.getTree(el);
        }
        catch (e)
        {
            return null;
        }
    }

    // small safe-invoke helpers to keep code compact and avoid repeated try/catch formatting
    function safeInvoke(target, methodName)
    {
        try
        {
            if (target && typeof target[methodName] === "function")
            {
                target[methodName]();
            }
        }
        catch (_)
        {
            // swallow
        }
    }

    function safeInvokeWithArg(target, methodName, arg)
    {
        try
        {
            if (target && typeof target[methodName] === "function")
            {
                target[methodName](arg);
            }
        }
        catch (_)
        {
            // swallow
        }
    }

    // Ensure a fancytree node is expanded and its children reloaded (returns a Promise).
    // Uses a forced nocache nodesUrl when available to avoid stale cached child lists.
    function ensureNodeExpandedAndReload(node)
    {
        return new Promise(function (resolve)
        {
            if (!node)
            {
                resolve();
                return;
            }

            function afterReload()
            {
                // Give a small delay to allow lazy children to populate.
                setTimeout(resolve, 120);
            }

            try
            {
                if (!node.expanded)
                {
                    var res = node.setExpanded(true);

                    if (res && typeof res.then === "function")
                    {
                        res.then(function ()
                        {
                            if (typeof node.reloadChildren === "function")
                            {
                                try
                                {
                                    if (_nodesUrl)
                                    {
                                        var url = _nocache(_appendQuery(_nodesUrl, 'id', node.key));
                                        var r2 = node.reloadChildren({ url: url });

                                        if (r2 && typeof r2.then === "function")
                                        {
                                            r2.then(afterReload, afterReload);
                                        }
                                        else if (r2 && r2.done)
                                        {
                                            r2.done(afterReload).fail(afterReload);
                                        }
                                        else
                                        {
                                            afterReload();
                                        }
                                    }
                                    else
                                    {
                                        var r2 = node.reloadChildren();

                                        if (r2 && typeof r2.then === "function")
                                        {
                                            r2.then(afterReload, afterReload);
                                        }
                                        else if (r2 && r2.done)
                                        {
                                            r2.done(afterReload).fail(afterReload);
                                        }
                                        else
                                        {
                                            afterReload();
                                        }
                                    }
                                }
                                catch (ex)
                                {
                                    afterReload();
                                }
                            }
                            else
                            {
                                afterReload();
                            }
                        }, afterReload);

                        return;
                    }
                    else if (res && typeof res.done === "function")
                    {
                        res.done(function ()
                        {
                            if (typeof node.reloadChildren === "function")
                            {
                                try
                                {
                                    if (_nodesUrl)
                                    {
                                        var url2 = _nocache(_appendQuery(_nodesUrl, 'id', node.key));
                                        var r2 = node.reloadChildren({ url: url2 });

                                        if (r2 && typeof r2.then === "function")
                                        {
                                            r2.then(afterReload, afterReload);
                                        }
                                        else if (r2 && r2.done)
                                        {
                                            r2.done(afterReload).fail(afterReload);
                                        }
                                        else
                                        {
                                            afterReload();
                                        }
                                    }
                                    else
                                    {
                                        var r2 = node.reloadChildren();

                                        if (r2 && typeof r2.then === "function")
                                        {
                                            r2.then(afterReload, afterReload);
                                        }
                                        else if (r2 && r2.done)
                                        {
                                            r2.done(afterReload).fail(afterReload);
                                        }
                                        else
                                        {
                                            afterReload();
                                        }
                                    }
                                }
                                catch (ex)
                                {
                                    afterReload();
                                }
                            }
                            else
                            {
                                afterReload();
                            }
                        }).fail(afterReload);

                        return;
                    }
                }

                if (typeof node.reloadChildren === "function")
                {
                    try
                    {
                        if (_nodesUrl)
                        {
                            var url3 = _nocache(_appendQuery(_nodesUrl, 'id', node.key));
                            var r = node.reloadChildren({ url: url3 });

                            if (r && typeof r.then === "function")
                            {
                                r.then(afterReload, afterReload);
                            }
                            else if (r && r.done)
                            {
                                r.done(afterReload).fail(afterReload);
                            }
                            else
                            {
                                afterReload();
                            }
                        }
                        else
                        {
                            var r = node.reloadChildren();

                            if (r && typeof r.then === "function")
                            {
                                r.then(afterReload, afterReload);
                            }
                            else if (r && r.done)
                            {
                                r.done(afterReload).fail(afterReload);
                            }
                            else
                            {
                                afterReload();
                            }
                        }
                    }
                    catch (ex)
                    {
                        afterReload();
                    }
                }
                else
                {
                    afterReload();
                }
            }
            catch (ex)
            {
                // best-effort
                setTimeout(resolve, 200);
            }
        });
    }

    function reloadParentAndSelect(key, parentKey)
    {
        var tree = getTreeGlobal();
        if (!tree)
        {
            return;
        }

        function trySelectNode(keyToFind, attemptsLeft)
        {
            try
            {
                var node = tree.getNodeByKey(keyToFind);

                if (!node && typeof keyToFind === "string" && !keyToFind.startsWith("code:"))
                {
                    node = tree.getNodeByKey("code:" + keyToFind);
                }

                if (node)
                {
                    try
                    {
                        safeInvoke(node, "makeVisible");
                    }
                    catch (e)
                    {
                        // swallow
                    }

                    try
                    {
                        safeInvokeWithArg(node, "setActive", true);
                    }
                    catch (e)
                    {
                        // swallow
                    }

                    try
                    {
                        var el = (typeof node.getEventTarget === "function") ? node.getEventTarget() : null;

                        if (el && el.scrollIntoView)
                        {
                            el.scrollIntoView({ block: "nearest", inline: "nearest" });
                        }
                    }
                    catch (e)
                    {
                        // swallow
                    }

                    return true;
                }

                if (attemptsLeft <= 0)
                {
                    return false;
                }

                setTimeout(function ()
                {
                    trySelectNode(keyToFind, attemptsLeft - 1);
                }, 300);
            }
            catch (ex)
            {
                if (attemptsLeft > 0)
                {
                    setTimeout(function ()
                    {
                        trySelectNode(keyToFind, attemptsLeft - 1);
                    }, 300);
                }
            }

            return false;
        }

        if (parentKey)
        {
            var parentNode = tree.getNodeByKey(parentKey);

            if (!parentNode)
            {
                var root = tree.getRootNode();
                var promises = [];

                if (root && root.children)
                {
                    for (var i = 0; i < root.children.length; i++)
                    {
                        var top = root.children[i];

                        if (top && top.expanded && typeof top.reloadChildren === "function")
                        {
                            (function (n)
                            {
                                promises.push(new Promise(function (res)
                                {
                                    try
                                    {
                                        if (_nodesUrl)
                                        {
                                            var url = _nocache(_appendQuery(_nodesUrl, 'id', n.key));
                                            var r = n.reloadChildren({ url: url });

                                            if (r && typeof r.then === "function")
                                            {
                                                r.then(res, res);
                                            }
                                            else if (r && r.done)
                                            {
                                                r.done(res).fail(res);
                                            }
                                            else
                                            {
                                                setTimeout(res, 150);
                                            }
                                        }
                                        else
                                        {
                                            var r = n.reloadChildren();

                                            if (r && typeof r.then === "function")
                                            {
                                                r.then(res, res);
                                            }
                                            else if (r && r.done)
                                            {
                                                r.done(res).fail(res);
                                            }
                                            else
                                            {
                                                setTimeout(res, 150);
                                            }
                                        }
                                    }
                                    catch (e)
                                    {
                                        setTimeout(res, 150);
                                    }
                                }));
                            })(top);
                        }
                    }
                }

                Promise.all(promises).then(function ()
                {
                    setTimeout(function ()
                    {
                        trySelectNode(key, 5);
                    }, 300);
                }).catch(function ()
                {
                    setTimeout(function ()
                    {
                        trySelectNode(key, 5);
                    }, 300);
                });

                return;
            }

            ensureNodeExpandedAndReload(parentNode)
                .then(function ()
                {
                    trySelectNode(key, 6);
                })
                .catch(function ()
                {
                    trySelectNode(key, 6);
                });

            return;
        }

        try
        {
            var root = tree.getRootNode();
            var topPromises = [];

            if (root && root.children)
            {
                for (var j = 0; j < root.children.length; j++)
                {
                    var n = root.children[j];

                    if (n && n.expanded && typeof n.reloadChildren === "function")
                    {
                        (function (nn)
                        {
                            topPromises.push(new Promise(function (res)
                            {
                                try
                                {
                                    if (_nodesUrl)
                                    {
                                        var url = _nocache(_appendQuery(_nodesUrl, 'id', nn.key));
                                        var r = nn.reloadChildren({ url: url });

                                        if (r && typeof r.then === "function")
                                        {
                                            r.then(res, res);
                                        }
                                        else if (r && r.done)
                                        {
                                            r.done(res).fail(res);
                                        }
                                        else
                                        {
                                            setTimeout(res, 150);
                                        }
                                    }
                                    else
                                    {
                                        var r = nn.reloadChildren();

                                        if (r && typeof r.then === "function")
                                        {
                                            r.then(res, res);
                                        }
                                        else if (r && r.done)
                                        {
                                            r.done(res).fail(res);
                                        }
                                        else
                                        {
                                            setTimeout(res, 150);
                                        }
                                    }
                                }
                                catch (e)
                                {
                                    setTimeout(res, 150);
                                }
                            }));
                        })(n);
                    }
                }
            }

            Promise.all(topPromises).then(function ()
            {
                setTimeout(function ()
                {
                    trySelectNode(key, 6);
                }, 300);
            }).catch(function ()
            {
                setTimeout(function ()
                {
                    trySelectNode(key, 6);
                }, 300);
            });
        }
        catch (ex)
        {
            setTimeout(function ()
            {
                trySelectNode(key, 6);
            }, 300);
        }
    }

    // Build a temporary node that mirrors server node shape (icon, title, data) so context menu works immediately.
    function tryInsertAndSelectUnderParent(tree, parentNode, key, title, polarityCode, categoryTypeCode, isEnabled)
    {
        if (!tree || !parentNode || !key)
        {
            return false;
        }

        function escapeHtml(str)
        {
            if (!str && str !== 0)
            {
                return "";
            }

            return String(str)
                .replace(/&/g, "&amp;")
                .replace(/</g, "&lt;")
                .replace(/>/g, "&gt;")
                .replace(/"/g, "&quot;")
                .replace(/'/g, "&#039;");
        }

        try
        {
            var existing = tree.getNodeByKey(key);

            if (existing)
            {
                safeInvoke(existing, "makeVisible");
                safeInvokeWithArg(existing, "setActive", true);
                return true;
            }

            if (parentNode && parentNode.expanded && typeof parentNode.addChildren === "function")
            {
                var safeTitle = escapeHtml(title || key);
                var safeCode = escapeHtml(key);
                var polClass = (Number(polarityCode) === 0) ? "expense" : (Number(polarityCode) === 1) ? "income" : "neutral";

                // Use same markup as server nodes so icon and title match
                var prefixedTitle = "<span class='tc-cat-icon tc-cat-" + polClass + "'></span> " + safeTitle + " (" + safeCode + ")";

                var newNodes = parentNode.addChildren(
                {
                    title: prefixedTitle,
                    key: key,
                    folder: true,
                    lazy: true,
                    icon: false,
                    extraClasses: (Number(isEnabled) === 0) ? "tc-disabled" : null,
                    data:
                    {
                        cashPolarity: Number(polarityCode),
                        categoryType: Number(categoryTypeCode),
                        nodeType: "category",
                        isEnabled: Number(isEnabled) === 0 ? 0 : 1,
                        categoryCode: key
                    }
                });

                var newNode = (newNodes && newNodes.length) ? newNodes[0] : null;

                if (newNode)
                {
                    safeInvoke(newNode, "makeVisible");
                    safeInvokeWithArg(newNode, "setActive", true);

                    // Schedule a refresh of parent's children to replace the temporary node with server-rendered node.
                    try
                    {
                        setTimeout(function ()
                        {
                            try
                            {
                                if (_nodesUrl && typeof parentNode.reloadChildren === "function")
                                {
                                    var url = _nocache(_appendQuery(_nodesUrl, 'id', parentNode.key));
                                    parentNode.reloadChildren({ url: url });
                                }
                                else if (typeof parentNode.reloadChildren === "function")
                                {
                                    parentNode.reloadChildren();
                                }
                            }
                            catch (e)
                            {
                                // ignore reload errors
                            }
                        }, 450);
                    }
                    catch (ex)
                    {
                        // ignore scheduling errors
                    }

                    return true;
                }
            }
        }
        catch (ex)
        {
            console.warn("tryInsertAndSelectUnderParent failed", ex);
        }

        return false;
    }

    function processMarker(marker)
    {
        if (!marker)
        {
            return;
        }

        var id = marker.id || "";

        if (id === "createResult" || id === "createCategoryResult")
        {
            // existing handling unchanged...
            var key = (marker.getAttribute("data-key") || "").trim();
            var parent = (marker.getAttribute("data-parent") || "").trim();
            var name = (marker.getAttribute("data-name") || "").trim();
            var polarity = parseInt((marker.getAttribute("data-polarity") || "2").trim(), 10);
            var categoryType = parseInt((marker.getAttribute("data-categorytype") || "0").trim(), 10);
            var isEnabled = (marker.getAttribute("data-isenabled") || "1").trim();

            if (!key)
            {
                return;
            }

            var tree = getTreeGlobal();

            if (tree && parent)
            {
                var parentNode = tree.getNodeByKey(parent);

                if (parentNode && parentNode.expanded)
                {
                    var ok = tryInsertAndSelectUnderParent(tree, parentNode, key, name, polarity, categoryType, isEnabled);

                    if (ok)
                    {
                        return;
                    }
                }
            }

            reloadParentAndSelect(key, parent);
        }
        else if (id === "createCodeResult")
        {
            try
            {
                // read and trim attributes
                var keyAttr = (marker.getAttribute("data-key") || "").trim();
                var cash = ((marker.getAttribute("data-cashcode") || marker.getAttribute("data-cashCode") || "")).trim();
                var category = ((marker.getAttribute("data-parent") || marker.getAttribute("data-category") || "")).trim();

                // If server provided a full node JSON, prefer that.
                var nodeJson = marker.getAttribute("data-node") || "";
                var nodeKey = keyAttr || (cash ? ("code:" + cash) : "");

                console.debug("embeddedCreate: createCodeResult marker:", {
                    keyAttr: keyAttr,
                    cash: cash,
                    category: category,
                    nodeKey: nodeKey,
                    nodeJsonPresent: !!nodeJson
                });

                var tree = getTreeGlobal();

                // If server sent the exact node JSON, insert it directly (preserves icon/title/data exactly)
                if (nodeJson && tree)
                {
                    try
                    {
                        var parsed = JSON.parse(nodeJson);
                        var parsedKey = (parsed && parsed.key) ? parsed.key : nodeKey;
                        var parentNode = category ? tree.getNodeByKey(category) : null;

                        // If node already exists, update it instead of adding a duplicate
                        var existing = tree.getNodeByKey(parsedKey);

                        if (existing)
                        {
                            // update title and data and classes, then render
                            try
                            {
                                existing.title = parsed.title || existing.title;
                            }
                            catch (_)
                            {
                                // swallow
                            }

                            try
                            {
                                existing.data = parsed.data || existing.data;
                            }
                            catch (_)
                            {
                                // swallow
                            }

                            try
                            {
                                if (parsed.extraClasses)
                                {
                                    existing.li && existing.li.classList && existing.li.classList.add(parsed.extraClasses);
                                }
                                else
                                {
                                    // remove tc-disabled if previously set and new node is enabled
                                    existing.li && existing.li.classList && existing.li.classList.remove("tc-disabled");
                                }
                            }
                            catch (_)
                            {
                                // swallow
                            }

                            safeInvoke(existing, "makeVisible");
                            safeInvokeWithArg(existing, "setActive", true);
                            safeInvoke(existing, "renderTitle");
                            return;
                        }

                        if (parentNode && typeof parentNode.addChildren === "function")
                        {
                            // addChildren expects an array or single object
                            var added = parentNode.addChildren(parsed);
                            // ensure selection of newly-added node
                            var newNode = tree.getNodeByKey(parsedKey) || (added && added.length ? added[0] : null);

                            if (newNode)
                            {
                                safeInvoke(newNode, "makeVisible");
                                safeInvokeWithArg(newNode, "setActive", true);
                                safeInvoke(newNode, "renderTitle");
                                return; // done
                            }
                        }
                        else
                        {
                            // no parent found - attempt to add at root level (fallback)
                            var root = tree.getRootNode();

                            if (root && typeof root.addChildren === "function")
                            {
                                var addedRoot = root.addChildren(parsed);
                                var newNode2 = tree.getNodeByKey(parsedKey) || (addedRoot && addedRoot.length ? addedRoot[0] : null);

                                if (newNode2)
                                {
                                    safeInvoke(newNode2, "makeVisible");
                                    safeInvokeWithArg(newNode2, "setActive", true);
                                    safeInvoke(newNode2, "renderTitle");
                                    return;
                                }
                            }
                        }
                    }
                    catch (ex)
                    {
                        console.warn("embeddedCreate: failed to parse/apply data-node JSON", ex);
                    }
                }

                // Fallback to existing behavior (try insert placeholder / reload parent & select)
                if (nodeKey)
                {
                    reloadParentAndSelect(nodeKey, category || "");
                }
            }
            catch (ex)
            {
                console.warn("embeddedCreate: error processing createCodeResult", ex);
            }
        }
    }

    function scanDetailsPaneForMarkers(el)
    {
        if (!el)
        {
            return;
        }

        var m = null;
        m = el.querySelector("#createResult, #createCategoryResult, #createCodeResult");

        if (!m && el.id && (el.id === "createResult" || el.id === "createCategoryResult" || el.id === "createCodeResult"))
        {
            m = el;
        }

        if (m)
        {
            processMarker(m);
        }
    }

    // Intercept embedded form submits inside detailsPane and POST via AJAX (keeps top-level page intact).
    function bindEmbeddedFormSubmit()
    {
        var pane = document.getElementById("detailsPane");

        if (!pane)
        {
            return;
        }

        pane.addEventListener("submit", function (e)
        {
            var form = e.target;

            if (!form || form.tagName !== "FORM")
            {
                return;
            }

            var actionUrl = form.getAttribute("action") || window.location.href;

            if (actionUrl.indexOf("embed=1") === -1 && !form.querySelector("input[name='embed'][value='1']"))
            {
                return;
            }

            e.preventDefault();
            e.stopPropagation();

            try
            {
                var fd = new FormData(form);

                fetch(actionUrl, {
                    method: "POST",
                    body: fd,
                    credentials: "same-origin",
                    headers: {
                        "X-Requested-With": "XMLHttpRequest"
                    }
                }).then(function (resp)
                {
                    return resp.text().then(function (text)
                    {
                        pane.innerHTML = text;

                        try
                        {
                            console.debug("Embedded form response loaded into detailsPane", actionUrl);
                            console.log("Embedded form response loaded into detailsPane", actionUrl);
                        }
                        catch (ex)
                        {
                            // swallow
                        }

                        scanDetailsPaneForMarkers(pane);
                    });
                }).catch(function (err)
                {
                    pane.innerHTML = "<div class='text-danger p-2 small'>Failed to submit form (network)</div>";
                    console.error("Embedded submit failed", err);
                });
            }
            catch (ex)
            {
                console.error("Embedded submit exception", ex);
            }
        });
    }

    document.addEventListener("DOMContentLoaded", function ()
    {
        var pane = document.getElementById("detailsPane");

        if (!pane)
        {
            return;
        }

        scanDetailsPaneForMarkers(pane);
        bindEmbeddedFormSubmit();

        var mo = new MutationObserver(function (mutations)
        {
            mutations.forEach(function (m)
            {
                if (m.addedNodes && m.addedNodes.length)
                {
                    for (var i = 0; i < m.addedNodes.length; i++)
                    {
                        var n = m.addedNodes[i];

                        if (n.nodeType === 1)
                        {
                            scanDetailsPaneForMarkers(n);
                        }
                    }
                }
            });
        });

        mo.observe(pane, { childList: true, subtree: true });
    });


    window.tcRefreshActiveNode = function ()
    {
        try
        {
            var tree = getTreeGlobal();
            var pane = document.getElementById("detailsPane");

            // If not embedded, fall back to index
            if (!pane)
            {
                try
                {
                    window.location.href = "/Cash/CategoryTree/Index";
                }
                catch (_)
                {
                    // swallow
                }
                return;
            }

            if (!tree || typeof tree.getActiveNode !== "function")
            {
                pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                return;
            }

            var active = tree.getActiveNode();
            if (!active || !active.key)
            {
                pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                return;
            }

            // Determine parentKey for reloadParentAndSelect (best-effort)
            var parentKey = "";
            try
            {
                var pnode = active.getParent && active.getParent();
                parentKey = (pnode && pnode.key) ? pnode.key : "";
            }
            catch (_)
            {
                parentKey = "";
            }

            // Use the tree's existing reloadParentAndSelect which handles lazy loads and selection safely.
            try
            {
                reloadParentAndSelect(active.key, parentKey);
            }
            catch (ex)
            {
                // If that fails, fallback to a safe fetch (no redirects)
                var url = "/Cash/CategoryTree/Details?key=" + encodeURIComponent(active.key) + "&embed=1";
                fetch(url, { credentials: "same-origin", redirect: "manual" })
                    .then(function (resp)
                    {
                        if (!resp || resp.type === "opaqueredirect" || (resp.status >= 300 && resp.status < 400) || resp.status !== 200)
                        {
                            throw new Error("bad response");
                        }
                        return resp.text();
                    })
                    .then(function (html) { pane.innerHTML = html; })
                    .catch(function () { pane.innerHTML = "<div class='text-muted small p-2'>No details</div>"; });
            }
        }
        catch (_)
        {
            try
            {
                window.location.href = "/Cash/CategoryTree/Index";
            }
            catch (_) { /* swallow */ }
        }
    };

    // delegated embedded cancel handler — inside the IIFE, after tcRefreshActiveNode is defined
    document.addEventListener("click", function (e)
    {
        try
        {
            var btn = e.target.closest && e.target.closest("[data-embedded-cancel]");
            if (!btn)
            {
                return;
            }

            e.preventDefault();

            if (typeof window.tcRefreshActiveNode === "function")
            {
                window.tcRefreshActiveNode();
            }
            else if (typeof window.tcEmbeddedReloadActive === "function")
            {
                window.tcEmbeddedReloadActive();
            }
            else
            {
                // last-resort: show placeholder so user doesn't get redirected unexpectedly
                var p = document.getElementById("detailsPane");
                if (p) 
                {
                    p.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                }
            }
        }
        catch (_)
        {
            // swallow
        }
    }, true);
}
)();