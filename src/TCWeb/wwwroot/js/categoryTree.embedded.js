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

        // 1. Optimistic insert if parent already expanded and child missing
        try
        {
            if (parentKey)
            {
                var parentNode0 = tree.getNodeByKey(parentKey);
                if (parentNode0 && parentNode0.expanded)
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
                // ... (unchanged pre-existing code)
                return;
            }

            // Ensure parent expanded + reloaded (nocache), then parent-aware selection
            ensureNodeExpandedAndReload(parentNode)
                .then(function ()
                {
                    // Parent-biased selection: search only under intended parent first
                    var attempts = 6;
                    var delay = 160;

                    (function trySelectUnderParent()
                    {
                        try
                        {
                            // Ensure we still have a fresh parent reference
                            var p = tree.getNodeByKey(parentKey);
                            var found = null;

                            if (p && p.children)
                            {
                                for (var i = 0; i < p.children.length; i++)
                                {
                                    var c = p.children[i];
                                    if (c && c.key === key)
                                    {
                                        found = c;
                                        break;
                                    }
                                }
                            }

                            if (found)
                            {
                                safeInvoke(found, "makeVisible");
                                safeInvokeWithArg(found, "setActive", true);
                                var el = (typeof found.getEventTarget === "function") ? found.getEventTarget() : null;
                                if (el && el.scrollIntoView)
                                {
                                    el.scrollIntoView({ block: "nearest", inline: "nearest" });
                                }
                                return;
                            }
                        }
                        catch (_)
                        {
                        }

                        if (--attempts > 0)
                        {
                            setTimeout(trySelectUnderParent, delay);
                            return;
                        }

                        // Fallback: global lookup with retry (may still find the Disconnected copy,
                        // but by now parent should be refreshed and contain the child)
                        selectWithRetry(key, 6, 160, 1.25);
                        setTimeout(function () { selectWithRetry(key, 4, 200, 1.25); }, 500);
                    })();
                })
                .catch(function ()
                {
                    // Fallback: global lookup with retry
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

            // Explicitly refresh RHS details (do not rely on activation)
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

            // Explicitly refresh RHS details (do not rely on activation)
            setTimeout(function () { loadDetailsEmbedded(keyEc, parentEc); }, 200);
            return;
        }

        // Create Cash Code
        if (id === "createCodeResult")
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
                    setTimeout(function () { loadDetailsEmbedded(nodeKey, category || ""); }, 350);
                }
            }
            catch (ex)
            {
                if (_debug) { console.warn("embeddedCreate: error processing createCodeResult", ex); }
            }
            return;
        }

        // Add Existing Total (attach existing total-type category under a parent)
        if (id === "addExistingTotalResult")
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

            // Remove the Disconnected copy first to avoid duplicate-key conflicts on reload
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
            }, 300);
            return;
        }

    }

    function scanDetailsPaneForMarkers(el)
    {
        if (!el)
        {
            return;
        }

        var selector = "#createResult, #createCategoryResult, #createCodeResult, #editTotalResult, #editCategoryResult, #addExistingTotalResult";
        var m = el.querySelector(selector)

        if (!m && el.id && (
                el.id === "createResult" ||
                el.id === "createCategoryResult" ||
                el.id === "createCodeResult" ||
                el.id === "editTotalResult" ||
                el.id === "editCategoryResult" ||
                el.id === "addExistingTotalResult" 
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
            catch (e)
            {
                void e;
            }

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
                            marker = pane.querySelector("#createResult, #createCategoryResult, #createCodeResult, #editTotalResult, #editCategoryResult, #addExistingTotalResult");
                        }
                        catch (e)
                        {
                            void e;
                        }

                        if (marker)
                        {
                            try
                            {
                                processMarker(marker);
                            }
                            catch (e)
                            {
                                void e;
                            }

                            var parentKey = (marker.getAttribute("data-parent") || "").trim();
                            try
                            {
                                if (marker.id === "createResult")
                                {
                                    var catTypeAttr = (marker.getAttribute("data-categorytype") || "").trim();
                                    var catTypeNum = catTypeAttr ? parseInt(catTypeAttr, 10) : NaN;
                                    if (catTypeNum === 1)
                                    {
                                        var cfgEl3 = document.getElementById("categoryTreeConfig");
                                        var discKey3 = cfgEl3 && cfgEl3.dataset ? (cfgEl3.dataset.disc || "") : "";
                                        if ((!parentKey || parentKey === "__ROOT__") && discKey3)
                                        {
                                            parentKey = discKey3;
                                        }
                                    }
                                }
                            }
                            catch (e)
                            {
                                void e;
                            }

                            // RHS details: explicitly reload with the active key now
                            var keyAttr = (marker.getAttribute("data-key") || "").trim();
                            setTimeout(function ()
                            {
                                loadDetailsEmbedded(keyAttr, parentKey);
                            }, 150);

                            // Keep anchors fresh, but don’t rely on them to populate RHS
                            refreshTopAnchorsLocal()
                                .then(function ()
                                {
                                    removeFromDiscIfDuplicated(parentKey);

                                    var a = getAnchors();
                                    var parentNode = pickParentUnderRoot(parentKey)
                                        || (a.tree && parentKey ? a.tree.getNodeByKey(parentKey) : null);

                                    if (parentNode)
                                    {
                                        return ensureNodeExpandedAndReload(parentNode);
                                    }
                                })
                                .catch(function () { });
                        }
                        else
                        {
                            // Validation view: leave pane content (form with errors) visible
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

})();
