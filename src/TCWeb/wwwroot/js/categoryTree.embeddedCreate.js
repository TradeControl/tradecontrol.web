(function ()
{
    var _cfgEl = document.getElementById("categoryTreeConfig");
    var _nodesUrl = _cfgEl ? _cfgEl.dataset.nodesUrl : null;
    var _debug = !!(window.tcTree && window.tcTree.debug);

    exposeTestHooks();

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

    function noThrow(action)
    {
        try
        {
            if (typeof action === "function")
            {
                action();
            }
        }
        catch (e)
        {
        }
    }

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

    function safeInvoke(target, methodName)
    {
        noThrow(function ()
        {
            if (target && typeof target[methodName] === "function")
            {
                target[methodName]();
            }
        });
    }

    function safeInvokeWithArg(target, methodName, arg)
    {
        noThrow(function ()
        {
            if (target && typeof target[methodName] === "function")
            {
                target[methodName](arg);
            }
        });
    }

    // --- New unified reconciliation helper (extracted) ---
    function reconcileAndSelect(opts)
    {
        // opts: { parentKey, childKey, name?, polarity?, categoryType?, isEnabled? }
        if (!opts || !opts.childKey)
        {
            return;
        }

        var parentKey = (opts.parentKey || "").trim();
        var childKey = (opts.childKey || "").trim();
        var name = opts.name;
        var polarity = opts.polarity;
        var categoryType = opts.categoryType;
        var isEnabled = opts.isEnabled;

        // 1. Optimistic insertion (if parent currently expanded)
        try
        {
            if (parentKey)
            {
                var tree0 = getTreeGlobal();
                var parentNode0 = tree0 ? tree0.getNodeByKey(parentKey) : null;
                if (tree0 && parentNode0 && parentNode0.expanded)
                {
                    tryInsertAndSelectUnderParent(tree0, parentNode0, childKey, name, polarity, categoryType, isEnabled);
                }
            }
        }
        catch (_)
        {
        }

        // 2. Anchor refresh -> duplicate purge -> parent resolution -> expand+reload -> selection
        refreshTopAnchorsLocal()
            .then(function ()
            {
                removeFromDiscIfDuplicated(parentKey);

                var parentNode = pickParentUnderRoot(parentKey);
                if (!parentNode)
                {
                    var a = getAnchors();
                    parentNode = a.tree ? a.tree.getNodeByKey(parentKey) : null;
                }

                if (!parentNode)
                {
                    // Fallback early
                    fullTreeReloadAndSelect(parentKey, childKey);
                    return;
                }

                return ensureNodeExpandedAndReload(parentNode)
                    .then(function ()
                    {
                        // Attempt selection via existing helper
                        reloadParentAndSelect(childKey, parentKey);

                        // Secondary attempt & details activation check
                        setTimeout(function ()
                        {
                            try
                            {
                                var tree = getTreeGlobal();
                                var found = tree && (tree.getNodeByKey(childKey) || tree.getNodeByKey("code:" + childKey));
                                if (!found)
                                {
                                    fullTreeReloadAndSelect(parentKey, childKey);
                                }
                            }
                            catch (_)
                            {
                                fullTreeReloadAndSelect(parentKey, childKey);
                            }
                        }, 400);
                    });
            })
            .catch(function ()
            {
                fullTreeReloadAndSelect(parentKey, childKey);
            });
    }
    // --- end new helper ---

    function fullTreeReloadAndSelect(parentKey, childKey)
    {
        try
        {
            var tree = getTreeGlobal();
            var cfgEl = document.getElementById("categoryTreeConfig");
            if (!tree || !cfgEl)
            {
                return;
            }

            var rootKey = cfgEl.dataset.root;
            var discKey = cfgEl.dataset.disc;

            function doSelection()
            {
                try
                {
                    var rootNode = rootKey ? tree.getNodeByKey(rootKey) : null;
                    if (rootNode && !rootNode.expanded)
                    {
                        try
                        {
                            rootNode.setExpanded(true);
                        }
                        catch (_)
                        {
                        }
                    }

                    var parentNode = parentKey ? tree.getNodeByKey(parentKey) : null;

                    if (parentNode && parentNode.getParent && parentNode.getParent().key === discKey)
                    {
                        try
                        {
                            var rootParentVersion = tree.getNodeByKey(parentKey);
                            if (rootParentVersion && rootParentVersion !== parentNode && rootParentVersion.getParent && rootParentVersion.getParent().key === rootKey)
                            {
                                try
                                {
                                    parentNode.remove();
                                }
                                catch (_)
                                {
                                }
                                parentNode = rootParentVersion;
                            }
                        }
                        catch (_)
                        {
                        }
                    }

                    if (parentNode)
                    {
                        ensureNodeExpandedAndReload(parentNode).then(function ()
                        {
                            selectChild(childKey, parentKey);
                        });
                    }
                    else
                    {
                        selectChild(childKey, parentKey);
                    }
                }
                catch (_)
                {
                    selectChild(childKey, parentKey);
                }
            }

            function selectChild(childKey)
            {
                // Use adaptive retry if available, otherwise fallback to loop
                if (window.tcTree && typeof window.tcTree.retry === "function")
                {
                    window.tcTree.retry(function ()
                    {
                        try
                        {
                            var n = tree.getNodeByKey(childKey) || tree.getNodeByKey("code:" + childKey);
                            if (n)
                            {
                                try
                                {
                                    n.makeVisible();
                                }
                                catch (_)
                                {
                                }
                                try
                                {
                                    n.setActive(true);
                                }
                                catch (_)
                                {
                                }
                                return true;
                            }
                        }
                        catch (_)
                        {
                        }
                        return false;
                    }, { attempts: 6, delayMs: 160, factor: 1.25 });
                }
                else
                {
                    try
                    {
                        var attempts = 6;
                        (function retry()
                        {
                            var n = tree.getNodeByKey(childKey) || tree.getNodeByKey("code:" + childKey);
                            if (n)
                            {
                                try
                                {
                                    n.makeVisible();
                                }
                                catch (_)
                                {
                                }
                                try
                                {
                                    n.setActive(true);
                                }
                                catch (_)
                                {
                                }
                                return;
                            }

                            if (--attempts > 0)
                            {
                                setTimeout(retry, 180);
                            }
                        })();
                    }
                    catch (_)
                    {
                    }
                }
            }

            var res = tree.reload({ url: _nocache(_nodesUrl) });
            if (res && typeof res.then === "function")
            {
                res.then(function ()
                {
                    setTimeout(doSelection, 60);
                }, function ()
                {
                    setTimeout(doSelection, 60);
                });
            }
            else if (res && typeof res.done === "function")
            {
                res.done(function ()
                {
                    setTimeout(doSelection, 60);
                }).fail(function ()
                {
                    setTimeout(doSelection, 60);
                });
            }
            else
            {
                setTimeout(doSelection, 120);
            }
        }
        catch (_)
        {
        }
    }

    function refreshTopAnchorsLocal()
    {
        function asPromise(node, url)
        {
            return new Promise(function (resolve)
            {
                try
                {
                    var r = url ? node.reloadChildren({ url: url }) : node.reloadChildren();
                    if (r && typeof r.then === "function")
                    {
                        r.then(function ()
                        {
                            resolve();
                        }, function ()
                        {
                            resolve();
                        });
                    }
                    else if (r && typeof r.done === "function")
                    {
                        r.done(function ()
                        {
                            resolve();
                        }).fail(function ()
                        {
                            resolve();
                        });
                    }
                    else
                    {
                        setTimeout(resolve, 100);
                    }
                }
                catch (_)
                {
                    resolve();
                }
            });
        }

        return new Promise(function (resolve)
        {
            try
            {
                var tree = getTreeGlobal();
                var cfgEl = document.getElementById("categoryTreeConfig");
                if (!tree || !cfgEl)
                {
                    resolve();
                    return;
                }

                var rootKey = cfgEl.dataset && cfgEl.dataset.root;
                var discKey = cfgEl.dataset && cfgEl.dataset.disc;
                var promises = [];

                [rootKey, discKey].forEach(function (k)
                {
                    if (!k)
                    {
                        return;
                    }

                    var n = tree.getNodeByKey(k);
                    if (!n || typeof n.reloadChildren !== "function")
                    {
                        return;
                    }

                    try
                    {
                        if (!n.expanded)
                        {
                            n.setExpanded(true);
                        }
                    }
                    catch (_)
                    {
                    }

                    var url = null;
                    if (_nodesUrl)
                    {
                        try
                        {
                            url = _nocache(_appendQuery(_nodesUrl, "id", n.key));
                        }
                        catch (_)
                        {
                            url = null;
                        }
                    }

                    promises.push(asPromise(n, url));
                });

                if (promises.length === 0)
                {
                    resolve();
                    return;
                }

                Promise.all(promises)
                    .then(function ()
                    {
                        setTimeout(resolve, 80);
                    })
                    .catch(function ()
                    {
                        setTimeout(resolve, 80);
                    });
            }
            catch (_)
            {
                resolve();
            }
        });
    }

    function getAnchors()
    {
        var tree = getTreeGlobal();
        var cfgEl = document.getElementById("categoryTreeConfig");
        if (!tree || !cfgEl)
        {
            return { tree: null, root: null, disc: null };
        }

        var rootKey = cfgEl.dataset && cfgEl.dataset.root;
        var discKey = cfgEl.dataset && cfgEl.dataset.disc;

        return {
            tree: tree,
            root: rootKey ? tree.getNodeByKey(rootKey) : null,
            disc: discKey ? tree.getNodeByKey(discKey) : null
        };
    }

    function pickParentUnderRoot(parentKey)
    {
        try
        {
            var a = getAnchors();
            if (!a.root || !a.root.children)
            {
                return null;
            }

            for (var i = 0; i < a.root.children.length; i++)
            {
                var ch = a.root.children[i];
                if (ch && ch.key === parentKey)
                {
                    return ch;
                }
            }

            return null;
        }
        catch (_)
        {
            return null;
        }
    }

    function removeFromDiscIfDuplicated(parentKey)
    {
        try
        {
            var a = getAnchors();
            if (!a.root || !a.disc)
            {
                return;
            }

            var existsUnderRoot = false;
            if (a.root.children)
            {
                for (var i = 0; i < a.root.children.length; i++)
                {
                    if (a.root.children[i] && a.root.children[i].key === parentKey)
                    {
                        existsUnderRoot = true;
                        break;
                    }
                }
            }

            if (!existsUnderRoot)
            {
                return;
            }

            if (a.disc.children)
            {
                for (var j = 0; j < a.disc.children.length; j++)
                {
                    var ch = a.disc.children[j];
                    if (ch && ch.key === parentKey)
                    {
                        try
                        {
                            ch.remove();
                        }
                        catch (_)
                        {
                        }
                        break;
                    }
                }
            }
        }
        catch (_)
        {
        }
    }

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
                                    var url = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, 'id', node.key)) : null;
                                    var r2 = url ? node.reloadChildren({ url: url }) : node.reloadChildren();

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
                                    var url2 = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, 'id', node.key)) : null;
                                    var r2b = url2 ? node.reloadChildren({ url: url2 }) : node.reloadChildren();

                                    if (r2b && typeof r2b.then === "function")
                                    {
                                        r2b.then(afterReload, afterReload);
                                    }
                                    else if (r2b && r2b.done)
                                    {
                                        r2b.done(afterReload).fail(afterReload);
                                    }
                                    else
                                    {
                                        afterReload();
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
                        var url3 = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, 'id', node.key)) : null;
                        var r = url3 ? node.reloadChildren({ url: url3 }) : node.reloadChildren();

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

        function selectWithRetry(keyToFind, attempts, delayMs, factor)
        {
            if (window.tcTree && typeof window.tcTree.retry === "function")
            {
                return window.tcTree.retry(function ()
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
                            noThrow(function ()
                            {
                                safeInvoke(node, "makeVisible");
                                safeInvokeWithArg(node, "setActive", true);

                                var el = (typeof node.getEventTarget === "function") ? node.getEventTarget() : null;
                                if (el && el.scrollIntoView)
                                {
                                    el.scrollIntoView({ block: "nearest", inline: "nearest" });
                                }
                            });

                            return true;
                        }
                    }
                    catch (ex)
                    {
                        // swallow and retry
                    }

                    return false;
                }, { attempts: attempts || 6, delayMs: delayMs || 160, factor: factor || 1.25 });
            }
            else
            {
                // Fallback to simple loop if helper not available
                var attemptsLeft = attempts || 6;

                (function retry()
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
                            noThrow(function ()
                            {
                                safeInvoke(node, "makeVisible");
                                safeInvokeWithArg(node, "setActive", true);

                                var el = (typeof node.getEventTarget === "function") ? node.getEventTarget() : null;
                                if (el && el.scrollIntoView)
                                {
                                    el.scrollIntoView({ block: "nearest", inline: "nearest" });
                                }
                            });

                            return;
                        }

                        if (--attemptsLeft > 0)
                        {
                            setTimeout(retry, delayMs || 180);
                        }
                    }
                    catch (ex)
                    {
                        if (--attemptsLeft > 0)
                        {
                            setTimeout(retry, delayMs || 180);
                        }
                    }
                })();

                return null;
            }
        }

        if (parentKey)
        {
            var parentNode = tree.getNodeByKey(parentKey);

            if (!parentNode)
            {
                // Reload all expanded top-level anchors, then schedule adaptive selection attempts
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
                                        var url = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", n.key)) : null;
                                        var r = url ? n.reloadChildren({ url: url }) : n.reloadChildren();

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
                                    catch (e)
                                    {
                                        setTimeout(res, 150);
                                    }
                                }));
                            })(top);
                        }
                    }
                }

                Promise.all(promises)
                    .then(function ()
                    {
                        setTimeout(function () { selectWithRetry(key, 6, 160, 1.25); }, 300);
                        setTimeout(function () { selectWithRetry(key, 4, 200, 1.25); }, 800);
                    })
                    .catch(function ()
                    {
                        setTimeout(function () { selectWithRetry(key, 6, 160, 1.25); }, 300);
                        setTimeout(function () { selectWithRetry(key, 4, 200, 1.25); }, 800);
                    });

                return;
            }

            // Ensure parent expanded + reloaded (nocache), then adaptive selection
            ensureNodeExpandedAndReload(parentNode)
                .then(function ()
                {
                    selectWithRetry(key, 6, 160, 1.25);
                    setTimeout(function () { selectWithRetry(key, 4, 200, 1.25); }, 500);
                })
                .catch(function ()
                {
                    selectWithRetry(key, 6, 160, 1.25);
                    setTimeout(function () { selectWithRetry(key, 4, 200, 1.25); }, 500);
                });

            return;
        }

        // No parent provided: reload all expanded top anchors, then adaptive selection
        try
        {
            var root2 = tree.getRootNode();
            var topPromises = [];

            if (root2 && root2.children)
            {
                for (var j = 0; j < root2.children.length; j++)
                {
                    var n = root2.children[j];
                    if (n && n.expanded && typeof n.reloadChildren === "function")
                    {
                        (function (nn)
                        {
                            topPromises.push(new Promise(function (res)
                            {
                                try
                                {
                                    var url = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", nn.key)) : null;
                                    var r = url ? nn.reloadChildren({ url: url }) : nn.reloadChildren();

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
                                catch (e)
                                {
                                    setTimeout(res, 150);
                                }
                            }));
                        })(n);
                    }
                }
            }

            Promise.all(topPromises)
                .then(function ()
                {
                    setTimeout(function () { selectWithRetry(key, 6, 160, 1.25); }, 300);
                    setTimeout(function () { selectWithRetry(key, 4, 200, 1.25); }, 800);
                })
                .catch(function ()
                {
                    setTimeout(function () { selectWithRetry(key, 6, 160, 1.25); }, 300);
                    setTimeout(function () { selectWithRetry(key, 4, 200, 1.25); }, 800);
                });
        }
        catch (ex)
        {
            setTimeout(function () { selectWithRetry(key, 6, 160, 1.25); }, 300);
            setTimeout(function () { selectWithRetry(key, 4, 200, 1.25); }, 800);
        }
    }

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

            if (parentNode.expanded && typeof parentNode.addChildren === "function")
            {
                var safeTitle = escapeHtml(title || key);
                var safeCode = escapeHtml(key);
                var polClass = (Number(polarityCode) === 0) ? "expense" : (Number(polarityCode) === 1) ? "income" : "neutral";

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
                            }
                        }, 450);
                    }
                    catch (ex)
                    {
                    }

                    return true;
                }
            }
        }
        catch (ex)
        {
            if (_debug)
            {
                console.warn("tryInsertAndSelectUnderParent failed", ex);
            }
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

            reconcileAndSelect({
                parentKey: parent,
                childKey: key,
                name: name,
                polarity: polarity,
                categoryType: categoryType,
                isEnabled: isEnabled
            });

            return;
        }
        else if (id === "createCodeResult")
        {
            try
            {
                var keyAttr = (marker.getAttribute("data-key") || "").trim();
                var cash = ((marker.getAttribute("data-cashcode") || marker.getAttribute("data-cashCode") || "")).trim();
                var category = ((marker.getAttribute("data-parent") || marker.getAttribute("data-category") || "")).trim();

                var nodeJson = marker.getAttribute("data-node") || "";
                var nodeKey = keyAttr || (cash ? ("code:" + cash) : "");
                var tree2 = getTreeGlobal();

                if (nodeJson && tree2)
                {
                    try
                    {
                        var parsed = JSON.parse(nodeJson);
                        var parsedKey = (parsed && parsed.key) ? parsed.key : nodeKey;
                        var parentNode = category ? tree2.getNodeByKey(category) : null;
                        var existing = tree2.getNodeByKey(parsedKey);

                        if (existing)
                        {
                            try
                            {
                                existing.title = parsed.title || existing.title;
                            }
                            catch (_)
                            {
                            }

                            try
                            {
                                existing.data = parsed.data || existing.data;
                            }
                            catch (_)
                            {
                            }

                            try
                            {
                                if (parsed.extraClasses)
                                {
                                    existing.li && existing.li.classList && existing.li.classList.add(parsed.extraClasses);
                                }
                                else
                                {
                                    existing.li && existing.li.classList && existing.li.classList.remove("tc-disabled");
                                }
                            }
                            catch (_)
                            {
                            }

                            safeInvoke(existing, "makeVisible");
                            safeInvokeWithArg(existing, "setActive", true);
                            safeInvoke(existing, "renderTitle");

                            reloadParentAndSelect(parsedKey, category || "");
                            return;
                        }

                        if (parentNode && typeof parentNode.addChildren === "function")
                        {
                            var added = parentNode.addChildren(parsed);
                            var newNode = tree2.getNodeByKey(parsedKey) || (added && added.length ? added[0] : null);
                            if (newNode)
                            {
                                safeInvoke(newNode, "makeVisible");
                                safeInvokeWithArg(newNode, "setActive", true);
                                safeInvoke(newNode, "renderTitle");
                                reloadParentAndSelect(parsedKey, category || "");
                                return;
                            }
                        }
                        else
                        {
                            var root = tree2.getRootNode();
                            if (root && typeof root.addChildren === "function")
                            {
                                var addedRoot = root.addChildren(parsed);
                                var newNode2 = tree2.getNodeByKey(parsedKey) || (addedRoot && addedRoot.length ? addedRoot[0] : null);
                                if (newNode2)
                                {
                                    safeInvoke(newNode2, "makeVisible");
                                    safeInvokeWithArg(newNode2, "setActive", true);
                                    safeInvoke(newNode2, "renderTitle");
                                    reloadParentAndSelect(parsedKey, category || "");
                                    return;
                                }
                            }
                        }
                    }
                    catch (ex)
                    {
                        if (_debug)
                        {
                            console.warn("embeddedCreate: failed to parse/apply data-node JSON", ex);
                        }
                    }
                }

                if (nodeKey)
                {
                    reconcileAndSelect({ parentKey: category || "", childKey: nodeKey });
                }
            }
            catch (ex)
            {
                if (_debug)
                {
                    console.warn("embeddedCreate: error processing createCodeResult", ex);
                }
            }
        }
    }

    function scanDetailsPaneForMarkers(el)
    {
        if (!el)
        {
            return;
        }

        var m = el.querySelector("#createResult, #createCategoryResult, #createCodeResult");

        if (!m && el.id && (el.id === "createResult" || el.id === "createCategoryResult" || el.id === "createCodeResult"))
        {
            m = el;
        }

        if (m)
        {
            processMarker(m);
        }
    }

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

            try
            {
                var parentInput = form.querySelector('input[name="ParentKey"]');
                var parentVal = parentInput && typeof parentInput.value === "string" ? parentInput.value : "";
                if (!parentVal)
                {
                    var cfgEl = document.getElementById("categoryTreeConfig");
                    var rootKey = (cfgEl && cfgEl.dataset && cfgEl.dataset.root) ? String(cfgEl.dataset.root) : "";
                    if (rootKey)
                    {
                        if (!parentInput)
                        {
                            parentInput = document.createElement("input");
                            parentInput.type = "hidden";
                            parentInput.name = "ParentKey";
                            form.appendChild(parentInput);
                        }
                        parentInput.value = rootKey;
                        form.setAttribute("data-parent", rootKey);
                    }
                }
            }
            catch (_)
            {
            }

            e.preventDefault();
            e.stopPropagation();

            try
            {
                var fd = new FormData(form);

                var token = (form.querySelector('input[name="__RequestVerificationToken"]') || {}).value
                    || (document.querySelector('meta[name="request-verification-token"]') || {}).content
                    || "";

                var headers = { "X-Requested-With": "XMLHttpRequest" };
                if (token)
                {
                    headers["RequestVerificationToken"] = token;
                }

                fetch(actionUrl, {
                    method: "POST",
                    body: fd,
                    credentials: "same-origin",
                    headers: headers
                })
                    .then(function (resp)
                    {
                        return resp.text();
                    })
                    .then(function (html)
                    {
                        pane.innerHTML = html;

                        try
                        {
                            var marker = pane.querySelector("#createResult, #createCategoryResult");
                            if (marker)
                            {
                                var parentKey = (marker.getAttribute("data-parent") || "").trim();

                                refreshTopAnchorsLocal()
                                    .then(function ()
                                    {
                                        removeFromDiscIfDuplicated(parentKey);

                                        var a = getAnchors();
                                        var parentNode = pickParentUnderRoot(parentKey) || (a.tree && parentKey ? a.tree.getNodeByKey(parentKey) : null);

                                        if (parentNode)
                                        {
                                            return ensureNodeExpandedAndReload(parentNode);
                                        }
                                    })
                                    .catch(function ()
                                    {
                                    });
                            }
                        }
                        catch (_)
                        {
                        }
                    })
                    .catch(function (err)
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

            if (!pane)
            {
                try
                {
                    window.location.href = "/Cash/CategoryTree/Index";
                }
                catch (_)
                {
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

            try
            {
                reloadParentAndSelect(active.key, parentKey);
            }
            catch (ex)
            {
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
                    .then(function (html)
                    {
                        pane.innerHTML = html;
                    })
                    .catch(function ()
                    {
                        pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                    });
            }
        }
        catch (_)
        {
            try
            {
                window.location.href = "/Cash/CategoryTree/Index";
            }
            catch (_)
            {
            }
        }
    };

    function byId(id)
    {
        try
        {
            return document.getElementById(id);
        }
        catch (e)
        {
            return null;
        }
    }

    function cfg()
    {
        try
        {
            return byId("categoryTreeConfig") || null;
        }
        catch (e)
        {
            return null;
        }
    }

    document.addEventListener("submit", function (e)
    {
        try
        {
            var form = e.target && e.target.closest ? e.target.closest("form") : null;
            if (!form)
            {
                return;
            }

            var isCreateTotal = (form.action && form.action.toLowerCase().indexOf("/createtotal") >= 0)
                || (form.getAttribute("data-action") === "CreateTotal");

            if (!isCreateTotal)
            {
                return;
            }

            var parentInput = form.querySelector('input[name="ParentKey"]');
            var parentVal = parentInput && typeof parentInput.value === "string" ? parentInput.value : "";

            if (!parentVal)
            {
                var c = cfg();
                var rootKey = (c && c.dataset && c.dataset.root) ? String(c.dataset.root) : "";

                if (rootKey)
                {
                    if (!parentInput)
                    {
                        parentInput = document.createElement("input");
                        parentInput.type = "hidden";
                        parentInput.name = "ParentKey";
                        form.appendChild(parentInput);
                    }
                    parentInput.value = rootKey;
                    form.setAttribute("data-parent", rootKey);
                }
            }
        }
        catch (ex)
        {
        }
    }, true);

    // ---- Test/Debug Hook Exposure ----
    function exposeTestHooks()
    {
        // Expose hooks only when debugging/under test
        var isPlaywright = /Playwright|pwtest/i.test(navigator.userAgent);
        var isForced = (typeof window.__TC_FORCE_TEST_HOOKS__ !== "undefined");
        var isDebug = !!(window.tcTree && window.tcTree.debug === true);

        if (!(isDebug || isPlaywright || isForced))
        {
            return;
        }

        window.tcTree = window.tcTree || {};

        if (!window.tcTree.reloadAndSelect && typeof reloadParentAndSelect === "function")
        {
            window.tcTree.reloadAndSelect = reloadParentAndSelect;
        }

        if (!window.tcTree.reconcileAndSelect && typeof reconcileAndSelect === "function")
        {
            window.tcTree.reconcileAndSelect = reconcileAndSelect;
        }

        if (!window.tcTree.ensureExpanded && typeof ensureNodeExpandedAndReload === "function")
        {
            window.tcTree.ensureExpanded = ensureNodeExpandedAndReload;
        }

        if (!window.tcTree.fullReloadSelect && typeof fullTreeReloadAndSelect === "function")
        {
            window.tcTree.fullReloadSelect = fullTreeReloadAndSelect;
        }
    }

})();
