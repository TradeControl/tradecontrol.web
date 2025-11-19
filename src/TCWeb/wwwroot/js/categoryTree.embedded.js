(function ()
{
    var _cfgEl = document.getElementById("categoryTreeConfig");
    var _nodesUrl = _cfgEl ? _cfgEl.dataset.nodesUrl : null;
    var _debug = !!(window.tcTree && window.tcTree.debug);

    if (_debug) { console.log("[embeddedCreate] debug enabled"); }

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

    function reconcileAndSelect(opts)
    {
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

        var tree = getTreeGlobal();
        if (!tree)
        {
            return;
        }

        if (_debug)
        {
            console.log("[reconcile] start", { parentKey: parentKey, childKey: childKey });
        }

        // 1. Optimistic insert ONLY for category nodes (never for cash code nodes)
        try
        {
            if (parentKey)
            {
                var parentNode0 = tree.getNodeByKey(parentKey);
                var isCodeChild = childKey.indexOf("code:") === 0;
                if (parentNode0 && parentNode0.expanded && !isCodeChild)
                {
                    var existingChild = tree.getNodeByKey(childKey);
                    if (!existingChild)
                    {
                        tryInsertAndSelectUnderParent(tree, parentNode0, childKey, name, polarity, categoryType, isEnabled);
                    }
                }
            }
        }
        catch (_)
        {
        }

        // 2. If no parent key (should not happen for add-existing), just full reload + select
        if (!parentKey)
        {
            fullTreeReloadAndSelect(parentKey, childKey);
            return;
        }

        var cfgEl = document.getElementById("categoryTreeConfig");
        var discKey = (cfgEl && cfgEl.dataset) ? (cfgEl.dataset.disc || "") : "";

        // Internal: select child strictly under parent
        function selectUnderParent()
        {
            try
            {
                var parentNode = tree.getNodeByKey(parentKey);
                if (!parentNode)
                {
                    if (_debug) { console.log("[reconcile] parent still missing - abort select"); }
                    return;
                }

                var found = null;
                if (parentNode.children)
                {
                    for (var i = 0; i < parentNode.children.length; i++)
                    {
                        var c = parentNode.children[i];
                        if (c && c.key === childKey)
                        {
                            found = c;
                            break;
                        }
                    }
                }

                if (found)
                {
                    if (_debug) { console.log("[reconcile] selecting child under parent"); }
                    try
                    {
                        found.makeVisible();
                    }
                    catch (_){}
                    try
                    {
                        found.setActive(true);
                    }
                    catch (_){}
                    // Only now remove lingering disconnected copy
                    if (discKey && parentKey !== discKey)
                    {
                        try
                        {
                            removeChildFromDisconnected(childKey);
                        }
                        catch (_){}
                    }
                    // Load RHS details explicitly
                    setTimeout(function () { loadDetailsEmbedded(childKey, parentKey); }, 160);
                }
                else
                {
                    if (_debug) { console.log("[reconcile] child not found under parent after reload"); }
                }
            }
            catch (_)
            {
            }
        }

        // 3. Ensure parent exists; if not, reload root anchors minimally
        var parentNode = tree.getNodeByKey(parentKey);
        if (!parentNode)
        {
            // Reload root & disconnected (single pass)
            var anchors = getAnchors();
            var jobs = [];
            [anchors.root, anchors.disc].forEach(function (n)
            {
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
                catch (_){}
                var url = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", n.key)) : null;
                jobs.push(new Promise(function (resolve)
                {
                    try
                    {
                        var r = url ? n.reloadChildren({ url: url }) : n.reloadChildren();
                        if (r && typeof r.then === "function") { r.then(resolve, resolve); }
                        else if (r && r.done) { r.done(resolve).fail(resolve); }
                        else { setTimeout(resolve, 80); }
                    }
                    catch (_)
                    {
                        setTimeout(resolve, 80);
                    }
                }));
            });

            Promise.all(jobs).then(function ()
            {
                parentNode = tree.getNodeByKey(parentKey);
                if (!parentNode)
                {
                    if (_debug) { console.log("[reconcile] parent missing after anchors reload; forcing full tree reload"); }
                    fullTreeReloadAndSelect(parentKey, childKey);
                    return;
                }
                proceedWithParent(parentNode);
            });
            return;
        }

        proceedWithParent(parentNode);

        // 4. Expand + reload parent once, then select
        function proceedWithParent(pNode)
        {
            // Ensure expanded
            var expandRes;
            try
            {
                if (!pNode.expanded && typeof pNode.setExpanded === "function")
                {
                    expandRes = pNode.setExpanded(true);
                }
            }
            catch (_){}

            function afterExpand()
            {
                try
                {
                    if (typeof pNode.reloadChildren === "function")
                    {
                        var url = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", pNode.key)) : null;
                        var r = url ? pNode.reloadChildren({ url: url }) : pNode.reloadChildren();
                        if (r && typeof r.then === "function")
                        {
                            r.then(function ()
                            {
                                selectUnderParent();
                            }, function ()
                            {
                                selectUnderParent();
                            });
                        }
                        else if (r && r.done)
                        {
                            r.done(function ()
                            {
                                selectUnderParent();
                            }).fail(function ()
                            {
                                selectUnderParent();
                            });
                        }
                        else
                        {
                            setTimeout(selectUnderParent, 120);
                        }
                    }
                    else
                    {
                        selectUnderParent();
                    }
                }
                catch (_)
                {
                    selectUnderParent();
                }
            }

            if (expandRes && typeof expandRes.then === "function")
            {
                expandRes.then(afterExpand, afterExpand);
            }
            else if (expandRes && typeof expandRes.done === "function")
            {
                expandRes.done(afterExpand).fail(afterExpand);
            }
            else
            {
                afterExpand();
            }
        }
    }

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
                            // Parent-first selection after full reload
                            var t = getTreeGlobal();
                            var p = t && t.getNodeByKey(parentKey);
                            var found = null;
                            if (p && p.children)
                            {
                                for (var i = 0; i < p.children.length; i++)
                                {
                                    if (p.children[i] && p.children[i].key === childKey)
                                    {
                                        found = p.children[i];
                                        break;
                                    }
                                }
                            }

                            if (found)
                            {
                                try
                                {
                                    found.makeVisible();
                                }
                                catch (_)
                                {
                                }

                                try
                                {
                                    found.setActive(true);
                                }
                                catch (_)
                                {
                                }
                            }
                            else
                            {
                                selectChild(childKey, parentKey);
                            }
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
                            var n = null;

                            try
                            {
                                n = tree.getNodeByKey(childKey) || tree.getNodeByKey("code:" + childKey);
                            }
                            catch (_)
                            {
                            }

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

    function removeExistingOutsideParent(childKey, parentKey)
    {
        try
        {
            var tree = getTreeGlobal();
            if (!tree || !childKey || !parentKey)
            {
                return;
            }

            var existing = null;
            try
            {
                existing = tree.getNodeByKey(childKey);
            }
            catch (_)
            {
                existing = null;
            }

            if (!existing)
            {
                return;
            }

            var ep = null;
            try
            {
                ep = existing.getParent && existing.getParent();
            }
            catch (_)
            {
                ep = null;
            }

            if (!ep || ep.key !== parentKey)
            {
                try
                {
                    existing.remove();
                }
                catch (_){ }
            }
        }
        catch (_)
        {
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

    function refreshAnchorsSelective(parentKey)
    {
        // Reload __DISCONNECTED__ always; reload __ROOT__ only when parent is a direct child of root
        return new Promise(function (resolve)
        {
            try
            {
                var a = getAnchors();
                var jobs = [];

                function asPromise(node, url)
                {
                    return new Promise(function (res)
                    {
                        try
                        {
                            var r = url ? node.reloadChildren({ url: url }) : node.reloadChildren();
                            if (r && typeof r.then === "function") { r.then(res, res); }
                            else if (r && r.done) { r.done(res).fail(res); }
                            else { setTimeout(res, 80); }
                        }
                        catch (_)
                        {
                            setTimeout(res, 80);
                        }
                    });
                }

                // Always keep anchors expanded
                try
                {
                    a.root && !a.root.expanded && a.root.setExpanded(true);
                }
                catch (_){}

                try
                {
                    a.disc && !a.disc.expanded && a.disc.setExpanded(true);
                }
                catch (_){}

                // Disconnected always
                if (a.disc)
                {
                    var urlD = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", a.disc.key)) : null;
                    jobs.push(asPromise(a.disc, urlD));
                }

                // Reload root only if parent is a root-level child (avoid nuking deeper subtrees)
                var isRootChild = !!pickParentUnderRoot(parentKey);
                if (isRootChild && a.root)
                {
                    var urlR = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", a.root.key)) : null;
                    jobs.push(asPromise(a.root, urlR));
                }

                if (jobs.length === 0)
                {
                    resolve();
                    return;
                }

                Promise.all(jobs).then(function () { setTimeout(resolve, 60); })
                                 .catch(function () { setTimeout(resolve, 60); });
            }
            catch (_)
            {
                resolve();
            }
        });
    }

    function removeChildFromDisconnected(childKey)
    {
        try
        {
            if (!childKey) { return; }

            var a = getAnchors();
            if (!a.disc) { return; }

            // Disconnected totals appear as direct children; remove if present
            if (a.disc.children)
            {
                for (var j = 0; j < a.disc.children.length; j++)
                {
                    var ch = a.disc.children[j];
                    if (ch && ch.key === childKey)
                    {
                        try
                        {
                            ch.remove();
                        }
                        catch (_) {}

                        break;
                    }
                }
            }
        }
        catch (_)
        {
        }
    }

    function expandAncestorsAndReload(targetParentKey)
    {
        return new Promise(function (resolve)
        {
            try
            {
                var tree = getTreeGlobal();
                if (!tree || !targetParentKey)
                {
                    resolve();
                    return;
                }

                var chain = [];
                var n = tree.getNodeByKey(targetParentKey);
                while (n && n.key && n.parent)
                {
                    chain.push(n);
                    n = n.parent;
                }

                // Include root if not already
                var a = getAnchors();
                if (a.root && chain.indexOf(a.root) < 0)
                {
                    chain.push(a.root);
                }

                chain.reverse(); // expand top-down

                function step(i)
                {
                    if (i >= chain.length)
                    {
                        resolve();
                        return;
                    }

                    var cur = chain[i];

                    function next()
                    {
                        step(i + 1);
                    }

                    try
                    {
                        if (!cur.expanded && typeof cur.setExpanded === "function")
                        {
                            var r = cur.setExpanded(true);
                            if (r && typeof r.then === "function") { r.then(next, next); }
                            else if (r && typeof r.done === "function") { r.done(next).fail(next); }
                            else { setTimeout(next, 60); }
                        }
                        else if (typeof cur.reloadChildren === "function")
                        {
                            var url = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", cur.key)) : null;
                            var rr = url ? cur.reloadChildren({ url: url }) : cur.reloadChildren();
                            if (rr && typeof rr.then === "function") { rr.then(next, next); }
                            else if (rr && typeof rr.done === "function") { rr.done(next).fail(next); }
                            else { setTimeout(next, 50); }
                        }
                        else
                        {
                            setTimeout(next, 40);
                        }
                    }
                    catch (_)
                    {
                        setTimeout(next, 60);
                    }
                }

                step(0);
            }
            catch (_)
            {
                resolve();
            }
        });
    }

    function ensureParentMaterialized(parentKey, maxDepth)
    {
        maxDepth = Math.max(1, Math.min(4, maxDepth || 3));

        return new Promise(function (resolve)
        {
            try
            {
                var tree = getTreeGlobal();
                if (!tree || !parentKey)
                {
                    resolve();
                    return;
                }

                // Quick success
                if (tree.getNodeByKey(parentKey))
                {
                    resolve();
                    return;
                }

                var level = 0;

                function expandWave()
                {
                    try
                    {
                        // If parent appeared while we waited, stop
                        if (tree.getNodeByKey(parentKey))
                        {
                            resolve();
                            return;
                        }

                        var nodes = [];
                        var root = tree.getRootNode();

                        // Collect all non-expanded folders currently visible to expand this wave
                        root.visit(function (n)
                        {
                            try
                            {
                                if (n && n.folder && !n.expanded)
                                {
                                    nodes.push(n);
                                }
                            }
                            catch (_)
                            {
                            }
                        });

                        if (nodes.length === 0)
                        {
                            resolve();
                            return;
                        }

                        var jobs = [];

                        function asPromise(node)
                        {
                            return new Promise(function (res)
                            {
                                try
                                {
                                    var r = node.setExpanded(true);
                                    if (r && typeof r.then === "function")
                                    {
                                        r.then(function ()
                                        {
                                            try
                                            {
                                                if (typeof node.reloadChildren === "function")
                                                {
                                                    var url = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", node.key)) : null;
                                                    var rr = url ? node.reloadChildren({ url: url }) : node.reloadChildren();

                                                    if (rr && typeof rr.then === "function") { rr.then(res, res); }
                                                    else if (rr && rr.done) { rr.done(res).fail(res); }
                                                    else { setTimeout(res, 60); }
                                                }
                                                else
                                                {
                                                    setTimeout(res, 40);
                                                }
                                            }
                                            catch (_)
                                            {
                                                setTimeout(res, 60);
                                            }
                                        }, function ()
                                        {
                                            setTimeout(res, 60);
                                        });
                                    }
                                    else if (r && typeof r.done === "function")
                                    {
                                        r.done(function ()
                                        {
                                            try
                                            {
                                                if (typeof node.reloadChildren === "function")
                                                {
                                                    var url2 = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", node.key)) : null;
                                                    var rr2 = url2 ? node.reloadChildren({ url: url2 }) : node.reloadChildren();
                                                    if (rr2 && typeof rr2.then === "function") { rr2.then(res, res); }
                                                    else if (rr2 && rr2.done) { rr2.done(res).fail(res); }
                                                    else { setTimeout(res, 60); }
                                                }
                                                else
                                                {
                                                    setTimeout(res, 40);
                                                }
                                            }
                                            catch (_)
                                            {
                                                setTimeout(res, 60);
                                            }
                                        }).fail(function ()
                                        {
                                            setTimeout(res, 60);
                                        });
                                    }
                                    else
                                    {
                                        setTimeout(res, 40);
                                    }
                                }
                                catch (_)
                                {
                                    setTimeout(res, 60);
                                }
                            });
                        }

                        for (var i = 0; i < nodes.length; i++)
                        {
                            jobs.push(asPromise(nodes[i]));
                        }

                        Promise.all(jobs)
                            .then(function ()
                            {
                                try
                                {
                                    if (tree.getNodeByKey(parentKey))
                                    {
                                        resolve();
                                        return;
                                    }

                                    if (++level >= maxDepth)
                                    {
                                        resolve();
                                        return;
                                    }

                                    // Next wave
                                    setTimeout(expandWave, 80);
                                }
                                catch (_)
                                {
                                    resolve();
                                }
                            })
                            .catch(function ()
                            {
                                resolve();
                            });
                    }
                    catch (_)
                    {
                        resolve();
                    }
                }

                // Start
                expandWave();
            }
            catch (_)
            {
                resolve();
            }
        });
    }

    function forceSelectChild(parentKey, childKey, attempts)
    {
        attempts = attempts || 8;

        var tree = getTreeGlobal();
        if (!tree || !childKey)
        {
            return;
        }

        (function retry()
        {
            try
            {
                var node = null;

                if (parentKey)
                {
                    var p = tree.getNodeByKey(parentKey);
                    if (p && p.children)
                    {
                        for (var i = 0; i < p.children.length; i++)
                        {
                            var c = p.children[i];
                            if (c && c.key === childKey)
                            {
                                node = c;
                                break;
                            }
                        }
                    }
                }

                if (!node)
                {
                    node = tree.getNodeByKey(childKey) || tree.getNodeByKey("code:" + childKey);
                }

                if (node)
                {
                    try
                    {
                        node.makeVisible();
                    }
                    catch (_){}
                    try
                    {
                        node.setActive(true);
                    }
                    catch (_){}
                    return;
                }
            }
            catch (_)
            {
            }

            if (--attempts > 0)
            {
                setTimeout(retry, 160);
            }
        })();
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
    if (!tree || !key) { return; }

    // Variants for code keys
    var rawKey = String(key).replace(/^code:/, "");
    var keyVariants = String(key).indexOf("code:") === 0 ? [String(key), rawKey] : ["code:" + rawKey, rawKey];

    function preferRootInstanceAndSelect(k)
    {
        try
        {
            var cfg = document.getElementById("categoryTreeConfig");
            var rootKey = (cfg && cfg.dataset && cfg.dataset.root) ? cfg.dataset.root : "";
            var root = tree.getRootNode();
            var preferred = null;
            var first = null;

            if (!root) { return false; }

            root.visit(function (n)
            {
                if (!n || !n.key) { return; }
                if (n.key !== k) { return; }

                if (!first) { first = n; }

                // Walk ancestry to see if under __ROOT__
                var p = n;
                var underRoot = false;
                while (p)
                {
                    if (p.key === rootKey) { underRoot = true; break; }
                    p = p.parent;
                }
                if (underRoot)
                {
                    preferred = n;
                    return false; // stop visiting
                }
            });

            var node = preferred || first;
            if (!node) { return false; }

            try
            {
                node.makeVisible();
            }
            catch (_){}
            try
            {
                node.setActive(true);
            }
            catch (_){}
            return true;
        }
        catch (_)
        {
            return false;
        }
    }

    function selectWithRetryAny()
    {
        // Prefer __ROOT__ ancestry if possible
        for (var i = 0; i < keyVariants.length; i++)
        {
            if (preferRootInstanceAndSelect(keyVariants[i])) { return; }
        }

        // Fallback adaptive retry by key
        if (window.tcTree && typeof window.tcTree.retry === "function")
        {
            window.tcTree.retry(function ()
            {
                for (var i = 0; i < keyVariants.length; i++)
                {
                    try
                    {
                        var n = tree.getNodeByKey(keyVariants[i]);
                        if (n)
                        {
                            try
                            {
                                n.makeVisible();
                            }
                            catch (_){}
                            try
                            {
                                n.setActive(true);
                            }
                            catch (_){}
                            return true;
                        }
                    }
                    catch (_){}
                }
                return false;
            }, { attempts: 6, delayMs: 160, factor: 1.25 });
        }
        else
        {
            var left = 6;
            (function retry()
            {
                for (var i = 0; i < keyVariants.length; i++)
                {
                    try
                    {
                        var n = tree.getNodeByKey(keyVariants[i]);
                        if (n)
                        {
                            try
                            {
                                n.makeVisible();
                            }
                            catch (_){}
                            try
                            {
                                n.setActive(true);
                            }
                            catch (_){}
                            return;
                        }
                    }
                    catch (_){}
                }
                if (--left > 0) { setTimeout(retry, 180); }
            })();
        }
    }

    function trySelectUnderParentOnce(pKey)
    {
        try
        {
            var p = tree.getNodeByKey(pKey);
            if (!p || !p.children) { return false; }
            for (var i = 0; i < p.children.length; i++)
            {
                var c = p.children[i];
                if (!c) { continue; }
                for (var k = 0; k < keyVariants.length; k++)
                {
                    if (c.key === keyVariants[k])
                    {
                        try
                        {
                            c.makeVisible();
                        }
                        catch (_){}
                        try
                        {
                            c.setActive(true);
                        }
                        catch (_){}
                        return true;
                    }
                }
            }
        }
        catch (_){}
        return false;
    }

    if (parentKey)
    {
        expandAncestorsAndReload(parentKey)
            .then(function ()
            {
                var pNode = tree.getNodeByKey(parentKey);
                if (!pNode) { selectWithRetryAny(); return; }

                return ensureNodeExpandedAndReload(pNode)
                    .then(function ()
                    {
                        if (trySelectUnderParentOnce(parentKey)) { return; }
                        return ensureNodeExpandedAndReload(pNode).then(function ()
                        {
                            if (trySelectUnderParentOnce(parentKey)) { return; }
                            selectWithRetryAny();
                        });
                    });
            })
            .catch(function () { selectWithRetryAny(); });
        return;
    }

    // No parent hint: try preferred instance, then retry
    selectWithRetryAny();
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
                // Only short-circuit if the existing node is already under the intended parent
                try
                {
                    var ep = existing.getParent && existing.getParent();
                    if (ep && ep.key === parentNode.key)
                    {
                        safeInvoke(existing, "makeVisible");
                        safeInvokeWithArg(existing, "setActive", true);
                        return true;
                    }
                }
                catch (_)
                {
                }

                // Otherwise, do NOT return here: let proper reload move it
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

    // Helper: fetch and render details into RHS (desktop + mobile)
    function loadDetailsEmbedded(key, parentKey)
    {
        try
        {
            var pane = document.getElementById("detailsPane");
            if (!pane || !key)
            {
                return;
            }

            var url = "/Cash/CategoryTree/Details";
            url = _appendQuery(url, "key", key);
            url = _appendQuery(url, "embed", "1");
            if (parentKey)
            {
                url = _appendQuery(url, "parentKey", parentKey);
            }

            fetch(_nocache(url), { credentials: "same-origin" })
                .then(function (resp)
                {
                    if (!resp || !resp.ok)
                    {
                        throw new Error("bad response");
                    }
                    return resp.text();
                })
                .then(function (html)
                {
                    pane.innerHTML = html;
                })
                .catch(function (e)
                {
                    void e;
                });
        }
        catch (e)
        {
            void e;
        }
    }

    function processMarker(marker)
    {
        if (!marker)
        {
            return;
        }

        if (_debug)
        {
            console.log("[processMarker]", marker.id);
        }

        var id = marker.id || "";

        function applyEditToExistingNode(node, name, isEnabled, polarity, cashType)
        {
            if (!node)
            {
                return;
            }

            if (!isNaN(polarity))
            {
                node.data.cashPolarity = polarity;
            }
            if (!isNaN(cashType))
            {
                node.data.cashType = cashType;
            }

            var pol = (typeof node.data.cashPolarity !== "undefined") ? Number(node.data.cashPolarity) : 2;
            var polClass = (pol === 0) ? "expense" : (pol === 1) ? "income" : "neutral";

            function esc(s)
            {
                return String(s || "")
                    .replace(/&/g, "&amp;")
                    .replace(/</g, "&lt;")
                    .replace(/>/g, "&gt;")
                    .replace(/"/g, "&quot;")
                    .replace(/'/g, "&#039;");
            }

            var safeName = esc(name || node.title || node.key);
            var safeCode = esc(node.key);
            node.title = "<span class='tc-cat-icon tc-cat-" + polClass + "'></span> " + safeName + " (" + safeCode + ")";
            node.data.isEnabled = (isEnabled === "0") ? 0 : 1;

            if (node.li && node.li.classList)
            {
                node.li.classList.toggle("tc-disabled", node.data.isEnabled === 0);
            }

            safeInvoke(node, "renderTitle");
            safeInvoke(node, "makeVisible");
            safeInvokeWithArg(node, "setActive", true);
        }

        // Create (Totals or Categories)
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

            try
            {
                if (categoryType === 1 && (!parent || parent === "__ROOT__"))
                {
                    var cfgEl2 = document.getElementById("categoryTreeConfig");
                    var discKey2 = cfgEl2 && cfgEl2.dataset ? (cfgEl2.dataset.disc || "") : "";
                    if (discKey2)
                    {
                        parent = discKey2;
                    }
                }
            }
            catch (_)
            {
            }

            reconcileAndSelect({
                parentKey: parent,
                childKey: key,
                name: name,
                polarity: polarity,
                categoryType: categoryType,
                isEnabled: isEnabled
            });

            // NEW: explicit parent reload and targeted selection to ensure new category becomes active (desktop)
            setTimeout(function ()
            {
                reloadParentAndSelect(key, parent || "");
            }, 120);

            // After create, show details explicitly
            setTimeout(function () { loadDetailsEmbedded(key, parent); }, 350);
            return;
        }

        // Edit Total
        if (id === "editTotalResult")
        {
            var keyEt = (marker.getAttribute("data-key") || "").trim();
            var parentEt = (marker.getAttribute("data-parent") || "").trim();
            var nameEt = (marker.getAttribute("data-name") || "").trim();
            var isEnabledEt = (marker.getAttribute("data-isenabled") || "1").trim();
            var polarityEt = parseInt((marker.getAttribute("data-polarity") || "").trim(), 10);
            var cashTypeEt = parseInt((marker.getAttribute("data-cashtype") || "").trim(), 10);

            if (!keyEt)
            {
                return;
            }

            try
            {
                var treeEt = getTreeGlobal();
                var nEt = treeEt && treeEt.getNodeByKey(keyEt);
                applyEditToExistingNode(nEt, nameEt, isEnabledEt, polarityEt, cashTypeEt);
            }
            catch (e)
            {
                void e;
            }

            setTimeout(function () { loadDetailsEmbedded(keyEt, parentEt); }, 200);
            return;
        }

        // Edit Category
        if (id === "editCategoryResult")
        {
            var keyEc = (marker.getAttribute("data-key") || "").trim();
            var parentEc = (marker.getAttribute("data-parent") || "").trim();
            var nameEc = (marker.getAttribute("data-name") || "").trim();
            var isEnabledEc = (marker.getAttribute("data-isenabled") || "1").trim();
            var polarityEc = parseInt((marker.getAttribute("data-polarity") || "").trim(), 10);
            var cashTypeEc = parseInt((marker.getAttribute("data-cashtype") || "").trim(), 10);

            if (!keyEc)
            {
                return;
            }

            try
            {
                var treeEc = getTreeGlobal();
                var nEc = treeEc && treeEc.getNodeByKey(keyEc);
                applyEditToExistingNode(nEc, nameEc, isEnabledEc, polarityEc, cashTypeEc);
            }
            catch (e)
            {
                void e;
            }

            setTimeout(function () { loadDetailsEmbedded(keyEc, parentEc); }, 200);
            return;
        }

        // Create Cash Code
        if (id === "createCashCodeResult")
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
                            catch (e) { void e; }
                            try
                            {
                                existing.data = parsed.data || existing.data;
                            }
                            catch (e) { void e; }
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
                            catch (e) { void e; }

                            safeInvoke(existing, "makeVisible");
                            safeInvokeWithArg(existing, "setActive", true);
                            safeInvoke(existing, "renderTitle");
                            reloadParentAndSelect(parsedKey, category || "");
                            setTimeout(function () { loadDetailsEmbedded(parsedKey, category || ""); }, 300);
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
                                setTimeout(function () { loadDetailsEmbedded(parsedKey, category || ""); }, 300);
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
                                    setTimeout(function () { loadDetailsEmbedded(parsedKey, category || ""); }, 300);
                                    return;
                                }
                            }
                        }
                    }
                    catch (ex)
                    {
                        if (_debug) { console.warn("embeddedCreate: failed to parse/apply data-node JSON", ex); }
                    }
                }

                if (nodeKey)
                {
                    reconcileAndSelect({ parentKey: category || "", childKey: nodeKey });

                    // NEW: ensure parent reload + selection (handles case where reconcile optimistic insert skipped)
                    setTimeout(function ()
                    {
                        reloadParentAndSelect(nodeKey, category || "");
                    }, 120);

                    setTimeout(function () { loadDetailsEmbedded(nodeKey, category || ""); }, 350);
                }
            }
            catch (ex)
            {
                if (_debug) { console.warn("embeddedCreate: error processing createCashCodeResult", ex); }
            }
            return;
        }

        // Edit Cash Code
        if (id === "editCashCodeResult")
        {
            var keyEcc = (marker.getAttribute("data-key") || "").trim();          // expects "code:XYZ"
            var parentEcc = (marker.getAttribute("data-parent") || "").trim();
            var nameEcc = (marker.getAttribute("data-name") || "").trim();
            var isEnabledEcc = (marker.getAttribute("data-isenabled") || "1").trim();
            var cashTypeEcc = parseInt((marker.getAttribute("data-cashtype") || "0").trim(), 10);

            if (!keyEcc)
            {
                return;
            }

            try
            {
                var treeEcc = getTreeGlobal();
                var nEcc = treeEcc && treeEcc.getNodeByKey(keyEcc);

                var iconClass = "bi-wallet2";
                if (cashTypeEcc === 1)
                {
                    iconClass = "bi-file-earmark-text";
                }
                else if (cashTypeEcc === 2)
                {
                    iconClass = "bi-bank";
                }

                function esc(s)
                {
                    return String(s || "")
                        .replace(/&/g, "&amp;")
                        .replace(/</g, "&lt;")
                        .replace(/>/g, "&gt;")
                        .replace(/"/g, "&quot;")
                        .replace(/'/g, "&#039;");
                }

                var rawCode = keyEcc.replace(/^code:/, "");
                var titleHtml = "<span class=\"tc-code-icon bi " + iconClass + "\"></span> "
                              + esc(rawCode) + " - " + esc(nameEcc || "");

                if (nEcc)
                {
                    nEcc.title = titleHtml;
                    nEcc.data = nEcc.data || {};
                    nEcc.data.nodeType = "code";
                    nEcc.data.isEnabled = (isEnabledEcc === "0") ? 0 : 1;
                    if (typeof cashTypeEcc === "number" && !isNaN(cashTypeEcc))
                    {
                        nEcc.data.cashType = cashTypeEcc;
                    }

                    if (nEcc.li && nEcc.li.classList)
                    {
                        nEcc.li.classList.toggle("tc-disabled", nEcc.data.isEnabled === 0);
                    }

                    safeInvoke(nEcc, "renderTitle");
                    safeInvoke(nEcc, "makeVisible");
                    safeInvokeWithArg(nEcc, "setActive", true);
                }
            }
            catch (_)
            {
            }

            setTimeout(function () { loadDetailsEmbedded(keyEcc, parentEcc); }, 200);
            return;
        }

        // Add Category
        if (id === "addExistingCategoryResult")
        {
            var keyAet = (marker.getAttribute("data-key") || "").trim();
            var parentAet = (marker.getAttribute("data-parent") || "").trim();
            var nameAet = (marker.getAttribute("data-name") || "").trim();
            var polarityAet = parseInt((marker.getAttribute("data-polarity") || "2").trim(), 10);
            var isEnabledAet = (marker.getAttribute("data-isenabled") || "1").trim();
            var categoryTypeAet = parseInt((marker.getAttribute("data-categorytype") || "1").trim(), 10);

            if (!keyAet)
            {
                return;
            }

            removeExistingOutsideParent(keyAet, parentAet);

            try
            {
                var cfgElA = document.getElementById("categoryTreeConfig");
                var discKeyA = (cfgElA && cfgElA.dataset) ? (cfgElA.dataset.disc || "") : "";
                if (discKeyA && parentAet && parentAet !== discKeyA)
                {
                    removeChildFromDisconnected(keyAet);
                }
            }
            catch (_)
            {
            }

            reconcileAndSelect({
                parentKey: parentAet,
                childKey: keyAet,
                name: nameAet,
                polarity: polarityAet,
                categoryType: categoryTypeAet,
                isEnabled: isEnabledAet
            });

            setTimeout(function ()
            {
                loadDetailsEmbedded(keyAet, parentAet);
                forceSelectChild(parentAet, keyAet, 6);
                // Ensure select even if optimistic insert skipped
                reloadParentAndSelect(keyAet, parentAet);
            }, 300);
            return;
        }

        // Add Existing Cash Code
        if (id === "addExistingCashCodeResult")
        {
            var rawKey = (marker.getAttribute("data-key") || "").trim();
            var parentAce = (marker.getAttribute("data-parent") || "").trim();
            var descOverride = (marker.getAttribute("data-description")
                || marker.getAttribute("data-desc")
                || marker.getAttribute("data-name")
                || marker.getAttribute("data-cashdescription")
                || "").trim();

            if (!rawKey) { return; }

            var prefKey = rawKey.indexOf("code:") === 0 ? rawKey : ("code:" + rawKey);
            var rawOnly = rawKey.replace(/^code:/, "");
            var variants = [prefKey, rawOnly];

            var tree = getTreeGlobal();
            if (!tree) { return; }

            function purgeOutsideTarget(rawBaseKey, targetParentKey)
            {
                try
                {
                    var root = tree.getRootNode();
                    if (!root) { return; }
                    var allKeys = [rawBaseKey, "code:" + rawBaseKey];
                    root.visit(function (n)
                    {
                        if (!n || !n.key) { return; }
                        if (allKeys.indexOf(n.key) < 0) { return; }
                        try
                        {
                            var p = n.getParent && n.getParent();
                            var pk = p && p.key ? p.key : "";
                            if (pk !== targetParentKey)
                            {
                                n.remove();
                            }
                        }
                        catch (_){ }
                    });
                }
                catch (_){ }
            }

            var existingNode = tree.getNodeByKey(prefKey) || tree.getNodeByKey(rawOnly);
            var oldParentKey = "";
            try
            {
                oldParentKey = existingNode && existingNode.getParent ? (existingNode.getParent().key || "") : "";
            }
            catch (_){}

            purgeOutsideTarget(rawOnly, parentAce);

            try
            {
                if (oldParentKey && oldParentKey !== parentAce)
                {
                    var oldParentNode = tree.getNodeByKey(oldParentKey);
                    if (oldParentNode && typeof oldParentNode.reloadChildren === "function")
                    {
                        var urlOld = _nodesUrl ? _nocache(_appendQuery(_nodesUrl, "id", oldParentNode.key)) : null;
                        oldParentNode.reloadChildren(urlOld ? { url: urlOld } : undefined);
                    }
                }
            }
            catch (_){}

            function ensureCodeNodePresent(parentNode, descriptionOverride)
            {
                if (!parentNode) { return null; }

                for (var i = 0; i < variants.length; i++)
                {
                    var existingUnderTarget = tree.getNodeByKey(variants[i]);
                    if (existingUnderTarget
                        && existingUnderTarget.getParent
                        && existingUnderTarget.getParent().key === parentNode.key)
                    {
                        if (descriptionOverride)
                        {
                            try
                            {
                                var cashTypeExisting = 0;
                                if (existingUnderTarget.data)
                                {
                                    if (typeof existingUnderTarget.data.cashType !== "undefined")
                                    {
                                        cashTypeExisting = Number(existingUnderTarget.data.cashType) || 0;
                                    }
                                    else if (typeof existingUnderTarget.data.cashTypeCode !== "undefined")
                                    {
                                        cashTypeExisting = Number(existingUnderTarget.data.cashTypeCode) || 0;
                                    }
                                }
                                var iconClassOverride = "bi-wallet2";
                                if (cashTypeExisting === 1) { iconClassOverride = "bi-file-earmark-text"; }
                                else if (cashTypeExisting === 2) { iconClassOverride = "bi-bank"; }
                                function esc(o)
                                {
                                    return String(o || "")
                                        .replace(/&/g, "&amp;")
                                        .replace(/</g, "&lt;")
                                        .replace(/>/g, "&gt;")
                                        .replace(/"/g, "&quot;")
                                        .replace(/'/g, "&#039;");
                                }
                                existingUnderTarget.title = "<span class='tc-code-icon bi " + iconClassOverride + "'></span> "
                                    + rawOnly + " - " + esc(descriptionOverride);
                                safeInvoke(existingUnderTarget, "renderTitle");
                            }
                            catch (_){ }
                        }
                        safeInvoke(existingUnderTarget, "makeVisible");
                        safeInvokeWithArg(existingUnderTarget, "setActive", true);
                        return existingUnderTarget;
                    }
                }

                var origNode = existingNode;
                var origData = (origNode && origNode.data) ? origNode.data : {};
                var origTitle = (origNode && typeof origNode.title === "string") ? origNode.title : "";
                var extractedDesc = "";
                try
                {
                    if (origTitle)
                    {
                        var plain = origTitle.replace(/<[^>]+>/g, "").trim();
                        var m = plain.match(/^[^\s-]+?\s*-\s*(.+)$/);
                        if (m && m[1]) { extractedDesc = m[1].trim(); }
                    }
                }
                catch (_){}

                var cashType = 0;
                try
                {
                    if (typeof origData.cashType !== "undefined")
                    {
                        cashType = Number(origData.cashType) || 0;
                    }
                    else if (typeof origData.cashTypeCode !== "undefined")
                    {
                        cashType = Number(origData.cashTypeCode) || 0;
                    }
                    else if (parentNode.data)
                    {
                        if (typeof parentNode.data.cashType !== "undefined")
                        {
                            cashType = Number(parentNode.data.cashType) || 0;
                        }
                        else if (typeof parentNode.data.cashTypeCode !== "undefined")
                        {
                            cashType = Number(parentNode.data.cashTypeCode) || 0;
                        }
                    }
                }
                catch (_){ cashType = 0; }

                var iconClass;
                switch (cashType)
                {
                    case 1: iconClass = "bi-file-earmark-text"; break;
                    case 2: iconClass = "bi-bank"; break;
                    default: iconClass = "bi-wallet2"; break;
                }

                function escapeHtml(str)
                {
                    return String(str || "")
                        .replace(/&/g, "&amp;")
                        .replace(/</g, "&lt;")
                        .replace(/>/g, "&gt;")
                        .replace(/"/g, "&quot;")
                        .replace(/'/g, "&#039;");
                }

                var finalDesc = descriptionOverride || extractedDesc || "(Description)";
                var titleHtml =
                    "<span class='tc-code-icon bi " + iconClass + "'></span> "
                    + rawOnly + " - " + escapeHtml(finalDesc);

                try
                {
                    var added = parentNode.addChildren({
                        title: titleHtml,
                        key: prefKey,
                        folder: false,
                        icon: false,
                        lazy: false,
                        extraClasses: (origData.isEnabled === 0) ? "tc-disabled" : null,
                        data: {
                            nodeType: "code",
                            cashCode: rawOnly,
                            isEnabled: (typeof origData.isEnabled === "number") ? origData.isEnabled : 1,
                            cashPolarity: (typeof origData.cashPolarity !== "undefined") ? origData.cashPolarity : undefined,
                            cashType: cashType,
                            categoryType: (typeof origData.categoryType !== "undefined") ? origData.categoryType : undefined
                        }
                    });

                    var newNode = tree.getNodeByKey(prefKey) || (added && added.length ? added[0] : null);
                    return newNode || null;
                }
                catch (_)
                {
                    return null;
                }
            }

            function selectAndShow(parentKey, keyVariants, attempts)
            {
                attempts = attempts || 8;
                (function retry()
                {
                    var node = null;
                    for (var i = 0; !node && i < keyVariants.length; i++)
                    {
                        try
                        {
                            node = tree.getNodeByKey(keyVariants[i]);
                        }
                        catch (_){ }
                    }
                    if (node)
                    {
                        try
                        {
                            node.makeVisible();
                        }
                        catch (_){ }
                        try
                        {
                            node.setActive(true);
                        }
                        catch (_){ }
                        loadDetailsEmbedded(node.key, parentKey);
                        return;
                    }
                    if (--attempts > 0)
                    {
                        setTimeout(retry, 160);
                    }
                    else
                    {
                        fullTreeReloadAndSelect(parentKey, prefKey);
                        setTimeout(function () { loadDetailsEmbedded(prefKey, parentKey); }, 450);
                    }
                })();
            }

            expandAncestorsAndReload(parentAce)
                .then(function ()
                {
                    var parentNode = tree.getNodeByKey(parentAce);
                    if (!parentNode) { throw new Error("parent missing after expand"); }
                    return ensureNodeExpandedAndReload(parentNode).then(function () { return parentNode; });
                })
                .then(function (parentNode)
                {
                    var present = ensureCodeNodePresent(parentNode, descOverride);
                    selectAndShow(parentAce, variants.concat(present ? [present.key] : []), 8);
                })
                .catch(function ()
                {
                    fullTreeReloadAndSelect(parentAce, prefKey);
                    setTimeout(function () { loadDetailsEmbedded(prefKey, parentAce); }, 450);
                });

            return;
        }
    }

    function scanDetailsPaneForMarkers(el)
    {
        if (!el)
        {
            return;
        }

        var selector = "#createResult, #createCategoryResult, #createCashCodeResult, #editTotalResult, #editCategoryResult, #addExistingCategoryResult, #addExistingCashCodeResult";
        var m = el.querySelector(selector)

        if (!m && el.id && (
                el.id === "createResult" ||
                el.id === "createCategoryResult" ||
                el.id === "createCashCodeResult" ||
                el.id === "editTotalResult" ||
                el.id === "editCategoryResult" ||
                el.id === "editCashCodeResult" ||
                el.id === "addExistingCategoryResult" ||
                el.id === "addExistingCashCodeResult"
            ))
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
    if (!pane) { return; }

    // Track last submitted parent (fallback when marker lacks data-parent)
    window.__tcLastSubmitParentKey = "";

    pane.addEventListener("submit", function (e)
    {
        var form = e.target;
        if (!form || form.tagName !== "FORM") { return; }

        var formId = (form.getAttribute("id") || "").toLowerCase();
        if (formId === "moveform") { return; }

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
                    parentVal = rootKey;
                }
            }

            // Record for fallback in processMarker
            window.__tcLastSubmitParentKey = parentVal || "";
        }
        catch (_){}

        e.preventDefault();
        e.stopPropagation();

        try
        {
            var fd = new FormData(form);
            var token =
                (form.querySelector('input[name="__RequestVerificationToken"]') || {}).value
                || (document.querySelector('meta[name="request-verification-token"]') || {}).content
                || "";

            var headers = { "X-Requested-With": "XMLHttpRequest" };
            if (token) { headers["RequestVerificationToken"] = token; }

            fetch(actionUrl, {
                method: "POST",
                body: fd,
                credentials: "same-origin",
                headers: headers
            })
                .then(function (resp) { return resp.text(); })
                .then(function (html)
                {
                    if (typeof html === "string"
                        && (html.indexOf('id="categoryTreeConfig"') >= 0 || html.indexOf('id="categoryTree"') >= 0))
                    {
                        window.location.href = "/Cash/CategoryTree/Index";
                        return;
                    }

                    pane.innerHTML = html;
                    var marker = null;
                    try
                    {
                        marker = pane.querySelector("#createResult, #createCategoryResult, #createCashCodeResult, #editTotalResult, #editCategoryResult, #editCashCodeResult, #addExistingCategoryResult, #addExistingCashCodeResult");
                    }
                    catch (_){}

                    if (marker)
                    {
                        try
                        {
                            processMarker(marker);
                        }
                        catch (_){}

                        var parentKey = (marker.getAttribute("data-parent") || "").trim() || window.__tcLastSubmitParentKey || "";
                        // RHS details refresh
                        var keyAttr = (marker.getAttribute("data-key") || "").trim();
                        setTimeout(function () { loadDetailsEmbedded(keyAttr, parentKey); }, 150);

                        refreshTopAnchorsLocal()
                            .then(function ()
                            {
                                removeFromDiscIfDuplicated(parentKey);
                                var a = getAnchors();
                                var parentNode = pickParentUnderRoot(parentKey)
                                    || (a.tree && parentKey ? a.tree.getNodeByKey(parentKey) : null);
                                if (parentNode) { return ensureNodeExpandedAndReload(parentNode); }
                            })
                            .catch(function () { });
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
            if (!isCreateTotal) { return; }

            var parentInput = form.querySelector('input[name="ParentKey"]');
            var parentVal = parentInput && typeof parentInput.value === "string" ? parentInput.value : "";
            var ctxParent = (form.getAttribute("data-parent") || "").trim();

            if (ctxParent === "__DISCONNECTED__")
            {
                if (!parentInput)
                {
                    parentInput = document.createElement("input");
                    parentInput.type = "hidden";
                    parentInput.name = "ParentKey";
                    form.appendChild(parentInput);
                }
                parentInput.value = ""; // disconnected create
            }
            else if (!parentVal)
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

    // Append + select a newly created cash code under an already expanded parent (desktop embedded create flow)
    window.CategoryTree.appendAndSelectCreatedCashCode = function (rawCode, parentCandidates, nodeJson)
    {
        try
        {
            var tree = $.ui && $.ui.fancytree && $.ui.fancytree.getTree("#categoryTree");
            if (!tree) { return; }

            var parsed = null;
            try
            {
                parsed = JSON.parse(nodeJson);
            }
            catch (_) { return; }

            var key = "code:" + rawCode;
            if (!parsed.key) { parsed.key = key; }

            // Find best parent: first candidate that exists and is not a type: synthetic
            function isTypeSynthetic(k)
            {
                return k && typeof k === "string" && k.indexOf("type:") === 0;
            }

            var parentNode = null;
            for (var i = 0; i < parentCandidates.length; i++)
            {
                var cand = parentCandidates[i];
                var n = tree.getNodeByKey(cand);
                if (!n) { continue; }
                if (isTypeSynthetic(n.key)) { continue; }
                parentNode = n;
                break;
            }
            if (!parentNode) { return; }

            // Ensure expanded & loaded (single attempt)
            function ensureExpandedLoaded(n, done)
            {
                try
                {
                    if (n.folder && !n.expanded)
                    {
                        var r = n.setExpanded(true);
                        if (r && r.then)
                        {
                            r.then(function () { done(); }, function () { done(); });
                            return;
                        }
                        if (r && r.done)
                        {
                            r.done(function () { done(); }).fail(function () { done(); });
                            return;
                        }
                    }
                }
                catch (_)
                {
                }
                done();
            }

            ensureExpandedLoaded(parentNode, function ()
            {
                // Does child already exist?
                var existing = parentNode.children && parentNode.children.find(function (c) { return c && c.key === key; });
                if (!existing)
                {
                    try
                    {
                        // Add node client-side
                        parentNode.addChildren([parsed]);
                        existing = parentNode.children && parentNode.children.find(function (c) { return c && c.key === key; });
                    }
                    catch (_)
                    {
                    }
                }

                if (!existing)
                {
                    // Final fallback: attempt one reload if possible, then re-check
                    if (typeof parentNode.reloadChildren === "function")
                    {
                        try
                        {
                            var base = tree.options && tree.options.source && tree.options.source.url;
                            var root = base ? base.split("?")[0] : "/Cash/CategoryTree";
                            var url = root + "?handler=Nodes&id=" + encodeURIComponent(parentNode.key) + "&_=" + Date.now();
                            var rr = parentNode.reloadChildren({ url: url });
                            var after = function ()
                            {
                                var re = parentNode.children && parentNode.children.find(function (c) { return c && c.key === key; });
                                if (re)
                                {
                                    try
                                    {
                                        re.makeVisible();
                                    }
                                    catch (_){}
                                    try
                                    {
                                        re.setActive(true);
                                    }
                                    catch (_){}
                                    if (typeof window.loadDetails === "function")
                                    {
                                        window.loadDetails(re);
                                    }
                                }
                            };
                            if (rr && rr.then) { rr.then(after, after); }
                            else if (rr && rr.done) { rr.done(after).fail(after); }
                            else { setTimeout(after, 120); }
                            return;
                        }
                        catch (_)
                        {
                            return;
                        }
                    }
                    return;
                }

                // Select & show details
                try
                {
                    existing.makeVisible();
                }
                catch (_){}
                try
                {
                    existing.setActive(true);
                }
                catch (_){}
                if (typeof window.loadDetails === "function")
                {
                    window.loadDetails(existing);
                }
            });
        }
        catch (_)
        {
            // silent
        }
    };

// Append + select newly created category (embedded desktop)
window.CategoryTree.appendAndSelectCreatedCategory = function (rawCategoryKey, parentCandidates, nodeJson)
{
    try
    {
        var tree = $.ui && $.ui.fancytree && $.ui.fancytree.getTree("#categoryTree");
        if (!tree)
        {
            return;
        }

        var parsed = null;
        try
        {
            parsed = JSON.parse(nodeJson);
        }
        catch (_)
        {
            return;
        }

        if (!parsed.key)
        {
            parsed.key = rawCategoryKey;
        }
        if (typeof parsed.folder === "undefined")
        {
            parsed.folder = true;
        }

        function isTypeSynthetic(key)
        {
            return key && key.indexOf("type:") === 0;
        }

        var parentNode = null;
        for (var i = 0; i < parentCandidates.length; i++)
        {
            var candKey = parentCandidates[i];
            if (!candKey)
            {
                continue;
            }
            var candNode = tree.getNodeByKey(candKey);
            if (candNode)
            {
                parentNode = candNode;
                break;
            }
        }

        // Fallback: if no candidate parent found, use ROOT (categories may appear top-level)
        if (!parentNode)
        {
            parentNode = tree.getNodeByKey("__ROOT__") || tree.getRootNode();
        }

        if (!parentNode)
        {
            return;
        }

        function ensureExpanded(node, done)
        {
            try
            {
                if (node.folder && !node.expanded)
                {
                    var r = node.setExpanded(true);
                    if (r && r.then)
                    {
                        r.then(function () { done(); }, function () { done(); });
                        return;
                    }
                    if (r && r.done)
                    {
                        r.done(function () { done(); }).fail(function () { done(); });
                        return;
                    }
                }
            }
            catch (_)
            {
            }
            done();
        }

        ensureExpanded(parentNode, function ()
        {
            var existing = parentNode.children && parentNode.children.find(function (c)
            {
                return c && c.key === parsed.key;
            });

            if (!existing)
            {
                try
                {
                    parentNode.addChildren([parsed]);
                    existing = parentNode.children && parentNode.children.find(function (c)
                    {
                        return c && c.key === parsed.key;
                    });
                }
                catch (_)
                {
                }
            }

            if (!existing)
            {
                // Single retry: load children if possible
                if (typeof parentNode.reloadChildren === "function")
                {
                    try
                    {
                        var base = tree.options && tree.options.source && tree.options.source.url;
                        var root = base ? base.split("?")[0] : "/Cash/CategoryTree";
                        var url = root + "?handler=Nodes&id=" + encodeURIComponent(parentNode.key) + "&_=" + Date.now();
                        var rld = parentNode.reloadChildren({ url: url });
                        var after = function ()
                        {
                            var re = parentNode.children && parentNode.children.find(function (c)
                            {
                                return c && c.key === parsed.key;
                            });
                            if (re)
                            {
                                try
                                {
                                    re.makeVisible();
                                }
                                catch (_){}
                                try
                                {
                                    re.setActive(true);
                                }
                                catch (_){}
                                if (typeof window.loadDetails === "function")
                                {
                                    window.loadDetails(re);
                                }
                            }
                        };
                        if (rld && rld.then)
                        {
                            rld.then(after, after);
                        }
                        else if (rld && rld.done)
                        {
                            rld.done(after).fail(after);
                        }
                        else
                        {
                            setTimeout(after, 120);
                        }
                    }
                    catch (_)
                    {
                    }
                }
                return;
            }

            try
            {
                existing.makeVisible();
            }
            catch (_){}
            try
            {
                existing.setActive(true);
            }
            catch (_){}
            if (typeof window.loadDetails === "function")
            {
                window.loadDetails(existing);
            }
        });
    }
    catch (_)
    {
        // silent
    }
};
})();
