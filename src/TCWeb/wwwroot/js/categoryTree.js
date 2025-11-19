/*
 CategoryTree: interactive cash category tree
 - Drag/drop reordering
   - Cash Type view: reorder siblings (server: ReorderType)
   - Totals/Disconnected: reorder siblings (server: ReorderSiblings)
   - Autoscrolls when dragging near container edges
 - Keyboard
   - Shift+Up/Down: reorder before/after previous/next sibling (persists)
   - Left/Right: collapse/expand
   - Home/End: jump to first/last sibling
 - Expand/Collapse Selected from context menu
 - State persistence: expanded nodes + active node (localStorage)
 - Accessibility: aria-live announcements on reorder; focus preserved
*/

window.CategoryTree = (function ()
{
    function appendQuery(url, key, value)
    {
        var sep = url.indexOf('?') === -1 ? '?' : '&';
        return url + sep + encodeURIComponent(key) + '=' + encodeURIComponent(value);
    }

    function nocache(url)
    {
        return appendQuery(url, "_", Date.now());
    }

    function init(cfg)
    {
        var treeSel = cfg.treeSelector || "#categoryTree";
        var menuSel = cfg.menuSelector || "#treeContextMenu";
        var actionBarSel = cfg.actionBarSelector || "#mobileActionBar";
        var nodesUrl = cfg.nodesUrl;
        var basePageUrl = cfg.basePageUrl;
        var detailsUrl = cfg.detailsUrl || null;
        var ROOT_KEY = cfg.rootKey;
        var DISC_KEY = cfg.discKey;
        var isAdmin = !!cfg.isAdmin;
        var CATEGORYTYPE_CASHTOTAL = 1; // server: (short)NodeEnum.CategoryType.CashTotal

        var menuOriginalHtml = null;
        var menuTemplates = { items: {}, dividerHtml: "<div class='dropdown-divider'></div>" };

        var querySelectionApplied = false;
        var selectionRetryCount = 0;
        var MAX_SELECTION_RETRIES = 50;

        // Capture pristine menu and templates once when DOM ready (before any context mutations)
        $(function ()
        {
            try
            {
                var $m = $(menuSel).first();
                if ($m.length)
                {
                    menuOriginalHtml = $m.html();
                }
            }
            catch (_)
            {
            }
            cacheMenuTemplates();
        });

        function handlerUrl(handlerName)
        {
            var sep = basePageUrl.indexOf('?') === -1 ? '?' : '&';
            return basePageUrl + sep + 'handler=' + handlerName;
        }

        function antiXsrf()
        {
            return document.querySelector("meta[name='request-verification-token']")?.getAttribute("content")
                || document.querySelector("input[name='__RequestVerificationToken']")?.value;
        }

        function isMobile()
        {
            return window.matchMedia && window.matchMedia("(max-width: 991.98px)").matches;
        }

        // Descendant loader: breadth-first load (no visible expand) folders under a parent until target key appears or cap reached.
        function loadDescendantsUntilKey(parentNode, targetKey, maxFolders, callback)
        {
            if (!parentNode || !targetKey)
            {
                callback(false);
                return;
            }

            var tree = getTree();
            if (!tree)
            {
                callback(false);
                return;
            }

            var queue = [];
            var visited = new Set();

            function enqueue(n)
            {
                if (!n) { return; }
                if (!n.key) { return; }
                if (visited.has(n.key)) { return; }
                visited.add(n.key);
                queue.push(n);
            }

            enqueue(parentNode);

            function step()
            {
                // Already present?
                var found = tree.getNodeByKey(targetKey);
                if (found)
                {
                    callback(true);
                    return;
                }

                if (queue.length === 0 || visited.size > maxFolders)
                {
                    callback(false);
                    return;
                }

                var current = queue.shift();

                // If current is lazy and not yet loaded, load its children without expanding.
                if (current.lazy && !current.loaded)
                {
                    try
                    {
                        var resLoad = current.load();
                        if (resLoad && typeof resLoad.then === "function")
                        {
                            resLoad.then(function ()
                            {
                                try
                                {
                                    if (current.children && current.children.length)
                                    {
                                        current.children.forEach(function (ch)
                                        {
                                            enqueue(ch);
                                        });
                                    }
                                }
                                catch (_)
                                {
                                }
                                // Continue BFS after a short yield
                                setTimeout(step, 25);
                            }).catch(function ()
                            {
                                setTimeout(step, 25);
                            });
                            return;
                        }
                        if (resLoad && typeof resLoad.done === "function")
                        {
                            resLoad.done(function ()
                            {
                                try
                                {
                                    if (current.children && current.children.length)
                                    {
                                        current.children.forEach(function (ch)
                                        {
                                            enqueue(ch);
                                        });
                                    }
                                }
                                catch (_)
                                {
                                }
                                setTimeout(step, 25);
                            }).fail(function ()
                            {
                                setTimeout(step, 25);
                            });
                            return;
                        }
                    }
                    catch (_)
                    {
                        // If load() threw, just continue
                    }
                }

                // Enqueue any already-loaded children (do NOT expand)
                try
                {
                    if (current.children && current.children.length)
                    {
                        current.children.forEach(function (ch)
                        {
                            enqueue(ch);
                        });
                    }
                }
                catch (_)
                {
                    // swallow
                }

                // Yield to avoid blocking UI
                setTimeout(step, 25);
            }

            step();
        }

        // Collapse all expanded folders except the ancestor chain of the given node.
        function pruneExpandedToBranch(node)
        {
            try
            {
                if (!node) { return; }

                var tree = getTree();
                if (!tree) { return; }

                // Build set of keys to keep expanded: node + its ancestors
                var keep = new Set();
                var cur = node;
                while (cur)
                {
                    if (cur.key)
                    {
                        keep.add(cur.key);
                    }

                    try
                    {
                        cur = cur.getParent ? cur.getParent() : null;
                    }
                    catch (_)
                    {
                        cur = null;
                    }
                }

                // Visit all nodes and collapse those expanded folders not on the keep list
                var root = tree.getRootNode();
                if (!root) { return; }

                root.visit(function (n)
                {
                    try
                    {
                        if (n && n.folder && n.expanded)
                        {
                            // Keep nodes in the ancestor chain (including the selected node)
                            if (!keep.has(n.key))
                            {
                                try
                                {
                                    n.setExpanded(false);
                                    persistExpanded(n, false);
                                }
                                catch (_)
                                {
                                    // swallow
                                }
                            }
                        }
                    }
                    catch (_)
                    {
                        // swallow
                    }
                });
            }
            catch (_)
            {
                // swallow
            }
        }

        // Focus branch without visible flicker: disable CSS transitions, expand ancestors (root->node),
        // then collapse non-ancestor nodes, restore transitions and resize.
        function focusBranch(node, onComplete)
        {
            try
            {
                if (!node)
                {
                    if (typeof onComplete === "function")
                    {
                        onComplete();
                    }
                    return;
                }

                var tree = getTree();
                if (!tree)
                {
                    if (typeof onComplete === "function")
                    {
                        onComplete();
                    }
                    return;
                }

                // Build ancestor chain (node -> root) and lookup set
                var ancestors = [];
                var keep = new Set();
                var cur = node;
                while (cur)
                {
                    if (cur.key)
                    {
                        ancestors.push(cur);
                        keep.add(cur.key);
                    }

                    try
                    {
                        cur = cur.getParent ? cur.getParent() : null;
                    }
                    catch (_)
                    {
                        cur = null;
                    }
                }
                ancestors.reverse(); // root -> node

                // Add class to suppress CSS transitions/animations during DOM updates
                var $container = $(treeSel);
                try
                {
                    $container.addClass("tc-no-transition");
                }
                catch (_)
                {
                    // swallow
                }

                // Expand ancestors sequentially (handles lazy loads)
                function expandSeq(idx)
                {
                    try
                    {
                        if (idx >= ancestors.length)
                        {
                            // After expansion, collapse non-ancestor expanded nodes in the next RAF tick
                            requestAnimationFrame(function ()
                            {
                                try
                                {
                                    var root = tree.getRootNode();
                                    if (root)
                                    {
                                        root.visit(function (n)
                                        {
                                            try
                                            {
                                                if (n && n.folder && n.expanded && !keep.has(n.key))
                                                {
                                                    try
                                                    {
                                                        n.setExpanded(false);
                                                        persistExpanded(n, false);
                                                    }
                                                    catch (_)
                                                    {
                                                        // swallow
                                                    }
                                                }
                                            }
                                            catch (_)
                                            {
                                                // swallow
                                            }
                                        });
                                    }
                                }
                                catch (_)
                                {
                                    // swallow
                                }

                                // Next RAF: restore transitions and resize, then call onComplete
                                requestAnimationFrame(function ()
                                {
                                    try
                                    {
                                        $container.removeClass("tc-no-transition");
                                    }
                                    catch (_)
                                    {
                                        // swallow
                                    }

                                    try
                                    {
                                        resizeColumns();
                                    }
                                    catch (_)
                                    {
                                        // swallow
                                    }

                                    if (typeof onComplete === "function")
                                    {
                                        onComplete();
                                    }
                                });
                            });

                            return;
                        }

                        var a = ancestors[idx];
                        if (!a || !a.folder)
                        {
                            expandSeq(idx + 1);
                            return;
                        }

                        if (a.expanded)
                        {
                            expandSeq(idx + 1);
                            return;
                        }

                        var res = null;
                        try
                        {
                            res = a.setExpanded(true);
                        }
                        catch (_)
                        {
                            res = null;
                        }

                        if (res && typeof res.then === "function")
                        {
                            res.then(function ()
                            {
                                try
                                {
                                    persistExpanded(a, true);
                                }
                                catch (_) { }
                                expandSeq(idx + 1);
                            }).catch(function ()
                            {
                                expandSeq(idx + 1);
                            });
                            return;
                        }

                        if (res && typeof res.done === "function")
                        {
                            res.done(function ()
                            {
                                try
                                {
                                    persistExpanded(a, true);
                                }
                                catch (_) { }
                                expandSeq(idx + 1);
                            }).fail(function ()
                            {
                                expandSeq(idx + 1);
                            });
                            return;
                        }

                        try
                        {
                            persistExpanded(a, true);
                        }
                        catch (_) { }
                        expandSeq(idx + 1);
                    }
                    catch (_)
                    {
                        // swallow and continue
                        expandSeq(idx + 1);
                    }
                }

                // Start expansion on next RAF to batch style changes
                requestAnimationFrame(function ()
                {
                    expandSeq(0);
                });
            }
            catch (_)
            {
                try
                {
                    if (typeof onComplete === "function")
                    {
                        onComplete();
                    }
                }
                catch (_)
                {
                }
            }
        }

        function applyQuerySelection()
        {
            if (querySelectionApplied)
            {
                return;
            }

            try
            {
                var params = new URLSearchParams(window.location.search);
                var selectKey = params.get("select") || params.get("returnKey") || params.get("key");
                var expandKey = params.get("expand") || params.get("parentKey");
                var keyParam = params.get("key");

                if (!selectKey && !expandKey)
                {
                    return;
                }

                var tree = getTree();
                if (!tree)
                {
                    return;
                }

                function keyVariants(key)
                {
                    if (!key)
                    {
                        return [];
                    }

                    var s = String(key);
                    if (s.indexOf("code:") === 0)
                    {
                        return [s, s.substring(5)];
                    }
                    else
                    {
                        return ["code:" + s, s];
                    }
                }

                var variants = keyVariants(selectKey);
                var isCodeKey = variants.length > 0 && (variants[0].indexOf("code:") === 0 || (variants[1] && variants[1].indexOf("code:") === 0));

                // Heuristic: post-create redirect (CreateCashCode desktop) => select==key (same code) and expand present.
                // Treat this as a fresh-create selection that must wait until the child is visible under ROOT/expandKey.
                var createdFlow = false;
                try
                {
                    if (isCodeKey && expandKey)
                    {
                        var kv = keyVariants(keyParam || "");
                        // If any variant of key equals any variant of select, assume 'fresh create' flow
                        createdFlow = kv.some(function (k) { return variants.indexOf(k) >= 0; });
                    }
                }
                catch (_)
                {
                    createdFlow = false;
                }

                function reloadChildrenNoCache(node)
                {
                    return new Promise(function (resolve)
                    {
                        try
                        {
                            if (!node || typeof node.reloadChildren !== "function")
                            {
                                resolve();
                                return;
                            }

                            var url = nodesUrl ? nocache(appendQuery(nodesUrl, "id", node.key)) : null;
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
                                return;
                            }
                            if (r && r.done)
                            {
                                r.done(function ()
                                {
                                    resolve();
                                }).fail(function ()
                                {
                                    resolve();
                                });
                                return;
                            }
                        }
                        catch (_)
                        {
                        }

                        setTimeout(resolve, 120);
                    });
                }

                function findAllNodesByKey(key)
                {
                    var list = [];
                    try
                    {
                        var root = tree.getRootNode();
                        if (!root)
                        {
                            return list;
                        }

                        root.visit(function (n)
                        {
                            try
                            {
                                if (n && n.key === key)
                                {
                                    list.push(n);
                                }
                            }
                            catch (_)
                            {
                            }
                        });
                    }
                    catch (_)
                    {
                    }
                    return list;
                }

                function ancestorChainContains(node, key)
                {
                    if (!node || !key)
                    {
                        return false;
                    }

                    var cur = node;
                    while (cur)
                    {
                        if (cur.key === key)
                        {
                            return true;
                        }
                        try
                        {
                            cur = cur.getParent ? cur.getParent() : null;
                        }
                        catch (_)
                        {
                            cur = null;
                        }
                    }
                    return false;
                }

                function isTypeSyntheticNode(node)
                {
                    if (!node)
                    {
                        return false;
                    }
                    var k = node.key || "";
                    var d = node.data || {};
                    if (k.indexOf("type:") === 0)
                    {
                        return true;
                    }
                    if (d.syntheticKind === "type" || d.isTypeContext === true)
                    {
                        return true;
                    }
                    return false;
                }

                function choosePreferredInstanceSingle(key, parentKeyHint)
                {
                    var all = findAllNodesByKey(key);
                    if (all.length === 0)
                    {
                        return null;
                    }
                    if (all.length === 1)
                    {
                        return all[0];
                    }

                    if (parentKeyHint)
                    {
                        for (var i = 0; i < all.length; i++)
                        {
                            var n0 = all[i];
                            if (!ancestorChainContains(n0, parentKeyHint))
                            {
                                continue;
                            }
                            if (!ancestorChainContains(n0, ROOT_KEY))
                            {
                                continue;
                            }

                            var hasType = false;
                            var cur0 = n0;
                            while (cur0)
                            {
                                if (cur0 !== n0 && isTypeSyntheticNode(cur0))
                                {
                                    hasType = true;
                                    break;
                                }
                                try
                                {
                                    cur0 = cur0.getParent ? cur0.getParent() : null;
                                }
                                catch (_)
                                {
                                    cur0 = null;
                                }
                            }
                            if (!hasType)
                            {
                                return n0;
                            }
                        }
                    }

                    if (parentKeyHint)
                    {
                        for (var j = 0; j < all.length; j++)
                        {
                            var n1 = all[j];
                            if (ancestorChainContains(n1, parentKeyHint) && ancestorChainContains(n1, ROOT_KEY))
                            {
                                return n1;
                            }
                        }
                    }

                    for (var k = 0; k < all.length; k++)
                    {
                        var n2 = all[k];
                        if (!ancestorChainContains(n2, ROOT_KEY))
                        {
                            continue;
                        }
                        var hasType2 = false;
                        var cur2 = n2;
                        while (cur2)
                        {
                            if (cur2 !== n2 && isTypeSyntheticNode(cur2))
                            {
                                hasType2 = true;
                                break;
                            }
                            try
                            {
                                cur2 = cur2.getParent ? cur2.getParent() : null;
                            }
                            catch (_)
                            {
                                cur2 = null;
                            }
                        }
                        if (!hasType2)
                        {
                            return n2;
                        }
                    }

                    for (var m = 0; m < all.length; m++)
                    {
                        if (ancestorChainContains(all[m], ROOT_KEY))
                        {
                            return all[m];
                        }
                    }

                    return all[0];
                }

                function choosePreferredInstanceFromVariants(variantKeys, parentKeyHint)
                {
                    for (var i = 0; i < variantKeys.length; i++)
                    {
                        var n = choosePreferredInstanceSingle(variantKeys[i], parentKeyHint);
                        if (n)
                        {
                            return n;
                        }
                    }
                    return null;
                }

                function loadUnderRootUntilKey(targetKey, maxFolders, callback)
                {
                    try
                    {
                        var rootNode = tree.getNodeByKey(ROOT_KEY);
                        if (!rootNode)
                        {
                            callback(false);
                            return;
                        }

                        var queue = [];
                        var visited = new Set();

                        function enqueue(n)
                        {
                            if (!n || !n.key)
                            {
                                return;
                            }
                            if (visited.has(n.key))
                            {
                                return;
                            }
                            visited.add(n.key);
                            queue.push(n);
                        }

                        enqueue(rootNode);

                        (function step()
                        {
                            try
                            {
                                var exist = tree.getNodeByKey(targetKey);
                                if (exist)
                                {
                                    callback(true);
                                    return;
                                }
                            }
                            catch (_)
                            {
                            }

                            if (queue.length === 0 || visited.size > maxFolders)
                            {
                                callback(false);
                                return;
                            }

                            var current = queue.shift();

                            if (current !== rootNode && isTypeSyntheticNode(current))
                            {
                                setTimeout(step, 10);
                                return;
                            }

                            if (current.lazy && !current.loaded)
                            {
                                try
                                {
                                    var res = current.load();
                                    var after = function ()
                                    {
                                        try
                                        {
                                            if (current.children && current.children.length)
                                            {
                                                for (var ci = 0; ci < current.children.length; ci++)
                                                {
                                                    enqueue(current.children[ci]);
                                                }
                                            }
                                        }
                                        catch (_)
                                        {
                                        }
                                        setTimeout(step, 15);
                                    };

                                    if (res && typeof res.then === "function")
                                    {
                                        res.then(after, after);
                                        return;
                                    }
                                    if (res && res.done)
                                    {
                                        res.done(after).fail(after);
                                        return;
                                    }
                                }
                                catch (_)
                                {
                                }
                            }

                            try
                            {
                                if (current.children && current.children.length)
                                {
                                    for (var cj = 0; cj < current.children.length; cj++)
                                    {
                                        enqueue(current.children[cj]);
                                    }
                                }
                            }
                            catch (_)
                            {
                            }

                            setTimeout(step, 15);
                        })();
                    }
                    catch (_)
                    {
                        callback(false);
                    }
                }

                function findPreferredParentUnderRoot(parentKey)
                {
                    var all = findAllNodesByKey(parentKey);
                    if (all.length === 0)
                    {
                        return null;
                    }

                    for (var i = 0; i < all.length; i++)
                    {
                        var n = all[i];
                        if (!ancestorChainContains(n, ROOT_KEY))
                        {
                            continue;
                        }

                        var hasType = false;
                        var cur = n;
                        while (cur)
                        {
                            if (cur !== n && isTypeSyntheticNode(cur))
                            {
                                hasType = true;
                                break;
                            }
                            try
                            {
                                cur = cur.getParent ? cur.getParent() : null;
                            }
                            catch (_)
                            {
                                cur = null;
                            }
                        }
                        if (!hasType)
                        {
                            return n;
                        }
                    }

                    for (var j = 0; j < all.length; j++)
                    {
                        if (ancestorChainContains(all[j], ROOT_KEY))
                        {
                            return all[j];
                        }
                    }

                    return null;
                }

                function findChildUnderParentByVariants(parentNode, variantKeys)
                {
                    if (!parentNode || !parentNode.children || parentNode.children.length === 0)
                    {
                        return null;
                    }

                    for (var i = 0; i < parentNode.children.length; i++)
                    {
                        var c = parentNode.children[i];
                        if (!c)
                        {
                            continue;
                        }
                        for (var v = 0; v < variantKeys.length; v++)
                        {
                            if (c.key === variantKeys[v])
                            {
                                return c;
                            }
                        }
                    }
                    return null;
                }

                function scheduleRetry()
                {
                    if (selectionRetryCount >= MAX_SELECTION_RETRIES)
                    {
                        return;
                    }
                    selectionRetryCount++;
                    setTimeout(applyQuerySelection, 110);
                }

                function activateNodeFinal(n)
                {
                    if (!n)
                    {
                        return;
                    }

                    try
                    {
                        focusBranch(n, function ()
                        {
                            try
                            {
                                n.setActive(true);
                            }
                            catch (_)
                            {
                            }

                            try
                            {
                                persistActiveKey(n);
                            }
                            catch (_)
                            {
                            }

                            if (isMobile())
                            {
                                updateActionBar(n);
                            }
                            else
                            {
                                loadDetails(n);
                            }

                            querySelectionApplied = true;
                        });
                    }
                    catch (_)
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

                        try
                        {
                            persistActiveKey(n);
                        }
                        catch (_)
                        {
                        }

                        if (isMobile())
                        {
                            updateActionBar(n);
                        }
                        else
                        {
                            loadDetails(n);
                        }

                        querySelectionApplied = true;
                    }
                }

                function finalizeActivateGeneric()
                {
                    var preferred = choosePreferredInstanceFromVariants(variants.length ? variants : [selectKey], expandKey);
                    if (!preferred)
                    {
                        var rootNode = tree.getRootNode();
                        if (!rootNode)
                        {
                            scheduleRetry();
                            return;
                        }

                        var idx = 0;
                        function next()
                        {
                            if (idx >= (variants.length ? variants.length : 1))
                            {
                                scheduleRetry();
                                return;
                            }
                            var t = variants.length ? variants[idx++] : selectKey;

                            loadUnderRootUntilKey(t, 600, function ()
                            {
                                var p = choosePreferredInstanceFromVariants(variants.length ? variants : [selectKey], expandKey);
                                if (p)
                                {
                                    activateNodeFinal(p);
                                    return;
                                }
                                setTimeout(next, 60);
                            });
                        }

                        next();
                        return;
                    }

                    activateNodeFinal(preferred);
                }

                // Creation-aware poll: reload the intended parent under ROOT until the child appears (strictly under ROOT).
                function pollForChildUnderParent(parentNode, variantKeys, maxTries, intervalMs, onDone)
                {
                    var tries = 0;

                    function tick()
                    {
                        tries++;

                        reloadChildrenNoCache(parentNode).then(function ()
                        {
                            var hit = findChildUnderParentByVariants(parentNode, variantKeys);
                            if (hit)
                            {
                                onDone(hit);
                                return;
                            }

                            if (tries >= maxTries)
                            {
                                onDone(null);
                                return;
                            }

                            setTimeout(tick, intervalMs);
                        });
                    }

                    tick();
                }

                function attemptCodeParentSelection()
                {
                    if (!expandKey)
                    {
                        finalizeActivateGeneric();
                        return;
                    }

                    var parentUnderRoot = findPreferredParentUnderRoot(expandKey);

                    if (!parentUnderRoot)
                    {
                        loadUnderRootUntilKey(expandKey, 400, function ()
                        {
                            setTimeout(attemptCodeParentSelection, 80);
                        });
                        return;
                    }

                    function afterReady()
                    {
                        var found = findChildUnderParentByVariants(parentUnderRoot, variants.length ? variants : [selectKey]);
                        if (found)
                        {
                            activateNodeFinal(found);
                            return;
                        }

                        // If this is a fresh-create redirect, wait a bit by reloading the correct parent under ROOT only.
                        if (createdFlow)
                        {
                            // ~6s total with 40 * 150ms; adjust if needed
                            pollForChildUnderParent(parentUnderRoot, (variants.length ? variants : [selectKey]), 40, 150, function (node)
                            {
                                if (node)
                                {
                                    activateNodeFinal(node);
                                    return;
                                }

                                // Fallback: try materializing under ROOT broadly, but still prefer ROOT instance
                                var vidx = 0;
                                function tryNextVariant()
                                {
                                    if (vidx >= (variants.length ? variants.length : 1))
                                    {
                                        finalizeActivateGeneric();
                                        return;
                                    }

                                    var vk = variants.length ? variants[vidx++] : selectKey;
                                    loadUnderRootUntilKey(vk, 600, function ()
                                    {
                                        var again = findChildUnderParentByVariants(parentUnderRoot, (variants.length ? variants : [selectKey]));
                                        if (again)
                                        {
                                            activateNodeFinal(again);
                                            return;
                                        }
                                        setTimeout(tryNextVariant, 60);
                                    });
                                }

                                tryNextVariant();
                            });
                            return;
                        }

                        // Non-create flow: materialize under ROOT, then prefer ROOT instance
                        var idx = 0;
                        function tryNext()
                        {
                            if (idx >= (variants.length ? variants.length : 1))
                            {
                                finalizeActivateGeneric();
                                return;
                            }

                            var t = variants.length ? variants[idx++] : selectKey;
                            loadUnderRootUntilKey(t, 600, function ()
                            {
                                var p = choosePreferredInstanceFromVariants(variants.length ? variants : [selectKey], expandKey);
                                if (p)
                                {
                                    activateNodeFinal(p);
                                    return;
                                }
                                setTimeout(tryNext, 80);
                            });
                        }

                        tryNext();
                    }

                    var doReload = function ()
                    {
                        reloadChildrenNoCache(parentUnderRoot).then(function ()
                        {
                            setTimeout(afterReady, 40);
                        });
                    };

                    if (parentUnderRoot.lazy && !parentUnderRoot.loaded)
                    {
                        var ex = parentUnderRoot.setExpanded(true);
                        if (ex && typeof ex.then === "function")
                        {
                            ex.then(doReload, doReload);
                        }
                        else if (ex && ex.done)
                        {
                            ex.done(doReload).fail(doReload);
                        }
                        else
                        {
                            doReload();
                        }
                        return;
                    }

                    if (parentUnderRoot.folder && !parentUnderRoot.expanded)
                    {
                        var ex2 = parentUnderRoot.setExpanded(true);
                        if (ex2 && typeof ex2.then === "function")
                        {
                            ex2.then(doReload, doReload);
                        }
                        else if (ex2 && ex2.done)
                        {
                            ex2.done(doReload).fail(doReload);
                        }
                        else
                        {
                            doReload();
                        }
                        return;
                    }

                    doReload();
                }

                if (isCodeKey)
                {
                    attemptCodeParentSelection();
                }
                else
                {
                    if (expandKey)
                    {
                        var parentUnderRoot = findPreferredParentUnderRoot(expandKey);
                        if (!parentUnderRoot)
                        {
                            loadUnderRootUntilKey(expandKey, 400, function ()
                            {
                                setTimeout(finalizeActivateGeneric, 60);
                            });
                            return;
                        }

                        var ex = parentUnderRoot.setExpanded(true);
                        var afterExpand = function ()
                        {
                            reloadChildrenNoCache(parentUnderRoot).then(function ()
                            {
                                setTimeout(finalizeActivateGeneric, 40);
                            });
                        };
                        if (ex && typeof ex.then === "function")
                        {
                            ex.then(afterExpand, afterExpand);
                            return;
                        }
                        if (ex && ex.done)
                        {
                            ex.done(afterExpand).fail(afterExpand);
                            return;
                        }
                        afterExpand();
                        return;
                    }

                    finalizeActivateGeneric();
                }
            }
            catch (_)
            {
                // silent
            }
        }

        // Fallback retry: when URL requests a selection, ensure applyQuerySelection runs until applied.
        // Conservative, throttled attempts (self-scheduling) to avoid overlapping calls.
        (function ()
        {
            try
            {
                var qs = new URLSearchParams(window.location.search || "");
                if (!(qs.has("select") || qs.has("returnKey") || qs.has("key"))) { return; }

                var tries = 0;
                var maxTries = 20; // 20 * minInterval = maximum total time
                var minInterval = 150; // ms between attempts (throttle)
                var lastCall = 0;

                function attempt()
                {
                    try
                    {
                        // Stop if applied or exhausted
                        if (typeof querySelectionApplied !== "undefined" && querySelectionApplied === true)
                        {
                            return;
                        }

                        if (tries >= maxTries)
                        {
                            return;
                        }

                        var now = Date.now();
                        var delta = now - lastCall;
                        if (delta < minInterval)
                        {
                            // schedule next attempt to respect throttle
                            setTimeout(attempt, minInterval - delta);
                            return;
                        }

                        lastCall = now;
                        tries++;

                        try
                        {
                            if (typeof applyQuerySelection === "function")
                            {
                                applyQuerySelection();
                            }
                        }
                        catch (_)
                        {
                        }

                        // If not yet applied, schedule next attempt
                        if (!(typeof querySelectionApplied !== "undefined" && querySelectionApplied === true) && tries < maxTries)
                        {
                            setTimeout(attempt, minInterval);
                        }
                    }
                    catch (_)
                    {
                        // swallow and schedule a safe retry up to limit
                        tries++;
                        if (tries < maxTries)
                        {
                            setTimeout(attempt, minInterval);
                        }
                    }
                }

                // Kick off first attempt
                setTimeout(attempt, minInterval);
            }
            catch (_)
            {
            }
        })();

        // Open an action page in RHS (desktop) or navigate (mobile)
        function openAction(actionName, key, parentKey, extras)
        {
            var base = basePageUrl;
            if (!base)
            {
                alert("Action endpoint not configured.");
                return;
            }

            // Determine returnKey (current active node preferred)
            var returnKey = "";
            try
            {
                var t = getTree();
                var active = t && t.getActiveNode ? t.getActiveNode() : null;
                returnKey = (active && active.key) ? active.key : (key || "");
            }
            catch (_)
            {
                returnKey = key || "";
            }

            // Persist for embedded Cancel
            try
            {
                if (typeof window.tcSetCancelReturn === "function")
                {
                    window.tcSetCancelReturn(returnKey || "");
                }
            }
            catch (_)
            {
            }

            // Build query parameters
            var parts = [];
            parts.push("key=" + encodeURIComponent(key || ""));
            if (parentKey)
            {
                parts.push("parentKey=" + encodeURIComponent(parentKey));
            }
            if (returnKey)
            {
                parts.push("returnKey=" + encodeURIComponent(returnKey));
            }
            if (extras && typeof extras === "object")
            {
                for (var p in extras)
                {
                    if (!Object.prototype.hasOwnProperty.call(extras, p))
                    {
                        continue;
                    }
                    var v = extras[p];
                    if (v === null || typeof v === "undefined")
                    {
                        continue;
                    }
                    parts.push(encodeURIComponent(p) + "=" + encodeURIComponent(v));
                }
            }

            var url = base + "/" + encodeURIComponent(actionName) + "?" + parts.join("&");

            // Mobile: full layout navigation (no embed)
            if (isMobile())
            {
                window.location.href = url;
                return;
            }

            // Desktop: embedded in RHS pane
            url = appendQuery(url, "embed", "1");
            var $pane = $("#detailsPane");
            if ($pane.length)
            {
                $pane.css("overflow", "auto");
                $.get(nocache(url))
                    .done(function (html)
                    {
                        $pane.html(html);

                        // If the page signaled a newly created node, apply it immediately
                        try
                        {
                            tryApplyEmbedMarker($pane);
                        }
                        catch (_)
                        {
                        }
                    })
                    .fail(function ()
                    {
                        $pane.html("<div class='text-danger small p-2'>Failed to load action.</div>");
                    });
            }
            else
            {
                // Fallback: full page if pane missing
                window.location.href = url;
            }
        }

        function openEmbeddedUrl(url)
        {
            if (isMobile())
            {
                window.location.href = url;
                return;
            }

            var $pane = $("#detailsPane");
            if ($pane.length)
            {
                $pane.css("overflow", "auto");
                $.get(nocache(url))
                    .done(function (html)
                    {
                        $pane.html(html);

                        // Also honor embed result markers when navigating arbitrary URLs into the pane
                        try
                        {
                            tryApplyEmbedMarker($pane);
                        }
                        catch (_)
                        {
                        }
                    })
                    .fail(function ()
                    {
                        $pane.html("<div class='text-muted small p-2'>No details</div>");
                    });
            }
            else
            {
                window.location.href = url;
            }
        }

        // Small global helpers to reduce duplication and keep behaviour identical
        function postJsonGlobal(handler, data)
        {
            var token = antiXsrf();
            return $.ajax({
                type: "POST",
                url: handlerUrl(handler),
                data: data,
                headers: token ? { "RequestVerificationToken": token } : {},
                dataType: "json"
            });
        }

        function getEffectiveParentKey(node, menuParentKey)
        {
            if (typeof menuParentKey === "string" && menuParentKey) { return menuParentKey; }
            try
            {
                var p = node && node.getParent ? node.getParent() : null;
                return (p && p.key) ? p.key : "";
            }
            catch (_)
            {
                return "";
            }
        }

        function refreshTopAnchors()
        {
            try
            {
                var t = getTree();
                if (!t) { return; }
                var root = t.getNodeByKey(ROOT_KEY);
                var disc = t.getNodeByKey(DISC_KEY);
                reloadIfExpandedNode(root);
                reloadIfExpandedNode(disc);
            }
            catch (_)
            {
                // swallow
            }
        }

        function openDeleteFor(node, menuParentKey)
        {
            try
            {
                if (!node)
                {
                    alert("Select a node first");
                    return;
                }

                var kinds = getNodeKinds(node);
                var key = kinds.key;
                var data = kinds.data || {};

                // Resolve parentKey
                var parentKey = (typeof menuParentKey === "string" && menuParentKey) ? menuParentKey : "";
                if (!parentKey)
                {
                    try
                    {
                        var p = node.getParent && node.getParent();
                        parentKey = (p && p.key) ? p.key : "";
                    }
                    catch (_)
                    {
                        parentKey = "";
                    }
                }

                // Cash Code leaf -> DeleteCashCode
                if (kinds.isCode || (key && typeof key === "string" && key.indexOf("code:") === 0))
                {
                    // Desktop embeds in RHS; Mobile uses full-page with layout (no embed)
                    var baseUrl = basePageUrl + "/DeleteCashCode?key=" + encodeURIComponent(key);

                    if (isMobile())
                    {
                        window.location.href = baseUrl; // no embed on mobile
                    }
                    else
                    {
                        openEmbeddedUrl(appendQuery(baseUrl, "embed", "1"));
                    }
                    return;
                }

                var catType = (typeof data.categoryType !== "undefined") ? Number(data.categoryType) : NaN;
                var isCashCodeCategory = (catType === 0);
                var isTotalCategory = (catType === CATEGORYTYPE_CASHTOTAL);

                if (isCashCodeCategory)
                {
                    openAction("DeleteCategory", key);
                    return;
                }

                // Disconnected Total -> delete the category (no mapping parent)
                if (parentKey === DISC_KEY && isTotalCategory)
                {
                    openAction("DeleteCategory", key);
                    return;
                }

                // ROOT-level Total -> delete the category tree (no mapping from ROOT exists)
                if (parentKey === ROOT_KEY && isTotalCategory)
                {
                    openAction("DeleteCategory", key);
                    return;
                }

                // Totals context with a real parent (not type or synthetic) -> DeleteTotal (mapping-based)
                var parentIsSyntheticType = (typeof parentKey === "string" && parentKey.indexOf("type:") === 0);
                if (parentKey && parentKey !== "" && !parentIsSyntheticType && isTotalCategory)
                {
                    openAction("DeleteTotal", "", parentKey, { childKey: key });
                    return;
                }

                // Fallback
                openAction("DeleteCategory", key);
            }
            catch (ex)
            {
                console.error("openDeleteFor failed", ex);
                alert("Unable to perform delete action");
            }
        }
        function tcIsSyntheticKey(key)
        {
            if (!key)
            {
                return true;
            }
            return key === "__DISCONNECTED__"
                || key === "__ROOT__"
                || /^root_\d+$/i.test(key)
                || (typeof key === "string" && key.indexOf("type:") === 0);
        }

        // Load details into RHS pane (desktop only)
        function loadDetails(node)
        {
            if (!detailsUrl) { return; }
            if (isMobile()) { return; }

            var $pane = $("#detailsPane");
            if ($pane.length)
            {
                $pane.css("overflow", "auto");
            }

            if (!node)
            {
                $pane.html("<div class='text-muted small p-2'>No details</div>");
                return;
            }

            // Extract keys
            var key = node.key || "";
            var parentKey = "";
            if (node && typeof node.getParent === "function")
            {
                var p = node.getParent();
                parentKey = (p && p.key) ? p.key : "";
            }

            // Synthetic fallback: show parent details if parent real
            if (tcIsSyntheticKey(key))
            {
                if (parentKey && !tcIsSyntheticKey(parentKey))
                {
                    var parentUrl = detailsUrl + "?key=" + encodeURIComponent(parentKey);
                    $.get(nocache(parentUrl))
                        .done(function (html)
                        {
                            $pane.html(html);
                            applyDetailsVisibility(node, $pane);
                        })
                        .fail(function ()
                        {
                            $pane.html("<div class='text-muted small p-2'>No details</div>");
                        });
                }
                else
                {
                    $pane.html("<div class='text-muted small p-2'>No details</div>");
                }
                return;
            }

            // Build URL for the selected node
            var url = detailsUrl + "?key=" + encodeURIComponent(key);
            if (parentKey && !tcIsSyntheticKey(parentKey))
            {
                url += "&parentKey=" + encodeURIComponent(parentKey);
            }

            $.get(nocache(url))
                .done(function (html)
                {
                    $pane.html(html);
                    applyDetailsVisibility(node, $pane);
                })
                .fail(function ()
                {
                    $pane.html("<div class='text-muted small p-2'>No details</div>");
                });
        }

        // Detect an embed result marker in the pane and trigger in-place selection
        function tryApplyEmbedMarker($scope)
        {
            try
            {
                var $m = ($scope && $scope.length ? $scope : $("#detailsPane")).find("#tcEmbedResult").first();
                if (!$m.length)
                {
                    return false;
                }

                var selectVal = $m.attr("data-select") || "";
                var expandVal = $m.attr("data-expand") || "";
                if (!selectVal || !expandVal)
                {
                    return false;
                }

                // Update the page URL (no navigation)
                try
                {
                    var u = new URL(window.location.href);
                    u.searchParams.set("select", selectVal);
                    u.searchParams.set("key", selectVal);
                    u.searchParams.set("expand", expandVal);
                    window.history.replaceState({}, "", u.toString());
                }
                catch (_)
                {
                }

                // Re-run selection pipeline (creation-aware)
                try
                {
                    querySelectionApplied = false;
                    selectionRetryCount = 0;
                    applyQuerySelection();
                }
                catch (_)
                {
                }

                return true;
            }
            catch (_)
            {
                return false;
            }
        }

        // Observe #detailsPane so POST-backs that replace its HTML still trigger selection
        function bindEmbedMarkerObserver()
        {
            try
            {
                var pane = document.getElementById("detailsPane");
                if (!pane || typeof MutationObserver === "undefined")
                {
                    return;
                }

                var obs = new MutationObserver(function (mutations)
                {
                    for (var i = 0; i < mutations.length; i++)
                    {
                        var m = mutations[i];
                        if (m.type === "childList" && m.addedNodes && m.addedNodes.length)
                        {
                            try
                            {
                                tryApplyEmbedMarker($(pane));
                            }
                            catch (_)
                            {
                            }
                        }
                    }
                });

                obs.observe(pane, { childList: true, subtree: true });
            }
            catch (_)
            {
            }
        }

        // Visibility application extracted for clarity
        function applyDetailsVisibility(node, $pane)
        {
            try
            {
                var kinds = getNodeKinds(node);
                var data = kinds.data || {};
                var key = kinds.key || "";
                var parentKeyNow = "";
                if (node && node.getParent)
                {
                    var pp = node.getParent();
                    parentKeyNow = (pp && pp.key) ? pp.key : "";
                }

                var isCode = kinds.isCode === true;
                var isCat = kinds.isCat === true;
                var isRoot = kinds.isRoot === true;
                var isDiscRoot = kinds.isDisconnect === true;
                var isTypeNode = (typeof key === "string" && key.indexOf("type:") === 0)
                    || (data && (data.syntheticKind === "type" || data.syntheticKind === "typesRoot" || data.isTypeContext === true));
                var inTypeCtx = (typeof parentKeyNow === "string" && parentKeyNow.indexOf("type:") === 0);

                var catType = (typeof data.categoryType !== "undefined") ? Number(data.categoryType) : NaN;
                var isCatTotal = (catType === 1);
                var isCatCashCode = (catType === 0);

                function dv(action, show)
                {
                    var $btns = $pane.find(actionSelectors(action).join(","));
                    $btns.toggle(!!show);
                    return $btns;
                }

                function setButtonText($scope, action, text)
                {
                    var $btns = $scope.find(actionSelectors(action).join(","));
                    $btns.each(function ()
                    {
                        var $b = $(this);
                        var html = $b.html() || "";
                        if (/<i[^>]*>/i.test(html))
                        {
                            $b.html(html.replace(/(<i[^>]*>.*?<\/i>\s*)[^<]*/i, "$1" + text));
                        }
                        else
                        {
                            $b.text(text);
                        }
                    });
                }

                var known = [
                    "edit","delete","toggleEnabled",
                    "addCategory","addCashCode","addTotal",
                    "move","moveUp","moveDown","setProfitRoot","setVatRoot","expand","collapse"
                ];
                for (var i = 0; i < known.length; i++)
                {
                    dv(known[i], false);
                }

                if (isRoot || isDiscRoot || isTypeNode)
                {
                    return;
                }

                var order = [];

                if (isCode)
                {
                    dv("edit", isAdmin);
                    dv("delete", isAdmin);
                    dv("toggleEnabled", isAdmin);
                    order = ["edit", "delete", "toggleEnabled"];
                    arrangeDetailsButtons($pane, order);
                    setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                    return;
                }

                if (!isCat)
                {
                    return;
                }

                if (inTypeCtx)
                {
                    dv("edit", isAdmin);
                    dv("delete", isAdmin);
                    dv("toggleEnabled", isAdmin);
                    dv("addCashCode", isAdmin);
                    dv("move", isAdmin);

                    order = ["edit", "delete", "toggleEnabled", "addCashCode", "move"];
                    arrangeDetailsButtons($pane, order);
                    setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                    return;
                }

                if (parentKeyNow === DISC_KEY)
                {
                    dv("edit", isAdmin);
                    dv("delete", isAdmin);
                    dv("toggleEnabled", isAdmin);

                    if (isCatTotal)
                    {
                        // Totals: show Add Category + Add Total; hide Add Cash Code
                        dv("addCategory", isAdmin);
                        dv("addTotal", isAdmin);

                        // Relabel buttons for clarity
                        setButtonText($pane, "addCategory", "Add Category");
                        setButtonText($pane, "addTotal", "Add Total");

                        dv("move", isAdmin);
                        order = ["edit", "delete", "toggleEnabled", "addCategory", "addTotal", "move"];
                    }
                    else if (isCatCashCode)
                    {
                        dv("addCashCode", isAdmin);
                        dv("move", isAdmin);
                        order = ["edit", "delete", "toggleEnabled", "addCashCode", "move"];
                    }

                    arrangeDetailsButtons($pane, order);
                    setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                    return;
                }

                if (parentKeyNow === ROOT_KEY)
                {
                    if (isCatTotal)
                    {
                        dv("edit", isAdmin);
                        dv("delete", isAdmin);
                        dv("toggleEnabled", isAdmin);

                        dv("addCategory", isAdmin);
                        dv("addTotal", isAdmin);

                        // Relabel
                        setButtonText($pane, "addCategory", "Add Category");
                        setButtonText($pane, "addTotal", "Add Total");

                        dv("move", isAdmin);
                        dv("setProfitRoot", isAdmin);
                        dv("setVatRoot", isAdmin);

                        order = ["edit", "delete", "toggleEnabled", "addCategory", "addTotal", "move", "setProfitRoot", "setVatRoot"];
                        arrangeDetailsButtons($pane, order);
                        setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                    }
                    else if (isCatCashCode)
                    {
                        dv("delete", isAdmin);
                        dv("toggleEnabled", isAdmin);
                        dv("addCashCode", isAdmin);
                        dv("move", isAdmin);

                        order = ["delete", "toggleEnabled", "addCashCode", "move"];
                        arrangeDetailsButtons($pane, order);
                        setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                    }
                    return;
                }

                // Normal totals context
                if (isCatTotal)
                {
                    dv("edit", isAdmin);
                    dv("delete", isAdmin);
                    dv("toggleEnabled", isAdmin);

                    dv("addCategory", isAdmin);
                    dv("addTotal", isAdmin);

                    // Relabel
                    setButtonText($pane, "addCategory", "Add Category");
                    setButtonText($pane, "addTotal", "Add Total");

                    dv("move", isAdmin);

                    order = ["edit", "delete", "toggleEnabled", "addCategory", "addTotal", "move"];
                    arrangeDetailsButtons($pane, order);
                    setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                }
                else if (isCatCashCode)
                {
                    dv("edit", isAdmin);
                    dv("delete", isAdmin);
                    dv("toggleEnabled", isAdmin);
                    dv("addCashCode", isAdmin);
                    dv("move", isAdmin);

                    order = ["edit", "delete", "toggleEnabled", "addCashCode", "move"];
                    arrangeDetailsButtons($pane, order);
                    setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                }
            }
            catch (_)
            {
            }
        }

        // Helpers to classify node
        function getNodeKinds(node)
        {
            var key = node ? node.key : null;
            var data = (node && node.data) || {};

            // Prefer explicit server-provided nodeType when available
            var nodeType = (typeof data.nodeType !== "undefined" && data.nodeType !== null) ? String(data.nodeType) : null; // "category" | "code" | "synthetic" | null

            // If not provided, infer conservatively from key and folder flag
            if (!nodeType)
            {
                if (typeof key === "string" && key.indexOf("code:") === 0)
                {
                    nodeType = "code";
                }
                else if (typeof key === "string" && key.indexOf("type:") === 0)
                {
                    nodeType = "synthetic";
                }
                else if (node && node.folder)
                {
                    // folder -> assume category (safe for menus that act on categories)
                    nodeType = "category";
                }
                else
                {
                    // fallback: treat as synthetic to avoid showing category-only actions for unknown leaves
                    nodeType = "synthetic";
                }
            }

            var isSynthetic = !node || key === ROOT_KEY || key === DISC_KEY || nodeType === "synthetic";
            var isCode = (nodeType === "code") || (key && typeof key === "string" && key.indexOf("code:") === 0);
            var isCat = !!(node && node.folder && !isCode && !isSynthetic);
            var isRoot = key === ROOT_KEY;
            var isDisconnect = key === DISC_KEY;

            // numeric metadata (defensive)
            var categoryType = (typeof data.categoryType !== "undefined") ? Number(data.categoryType) : undefined;
            var cashPolarity = (typeof data.cashPolarity !== "undefined") ? Number(data.cashPolarity) : undefined;

            return {
                key: key,
                data: data,
                nodeType: nodeType,
                isSynthetic: isSynthetic,
                isCode: isCode,
                isCat: isCat,
                isRoot: isRoot,
                isDisconnect: isDisconnect,
                categoryType: categoryType,
                cashPolarity: cashPolarity
            };
        }

        // Column sizing: keep tree and details aligned to footer
        function resizeColumns()
        {
            try
            {
                var $tree = $(treeSel);
                var $pane = $("#detailsPane");
                if ($tree.length === 0) { return; }

                $tree.css("overflow", "auto");
                $pane.css("overflow", "auto");

                // Use viewport-relative top to avoid scroll offset issues
                var treeEl = $tree.get(0);
                var rectTop = treeEl ? treeEl.getBoundingClientRect().top : 0;

                var vh = window.innerHeight || document.documentElement.clientHeight || 0;
                var footerH = getFooterHeight();
                var gutter = 16;

                var h = Math.max(220, Math.floor(vh - rectTop - footerH - gutter));

                $tree.css("height", h + "px");
                if (!isMobile() && $pane.length)
                {
                    $pane.css("height", h + "px");
                }

                ensureTreeContainerSizing();
            }
            catch { /* no-op */ }
        }

        function getFooterHeight()
        {
            var $f = $("footer");
            if ($f.length === 0) { return 0; }
            return $f.outerHeight(true) || 0;
        }

        function ensureTreeContainerSizing()
        {
            var $ft = $(treeSel).find(".fancytree-container");
            if ($ft.length)
            {
                $ft.css({ height: "100%", overflow: "auto" });
            }
        }

        // Short helper to get the Fancytree Tree instance without using deprecated plugin call
        function getTree()
        {
            try
            {
                if (!$.ui || !$.ui.fancytree) { return null; }
                var el = $(treeSel).get(0);
                return $.ui.fancytree.getTree(el);
            }
            catch (ex)
            {
                return null;
            }
        }

        // Safe reload helper — resolves key/plain object and calls reloadChildren(options) only if available/expanded
        function reloadIfExpandedNode(nodeOrKey, options)
        {
            try
            {
                var tree = getTree();
                if (!tree) { return; }
                if (!nodeOrKey) { return; }

                var node = null;

                if (typeof nodeOrKey === "string")
                {
                    node = tree.getNodeByKey(nodeOrKey);
                }
                else if (nodeOrKey && typeof nodeOrKey.key === "string")
                {
                    node = tree.getNodeByKey(nodeOrKey.key) || nodeOrKey;
                }
                else
                {
                    node = nodeOrKey;
                }

                if (node && typeof node.reloadChildren === "function" && node.expanded)
                {
                    if (options) { node.reloadChildren(options); }
                    else { node.reloadChildren(); }
                }
            }
            catch (ex)
            {
                console.warn("reloadIfExpandedNode failed", ex);
            }
        }

        // ---------- State persistence (expanded/active), toast, aria-live, autoscroll ----------

        function stateKey(name)
        {
            // Namespace per page and root
            var rootPart = (typeof ROOT_KEY !== "undefined" && ROOT_KEY) ? ROOT_KEY : "root";
            var pagePart = (window.location && window.location.pathname) ? window.location.pathname : "page";
            return "tc.categoryTree." + name + "." + pagePart + "." + rootPart;
        }

        function loadExpandedSet()
        {
            try
            {
                var raw = localStorage.getItem(stateKey("expanded"));
                var arr = raw ? JSON.parse(raw) : [];
                if (!Array.isArray(arr)) { arr = []; }
                return new Set(arr);
            }
            catch (ex)
            {
                return new Set();
            }
        }

        function saveExpandedSet(set)
        {
            try
            {
                localStorage.setItem(stateKey("expanded"), JSON.stringify(Array.from(set)));
            }
            catch (ex)
            {
            }
        }

        function persistExpanded(node, expanded)
        {
            if (!node || !node.key) { return; }
            var set = loadExpandedSet();
            if (expanded)
            {
                set.add(node.key);
            }
            else
            {
                set.delete(node.key);
            }
            saveExpandedSet(set);
        }

        function restoreExpandedForNode(node, expandSet)
        {
            try
            {
                if (!node || !node.children) { return; }
                var set = expandSet || loadExpandedSet();
                // Expand any child that is marked expanded; lazy expand is handled by Fancytree on demand
                node.children.forEach(function (ch)
                {
                    if (ch && ch.key && set.has(ch.key))
                    {
                        ch.setExpanded(true);
                    }
                });
            }
            catch (ex)
            {
            }
        }

        function persistActiveKey(node)
        {
            try
            {
                var k = (node && node.key) ? node.key : "";
                localStorage.setItem(stateKey("active"), k || "");
            }
            catch (ex)
            {
            }
        }

        function loadActiveKey()
        {
            try
            {
                return localStorage.getItem(stateKey("active")) || "";
            }
            catch (ex)
            {
                return "";
            }
        }

        // Lightweight toast
        function notify(message, kind)
        {
            try
            {
                var type = kind || "info"; // info|success|warning|danger
                var el = document.createElement("div");
                el.className = "tc-toast alert alert-" + (type === "error" ? "danger" : type);
                el.textContent = message || "";
                el.style.position = "fixed";
                el.style.right = "12px";
                el.style.bottom = "12px";
                el.style.zIndex = "1080";
                el.style.padding = "8px 12px";
                el.style.boxShadow = "0 0.25rem 0.75rem rgba(0,0,0,.15)";
                document.body.appendChild(el);
                setTimeout(function ()
                {
                    if (el && el.parentNode)
                    {
                        el.parentNode.removeChild(el);
                    }
                }, 1800);
            }
            catch (ex)
            {
            }
        }

        // Accessibility: aria-live polite region
        function ensureAriaLive()
        {
            var live = document.getElementById("tcAriaLive");
            if (!live)
            {
                live = document.createElement("div");
                live.id = "tcAriaLive";
                live.setAttribute("aria-live", "polite");
                live.className = "visually-hidden";
                document.body.appendChild(live);
            }
            return live;
        }

        function announce(text)
        {
            try
            {
                var live = ensureAriaLive();
                // Clear then set to force screen readers to read
                live.textContent = "";
                setTimeout(function () { live.textContent = text || ""; }, 10);
            }
            catch (ex)
            {
            }
        }

        // Autoscroll tree container while dragging near edges
        function bindAutoscrollHandlers()
        {
            var $cont = $(treeSel).find(".fancytree-container");
            if ($cont.length === 0) { return; }

            $cont.on("dragover", function (e)
            {
                var container = this;
                var rect = container.getBoundingClientRect();
                var y = (e.originalEvent && e.originalEvent.clientY) ? e.originalEvent.clientY : e.clientY;
                var topGap = y - rect.top;
                var bottomGap = rect.bottom - y;

                var threshold = 40; // px
                var maxStep = 18;   // px per event

                if (topGap < threshold)
                {
                    var stepUp = Math.ceil((threshold - topGap) / 4);
                    container.scrollTop = Math.max(0, container.scrollTop - Math.min(maxStep, stepUp));
                }
                else if (bottomGap < threshold)
                {
                    var stepDown = Math.ceil((threshold - bottomGap) / 4);
                    container.scrollTop = container.scrollTop + Math.min(maxStep, stepDown);
                }
            });
        }

        // Allow moving leafs under Disconnected; restrict to folder-only elsewhere (via menu)
        function moveNodeInUi(node, direction)
        {
            if (!node) {return;}
            var parent = node.getParent && node.getParent();
            var parentKey = parent ? parent.key : "";
            var cursor = direction === "up" ? node.getPrevSibling() : node.getNextSibling();

            if (parentKey !== DISC_KEY)
            {
                while (cursor && !cursor.folder)
                {
                    cursor = direction === "up" ? cursor.getPrevSibling() : cursor.getNextSibling();
                }
            }

            if (cursor)
            {
                node.moveTo(cursor, direction === "up" ? "before" : "after");
            }
        }

        // Recursively expand selected node and all descendants (handles lazy nodes)
        function expandSubtree(node)
        {
            function expandNode(n)
            {
                return new Promise(function (resolve, reject)
                {
                    if (!n || !n.folder)
                    {
                        resolve();
                        return;
                    }

                    var res = n.setExpanded(true);

                    function afterExpand()
                    {
                        var children = n.children || [];
                        var chain = Promise.resolve();
                        children.forEach(function (ch)
                        {
                            chain = chain.then(function ()
                            {
                                return expandNode(ch);
                            });
                        });
                        chain.then(resolve).catch(reject);
                    }

                    if (res && typeof res.then === "function")
                    {
                        res.then(afterExpand, reject);
                    }
                    else if (res && typeof res.done === "function")
                    {
                        res.done(afterExpand).fail(reject);
                    }
                    else
                    {
                        afterExpand();
                    }
                });
            }

            return expandNode(node);
        }

        // Recursively collapse the selected node and all descendants (no lazy-load needed)
        function collapseSubtree(node)
        {
            if (!node) {return;}

            node.visit(function (n)
            {
                if (n !== node && n.folder && n.expanded)
                {
                    n.setExpanded(false);
                }
            });

            if (node.folder && node.expanded)
            {
                node.setExpanded(false);
            }
        }

        // Update UI enabled state; optionally cascade to descendant categories (never codes)
        function setNodeEnabledInUi(node, enabled, cascadeCategories)
        {
            function isCategory(n)
            {
                return !!(n && n.folder && n.data && n.data.nodeType === "category");
            }

            function apply(n)
            {
                if (!n) {return;}
                n.data = n.data || {};
                n.data.isEnabled = enabled ? 1 : 0;
                n.toggleClass("tc-disabled", !enabled);
            }

            if (!cascadeCategories || !isCategory(node))
            {
                if (isCategory(node) || (node && node.data && node.data.nodeType === "code"))
                {
                    apply(node);
                }
                return;
            }

            apply(node);
            node.visit(function (n)
            {
                if (n !== node && isCategory(n))
                {
                    apply(n);
                }
            });
        }

        // Remove leading/trailing and duplicate adjacent separators
        function normalizeDividers($menu)
        {
            var $children = $menu.children().filter(":visible");
            $menu.find(".dropdown-divider").hide();
            $children = $menu.children().filter(":visible");
            if ($children.length === 0) {return;}

            var prevWasItem = false;
            $menu.children().each(function ()
            {
                var $el = $(this);
                if (!$el.is(":visible")) {return;}

                if ($el.hasClass("dropdown-item"))
                {
                    prevWasItem = true;
                    return;
                }

                if ($el.hasClass("dropdown-divider"))
                {
                    var $nextVisible = $el.nextAll(":visible").first();
                    var nextIsItem = $nextVisible.length && $nextVisible.hasClass("dropdown-item");
                    var show = prevWasItem && nextIsItem;
                    $el.toggle(show);
                }
            });

            var lastWasDivider = false;
            $menu.children().each(function ()
            {
                var $el = $(this);
                if (!$el.is(":visible")) {return;}

                if ($el.hasClass("dropdown-divider"))
                {
                    if (lastWasDivider)
                    {
                        $el.hide();
                    }
                    lastWasDivider = $el.is(":visible");
                }
                else
                {
                    lastWasDivider = false;
                }
            });

            var $firstVisible = $menu.children(":visible").first();
            if ($firstVisible.hasClass("dropdown-divider"))
            {
                $firstVisible.hide();
            }

            var $lastVisible = $menu.children(":visible").last();
            if ($lastVisible.hasClass("dropdown-divider"))
            {
                $lastVisible.hide();
            }
        }

        // Helper: alias resolution for data-action selectors
        function actionSelectors(action)
        {
            switch (action)
            {
                case "addCategory":
                    return ["[data-action='addExistingCategory']", "[data-action='addCategory']"];
                case "addCashCode":
                    return ["[data-action='addExistingCashCode']", "[data-action='addCashCode']", "[data-action='addExistingCode']"];
                case "addTotal":
                    // Prefer Add Existing Category if present, else fall back to New Total
                    return ["[data-action='addExistingCategory']", "[data-action='createTotal']"];
                case "expand":
                    return ["[data-action='expandSelected']"];
                case "collapse":
                    return ["[data-action='collapseSelected']"];
                default:
                    return ["[data-action='" + action + "']"];
            }
        }

        // Helper: set Enable/Disable label in a given scope (menu or pane)
        function setToggleEnabledLabel($scope, isEnabled)
        {
            try
            {
                var $btn = $scope.find("[data-action='toggleEnabled']");
                if ($btn.length)
                {
                    $btn.each(function ()
                    {
                        var $b = $(this);
                        // If the item contains an icon, preserve it and replace trailing text
                        var html = $b.html() || "";
                        if (/<i[^>]*>/i.test(html))
                        {
                            // Replace text after icon
                            $b.html(html.replace(/(<i[^>]*>.*?<\/i>\s*)[^<]*/i, "$1" + (isEnabled ? "Disable" : "Enable")));
                        }
                        else
                        {
                            $b.text(isEnabled ? "Disable" : "Enable");
                        }
                    });
                }
            }
            catch (_)
            {
            }
        }

        // Helper: ensure only one visible instance for a given action
        function ensureSingleVisible($scope, action)
        {
            try
            {
                var sels = actionSelectors(action).join(",");
                var $items = $scope.find(sels).filter(":visible");
                if ($items.length > 1)
                {
                    // Keep the first, hide the rest
                    $items.slice(1).hide();
                }
            }
            catch (_)
            {
            }
        }

        // Helper: reorder menu items into a specific order and show group dividers
        function reorderMenu($menu, order, groups)
        {
            try
            {
                var items = [];
                order.forEach(function (act)
                {
                    var $els = $menu.find(actionSelectors(act).join(",")).filter(":visible");
                    if ($els.length)
                    {
                        // Use the first instance for order; hide the rest duplicates
                        var $first = $els.first();
                        $els.slice(1).hide();
                        items.push($first);
                    }
                });

                if (items.length === 0)
                {
                    return;
                }

                // Append in order to re-sequence
                var $container = $menu;
                items.forEach(function ($el)
                {
                    $container.append($el);
                });

                // Handle dividers if present; otherwise harmless
                $menu.find(".dropdown-divider").hide();

                if (Array.isArray(groups) && groups.length > 0)
                {
                    // groups is an array of indices where a new group starts (except 0)
                    groups.forEach(function (startIdx)
                    {
                        if (startIdx > 0 && startIdx < items.length)
                        {
                            var $before = items[startIdx];
                            // Find an existing divider near this spot or create one
                            var $div = $menu.find(".dropdown-divider:hidden").first();
                            if ($div.length === 0)
                            {
                                $div = $("<div class='dropdown-divider'></div>");
                                $menu.append($div);
                            }
                            $div.insertBefore($before).show();
                        }
                    });
                }

                normalizeDividers($menu);
            }
            catch (_)
            {
            }
        }

        // Helper: reorder details pane buttons to match order
        function arrangeDetailsButtons($pane, order)
        {
            try
            {
                var $all = $pane.find("[data-action]");
                if ($all.length === 0)
                {
                    return;
                }

                // Try to use the parent of the first button as container
                var $container = $($all.get(0)).parent();
                if ($container.length === 0)
                {
                    return;
                }

                // Collect in order; prefer visible ones that exist
                var list = [];
                order.forEach(function (act)
                {
                    var $els = $pane.find(actionSelectors(act).join(",")).filter(":visible");
                    if ($els.length)
                    {
                        // Keep first instance; hide dupes
                        var $first = $els.first();
                        $els.slice(1).hide();
                        list.push($first);
                    }
                });

                if (list.length === 0)
                {
                    return;
                }

                list.forEach(function ($el)
                {
                    $container.append($el);
                });
            }
            catch (_)
            {
            }
        }

        // Mobile action bar
        function updateActionBar(node)
        {
            var bar = document.querySelector(actionBarSel);
            if (!bar) {return;}

            if (!isMobile())
            {
                bar.classList.remove("tc-visible");
                return;
            }

            var kinds = getNodeKinds(node);
            var data = kinds.data;
            var isSynthetic = kinds.isSynthetic;

            if (!node || isSynthetic)
            {
                bar.classList.remove("tc-visible");
                return;
            }

            // robust type context detection using parent key
            var parentKeyNow = (node && node.getParent ? (node.getParent()?.key || "") : "");
            var inTypeCtx = !!(data && (data.isTypeContext === true || data.syntheticKind === "type"))
                            || (typeof parentKeyNow === "string" && parentKeyNow.indexOf("type:") === 0);

            bar.querySelectorAll(".admin-only").forEach(function (el)
            {
                el.style.display = isAdmin ? "" : "none";
            });

            setBarButtonVisible("view", true);
            setBarButtonVisible("edit", isAdmin);
            // Hide move/delete in type subtree
            setBarButtonVisible("move", isAdmin && !inTypeCtx);
            setBarButtonVisible("delete", isAdmin && !inTypeCtx);

            // Remove maintenance buttons from mobile action bar
            setBarButtonVisible("makePrimary", false);
            setBarButtonVisible("setProfitRoot", false);
            setBarButtonVisible("setVatRoot", false);

            if (isAdmin && typeof data.isEnabled !== "undefined")
            {
                var toggleBtn = bar.querySelector("[data-action='toggleEnabled']");
                if (toggleBtn)
                {
                    toggleBtn.style.display = "";
                    var currentLabel = (data.isEnabled === 1) ? "Disable" : "Enable";
                    toggleBtn.innerHTML = "<i class='bi bi-power'></i> " + currentLabel;
                }
            }
            else
            {
                setBarButtonVisible("toggleEnabled", false);
            }

            bar.classList.add("tc-visible");

            function setBarButtonVisible(action, visible)
            {
                var btn = bar.querySelector("[data-action='" + action + "']");
                if (!btn) {return;}
                btn.style.display = visible ? "" : "none";
            }
        }

        function hideActionBar()
        {
            var bar = document.querySelector(actionBarSel);
            if (bar) {bar.classList.remove("tc-visible");}
        }

        function getMenuInstance()
        {
            // Prefer the top-level menu (not inside #detailsPane)
            var $all = $(menuSel);
            if ($all.length <= 1) { return $all.first(); }

            var $topLevel = $all.filter(function () { return $(this).closest("#detailsPane").length === 0; });
            var $chosen = $topLevel.length ? $topLevel.first() : $all.first();

            return $chosen;
        }

        function cacheMenuTemplates()
        {
            try
            {
                var $m = $(menuSel).first();
                if ($m.length === 0) { return; }

                $m.find(".dropdown-item[data-action]").each(function ()
                {
                    var $it = $(this);
                    var act = String($it.data("action") || "");
                    if (act && !menuTemplates.items[act])
                    {
                        menuTemplates.items[act] = $it.prop("outerHTML");
                    }
                });

                var $div = $m.find(".dropdown-divider").first();
                if ($div.length)
                {
                    menuTemplates.dividerHtml = $div.prop("outerHTML");
                }
            }
            catch (_)
            {
            }
        }

        function renderMenuFromTemplates($menu, actions, groupSplits)
        {
            try
            {
                function templateKeyForAction(act)
                {
                    try
                    {
                        var sels = actionSelectors(act);
                        if (Array.isArray(sels) && sels.length > 0)
                        {
                            var m = /\[data-action='([^']+)'\]/i.exec(sels[0]);
                            if (m && m[1])
                            {
                                return m[1];
                            }
                        }
                    }
                    catch (_)
                    {
                    }

                    // Fallback to input action if no alias mapping found
                    return act;
                }

                var parts = [];
                var splits = Array.isArray(groupSplits) ? groupSplits.slice(0) : [];
                splits.sort(function (a, b) { return a - b; });

                for (var i = 0; i < actions.length; i++)
                {
                    if (splits.length && i === splits[0])
                    {
                        parts.push(menuTemplates.dividerHtml);
                        splits.shift();
                    }

                    var act = actions[i];
                    var key = templateKeyForAction(act);
                    var tpl = menuTemplates.items[key];

                    if (tpl)
                    {
                        parts.push(tpl);
                    }
                }

                $menu.html(parts.join(""));

                // Normalize if last/first ended up a divider
                normalizeDividers($menu);
            }
            catch (_)
            {
            }
        }

        function showContextMenu(x, y, node)
        {
            if (!node)
            {
                return;
            }

            var $menu = getMenuInstance();

            // Cache pristine once so we can cache templates (done at startup); do not restore for shows
            if (menuOriginalHtml === null)
            {
                menuOriginalHtml = $menu.html();
            }

            // ---- Classification preamble ----
            var data = node.data || {};
            var parentKeyNow = "";
            try
            {
                var pTmp = node.getParent && node.getParent();
                parentKeyNow = (pTmp && pTmp.key) ? pTmp.key : "";
            }
            catch (_)
            {
                parentKeyNow = "";
            }

            var key = node.key;
            var isRootKey = (key === ROOT_KEY);
            var isDiscKey = (key === DISC_KEY);
            var isTypeSynthetic =
                (typeof key === "string" && key.indexOf("type:") === 0)
                || (data && (data.syntheticKind === "type" || data.syntheticKind === "typesRoot" || data.isTypeContext === true));

            var isTopAnchor = isRootKey || isDiscKey || isTypeSynthetic;

            // Defer only when we actually miss critical metadata for building a full menu.
            var needsCategoryType = !!(node.folder && typeof data.categoryType === "undefined");
            if (!isTopAnchor && needsCategoryType && node.lazy && !node.loaded)
            {
                if (node._ctxMenuLoading === true)
                {
                    return;
                }

                node._ctxMenuLoading = true;

                try
                {
                    $menu.hide();
                }
                catch (_)
                {
                }

                try
                {
                    var loadResult = node.load();

                    var resume = function ()
                    {
                        node._ctxMenuLoading = false;
                        setTimeout(function ()
                        {
                            showContextMenu(x, y, node);
                        }, 0);
                    };

                    if (loadResult && typeof loadResult.then === "function")
                    {
                        loadResult.then(resume).catch(function ()
                        {
                            node._ctxMenuLoading = false;
                        });
                        return;
                    }
                    if (loadResult && typeof loadResult.done === "function")
                    {
                        loadResult.done(resume).fail(function ()
                        {
                            node._ctxMenuLoading = false;
                        });
                        return;
                    }

                    resume();
                    return;
                }
                catch (_)
                {
                    node._ctxMenuLoading = false;
                }
            }

            // Heuristic for root-level totals (defensive)
            if (node.folder && typeof data.categoryType === "undefined" && parentKeyNow === ROOT_KEY)
            {
                data.categoryType = CATEGORYTYPE_CASHTOTAL;
                node.data.categoryType = CATEGORYTYPE_CASHTOTAL;
            }

            // Always start from a clean container to avoid stale menus
            $menu.empty();

            // Classify node
            var kinds = getNodeKinds(node);
            var isCode = kinds.isCode === true;
            var isCat = kinds.isCat === true;
            var catType = (typeof kinds.data.categoryType !== "undefined") ? Number(kinds.data.categoryType) : NaN;
            var isCatTotal = (catType === CATEGORYTYPE_CASHTOTAL);
            var isCatCashCode = (catType === 0);
            var inTypeCtx = (typeof parentKeyNow === "string" && parentKeyNow.indexOf("type:") === 0);

            var order = [];
            var groups = [];

            function pushIf(cond, act)
            {
                if (cond)
                {
                    order.push(act);
                }
            }

            // Top anchors
            if (isRootKey || isTypeSynthetic)
            {
                order = ["expandSelected", "collapseSelected"];
                renderMenuFromTemplates($menu, order, groups);
            }
            else if (key === DISC_KEY)
            {
                // Disconnected anchor menu
                pushIf(isAdmin, "createTotal");
                pushIf(isAdmin, "createCategory");
                order.push("expandSelected", "collapseSelected");
                groups = [2];
                renderMenuFromTemplates($menu, order, groups);
            }
            // Leaf: code
            else if (isCode)
            {
                order = ["view"];
                pushIf(isAdmin, "createCashCode");
                pushIf(isAdmin, "edit");
                pushIf(isAdmin, "delete");
                pushIf(isAdmin, "toggleEnabled");
                renderMenuFromTemplates($menu, order, groups);

                var $cc = $menu.find("[data-action='createCashCode']");
                if ($cc.length)
                {
                    $cc.text("New Cash Code like this…");
                }
                setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
            }
            // Category nodes
            else if (isCat)
            {
                if (inTypeCtx)
                {
                    order = ["view"];
                    pushIf(isAdmin, "moveUp");
                    pushIf(isAdmin, "moveDown");
                    pushIf(isAdmin, "toggleEnabled");
                    renderMenuFromTemplates($menu, order, groups);
                    setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                }
                else if (parentKeyNow === DISC_KEY)
                {
                    order = ["view"];
                    if (isCatTotal)
                    {
                        // EXACT requirement: Totals -> no Add Cash Code; show Add Total (existing) AND keep Add Category
                        pushIf(isAdmin, "createTotal");          // New Total
                        pushIf(isAdmin, "createCategory");       // New Category
                        pushIf(isAdmin, "addExistingCategory");  // Add Total (labelled below)
                        // NO cash-code actions here
                    }
                    else if (isCatCashCode)
                    {
                        // Cash Code Category -> allow code actions only
                        pushIf(isAdmin, "createCashCode");
                        pushIf(isAdmin, "addExistingCashCode");
                    }

                    pushIf(isAdmin, "edit");
                    pushIf(isAdmin, "delete");
                    pushIf(isAdmin, "toggleEnabled");
                    pushIf(isAdmin, "move");

                    groups = isCatCashCode ? [4, 7] : [3, 6];
                    renderMenuFromTemplates($menu, order, groups);
                    setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                }
                else if (parentKeyNow === ROOT_KEY)
                {
                    if (isCatTotal)
                    {
                        order = ["view"];
                        pushIf(isAdmin, "createTotal");          // New Total
                        pushIf(isAdmin, "createCategory");       // New Category
                        pushIf(isAdmin, "edit");
                        pushIf(isAdmin, "delete");
                        pushIf(isAdmin, "toggleEnabled");
                        pushIf(isAdmin, "addExistingCategory");  // Add Total (labelled below)
                        pushIf(isAdmin, "move");
                        pushIf(isAdmin, "moveUp");
                        pushIf(isAdmin, "moveDown");
                        order.push("expandSelected", "collapseSelected");
                        pushIf(isAdmin, "setProfitRoot");
                        pushIf(isAdmin, "setVatRoot");
                        groups = [3, 6, 9, 13];
                        renderMenuFromTemplates($menu, order, groups);
                        setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                    }
                    else if (isCatCashCode)
                    {
                        order = ["view"];
                        pushIf(isAdmin, "createCashCode");
                        pushIf(isAdmin, "edit");
                        pushIf(isAdmin, "delete");
                        pushIf(isAdmin, "toggleEnabled");
                        pushIf(isAdmin, "addExistingCashCode");
                        pushIf(isAdmin, "move");
                        pushIf(isAdmin, "moveUp");
                        pushIf(isAdmin, "moveDown");
                        order.push("expandSelected", "collapseSelected");
                        groups = [2, 5, 7];
                        renderMenuFromTemplates($menu, order, groups);
                        setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                    }
                }
                else
                {
                    if (isCatTotal)
                    {
                        order = ["view"];
                        pushIf(isAdmin, "createTotal");          // New Total
                        pushIf(isAdmin, "createCategory");       // New Category
                        pushIf(isAdmin, "edit");
                        pushIf(isAdmin, "delete");
                        pushIf(isAdmin, "toggleEnabled");
                        pushIf(isAdmin, "addExistingCategory");  // Add Total (labelled below)
                        pushIf(isAdmin, "move");
                        pushIf(isAdmin, "moveUp");
                        pushIf(isAdmin, "moveDown");
                        order.push("expandSelected", "collapseSelected");
                        groups = [3, 6, 9];
                        renderMenuFromTemplates($menu, order, groups);
                        setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                    }
                    else if (isCatCashCode)
                    {
                        order = ["view"];
                        pushIf(isAdmin, "createCashCode");
                        pushIf(isAdmin, "edit");
                        pushIf(isAdmin, "delete");
                        pushIf(isAdmin, "toggleEnabled");
                        pushIf(isAdmin, "addExistingCashCode");
                        pushIf(isAdmin, "move");
                        pushIf(isAdmin, "moveUp");
                        pushIf(isAdmin, "moveDown");
                        order.push("expandSelected", "collapseSelected");
                        groups = [2, 5, 7];
                        renderMenuFromTemplates($menu, order, groups);
                        setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                    }
                }
            }
            else
            {
                // Fallback synthetic
                renderMenuFromTemplates($menu, ["expandSelected", "collapseSelected"], []);
            }

            // Post-render label fixes for Total categories:
            // - Present "Add Existing Category" as "Add Total"
            if (isCat && isCatTotal)
            {
                var $addCat = $menu.find("[data-action='addExistingCategory']");
                if ($addCat.length)
                {
                    $addCat.each(function ()
                    {
                        var $it = $(this);
                        var html = $it.html() || "";
                        if (/<i[^>]*>/i.test(html))
                        {
                            $it.html(html.replace(/(<i[^>]*>.*?<\/i>\s*)[^<]*/i, "$1Add Category"));
                        }
                        else
                        {
                            $it.text("Add Category");
                        }
                    });
                }
            }

            // Hard-hide Set Profit/VAT Root unless the parent is exactly ROOT
            if (parentKeyNow !== ROOT_KEY)
            {
                $menu.find("[data-action='setProfitRoot'],[data-action='setVatRoot']").hide();
            }

            // Position & show
            if (isMobile())
            {
                $menu.addClass("mobile-sheet").css({ top: "", left: "" }).show();
            }
            else
            {
                $menu.removeClass("mobile-sheet").css({ top: y + "px", left: x + "px" }).show();
            }

            // Store node context (blank parentKey for synthetic anchors)
            $menu.data("nodeKey", key).data("parentKey", (kinds.isSynthetic ? "" : parentKeyNow));

            // Auto-hide on next document click
            setTimeout(function ()
            {
                $(document).one("click.treeCtx", function ()
                {
                    $menu.hide().data("nodeKey", null).data("parentKey", null);
                    if (isMobile() && node && !kinds.isSynthetic)
                    {
                        updateActionBar(node);
                    }
                    if (!isMobile())
                    {
                        resizeColumns();
                    }
                });
            }, 10);
        }

        function bindKeyboardHandlers()
        {
            $(treeSel).on("keydown", function (e)
            {
                var key = e.key || (function (kc)
                {
                    switch (kc)
                    {
                        case 37: return "ArrowLeft";
                        case 39: return "ArrowRight";
                        case 36: return "Home";
                        case 35: return "End";
                        case 38: return "ArrowUp";
                        case 40: return "ArrowDown";
                        default: return "";
                    }
                })(e.which);

                // Non-shift navigation: Left/Right/Home/End
                if (!e.shiftKey)
                {
                    var treeNav = getTree();
                    var cur = treeNav && treeNav.getActiveNode ? treeNav.getActiveNode() : null;
                    if (!cur) { return; }

                    // Left: collapse, or go to parent if already collapsed
                    if (key === "ArrowLeft")
                    {
                        if (cur.folder && cur.expanded)
                        {
                            cur.setExpanded(false);
                            persistExpanded(cur, false);
                        }
                        else
                        {
                            var par = cur.getParent && cur.getParent();
                            if (par && par.key)
                            {
                                par.setActive(true);
                                persistActiveKey(par);
                            }
                        }
                        e.preventDefault();
                        e.stopPropagation();
                        return;
                    }

                    // Right: expand
                    if (key === "ArrowRight")
                    {
                        if (cur.folder && !cur.expanded)
                        {
                            cur.setExpanded(true);
                            persistExpanded(cur, true);
                        }
                        e.preventDefault();
                        e.stopPropagation();
                        return;
                    }

                    // Home: first sibling
                    if (key === "Home")
                    {
                        var first = cur.getParent && cur.getParent() && cur.getParent().getFirstChild && cur.getParent().getFirstChild();
                        if (first) { first.setActive(true); persistActiveKey(first); }
                        e.preventDefault();
                        e.stopPropagation();
                        return;
                    }

                    // End: last sibling
                    if (key === "End")
                    {
                        var last = cur.getParent && cur.getParent() && cur.getParent().getLastChild && cur.getParent().getLastChild();
                        if (last) { last.setActive(true); persistActiveKey(last); }
                        e.preventDefault();
                        e.stopPropagation();
                        return;
                    }

                    // Other non-shift keys: do not interfere
                    return;
                }

                // Shift+ArrowUp / Shift+ArrowDown => reorder before/after sibling
                var isUp = (key === "ArrowUp");
                var isDown = (key === "ArrowDown");
                if (!isUp && !isDown)
                {
                    return;
                }

                if (isMobile()) { return; }
                if (!isAdmin) { return; }

                var tree = getTree();
                if (!tree) { return; }

                var node = tree.getActiveNode && tree.getActiveNode();
                if (!node) { return; }

                // Categories only
                var kinds = getNodeKinds(node);
                if (!kinds.isCat) { return; }

                var parent = node.getParent ? node.getParent() : null;
                if (!parent) { return; }

                // Find anchor sibling (skip non-folders except under Disconnected)
                function findAnchor(n, direction)
                {
                    var p = n.getParent ? n.getParent() : null;
                    var parentKey = p ? (p.key || "") : "";
                    var cur = (direction === "up") ? n.getPrevSibling() : n.getNextSibling();

                    if (parentKey !== DISC_KEY)
                    {
                        while (cur && !cur.folder)
                        {
                            cur = (direction === "up") ? cur.getPrevSibling() : cur.getNextSibling();
                        }
                    }
                    return cur || null;
                }

                // Identify Cash Type container parents (synthetic/type)
                function isTypeContainer(p)
                {
                    if (!p) { return false; }
                    var d = p.data || {};
                    var k = p.key || "";
                    return (d.nodeType === "synthetic" && (d.syntheticKind === "type" || d.isTypeContext === true))
                           || (typeof k === "string" && k.indexOf("type:") === 0);
                }

                var anchor = findAnchor(node, isUp ? "up" : "down");
                if (!anchor)
                {
                    // No sibling in that direction
                    e.preventDefault();
                    e.stopPropagation();
                    return;
                }

                var mode = isUp ? "before" : "after";

                // Cash Type view => ReorderType
                if (isTypeContainer(parent))
                {
                    postJsonGlobal("ReorderType", { key: node.key, anchorKey: anchor.key, mode: mode })
                    .done(function (res)
                    {
                        if (res && res.success)
                        {
                            try
                            {
                                node.moveTo(anchor, mode);
                            }
                            catch (ex)
                            {
                            }

                            reloadIfExpandedNode(parent);

                            node.setActive(true);
                            persistActiveKey(node);
                            announce("Moved " + (node.title || node.key) + " " + (mode === "before" ? "before " : "after ") + (anchor.title || anchor.key));
                            notify("Order updated", "success");

                            if (!isMobile())
                            {
                                loadDetails(node);
                                resizeColumns();
                            }
                        }
                        else
                        {
                            alert((res && res.message) || "Reorder failed");
                        }
                    }).fail(function (xhr)
                    {
                        alert("Server error (" + xhr.status + ")");
                    });
                }
                else
                {
                    // Totals/Disconnected => ReorderSiblings
                    postJsonGlobal("ReorderSiblings", { parentKey: parent.key || "", key: node.key, anchorKey: anchor.key, mode: mode })
                    .done(function (res)
                    {
                        if (res && res.success)
                        {
                            try
                            {
                                node.moveTo(anchor, mode);
                            }
                            catch (ex)
                            {
                            }

                            reloadIfExpandedNode(parent);

                            node.setActive(true);
                            persistActiveKey(node);
                            announce("Moved " + (node.title || node.key) + " " + (mode === "before" ? "before " : "after ") + (anchor.title || anchor.key));
                            notify("Order updated", "success");

                            refreshTopAnchors();

                            if (!isMobile())
                            {
                                loadDetails(node);
                                resizeColumns();
                            }
                        }
                        else
                        {
                            alert((res && res.message) || "Reorder failed");
                        }
                    }).fail(function (xhr)
                    {
                        alert("Server error (" + xhr.status + ")");
                    });
                }

                // Consume the key
                e.preventDefault();
                e.stopPropagation();
            });
        }

        function bindContextMenuHandlers()
        {
            var $menu = $(menuSel);

            $menu.off("click.categoryActions").on("click.categoryActions", "[data-action]", function ()
            {
                var action = $(this).data("action");
                var key = $menu.data("nodeKey");
                var parentKey = $menu.data("parentKey");

                var tree = getTree();
                var node = key ? tree.getNodeByKey(key) : null;

                var token = antiXsrf();
                $menu.hide();

                function alertFail(xhr)
                {
                    alert("Server error (" + xhr.status + ")");
                }

                function refreshNode(nodeKeyToRefresh)
                {
                    if (!nodeKeyToRefresh) { return; }
                    var n = tree.getNodeByKey(nodeKeyToRefresh);
                    if (!n) { return; }
                    if (n.expanded)
                    {
                        reloadIfExpandedNode(n);
                    }
                    else
                    {
                        var ex = n.setExpanded(true);
                        if (ex && typeof ex.done === "function")
                        {
                            ex.done(function () { reloadIfExpandedNode(n); });
                        }
                        else
                        {
                            reloadIfExpandedNode(n);
                        }
                    }
                }

                function isDiscCategoryNode(n)
                {
                    if (!n || !n.folder) { return false; }
                    var p = n.getParent && n.getParent();
                    return !!(p && p.key === DISC_KEY);
                }

                switch (action)
                {
                    case "expandSelected":
                    {
                        if (!node || !node.folder)
                        {
                            alert("Select a folder");
                            break;
                        }
                        expandSubtree(node).then(function () { resizeColumns(); }).catch(function () { resizeColumns(); });
                        break;
                    }

                    case "collapseSelected":
                    {
                        if (!node || !node.folder)
                        {
                            alert("Select a folder");
                            break;
                        }
                        collapseSubtree(node);
                        resizeColumns();
                        break;
                    }

                    case "view":
                    {
                        if (!node) { alert("Select a node first"); break; }
                        if (isMobile())
                        {
                            var url = detailsUrl + "?key=" + encodeURIComponent(key);
                            if (parentKey) { url += "&parentKey=" + encodeURIComponent(parentKey); }
                            window.location.href = url;
                        }
                        else
                        {
                            // Desktop RHS
                            loadDetails(node);
                        }
                        break;
                    }

                    case "edit":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var kinds = getNodeKinds(node);

                        // Cash Code
                        if (kinds.isCode)
                        {
                            var raw = (typeof key === "string" && key.indexOf("code:") === 0) ? key.substring(5) : key;
                            openAction("EditCashCode", raw);
                            break;
                        }

                        // Total category
                        if (kinds.data && kinds.data.categoryType === CATEGORYTYPE_CASHTOTAL)
                        {
                            openAction("EditTotal", key);
                            break;
                        }

                        // Cash Code category (categoryType == 0)
                        if (kinds.data && kinds.data.categoryType === 0)
                        {
                            openAction("EditCategory", key);
                            break;
                        }

                        // Fallback
                        openAction("EditCategory", key);
                        break;
                    }

                    case "addExistingCategory":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        var targetParent = (node && node.folder) ? key : parentKey;
                        if (!targetParent)
                        {
                            alert("Select a parent category");
                            break;
                        }

                        // Open embedded AddCategory page (select from dropdown)
                        openAction("AddCategory", "", targetParent);
                        break;
                    }

                    case "addExistingCashCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        // Use the node itself if it is a folder; otherwise its parent (mirrors addCategory)
                        var targetCategory = (node && node.folder) ? key : parentKey;
                        if (!targetCategory)
                        {
                            alert("Select a category to add an existing code under");
                            break;
                        }

                        // Open embedded AddCashCode page (idempotent attach/move)
                        openAction("AddCashCode", "", targetCategory);
                        break;
                    }

                    case "move":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node || !node.folder) { alert("Select a category"); break; }
                        openAction("Move", key, parentKey);
                        break;
                    }

                    case "createTotal":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        // Use the node itself if it is a folder; otherwise fallback to its parentKey.
                        var targetParent = (node && node.folder) ? key : parentKey;
                        if (!targetParent)
                        {
                            alert("Select a parent category");
                            break;
                        }

                        if (targetParent === DISC_KEY)
                        {
                            // Disconnected create: pass Disconnected context. Submit logic will blank ParentKey.
                            openAction("CreateTotal", "", DISC_KEY);
                            break;
                        }

                        // Normal path: create a new Total as a child of the selected (possibly disconnected) category.
                        // Server will create a mapping parentKey -> new Total, making the parent a root-level total.
                        openAction("CreateTotal", "", targetParent);
                        break;
                    }

                    case "createCashCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var isCodeNode = !!(node.data && node.data.nodeType === "code") || (key && typeof key === "string" && key.indexOf("code:") === 0);

                        var targetCategory = (node && node.folder) ? key : parentKey;
                        if (!targetCategory)
                        {
                            alert("Select a category to add a code under");
                            break;
                        }

                        if (isCodeNode)
                        {
                            var siblingCash = (key && key.indexOf("code:") === 0) ? key.substring(5) : (node && node.data && node.data.cashCode) || "";
                            openAction("CreateCashCode", targetCategory, null, { siblingCashCode: siblingCash });
                            break;
                        }
                        else
                        {
                            openAction("CreateCashCode", targetCategory);
                            break;
                        }
                    }

                    case "moveUp":
                    case "moveDown":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }
                        if (key && key.indexOf("code:") === 0)
                        {
                            alert("Cannot reorder cash code nodes");
                            break;
                        }

                        var handler = action === "moveUp" ? "MoveUp" : "MoveDown";
                        postJsonGlobal(handler, { key: key, parentKey: parentKey })
                            .done(function (res)
                            {
                                if (res && res.success)
                                {
                                    moveNodeInUi(node, action === "moveUp" ? "up" : "down");
                                    resizeColumns();
                                }
                                else
                                {
                                    alert((res && res.message) || (handler + " failed"));
                                }
                            })
                            .fail(alertFail);
                        break;
                    }

                    case "toggleEnabled":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var makeEnabled = (node.data && node.data.isEnabled === 1) ? 0 : 1;
                        postJsonGlobal("SetEnabled", { key: key, enabled: makeEnabled })
                            .done(function (res)
                            {
                                if (res && res.success)
                                {
                                    var isCodeNode2 = (node.data && node.data.nodeType === "code") || (key && key.indexOf("code:") === 0);
                                    setNodeEnabledInUi(node, !!makeEnabled, !isCodeNode2);
                                    if (!isMobile()) { loadDetails(node); }
                                    refreshTopAnchors();
                                }
                                else
                                {
                                    alert((res && res.message) || "Update failed");
                                }
                            })
                            .fail(alertFail);
                        break;
                    }

                    case "delete":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        openDeleteFor(node, $menu.data("parentKey") || "");
                        break;
                    }

                    case "makePrimary":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!parentKey) { alert("Open from a parent context to make primary."); break; }

                        postJsonGlobal("MakePrimary", { key: key, parentKey: parentKey })
                            .done(function (res)
                            {
                                alert((res && res.message) || "Not Yet Implemented");
                                if (res && res.success)
                                {
                                    var p = tree.getNodeByKey(parentKey);
                                    reloadIfExpandedNode(p);
                                    refreshTopAnchors();
                                    loadDetails(tree.getActiveNode());
                                }
                            })
                            .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "setProfitRoot":
                    case "setVatRoot":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node.folder) { alert("Select a category"); break; }

                        var kind = (action === "setProfitRoot") ? "Profit" : "VAT";
                        if (!confirm("Set " + key + " as the " + kind + " primary root?")) { break; }

                        postJsonGlobal("SetPrimaryRoot", { kind: kind, categoryCode: key })
                            .done(function (res)
                            {
                                alert((res && res.message) || "Not Yet Implemented");
                                if (res && res.success)
                                {
                                    refreshTopAnchors();
                                    loadDetails(tree.getActiveNode());
                                }
                            })
                            .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "createCategory":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        var targetParent = (node && node.folder) ? key : parentKey;
                        if (!targetParent)
                        {
                            alert("Select a parent category");
                            break;
                        }

                        // Always open CreateCategory (user intent is explicit via menu label)
                        openAction("CreateCategory", "", targetParent);
                        break;
                    }
                }
            });
        }

        function bindActionBarHandlers()
        {
            var bar = document.querySelector(actionBarSel);
            if (!bar) {return;}

            bar.addEventListener("click", function (e)
            {
                var target = e.target.closest("[data-action]");
                if (!target) {return;}

                var action = target.getAttribute("data-action");
                var tree = getTree();
                var node = tree && tree.getActiveNode ? tree.getActiveNode() : null;

                if (!node)
                {
                    alert("Select a node first");
                    return;
                }

                var kinds = getNodeKinds(node);
                var key = kinds.key;
                var parentKey = "";
                if (node.getParent)
                {
                    var p = node.getParent();
                    parentKey = (p && p.key) ? p.key : "";
                }

                function refreshAnchors()
                {
                    var t = getTree();
                    if (!t) { return; }
                    var top = t.getRootNode();
                    if (!top || !top.children) { return; }
                    for (var i = 0; i < top.children.length; i++)
                    {
                        var n = top.children[i];
                        if (n && n.expanded)
                        {
                            reloadIfExpandedNode(n, { url: nocache(appendQuery(nodesUrl, 'id', n.key)) });
                        }
                    }
                }

                function callStub(handler, extra)
                {
                    return postJsonGlobal(handler, Object.assign({ key: key, parentKey: parentKey }, extra || {}));
                }

                switch (action)
                {
                    case "view":
                    {
                        if (!node) { alert("Select a node first"); break; }
                        if (isMobile())
                        {
                            var url = detailsUrl + "?key=" + encodeURIComponent(key);
                            if (parentKey) { url += "&parentKey=" + encodeURIComponent(parentKey); }
                            window.location.href = url;
                        }
                        else
                        {
                            loadDetails(node);
                        }
                        break;
                    }

                    case "edit":
                    {
                        if (!isAdmin)
                        {
                            alert("Insufficient privileges");
                            break;
                        }

                        if (!node)
                        {
                            alert("Select a node first");
                            break;
                        }

                        var kinds2 = getNodeKinds(node);

                        if (kinds2.isCode)
                        {
                            var raw = (typeof key === "string" && key.indexOf("code:") === 0) ? key.substring(5) : key;
                            openAction("EditCashCode", raw);
                            break;
                        }
                        if (kinds2.data && kinds2.data.categoryType === CATEGORYTYPE_CASHTOTAL)
                        {
                            openAction("EditTotal", key);
                            break;
                        }
                        if (kinds2.data && kinds2.data.categoryType === 0)
                        {
                            openAction("EditCategory", key);
                            break;
                        }

                        openAction("EditCategory", key);
                        break;
                    }

                    case "move":
                    {
                        if (!isAdmin) 
                        {
                            alert("Insufficient privileges");
                            break;
                        }
                        var inTypeCtx = !!(node.data && (node.data.isTypeContext === true || node.data.syntheticKind === "type"));
                        if (inTypeCtx)
                        {
                            alert("Action not available in this view.");
                            break;
                        }
                        if (!node.folder) 
                        {
                            alert("Select a category");
                            break;
                        }

                        openAction("Move", key, parentKey);
                        break;
                    }

                    case "delete":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        openDeleteFor(node, parentKey || "");
                        break;
                    }

                    case "toggleEnabled":
                    {
                        if (!isAdmin)
                        {
                            alert("Insufficient privileges");
                            break;
                        }
                        var makeEnabled = (node.data && node.data.isEnabled === 1) ? 0 : 1;

                        postJsonGlobal("SetEnabled", { key: key, enabled: makeEnabled })
                            .done(function (res)
                            {
                                if (res && res.success)
                                {
                                    setNodeEnabledInUi(node, !!makeEnabled, !kinds.isCode);
                                    updateActionBar(node);
                                    if (!isMobile())
                                    {
                                        loadDetails(node);
                                    }
                                    refreshTopAnchors();
                                }
                                else
                                {
                                    alert((res && res.message) || "Update failed");
                                }
                            }).fail(function (xhr)
                            {
                                alert("Server error (" + xhr.status + ")");
                            });
                        break;
                    }
                }
            });
        }

        function bindCancelInPane()
        {
            var $pane = $("#detailsPane");
            if ($pane.length === 0)
            {
                return;
            }

            $pane.off("click.cancelEmbedded").on("click.cancelEmbedded", "[data-embedded-cancel]", function (e)
            {
                e.preventDefault();
                try
                {
                    if (typeof window.tcCancel === "function")
                    {
                        window.tcCancel();
                    }
                }
                catch (_)
                {
                    // swallow
                }
            });
        }

        function bindDetailsPaneHandlers()
        {
            var $pane = $("#detailsPane");
            if ($pane.length === 0) { return; }

            $pane.off("click.detailsActions").on("click.detailsActions", "[data-action]", function ()
            {
                var action = $(this).data("action");
                var tree = getTree();
                var node = tree && tree.getActiveNode ? tree.getActiveNode() : null;

                if (!node)
                {
                    alert("Select a node first");
                    return;
                }

                var kinds = getNodeKinds(node);
                var key = kinds.key;
                var parentKey = "";
                if (node.getParent)
                {
                    var p = node.getParent();
                    parentKey = (p && p.key) ? p.key : "";
                }

                function refreshAnchors()
                {
                    var t = getTree();
                    if (!t) { return; }
                    var top = t.getRootNode();
                    if (!top || !top.children) { return; }
                    for (var i = 0; i < top.children.length; i++)
                    {
                        var n = top.children[i];
                        if (n && n.expanded)
                        {
                            reloadIfExpandedNode(n, { url: nocache(appendQuery(nodesUrl, 'id', n.key)) });
                        }
                    }
                }

                function callStub(handler, extra)
                {
                    return postJsonGlobal(handler, Object.assign({ key: key, parentKey: parentKey }, extra || {}));
                }

                switch (action)
                {
                    case "view":
                    {
                        if (!node) { alert("Select a node first"); break; }
                        if (isMobile())
                        {
                            var url = detailsUrl + "?key=" + encodeURIComponent(key);
                            if (parentKey) { url += "&parentKey=" + encodeURIComponent(parentKey); }
                            window.location.href = url;
                        }
                        else
                        {
                            loadDetails(node);
                        }
                        break;
                    }
                    case "edit":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var kinds3 = getNodeKinds(node);

                        if (kinds3.isCode)
                        {
                            var raw = (typeof key === "string" && key.indexOf("code:") === 0) ? key.substring(5) : key;
                            openAction("EditCashCode", raw);
                            break;
                        }
                        if (kinds3.data && kinds3.data.categoryType === CATEGORYTYPE_CASHTOTAL)
                        {
                            openAction("EditTotal", key);
                            break;
                        }
                        if (kinds3.data && kinds3.data.categoryType === 0)
                        {
                            openAction("EditCategory", key);
                            break;
                        }

                        openAction("EditCategory", key);
                        break;
                    }

                    case "addExistingCategory":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node.folder) { alert("Select a category"); break; }

                        openAction("AddCategory", "", key);
                        break;
                    }

                    case "addExistingCashCode":
                    {
                    if (!isAdmin) { alert("Insufficient privileges"); break; }
                    if (!node.folder) { alert("Select a category"); break; }

                    // Open embedded AddCashCode with this category as parent
                    openAction("AddCashCode", "", key);
                    break;
                    }

                    case "move":
                    {
                        if (!isAdmin)
                        {
                            alert("Insufficient privileges");
                            break;
                        }
                        if (!node.folder)
                        {
                            alert("Select a category");
                            break;
                        }

                        openAction("Move", key, parentKey);
                        break;
                    }
                    case "toggleEnabled":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        var makeEnabled = (node.data && node.data.isEnabled === 1) ? 0 : 1;

                        postJsonGlobal("SetEnabled", { key: key, enabled: makeEnabled })
                            .done(function (res)
                            {
                                if (res && res.success)
                                {
                                    setNodeEnabledInUi(node, !!makeEnabled, !kinds.isCode);
                                    loadDetails(node);
                                    refreshTopAnchors();
                                }
                                else
                                {
                                    alert((res && res.message) || "Update failed");
                                }
                            })
                            .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "delete":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        openDeleteFor(node, parentKey || "");
                        break;
                    }

                    case "makePrimary":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!parentKey) { alert("Open from a parent context to make primary."); break; }

                        postJsonGlobal("MakePrimary", { key: key, parentKey: parentKey })
                        .done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                var p = tree.getNodeByKey(parentKey);
                                reloadIfExpandedNode(p);
                                refreshTopAnchors();
                                loadDetails(tree.getActiveNode());
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "setProfitRoot":
                    case "setVatRoot":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node.folder) { alert("Select a category"); break; }

                        var kind = (action === "setProfitRoot") ? "Profit" : "VAT";
                        if (!confirm("Set " + key + " as the " + kind + " primary root?")) { break; }

                        postJsonGlobal("SetPrimaryRoot", { kind: kind, categoryCode: key })
                        .done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                refreshTopAnchors();
                                loadDetails(tree.getActiveNode());
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "createCategory":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        // Determine parent target (use current node if folder, otherwise parentKey)
                        var targetParent = (node && node.folder) ? key : parentKey;
                        if (!targetParent)
                        {
                            alert("Select a parent category");
                            break;
                        }
                        openAction("CreateCategory", "", targetParent);
                        break;
                    }
                                    }
            });
        }

        function initTree()
        {
            if (typeof $.ui === "undefined" || typeof $.fn.fancytree !== "function") {return;}

            $.ajaxSetup({ cache: false });

            var exts = [];
            if ($.ui && $.ui.fancytree && $.ui.fancytree._extensions && $.ui.fancytree._extensions.dnd5)
            {
                exts.push("dnd5");
            }

            $(treeSel).fancytree({
                extensions: exts,
                source: { url: nocache(nodesUrl) },
                escapeTitles: false,
                minExpandLevel: 1,
                clickFolderMode: 3,
                lazyLoad: function (event, data)
                {
                    var node = data.node;
                    data.result = { url: nocache(appendQuery(nodesUrl, 'id', node.key)) };
                },
                init: function (event, data)
                {
                    var root = data.tree.getNodeByKey(ROOT_KEY);
                    if (root && !root.expanded) {root.setExpanded(true);}
                    ensureTreeContainerSizing();
                    resizeColumns();
                },
                expand: function (event, data)
                {
                    persistExpanded(data.node, true);
                },
                collapse: function (event, data)
                {
                    persistExpanded(data.node, false);
                },
                load: function (event, data)
                {
                    if (!data.node)
                    {
                        var root = data.tree.getNodeByKey(ROOT_KEY);
                        if (root && !root.expanded)
                        {
                            root.setExpanded(true);
                        }
                    }

                    var set = loadExpandedSet();
                    var scope = data.node ? data.node : data.tree.getRootNode();
                    restoreExpandedForNode(scope, set);

                    ensureTreeContainerSizing();
                    resizeColumns();

                    // Apply query selection (event-driven short delay to avoid race with lazy-load)
                    try
                    {
                        var _qs = new URLSearchParams(window.location.search || "");
                        if (_qs.has("select") || _qs.has("returnKey") || _qs.has("key"))
                        {
                            setTimeout(function ()
                            {
                                try
                                {
                                    applyQuerySelection();
                                }
                                catch (_) { }
                            }, 50);
                        }
                        else
                        {
                            applyQuerySelection();
                        }
                    }
                    catch (_)
                    {
                        applyQuerySelection();
                    }
                },
                activate: function (event, data)
                {
                    persistActiveKey(data.node);

                    if (isMobile())
                    {
                        updateActionBar(data.node);
                    }
                    else
                    {
                        loadDetails(data.node);
                    }
                },
                dnd5: {
                    autoExpandMS: 300,
                    multiSource: false,

                    dragStart: function (node, data)
                    {
                        if (isMobile()) { return false; }
                        if (!isAdmin) { return false; }

                        var kinds = getNodeKinds(node);
                        if (!kinds.isCat) { return false; } // categories only
                        return true;
                    },

                    dragEnter: function (node, data)
                    {
                        if (isMobile()) { return false; }
                        if (!isAdmin) { return false; }

                        var src = data.otherNode;
                        if (!src) { return false; }
                        if (node === src || node.isDescendantOf(src)) { return false; }

                        var srcKinds = getNodeKinds(src);
                        var tgtKinds = getNodeKinds(node);
                        if (!srcKinds.isCat || !tgtKinds.isCat) { return false; }

                        // Same parent in Type or non-Type context => allow sibling reordering
                        var srcParent = src.getParent ? src.getParent() : null;
                        var tgtParent = node.getParent ? node.getParent() : null;

                        function isTypeContainer(p)
                        {
                            if (!p) { return false; }
                            var d = p.data || {};
                            var k = p.key || "";
                            return (d.nodeType === "synthetic" && (d.syntheticKind === "type" || d.isTypeContext === true))
                                    || (typeof k === "string" && k.indexOf("type:") === 0);
                        }

                        // Cash Type siblings
                        if (srcParent && tgtParent && srcParent === tgtParent && isTypeContainer(tgtParent))
                        {
                            return ["before", "after"];
                        }

                        // Totals/Disconnected siblings
                        if (srcParent && tgtParent && srcParent === tgtParent && !isTypeContainer(tgtParent))
                        {
                            return ["before", "after"];
                        }

                        // Otherwise, consider child drop (over). Only allow "over" when target is a Total category.
                        try
                        {
                            var tgtData = node.data || {};
                            if (typeof tgtData.categoryType !== "undefined")
                            {
                                if (Number(tgtData.categoryType) === CATEGORYTYPE_CASHTOTAL)
                                {
                                    return ["over"];
                                }
                                // target is not a Total -> do not allow child drops
                                return false;
                            }

                            // No explicit categoryType (synthetic/roots) — disallow child drops to be safe
                            return false;
                        }
                        catch (ex)
                        {
                            // conservative fallback: disallow child drops
                            return false;
                        }
                    },

                    dragDrop: function (node, data)
                    {
                        if (isMobile()) { return false; }
                        if (!isAdmin) { return false; }

                        var src = data.otherNode;
                        if (!src)
                        {
                            return false;
                        }

                        if (node === src || node.isDescendantOf(src))
                        {
                            alert("Invalid move: cannot move a category under itself or its descendant.");
                            return false;
                        }

                        // Prevent dropping a CashCode-category under a non-Total category (extra safety server-side)
                        if (data.hitMode === "over")
                        {
                            try
                            {
                                var tgtData = node.data || {};
                                if (typeof tgtData.categoryType !== "undefined" && Number(tgtData.categoryType) !== CATEGORYTYPE_CASHTOTAL)
                                {
                                    alert("Invalid move: only Total-type categories may have child categories.");
                                    return false;
                                }
                            }
                            catch (ex)
                            {
                                alert("Invalid move: cannot determine target category type.");
                                return false;
                            }
                        }

                        // Ensure we have tree reference for reloads
                        var t = getTree();

                        // Helper: detect synthetic/type container parents
                        function isTypeContainer(p)
                        {
                            if (!p) { return false; }
                            var d = p.data || {};
                            var k = p.key || "";
                            return (d.nodeType === "synthetic" && (d.syntheticKind === "type" || d.isTypeContext === true))
                                    || (typeof k === "string" && k.indexOf("type:") === 0);
                        }

                        // Sibling reordering (before/after)
                        if (data.hitMode === "before" || data.hitMode === "after")
                        {
                            var parent = node.getParent ? node.getParent() : null;

                            // Cash Type sibling reorder -> ReorderType
                            if (parent && isTypeContainer(parent))
                            {
                                postJsonGlobal("ReorderType", { key: src.key, anchorKey: node.key, mode: data.hitMode })
                                .done(function (res)
                                {
                                    if (res && res.success)
                                    {
                                        try
                                        {
                                            src.moveTo(node, data.hitMode);
                                        }
                                        catch (ex)
                                        {
                                            // swallow
                                        }

                                        reloadIfExpandedNode(parent);

                                        src.setActive(true);
                                        persistActiveKey(src);
                                        announce("Moved " + (src.title || src.key) + (data.hitMode === "before" ? " before " : " after ") + (node.title || node.key));
                                        notify("Order updated", "success");

                                        if (!isMobile())
                                        {
                                            loadDetails(src);
                                            resizeColumns();
                                        }
                                    }
                                    else
                                    {
                                        alert((res && res.message) || "Reorder failed");
                                    }
                                }).fail(function (xhr)
                                {
                                    alert("Server error (" + xhr.status + ")");
                                });

                                return;
                            }

                            // Totals/Disconnected sibling reorder -> ReorderSiblings
                            if (parent && !isTypeContainer(parent))
                            {
                                postJsonGlobal("ReorderSiblings", { parentKey: parent.key || "", key: src.key, anchorKey: node.key, mode: data.hitMode })
                                .done(function (res)
                                {
                                    if (res && res.success)
                                    {
                                        try
                                        {
                                            src.moveTo(node, data.hitMode);
                                        }
                                        catch (ex)
                                        {
                                        }

                                        reloadIfExpandedNode(parent);

                                        src.setActive(true);
                                        persistActiveKey(src);
                                        announce("Moved " + (src.title || src.key) + (data.hitMode === "before" ? " before " : " after ") + (node.title || node.key));
                                        notify("Order updated", "success");

                                        refreshTopAnchors();

                                        if (!isMobile())
                                        {
                                            loadDetails(src);
                                            resizeColumns();
                                        }
                                    }
                                    else
                                    {
                                        alert((res && res.message) || "Reorder failed");
                                    }
                                }).fail(function (xhr)
                                {
                                    alert("Server error (" + xhr.status + ")");
                                });

                                return;
                            }
                        }

                        // Fallback: child drop (move under parent) - call Move handler when hitMode === "over"
                        if (data.hitMode !== "over") { return false; }

                        postJsonGlobal("Move", { key: src.key, targetParentKey: node.key })
                        .done(function (res)
                        {
                            if (res && res.success)
                            {
                                try
                                {
                                    src.moveTo(node, "child");
                                }
                                catch (ex)
                                {
                                }

                                // reload relevant parents
                                reloadIfExpandedNode(src.getParent ? (src.getParent() && src.getParent().key) || "" : "");
                                reloadIfExpandedNode(node.key);

                                refreshTopAnchors();

                                src.setActive(true);
                                persistActiveKey(src);

                                if (!isMobile()) { loadDetails(src); resizeColumns(); }
                                notify("Moved", "success");
                                announce("Moved " + (src.title || src.key) + " under " + (node.title || node.key));
                            }
                            else
                            {
                                alert((res && res.message) || "Move failed");
                            }
                        }).fail(function (xhr)
                        {
                            alert("Server error (" + xhr.status + ")");
                        });

                        return;
                    }
                }

            });

            // Desktop: right click
            $(treeSel).on("contextmenu", ".fancytree-node", function (e)
            {
                if (isMobile()) {return;}
                e.preventDefault();
                var node = $.ui.fancytree.getNode(this);
                if (!node) {return;}
                node.setActive(true);
                showContextMenu(e.pageX, e.pageY, node);
            });

            // Mobile: long-press (500ms) to open menu
            (function bindLongPress()
            {
                var pressTimer = null;
                var startX = 0, startY = 0;

                $(treeSel).on("touchstart", ".fancytree-node", function (e)
                {
                    if (!isMobile()) {return;}
                    var node = $.ui.fancytree.getNode(this);
                    if (!node) {return;}
                    var touch = e.originalEvent.touches && e.originalEvent.touches[0];
                    if (!touch) {return;}

                    startX = touch.clientX + window.scrollX;
                    startY = touch.clientY + window.scrollY;

                    pressTimer = setTimeout(function ()
                    {
                        node.setActive(true);
                        showContextMenu(startX, startY, node);
                    }, 500);
                });

                function cancelPress()
                {
                    if (pressTimer)
                    {
                        clearTimeout(pressTimer);
                        pressTimer = null;
                    }
                }

                $(treeSel).on("touchend touchcancel touchmove", ".fancytree-node", function ()
                {
                    cancelPress();
                });
                $(window).on("scroll", cancelPress);
            })();

            bindContextMenuHandlers();
            bindActionBarHandlers();
            bindDetailsPaneHandlers();
            bindCancelInPane();
            bindKeyboardHandlers();
            bindAutoscrollHandlers();            
            bindEmbedMarkerObserver();  // Observe RHS for create-result markers so we can refresh and select without a full reload

            if (!isMobile())
            {
                var tree = getTree();
                if (tree) { loadDetails(tree.getActiveNode()); }
            }

            $(window).on("resize orientationchange", function ()
            {
                resizeColumns();
            });
            resizeColumns();
        }

        // Public helper: select a new code under a parent immediately after creation (embed flow)
        window.CategoryTree.selectNewCode = function (codeKey, parentKey)
        {
            try
            {
                if (!codeKey || !parentKey)
                {
                    return;
                }

                // Normalize key to code: prefix
                if (codeKey.indexOf("code:") !== 0)
                {
                    codeKey = "code:" + codeKey;
                }

                // Update URL (no navigation) so refresh/bookmark preserves selection
                try
                {
                    var u = new URL(window.location.href);
                    u.searchParams.set("select", codeKey);
                    u.searchParams.set("key", codeKey);
                    u.searchParams.set("expand", parentKey);
                    history.replaceState({}, "", u.toString());
                }
                catch (_)
                {
                }

                // Reset internal guards and re-run selection pipeline with current params
                try
                {
                    querySelectionApplied = false;
                    selectionRetryCount = 0;
                    applyQuerySelection();
                }
                catch (_)
                {
                }
            }
            catch (_)
            {
            }
        };

        // Shim: embedded binder lives in categoryTree.embedded.js and auto-binds at DOMContentLoaded.
        // Expose a no-op to avoid duplicate binding and keep existing pages' calls harmless.
        window.tcTree = window.tcTree || {};
        if (typeof window.tcTree.bindEmbeddedFormSubmit !== "function")
        {
            window.tcTree.bindEmbeddedFormSubmit = function () { /* embedded.js handles this */ };
        }

        // Initialize the tree after cfg is available
        initTree();
    }

    return { init: init };
})();
