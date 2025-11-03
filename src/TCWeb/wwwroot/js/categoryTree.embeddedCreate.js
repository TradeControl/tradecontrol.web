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
                        if (typeof node.makeVisible === "function")
                        {
                            node.makeVisible();
                        }
                    }
                    catch (e)
                    {
                    }

                    try
                    {
                        if (typeof node.setActive === "function")
                        {
                            node.setActive(true);
                        }
                    }
                    catch (e)
                    {
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
                if (typeof existing.makeVisible === "function")
                {
                    existing.makeVisible();
                }
                if (typeof existing.setActive === "function")
                {
                    existing.setActive(true);
                }
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
                    if (typeof newNode.makeVisible === "function")
                    {
                        newNode.makeVisible();
                    }
                    if (typeof newNode.setActive === "function")
                    {
                        newNode.setActive(true);
                    }

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
                // Prefer explicit data-key if server returned it (e.g. "code:ABC").
                var keyAttr = (marker.getAttribute("data-key") || "").trim();
                var cash = ((marker.getAttribute("data-cashcode") || marker.getAttribute("data-cashCode") || "")).trim();
                var category = ((marker.getAttribute("data-parent") || marker.getAttribute("data-category") || "")).trim();

                // If data-key missing but we have cash code, construct the key
                var nodeKey = keyAttr || (cash ? ("code:" + cash) : "");

                console.debug("embeddedCreate: createCodeResult marker:", {
                    keyAttr: keyAttr,
                    cash: cash,
                    category: category,
                    nodeKey: nodeKey
                });

                console.log("embeddedCreate: createCodeResult marker:", {
                    keyAttr: keyAttr,
                    cash: cash,
                    category: category,
                    nodeKey: nodeKey
                });
                if (nodeKey)
                {
                    reloadParentAndSelect(nodeKey, category || "");

                    // Fallback: ensure RHS details are populated.
                    // If detailsPane remains empty or still contains the marker, fetch details directly.
                    setTimeout(function ()
                    {
                        try
                        {
                            var pane = document.getElementById("detailsPane");
                            if (!pane) { return; }
                            var content = (pane.innerHTML || "").trim();

                            // If still empty or still showing the success marker, request details.
                            if (!content || content.indexOf("createCodeResult") !== -1 || content.indexOf("createResult") !== -1)
                            {
                                var cfg = document.getElementById("categoryTreeConfig");
                                var detailsUrl = cfg ? cfg.dataset.detailsUrl : null;
                                if (!detailsUrl) { return; }

                                var durl = detailsUrl + "?key=" + encodeURIComponent(nodeKey);
                                if (category) { durl += "&parentKey=" + encodeURIComponent(category); }
                                // use nocache helper to avoid stale result
                                fetch(_nocache(durl), { credentials: "same-origin" })
                                    .then(function (r) { return r.text(); })
                                    .then(function (html)
                                    {
                                        // Replace pane with actual details HTML
                                        pane.innerHTML = html;
                                    })
                                    .catch(function ()
                                    {
                                        // best-effort: ignore errors
                                    });
                            }
                        }
                        catch (ex) { /* swallow */ }
                    }, 500);
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
}
)();