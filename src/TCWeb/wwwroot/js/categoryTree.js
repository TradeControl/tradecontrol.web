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
        var EXPR_ROOT_KEY = "__EXPRESSIONS__";
        var EXPR_PREFIX = "expr:";
        var isAdmin = !!cfg.isAdmin;
        var CATEGORYTYPE_CASHTOTAL = 1; // server: (short)NodeEnum.CategoryType.CashTotal

        var menuOriginalHtml = null;
        var menuTemplates = { items: {}, dividerHtml: "<div class='dropdown-divider'></div>" };

        var querySelectionApplied = false;
        var selectionRetryCount = 0;
        var MAX_SELECTION_RETRIES = 50;

        const MAX_BFS_FOLDERS = 150;          // was 600
        const CODE_POLL_MAX_TRIES = 12;       // was 40
        const CODE_POLL_INTERVAL_MS = 150;    // unchanged interval, fewer tries
        const BFS_STEP_DELAY_MS = 25;         // existing 25; keep symbolic

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

            // Guard: already present
            if (tree.getNodeByKey(targetKey))
            {
                callback(true);
                return;
            }

            maxFolders = Math.min(maxFolders || MAX_BFS_FOLDERS, MAX_BFS_FOLDERS);

            var queue = [];
            var visited = new Set();

            function enqueue(n)
            {
                if (!n || !n.key || visited.has(n.key))
                {
                    return;
                }
                visited.add(n.key);
                queue.push(n);
            }

            enqueue(parentNode);

            function step()
            {
                if (tree.getNodeByKey(targetKey))
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

                if (current.lazy && !current.loaded)
                {
                    try
                    {
                        var resLoad = current.load();
                        var after = function ()
                        {
                            try
                            {
                                (current.children || []).forEach(enqueue);
                            }
                            catch (_) {}
                            setTimeout(step, BFS_STEP_DELAY_MS);
                        };

                        if (resLoad && typeof resLoad.then === "function")
                        {
                            resLoad.then(after, after);
                            return;
                        }
                        if (resLoad && typeof resLoad.done === "function")
                        {
                            resLoad.done(after).fail(after);
                            return;
                        }
                    }
                    catch (_) {}
                }
                else
                {
                    try
                    {
                        (current.children || []).forEach(enqueue);
                    }
                    catch (_) {}
                }

                setTimeout(step, BFS_STEP_DELAY_MS);
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
                    if (s.indexOf(EXPR_PREFIX) === 0)
                    {
                        // Expression keys have no code variants
                        return [s];
                    }

                    if (s.indexOf("code:") === 0)
                    {
                        return [s, s.substring(5)];
                    }

                    return ["code:" + s, s];
                }

                var variants = keyVariants(selectKey);
                var isCodeKey =
                    variants.length > 0
                    && (variants[0].indexOf("code:") === 0
                        || (variants[1] && variants[1].indexOf("code:") === 0));

                var isExprKey = (typeof selectKey === "string" && selectKey.indexOf(EXPR_PREFIX) === 0);
                if (isExprKey)
                {
                    // Ensure we never walk the code selection path for expressions
                    isCodeKey = false;
                }

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
				        var tree = getTree();
				        if (!tree)
				        {
					        callback(false);
					        return;
				        }

				        // Guard: already present
				        if (tree.getNodeByKey(targetKey))
				        {
					        callback(true);
					        return;
				        }

				        var rootNode = tree.getNodeByKey(ROOT_KEY);
				        if (!rootNode)
				        {
					        callback(false);
					        return;
				        }

				        maxFolders = Math.min(maxFolders || MAX_BFS_FOLDERS, MAX_BFS_FOLDERS);

				        var queue = [];
				        var visited = new Set();

				        function enqueue(n)
				        {
					        if (!n || !n.key || visited.has(n.key))
					        {
						        return;
					        }
					        visited.add(n.key);
					        queue.push(n);
				        }

				        enqueue(rootNode);

				        (function step()
				        {
					        if (tree.getNodeByKey(targetKey))
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

					        function isTypeSyntheticNode(node)
					        {
						        if (!node) { return false; }
						        var k = node.key || "";
						        var d = node.data || {};
						        return k.indexOf("type:") === 0 || d.syntheticKind === "type" || d.isTypeContext === true;
					        }

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
									        (current.children || []).forEach(enqueue);
								        }
								        catch (_) {}
								        setTimeout(step, 15);
							        };
							        if (res && res.then) { res.then(after, after); return; }
							        if (res && res.done) { res.done(after).fail(after); return; }
						        }
						        catch (_) {}
					        }
					        else
					        {
						        try
						        {
							        (current.children || []).forEach(enqueue);
						        }
						        catch (_) {}
					        }

					        setTimeout(step, 15);
				        })();
			        }
			        catch (_)
			        {
				        callback(false);
			        }
		        }
 
		        function loadUnderDiscUntilKey(targetKey, maxFolders, callback)
		        {
			        try
			        {
				        var tree = getTree();
				        if (!tree)
				        {
					        callback(false);
					        return;
				        }
				        if (tree.getNodeByKey(targetKey))
				        {
					        callback(true);
					        return;
				        }
				        var discNode = tree.getNodeByKey(DISC_KEY);
				        if (!discNode)
				        {
					        callback(false);
					        return;
				        }

				        maxFolders = Math.min(maxFolders || MAX_BFS_FOLDERS, MAX_BFS_FOLDERS);

				        var queue = [];
				        var visited = new Set();
				        function enqueue(n)
				        {
					        if (!n || !n.key || visited.has(n.key)) { return; }
					        visited.add(n.key);
					        queue.push(n);
				        }

				        enqueue(discNode);

				        (function step()
				        {
					        if (tree.getNodeByKey(targetKey))
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

					        if (current.lazy && !current.loaded)
					        {
						        try
						        {
							        var res = current.load();
							        var after = function ()
							        {
								        try
                                        {
                                            (current.children || []).forEach(enqueue);
                                        }
                                        catch(_){}
								        setTimeout(step, 15);
							        };
							        if (res && res.then) { res.then(after, after); return; }
							        if (res && res.done) { res.done(after).fail(after); return; }
						        }
						        catch(_){}
					        }
					        else
					        {
						        try
                                {
                                    (current.children || []).forEach(enqueue);
                                }
                                catch(_){}
					        }
					        setTimeout(step, 15);
				        })();
			        }
			        catch(_){ callback(false); }
		        }

                function loadUnderExprUntilKey(targetKey, maxFolders, callback)
                {
                    try
                    {
                        var tree = getTree();
                        if (!tree) { callback(false); return; }
                        if (tree.getNodeByKey(targetKey)) { callback(true); return; }

                        var exprRoot = tree.getNodeByKey(EXPR_ROOT_KEY);
                        if (!exprRoot) { callback(false); return; }

                        maxFolders = Math.min(maxFolders || MAX_BFS_FOLDERS, MAX_BFS_FOLDERS);

                        var queue = [];
                        var visited = new Set();

                        function enqueue(n)
                        {
                            if (!n || !n.key || visited.has(n.key)) { return; }
                            visited.add(n.key);
                            queue.push(n);
                        }

                        enqueue(exprRoot);

                        (function step()
                        {
                            if (tree.getNodeByKey(targetKey))
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

                            if (current.lazy && !current.loaded)
                            {
                                try
                                {
                                    var res = current.load();
                                    var after = function ()
                                    {
                                        try
                                        {
                                            (current.children || []).forEach(enqueue);
                                        }
                                        catch(_){}
                                        setTimeout(step, 15);
                                    };
                                    if (res && res.then) { res.then(after, after); return; }
                                    if (res && res.done) { res.done(after).fail(after); return; }
                                }
                                catch(_){}
                            }
                            else
                            {
                                try
                                {
                                    (current.children || []).forEach(enqueue);
                                }
                                catch(_){}
                            }

                            setTimeout(step, 15);
                        })();
                    }
                    catch(_){ callback(false); }
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
			        if (querySelectionApplied)
			        {
				        return;
			        }
			        querySelectionApplied = true;

			        try
			        {
				        focusBranch(n, function ()
				        {
					        try
                            {
                                n.setActive(true);

                            }
                            catch (_) {}
					        try
                            {
                                persistActiveKey(n);
                            }
                            catch (_) {}
					        if (isMobile())
                            {
                                updateActionBar(n);
                            }
                            else
                            {
                                loadDetails(n);
                            }
				        });
			        }
			        catch (_)
			        {
				        try
                        {
                            n.makeVisible();
                        }
                        catch (_) {}
				        try
                        {
                            n.setActive(true);
                        }
                        catch (_) {}
				        try
                        {
                            persistActiveKey(n);
                        }
                        catch (_) {}
				        if (isMobile())
                        {
                            updateActionBar(n);
                        }
                        else
                        {
                            loadDetails(n);
                        }
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

                if (isExprKey)
                {
                    // Ensure Expressions root exists and then poll its children for our expr node
                    var tree = getTree();
                    if (!tree)
                    {
                        scheduleRetry();
                        return;
                    }

                    var exprRoot = tree.getNodeByKey(EXPR_ROOT_KEY);
                    if (!exprRoot)
                    {
                        // materialize expr root at top-level if needed
                        setTimeout(function ()
                        {
                            loadUnderExprUntilKey(selectKey, 200, function (ok)
                            {
                                if (ok)
                                {
                                    var n = tree.getNodeByKey(selectKey);
                                    if (n) { activateNodeFinal(n); return; }
                                }
                                scheduleRetry();
                            });
                        }, 50);
                        return;
                    }

                    function afterReady()
                    {
                        var n = tree.getNodeByKey(selectKey);
                        if (n)
                        {
                            activateNodeFinal(n);
                            return;
                        }

                        var tries = 0;
                        var maxTries = 40, intervalMs = 150;

                        (function poll()
                        {
                            tries++;
                            var r = exprRoot.reloadChildren
                                ? exprRoot.reloadChildren({ url: nocache(appendQuery(nodesUrl, "id", exprRoot.key)) })
                                : null;

                            var cont = function ()
                            {
                                var hit = tree.getNodeByKey(selectKey);
                                if (hit) { activateNodeFinal(hit); return; }
                                if (tries >= maxTries) { scheduleRetry(); return; }
                                setTimeout(poll, intervalMs);
                            };

                            if (r && r.then) { r.then(cont, cont); }
                            else if (r && r.done) { r.done(cont).fail(cont); }
                            else { cont(); }
                        })();
                    }

                    if (exprRoot.folder && !exprRoot.expanded)
                    {
                        var ex = exprRoot.setExpanded(true);
                        if (ex && ex.then) { ex.then(afterReady, afterReady); }
                        else if (ex && ex.done) { ex.done(afterReady).fail(afterReady); }
                        else { afterReady(); }
                    }
                    else
                    {
                        afterReady();
                    }
                    return;
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

        function resetExpressionError(key, onDone)
        {
            postJsonGlobal("ResetExpressionError", { key: key })
                .done(function (res)
                {
                    if (!res || res.success === false)
                    {
                        var message = (res && res.message) ? res.message : "Failed to reset expression error.";
                        notify(message, "warning");

                        if (typeof onDone === "function")
                        {
                            onDone(false, res);
                        }

                        return;
                    }

                    notify("Expression error status reset.", "success");

                    if (typeof onDone === "function")
                    {
                        onDone(true, res);
                    }
                })
                .fail(function ()
                {
                    notify("Failed to reset expression error.", "warning");

                    if (typeof onDone === "function")
                    {
                        onDone(false, null);
                    }
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
                || key === "__EXPRESSIONS__"            // treat expressions root as synthetic anchor
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

            if (typeof key === "string" && key.indexOf(EXPR_PREFIX) === 0)
            {
                // Use existing detailsUrl (TreeDetailsModel.OnGetAsync) with embed
                var url = detailsUrl + "?key=" + encodeURIComponent(key);
                url = appendQuery(url, "embed", "1");
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
                return;
            }

            // Synthetic fallback: show parent details if parent real
            if (tcIsSyntheticKey(key))
            {
                if (parentKey && !tcIsSyntheticKey(parentKey))
                {
                    var parentUrl = detailsUrl + "?key=" + encodeURIComponent(parentKey);

                    // Desktop RHS should fetch embed=1 (no layout)
                    parentUrl = appendQuery(parentUrl, "embed", "1");

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

            // Desktop RHS should fetch embed=1 (no layout)
            url = appendQuery(url, "embed", "1");

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
            var $pane = ($scope && $scope.length ? $scope : $("#detailsPane"));
            var $marker = $pane.find("#tcEmbedResult").first();

            if (!$marker.length)
            {
                return false;
            }

            var selectVal = $marker.attr("data-select") || "";
            var expandVal = $marker.attr("data-expand") || "";
            var nodeJson = $marker.attr("data-node") || "";
            var removeKey = $marker.attr("data-remove") || "";

            if (!selectVal || !expandVal)
            {
                return false;
            }

            var tree = getTree();
            if (!tree)
            {
                return false;
            }

            var isExprKey = (selectVal.indexOf("expr:") === 0);
            if (!isExprKey && selectVal.indexOf("code:") !== 0)
            {
                selectVal = "code:" + selectVal;
            }

            // 1) Desktop edit-save refresh path:
            // If the target node already exists, just refresh its details pane and keep selection.
            var existing = tree.getNodeByKey(selectVal);
            if (existing)
            {
                try
                {
                    // Keep it active; refresh RHS details only
                    existing.setActive(true);
                    persistActiveKey(existing);

                    if (!isMobile())
                    {
                        loadDetails(existing);
                    }
                    else
                    {
                        updateActionBar(existing);
                    }
                }
                catch (_)
                {
                }

                querySelectionApplied = true;
                return true;
            }

            // 2) Deletion flow (rare for expressions during edit page, but keep behaviour)
            if (removeKey)
            {
                var isExprRemove = removeKey.indexOf("expr:") === 0;
                if (!isExprRemove && removeKey.indexOf("code:") !== 0)
                {
                    removeKey = "code:" + removeKey;
                }

                var removedParent = null;

                try
                {
                    var root = tree.getRootNode();

                    if (root)
                    {
                        root.visit(function (n)
                        {
                            if (n && n.key === removeKey)
                            {
                                try
                                {
                                    removedParent = n.getParent ? n.getParent() : null;
                                }
                                catch (_)
                                {
                                    removedParent = null;
                                }

                                try
                                {
                                    n.remove();
                                }
                                catch (_)
                                {
                                }

                                return false;
                            }

                            return true;
                        });
                    }
                }
                catch (_)
                {
                }

                var parentNode = removedParent || tree.getNodeByKey(expandVal);

                if (parentNode)
                {
                    try
                    {
                        parentNode.setActive(true);
                        persistActiveKey(parentNode);

                        if (!isMobile())
                        {
                            loadDetails(parentNode);
                        }
                        else
                        {
                            updateActionBar(parentNode);
                        }
                    }
                    catch (_)
                    {
                    }

                    querySelectionApplied = true;
                    return true;
                }

                querySelectionApplied = false;
                selectionRetryCount = 0;
                applyQuerySelection();
                return true;
            }

            // 3) Creation/injection (keep existing behaviour for completeness)
            var spec = null;

            if (nodeJson)
            {
                try
                {
                    spec = JSON.parse(nodeJson);
                }
                catch (_)
                {
                    spec = null;
                }
            }

            var parentForCreate = tree.getNodeByKey(expandVal);

            // Expression CREATE: inject and select
            if (spec && spec.key && spec.key.indexOf("expr:") === 0 && !tree.getNodeByKey(spec.key) && parentForCreate)
            {
                var exprNodeData =
                {
                    title: spec.title || (spec.data && spec.data.categoryCode) || spec.key,
                    key: spec.key,
                    folder: false,
                    lazy: false,
                    icon: false,
                    data: spec.data || { nodeType: "expression" }
                };

                function finishExprInjection()
                {
                    try
                    {
                        parentForCreate.addChildren([exprNodeData]);
                    }
                    catch (_)
                    {
                    }

                    var injected = tree.getNodeByKey(spec.key);

                    if (injected)
                    {
                        try
                        {
                            injected.setActive(true);
                            persistActiveKey(injected);

                            if (isMobile())
                            {
                                updateActionBar(injected);
                            }
                            else
                            {
                                loadDetails(injected);
                            }
                        }
                        catch (_)
                        {
                        }

                        querySelectionApplied = true;
                        return true;
                    }

                    querySelectionApplied = false;
                    selectionRetryCount = 0;
                    applyQuerySelection();
                    return true;
                }

                if (parentForCreate.folder && !parentForCreate.expanded)
                {
                    var exprExpandRes = parentForCreate.setExpanded(true);

                    if (exprExpandRes && exprExpandRes.then)
                    {
                        exprExpandRes.then(finishExprInjection, finishExprInjection);
                    }
                    else if (exprExpandRes && exprExpandRes.done)
                    {
                        exprExpandRes.done(finishExprInjection).fail(finishExprInjection);
                    }
                    else
                    {
                        finishExprInjection();
                    }
                }
                else
                {
                    finishExprInjection();
                }

                return true;
            }

            // Code CREATE: inject and select (unchanged)
            if (spec && spec.key && spec.key.indexOf("code:") === 0 && !tree.getNodeByKey(spec.key) && parentForCreate)
            {
                var siblingTemplate = (function ()
                {
                    try
                    {
                        if (!parentForCreate || !parentForCreate.children)
                        {
                            return null;
                        }

                        for (var i = 0; i < parentForCreate.children.length; i++)
                        {
                            var ch = parentForCreate.children[i];
                            if (!ch || !ch.key || ch.key.indexOf("code:") !== 0)
                            {
                                continue;
                            }
                            if (typeof ch.title === "string" && ch.title.length > 0)
                            {
                                return ch.title;
                            }
                        }
                    }
                    catch (_)
                    {
                    }

                    return null;
                })();

                var formattedTitle = buildFormattedCodeTitle(spec, siblingTemplate);

                var siblingCodeNode = null;

                try
                {
                    if (parentForCreate && parentForCreate.children)
                    {
                        for (var si = 0; si < parentForCreate.children.length; si++)
                        {
                            var sc = parentForCreate.children[si];
                            if (sc && sc.key && sc.key.indexOf("code:") === 0)
                            {
                                siblingCodeNode = sc;
                                break;
                            }
                        }
                    }
                }
                catch (_)
                {
                    siblingCodeNode = null;
                }

                var newNodeData =
                {
                    title: formattedTitle,
                    key: spec.key,
                    folder: false,
                    lazy: false,
                    icon: false,
                    extraClasses: siblingCodeNode && siblingCodeNode.extraClasses ? siblingCodeNode.extraClasses : "",
                    data: (function ()
                    {
                        var d = spec.data || {};
                        if (!d.nodeType)
                        {
                            d.nodeType = "code";
                        }
                        if (typeof d.isEnabled === "undefined" && siblingCodeNode && siblingCodeNode.data)
                        {
                            d.isEnabled = siblingCodeNode.data.isEnabled;
                        }
                        return d;
                    })()
                };

                function afterExpand()
                {
                    try
                    {
                        parentForCreate.addChildren([newNodeData]);
                    }
                    catch (_)
                    {
                    }

                    var created = tree.getNodeByKey(spec.key);

                    if (created)
                    {
                        try
                        {
                            created.icon = false;

                            var $li = $(created.span).closest("li");
                            $li.find("> span.fancytree-node > span.fancytree-icon").remove();
                            $(created.span).siblings(".fancytree-icon").remove();

                            created.render(false);
                        }
                        catch (_)
                        {
                        }

                        try
                        {
                            created.setActive(true);
                            persistActiveKey(created);

                            if (isMobile())
                            {
                                updateActionBar(created);
                            }
                            else
                            {
                                loadDetails(created);
                            }
                        }
                        catch (_)
                        {
                        }

                        querySelectionApplied = true;
                        return true;
                    }

                    querySelectionApplied = false;
                    selectionRetryCount = 0;
                    applyQuerySelection();
                    return true;
                }

                if (parentForCreate.folder && !parentForCreate.expanded)
                {
                    var expRes = parentForCreate.setExpanded(true);

                    if (expRes && typeof expRes.then === "function")
                    {
                        expRes.then(afterExpand, afterExpand);
                    }
                    else if (expRes && expRes.done)
                    {
                        expRes.done(afterExpand).fail(afterExpand);
                    }
                    else
                    {
                        afterExpand();
                    }
                }
                else
                {
                    afterExpand();
                }

                return true;
            }

            // 4) Fallback: rely on existing query selection pipeline
            querySelectionApplied = false;
            selectionRetryCount = 0;
            applyQuerySelection();
            return true;
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
                                if (querySelectionApplied)
                                {
                                    var marker = pane.querySelector("#tcEmbedResult");
                                    if (marker)
                                    {
                                        var sel = marker.getAttribute("data-select") || "";
                                        if (sel && !getTree().getNodeByKey(sel))
                                        {
                                            // allow; fall through
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                }
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

        function postJsonFlexible(handler, data)
        {
            var token = antiXsrf();
            return $.ajax({
                type: "POST",
                url: handlerUrl(handler),
                data: data,
                headers: token ? { "RequestVerificationToken": token } : {},
                // Accept text and parse JSON if possible, avoiding jQuery 'parseerror' with status 200
                dataType: "text"
            }).then(function (text, _, jqXHR)
            {
                var res = null;
                try
                {
                    res = JSON.parse(text);
                }
                catch (_)
                {
                    // If not JSON, synthesize a success wrapper with raw text
                    res = { success: true, message: (typeof text === "string" ? text : "") };
                }
                // Normalize server ok (status 200) with missing JSON into success by default
                return res;
            }, function (jqXHR)
            {
                // If server returned 200 but jQuery treated it as failure (parseerror), expose payload
                if (jqXHR && jqXHR.status === 200)
                {
                    var payload = jqXHR.responseText || "";
                    var res = null;
                    try
                    {
                        res = JSON.parse(payload);
                    }
                    catch (_)
                    {
                        res = { success: true, message: payload };
                    }
                    return res;
                }
                // Propagate a real failure
                return $.Deferred().reject(jqXHR);
            });
        }

        function postToggleEnabled(nodeOrKey, enabled)
        {
            try
            {
                var tree = getTree();
                var node = (typeof nodeOrKey === "string") ? (tree ? tree.getNodeByKey(nodeOrKey) : null) : nodeOrKey;

                var kinds = getNodeKinds(node);
                var key = (node && node.key) ? node.key : "";

                // Primary handler by kind
                var primary = (kinds && kinds.nodeType === "expression") ? "SetExpressionEnabled" : "SetEnabled";
                var alternate = (primary === "SetExpressionEnabled") ? "SetEnabled" : "SetExpressionEnabled";

                // Attempt primary, then fallback if the server reports "not found"
                return postJsonFlexible(primary, { key: key, enabled: enabled }).then(function (res)
                {
                    var msg = (res && res.message) ? String(res.message) : "";
                    var failed = (res && res.success === false);

                    if (failed && /not\s+found/i.test(msg))
                    {
                        // Try the alternate handler automatically
                        return postJsonFlexible(alternate, { key: key, enabled: enabled });
                    }

                    return res;
                });
            }
            catch (_)
            {
                var k = (typeof nodeOrKey === "string") ? nodeOrKey : ((nodeOrKey && nodeOrKey.key) ? nodeOrKey.key : "");
                return postJsonFlexible("SetEnabled", { key: k, enabled: enabled });
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

                var isExpression = (typeof key === "string" && key.indexOf("expr:") === 0) || (data.nodeType === "expression");
                if (isExpression)
                {
                    // Hide all then show expression-specific controls
                    $pane.find("[data-action]").hide();

                    // Admin controls: Edit / Delete / Toggle / Reset Error
                    if (isAdmin)
                    {
                        $pane.find("[data-action='editExpression']").show();
                        $pane.find("[data-action='deleteExpression']").show();
                        $pane.find("[data-action='resetExpressionError']").show();

                        // Ensure Toggle button exists in the card for expressions
                        var $toggle = $pane.find("[data-action='toggleEnabled']");
                        if ($toggle.length === 0)
                        {
                            var $anchor = $pane.find("[data-action='editExpression']").first();
                            var $container = $anchor.length ? $anchor.parent() : $pane.find("[data-action]").first().parent();
                            if ($container && $container.length)
                            {
                                var btnHtml = "<button type='button' class='btn btn-sm btn-outline-secondary' data-action='toggleEnabled'><i class='bi bi-power'></i> Enable</button>";
                                $container.append(btnHtml);
                                $toggle = $pane.find("[data-action='toggleEnabled']");
                            }
                        }
                        if ($toggle.length)
                        {
                            $toggle.show();
                            setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                        }
                    }

                    // Non-admin: View only
                    $pane.find("[data-action='viewExpression']").show();
                    return;
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
                    "addCategory","createCategory","newTotal","createTotal",
                    "addCashCode","createCashCode",
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
                    dv("createCashCode", isAdmin);
                    dv("addCashCode", isAdmin);

                    setButtonText($pane, "createCashCode", "New Cash Code");

                    order = ["edit","delete","toggleEnabled","createCashCode","addCashCode"];
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

                    order = ["edit","delete","toggleEnabled","addCashCode","move"];
                    arrangeDetailsButtons($pane, order);
                    setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                    return;
                }

                function showTotalCategory()
                {
                    dv("edit", isAdmin);
                    dv("delete", isAdmin);
                    dv("toggleEnabled", isAdmin);

                    dv("addCategory", isAdmin);
                    dv("createTotal", isAdmin);
                    dv("createCategory", isAdmin);

                    dv("move", isAdmin);

                    setButtonText($pane, "addCategory", "Add Category");
                    setButtonText($pane, "createTotal", "New Total");
                    setButtonText($pane, "createCategory", "New Category");

                    order = ["edit","delete","toggleEnabled","addCategory","createTotal","createCategory","move"];
                    arrangeDetailsButtons($pane, order);
                    setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                }

                function showCashCodeCategory()
                {
                    dv("edit", isAdmin);
                    dv("delete", isAdmin);
                    dv("toggleEnabled", isAdmin);

                    dv("addCashCode", isAdmin);
                    dv("createCashCode", isAdmin);

                    dv("move", isAdmin);

                    setButtonText($pane, "addCashCode", "Add Cash Code");
                    setButtonText($pane, "createCashCode", "New Cash Code");

                    order = ["edit","delete","toggleEnabled","addCashCode","createCashCode","move"];
                    arrangeDetailsButtons($pane, order);
                    setToggleEnabledLabel($pane, (data && data.isEnabled === 1));
                }

                if (parentKeyNow === DISC_KEY)
                {
                    if (isCatTotal)
                    {
                        showTotalCategory();
                    }
                    else if (isCatCashCode)
                    {
                        showCashCodeCategory();
                    }
                    return;
                }

                if (parentKeyNow === ROOT_KEY)
                {
                    if (isCatTotal)
                    {
                        showTotalCategory();
                        dv("setProfitRoot", isAdmin);
                        dv("setVatRoot", isAdmin);
                        order = ["edit","delete","toggleEnabled","addCategory","createTotal","createCategory","move","setProfitRoot","setVatRoot"];
                        arrangeDetailsButtons($pane, order);
                    }
                    else if (isCatCashCode)
                    {
                        showCashCodeCategory();
                    }
                    return;
                }

                if (isCatTotal)
                {
                    showTotalCategory();
                }
                else if (isCatCashCode)
                {
                    showCashCodeCategory();
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

            var nodeType =
                (typeof data.nodeType !== "undefined" && data.nodeType !== null)
                    ? String(data.nodeType)
                    : null;

            if (!nodeType)
            {
                if (typeof key === "string" && key.indexOf("expr:") === 0)
                {
                    nodeType = "expression";
                }
                else if (typeof key === "string" && key.indexOf("code:") === 0)
                {
                    nodeType = "code";
                }
                else if (typeof key === "string" && key.indexOf("type:") === 0)
                {
                    nodeType = "synthetic";
                }
                else if (key === "__EXPRESSIONS__")
                {
                    nodeType = "synthetic"; // treat expressions root as synthetic context
                }
                else if (node && node.folder)
                {
                    nodeType = "category";
                }
                else
                {
                    nodeType = "synthetic";
                }
            }

            var isExpression = nodeType === "expression";

            var isSynthetic =
                !node
                || key === ROOT_KEY
                || key === DISC_KEY
                || key === "__EXPRESSIONS__"        // ensure expressions root is synthetic
                || (nodeType === "synthetic" && !isExpression);

            var isCode = (nodeType === "code") || (key && typeof key === "string" && key.indexOf("code:") === 0);
            var isCat = !!(node && node.folder && !isCode && !isSynthetic);
            var isRoot = key === ROOT_KEY;
            var isDisconnect = key === DISC_KEY;

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

            function isCode(n)
            {
                return !!(n && n.data && n.data.nodeType === "code");
            }

            function isExpression(n)
            {
                return !!(n && n.data && n.data.nodeType === "expression");
            }

            function apply(n)
            {
                if (!n)
                {
                    return;
                }
                n.data = n.data || {};
                n.data.isEnabled = enabled ? 1 : 0;
                try
                {
                    n.toggleClass("tc-disabled", !enabled);
                }
                catch (_)
                {
                }
            }

            // Expressions: apply directly (no cascade)
            if (isExpression(node))
            {
                apply(node);
                return;
            }

            // Codes and categories:
            if (!cascadeCategories || !isCategory(node))
            {
                if (isCategory(node) || isCode(node))
                {
                    apply(node);
                }
                return;
            }

            // Cascade to descendant categories only
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
                case "addCategory":          // attach existing category (mapping)
                    return ["[data-action='addExistingCategory']"];
                case "createCategory":       // create a new child category under a Total
                    return ["[data-action='createCategory']"];
                case "createTotal":          // canonical new Total action
                    return ["[data-action='createTotal']"];
                case "newTotal":             // legacy/migrated name mapped to createTotal
                    return ["[data-action='createTotal']"];
                case "addCashCode":          // attach existing cash code to category
                    return ["[data-action='addExistingCashCode']", "[data-action='addExistingCode']"];
                case "createCashCode":       // create new cash code
                    return ["[data-action='createCashCode']"];
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
            if (!bar)
            {
                return;
            }

            if (!isMobile())
            {
                bar.classList.remove("tc-visible");
                return;
            }

            var kinds = getNodeKinds(node);
            var data = kinds.data;
            var key = kinds.key || "";
            var nodeType = kinds.nodeType || "";

            // Hide bar entirely for synthetic anchors (root, disconnected, types, expressions root)
            if (!node || kinds.isSynthetic || key === "__EXPRESSIONS__")
            {
                bar.classList.remove("tc-visible");
                return;
            }

            var parentKeyNow = (node && node.getParent ? (node.getParent()?.key || "") : "");
            var inTypeCtx = !!(data && (data.isTypeContext === true || data.syntheticKind === "type"))
                            || (typeof parentKeyNow === "string" && parentKeyNow.indexOf("type:") === 0);

            bar.querySelectorAll(".admin-only").forEach(function (el)
            {
                el.style.display = isAdmin ? "" : "none";
            });

            var isExpression = (nodeType === "expression");

            setBarButtonVisible("view", true);
            setBarButtonVisible("edit", isAdmin);

            // Move: hide for expressions explicitly
            setBarButtonVisible("move", isAdmin && !inTypeCtx && !isExpression);

            setBarButtonVisible("delete", isAdmin && !inTypeCtx);
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
                if (!btn)
                {
                    return;
                }
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

        function plainNodeTitle(node)
        {
            try
            {
                if (!node) { return ""; }
                var raw = node.title || "";
                if (!raw) { return node.key || ""; }
                var div = document.createElement("div");
                div.innerHTML = raw;
                var txt = (div.textContent || "").trim();
                return txt || node.key || "";
            }
            catch (_)
            {
                return node && node.key ? node.key : "";
            }
        }

        function showContextMenu(x, y, node)
        {
            if (!node)
            {
                return;
            }

            var $menu = getMenuInstance();
            if (menuOriginalHtml === null)
            {
                menuOriginalHtml = $menu.html();
            }

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
            var isExprRoot = (key === EXPR_ROOT_KEY);
            var isExpressionNode = (typeof key === "string" && key.indexOf("expr:") === 0);

            var isTypeSynthetic =
                (typeof key === "string" && key.indexOf("type:") === 0)
                || (data && (data.syntheticKind === "type" || data.syntheticKind === "typesRoot" || data.isTypeContext === true));

            var isTopAnchor = isRootKey || isDiscKey || isTypeSynthetic || isExprRoot;

            if (node.folder && typeof data.categoryType === "undefined" && parentKeyNow === ROOT_KEY && !isExprRoot)
            {
                data.categoryType = CATEGORYTYPE_CASHTOTAL;
                node.data.categoryType = CATEGORYTYPE_CASHTOTAL;
            }

            $menu.empty();

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

            if (!menuTemplates.items.createExpression)
            {
                menuTemplates.items.createExpression = "<div class='dropdown-item' data-action='createExpression'>New Expression</div>";
            }
            if (!menuTemplates.items.viewExpression)
            {
                menuTemplates.items.viewExpression = "<div class='dropdown-item' data-action='viewExpression'>View</div>";
            }
            if (!menuTemplates.items.editExpression)
            {
                menuTemplates.items.editExpression = "<div class='dropdown-item' data-action='editExpression'>Edit</div>";
            }
            if (!menuTemplates.items.deleteExpression)
            {
                menuTemplates.items.deleteExpression = "<div class='dropdown-item' data-action='deleteExpression'>Delete</div>";
            }
            if (!menuTemplates.items.toggleEnabled)
            {
                menuTemplates.items.toggleEnabled = "<div class='dropdown-item' data-action='toggleEnabled'><i class='bi bi-power'></i> Enable</div>";
            }

            if (isExprRoot)
            {
                pushIf(isAdmin, "createExpression");
                renderMenuFromTemplates($menu, order, groups);
            }
            else if (isExpressionNode)
            {
                order = ["viewExpression"];
                if (isAdmin)
                {
                    order.push("editExpression");
                    order.push("deleteExpression");
                    order.push("toggleEnabled"); // add toggle for expressions
                    order.push("moveUp");
                    order.push("moveDown");
                }
                renderMenuFromTemplates($menu, order, []);
                // Set Enable/Disable text according to current state
                setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
            }
            else if (isRootKey || isTypeSynthetic)
            {
                order = ["expandSelected", "collapseSelected"];
                renderMenuFromTemplates($menu, order, groups);
            }
            else if (key === DISC_KEY)
            {
                pushIf(isAdmin, "createTotal");
                pushIf(isAdmin, "createCategory");
                order.push("expandSelected", "collapseSelected");
                groups = [2];
                renderMenuFromTemplates($menu, order, groups);
            }
            else if (isCode)
            {
                order = ["view"];
                pushIf(isAdmin, "createCashCode");
                pushIf(isAdmin, "edit");
                pushIf(isAdmin, "delete");
                pushIf(isAdmin, "toggleEnabled");
                renderMenuFromTemplates($menu, order, groups);
                var $cc = $menu.find("[data-action='createCashCode']");
                if ($cc.length) { $cc.text("New Cash Code like this…"); }
                setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
            }
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
                        pushIf(isAdmin, "createTotal");
                        pushIf(isAdmin, "createCategory");
                        pushIf(isAdmin, "addExistingCategory");
                    }
                    else if (isCatCashCode)
                    {
                        pushIf(isAdmin, "createCashCode");
                        pushIf(isAdmin, "addExistingCashCode");
                    }
                    pushIf(isAdmin, "edit");
                    pushIf(isAdmin, "delete");
                    pushIf(isAdmin, "toggleEnabled");
                    pushIf(isAdmin, "move");
                    groups = [3];
                    renderMenuFromTemplates($menu, order, groups);
                    setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                }
                else if (parentKeyNow === ROOT_KEY)
                {
                    if (isCatTotal)
                    {
                        order = ["view"];
                        pushIf(isAdmin, "createTotal");
                        pushIf(isAdmin, "createCategory");
                        pushIf(isAdmin, "addExistingCategory");
                        pushIf(isAdmin, "edit");
                        pushIf(isAdmin, "delete");
                        pushIf(isAdmin, "toggleEnabled");
                        pushIf(isAdmin, "move");
                        pushIf(isAdmin, "moveUp");
                        pushIf(isAdmin, "moveDown");
                        if (!menuTemplates.items.setProfitRoot)
                        {
                            menuTemplates.items.setProfitRoot = "<div class='dropdown-item' data-action='setProfitRoot'>Set Profit Root</div>";
                        }
                        if (!menuTemplates.items.setVatRoot)
                        {
                            menuTemplates.items.setVatRoot = "<div class='dropdown-item' data-action='setVatRoot'>Set VAT Root</div>";
                        }
                        pushIf(isAdmin, "setProfitRoot");
                        pushIf(isAdmin, "setVatRoot");
                        order.push("expandSelected", "collapseSelected");
                        renderMenuFromTemplates($menu, order, [3, 6, 9, 13]);
                        setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                    }
                    else if (isCatCashCode)
                    {
                        order = ["view"];
                        pushIf(isAdmin, "createCashCode");
                        pushIf(isAdmin, "addExistingCashCode");
                        pushIf(isAdmin, "edit");
                        pushIf(isAdmin, "delete");
                        pushIf(isAdmin, "toggleEnabled");
                        pushIf(isAdmin, "move");
                        pushIf(isAdmin, "moveUp");
                        pushIf(isAdmin, "moveDown");
                        order.push("expandSelected", "collapseSelected");
                        renderMenuFromTemplates($menu, order, [2, 5, 7]);
                        setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                    }
                }
                else
                {
                    if (isCatTotal)
                    {
                        order = ["view"];
                        pushIf(isAdmin, "createTotal");
                        pushIf(isAdmin, "createCategory");
                        pushIf(isAdmin, "addExistingCategory");
                        pushIf(isAdmin, "edit");
                        pushIf(isAdmin, "delete");
                        pushIf(isAdmin, "toggleEnabled");
                        pushIf(isAdmin, "move");
                        pushIf(isAdmin, "moveUp");
                        pushIf(isAdmin, "moveDown");
                        order.push("expandSelected", "collapseSelected");
                        renderMenuFromTemplates($menu, order, [3, 6, 9]);
                        setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                    }
                    else if (isCatCashCode)
                    {
                        order = ["view"];
                        pushIf(isAdmin, "createCashCode");
                        pushIf(isAdmin, "addExistingCashCode");
                        pushIf(isAdmin, "edit");
                        pushIf(isAdmin, "delete");
                        pushIf(isAdmin, "toggleEnabled");
                        pushIf(isAdmin, "move");
                        pushIf(isAdmin, "moveUp");
                        pushIf(isAdmin, "moveDown");
                        order.push("expandSelected", "collapseSelected");
                        renderMenuFromTemplates($menu, order, [2, 5, 7]);
                        setToggleEnabledLabel($menu, (kinds.data && kinds.data.isEnabled === 1));
                    }
                }
            }
            else
            {
                renderMenuFromTemplates($menu, ["expandSelected", "collapseSelected"], []);
            }

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

            if (parentKeyNow !== ROOT_KEY)
            {
                $menu.find("[data-action='setProfitRoot'],[data-action='setVatRoot']").hide();
            }

            if (!isMobile())
            {
                try
                {
                    $menu.find("[data-action='view']").remove();
                    $menu.find("[data-action='viewExpression']").remove();
                    normalizeDividers($menu);
                }
                catch (_)
                {
                }
            }

            if (isMobile())
            {
                $menu.addClass("mobile-sheet").css({ top: "", left: "" }).show();
                $menu.css
                (
                    {
                        maxHeight: (window.innerHeight - 24) + "px",
                        overflowY: "auto",
                        paddingBottom: "64px"
                    }
                );
            }
            else
            {
                $menu.removeClass("mobile-sheet").css({ top: y + "px", left: x + "px" }).show();
            }

            $menu.data("nodeKey", key).data("parentKey", (kinds.isSynthetic || isExprRoot ? "" : parentKeyNow));

            setTimeout(function ()
            {
                $(document).one("click.treeCtx", function ()
                {
                    $menu.hide().data("nodeKey", null).data("parentKey", null);
                    if (isMobile() && node && !kinds.isSynthetic && !isExprRoot)
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

                    if (key === "Home")
                    {
                        var first = cur.getParent && cur.getParent() && cur.getParent().getFirstChild && cur.getParent().getFirstChild();
                        if (first) { first.setActive(true); persistActiveKey(first); }
                        e.preventDefault();
                        e.stopPropagation();
                        return;
                    }

                    if (key === "End")
                    {
                        var last = cur.getParent && cur.getParent() && cur.getParent().getLastChild && cur.getParent().getLastChild();
                        if (last) { last.setActive(true); persistActiveKey(last); }
                        e.preventDefault();
                        e.stopPropagation();
                        return;
                    }

                    return;
                }

                // Shift+ArrowUp / Shift+ArrowDown => reorder
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

                var kinds = getNodeKinds(node);

                // A) Expressions: reorder among expression siblings (flat under __EXPRESSIONS__)
                if (kinds.nodeType === "expression")
                {
                    var anchorExpr = isUp ? node.getPrevSibling() : node.getNextSibling();
                    while (anchorExpr && (!anchorExpr.key || anchorExpr.key.indexOf("expr:") !== 0))
                    {
                        anchorExpr = isUp ? anchorExpr.getPrevSibling() : anchorExpr.getNextSibling();
                    }

                    if (!anchorExpr)
                    {
                        e.preventDefault();
                        e.stopPropagation();
                        return;
                    }

                    var modeExpr = isUp ? "before" : "after";
                    postJsonGlobal("ReorderExpression", { key: node.key, anchorKey: anchorExpr.key, mode: modeExpr })
                        .done(function (res)
                        {
                            if (res && res.success)
                            {
                                try
                                {
                                    node.moveTo(anchorExpr, modeExpr);
                                }
                                catch (_) {}
                                node.setActive(true);
                                persistActiveKey(node);
                                announce("Moved expression " + (node.title || node.key) + (modeExpr === "before" ? " before " : " after ") + (anchorExpr.title || anchorExpr.key));
                                notify("Order updated", "success");
                                if (!isMobile()) { loadDetails(node); resizeColumns(); }
                            }
                            else
                            {
                                alert((res && res.message) || "Reorder failed");
                            }
                        })
                        .fail(function (xhr)
                        {
                            alert("Server error (" + xhr.status + ")");
                        });

                    e.preventDefault();
                    e.stopPropagation();
                    return;
                }

                // B) Categories: existing behaviour
                if (!kinds.isCat) { return; }

                var parent = node.getParent ? node.getParent() : null;
                if (!parent) { return; }

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
                    e.preventDefault();
                    e.stopPropagation();
                    return;
                }

                var mode = isUp ? "before" : "after";

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
                            catch (ex) {}
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
                    postJsonGlobal("ReorderSiblings", { parentKey: parent.key || "", key: node.key, anchorKey: anchor.key, mode: mode })
                    .done(function (res)
                    {
                        if (res && res.success)
                        {
                            try
                            {
                                node.moveTo(anchor, mode);
                            }
                            catch (ex) {}
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

                function reorderExpression(direction)
                {
                    if (!isAdmin)
                    {
                        alert("Insufficient privileges");
                        return;
                    }
                    if (!key || key.indexOf("expr:") !== 0)
                    {
                        alert("Select an expression");
                        return;
                    }
                    var curNode = tree.getNodeByKey(key);
                    if (!curNode)
                    {
                        alert("Node not found");
                        return;
                    }
                    var anchor = (direction === "up") ? curNode.getPrevSibling() : curNode.getNextSibling();
                    while (anchor && anchor.key.indexOf("expr:") !== 0)
                    {
                        anchor = (direction === "up") ? anchor.getPrevSibling() : anchor.getNextSibling();
                    }
                    if (!anchor)
                    {
                        return;
                    }
                    var mode = (direction === "up") ? "before" : "after";
                    postJsonGlobal("ReorderExpression",
                        {
                            key: curNode.key,
                            anchorKey: anchor.key,
                            mode: mode
                        })
                        .done(function (res)
                        {
                            if (res && res.success)
                            {
                                try
                                {
                                    curNode.moveTo(anchor, mode);
                                }
                                catch (_)
                                {
                                }
                                curNode.setActive(true);
                                persistActiveKey(curNode);
                                notify("Expression order updated", "success");
                                if (!isMobile())
                                {
                                    loadDetails(curNode);
                                }
                            }
                            else
                            {
                                alert((res && res.message) || "Reorder failed");
                            }
                        })
                        .fail(function (xhr)
                        {
                            alert("Server error (" + xhr.status + ")");
                        });
                }

                switch (action)
                {
                    case "createExpression":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        openAction("CreateExpression", "");
                        break;
                    }
                    case "viewExpression":
                    {
                        if (!key) { alert("Select an expression"); break; }
                        if (isMobile())
                        {
                            var url = detailsUrl + "?key=" + encodeURIComponent(key) + "&parentKey=" + encodeURIComponent("__EXPRESSIONS__");
                            window.location.href = url;
                        }
                        else
                        {
                            loadDetails(node);
                        }
                        break;
                    }
                    case "editExpression":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!key) { alert("Select an expression"); break; }
                        openAction("EditExpression", key);
                        break;
                    }
                    case "deleteExpression":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!key) { alert("Select an expression"); break; }
                        openAction("DeleteExpression", key);
                        break;
                    }
                    case "moveUp":
                    {
                        if (key && key.indexOf("expr:") === 0)
                        {
                            reorderExpression("up");
                            break;
                        }
                    }
                    case "moveDown":
                    {
                        if (key && key.indexOf("expr:") === 0)
                        {
                            reorderExpression("down");
                            break;
                        }
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }
                        if (key && key.indexOf("code:") === 0)
                        {
                            alert("Cannot reorder cash code nodes");
                            break;
                        }
                        var handler = (action === "moveUp") ? "MoveUp" : "MoveDown";
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
                            loadDetails(node);
                        }
                        break;
                    }

                    case "edit":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var kinds = getNodeKinds(node);
                        if (kinds.nodeType === "expression")
                        {
                            openAction("EditExpression", key);
                            break;
                        }

                        if (kinds.isCode)
                        {
                            var raw = (typeof key === "string" && key.indexOf("code:") === 0) ? key.substring(5) : key;
                            openAction("EditCashCode", raw);
                            break;
                        }
                        if (kinds.data && kinds.data.categoryType === CATEGORYTYPE_CASHTOTAL)
                        {
                            openAction("EditTotal", key);
                            break;
                        }
                        if (kinds.data && kinds.data.categoryType === 0)
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
                        var targetParent = (node && node.folder) ? key : parentKey;
                        if (!targetParent)
                        {
                            alert("Select a parent category");
                            break;
                        }
                        openAction("AddCategory", "", targetParent);
                        break;
                    }

                    case "addExistingCashCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        var targetCategory = node && node.folder ? key : parentKey;
                        if (!targetCategory)
                        {
                            alert("Select a category first");
                            break;
                        }
                        openAction("AddCashCode", "", targetCategory);
                        break;
                    }

                    case "move":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node || !node.folder) { alert("Select a category"); break; }
                        if (key && key.indexOf("expr:") === 0)
                        {
                            alert("Expressions cannot be moved under other nodes.");
                            break;
                        }
                        openAction("Move", key, parentKey);
                        break;
                    }

                    case "createTotal":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        var targetParent = (node && node.folder) ? key : parentKey;
                        if (!targetParent)
                        {
                            alert("Select a parent category");
                            break;
                        }
                        if (targetParent === DISC_KEY)
                        {
                            openAction("CreateTotal", "", DISC_KEY);
                            break;
                        }
                        openAction("CreateTotal", "", targetParent);
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
                        openAction("CreateCategory", "", targetParent);
                        break;
                    }

                    case "createCashCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node)
                        {
                            alert("Select a node first");
                            break;
                        }
                        var isCodeNode = !!(node.data && node.data.nodeType === "code") || (key && key.indexOf("code:") === 0);
                        var targetCategory = (node && node.folder) ? key : parentKey;
                        if (!targetCategory)
                        {
                            alert("Select a category to add a code under");
                            break;
                        }
                        if (isCodeNode)
                        {
                            var siblingCash = (key.indexOf("code:") === 0) ? key.substring(5) : (node.data && node.data.cashCode) || "";
                            openAction("CreateCashCode", targetCategory, null, { siblingCashCode: siblingCash });
                        }
                        else
                        {
                            openAction("CreateCashCode", targetCategory);
                        }
                        break;
                    }

                    case "toggleEnabled":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var makeEnabled = (node.data && node.data.isEnabled === 1) ? 0 : 1;
                        postToggleEnabled(node, makeEnabled)
                            .done(function (res)
                            {
                                if (res && (res.success === true || typeof res.success === "undefined"))
                                {
                                    var kinds2 = getNodeKinds(node);
                                    var treatAsCategory = kinds2.isCat && !kinds2.isCode && kinds2.nodeType !== "expression";
                                    setNodeEnabledInUi(node, !!makeEnabled, treatAsCategory);

                                    if (!isMobile())
                                    {
                                        loadDetails(node);
                                    }
                                    refreshTopAnchors();
                                    notify((makeEnabled ? "Enabled" : "Disabled"), "success");
                                }
                                else
                                {
                                    alert((res && res.message) || "Update failed");
                                }
                            })
                            .fail(function (xhr)
                            {
                                var msg = (xhr && xhr.status === 200) ? (xhr.responseText || "Parse error") : ("Server error (" + xhr.status + ")");
                                alert(msg);
                            });
                        break;
                    }


                    case "delete":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        var kinds2 = node ? getNodeKinds(node) : {};
                        if (kinds2.nodeType === "expression")
                        {
                            openAction("DeleteExpression", key);
                            break;
                        }
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
                        if (!node || !node.folder) { alert("Select a category"); break; }
                        var kind = (action === "setProfitRoot") ? "Profit" : "VAT";
                        var name = plainNodeTitle(node);
                        if (!confirm("Set '" + name + "' as the " + kind + " root?"))
                        {
                            break;
                        }
                        postJsonGlobal("SetPrimaryRoot", { kind: kind, categoryCode: key })
                            .done(function (res)
                            {
                                if (res && res.success)
                                {
                                    refreshTopAnchors();
                                    loadDetails(node);
                                    notify(kind + " root updated", "success");
                                }
                                else
                                {
                                    alert((res && res.message) || ("Failed to set " + kind + " root."));
                                }
                            })
                            .fail(function (xhr)
                            {
                                alert("Server error (" + xhr.status + ")");
                            });
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

                        if (kinds2.nodeType === "expression")
                        {
                            openAction("EditExpression", key);
                            break;
                        }

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

                        var kinds2 = getNodeKinds(node);
                        if (kinds2.nodeType === "expression")
                        {
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

                        openAction("Move", kinds2.key, parentKey);
                        break;
                    }

                    case "delete":
                    {
                        if (!isAdmin)
                        {
                            alert("Insufficient privileges");
                            break;
                        }
                        var kinds2 = getNodeKinds(node);
                        if (kinds2.nodeType === "expression")
                        {
                            openAction("DeleteExpression", key);
                            break;
                        }
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

                        postToggleEnabled(node, makeEnabled)
                            .done(function (res)
                            {
                                if (res && (res.success === true || typeof res.success === "undefined"))
                                {
                                    setNodeEnabledInUi(node, !!makeEnabled, !kinds.isCode);
                                    updateActionBar(node);
                                    if (!isMobile())
                                    {
                                        loadDetails(node);
                                    }
                                    refreshTopAnchors();
                                    notify((makeEnabled ? "Enabled" : "Disabled"), "success");
                                }
                                else
                                {
                                    alert((res && res.message) || "Update failed");
                                }
                            }).fail(function (xhr)
                            {
                                var msg = (xhr && xhr.status === 200) ? (xhr.responseText || "Parse error") : ("Server error (" + xhr.status + ")");
                                alert(msg);
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

        // add createTotal/createCashCode cases for desktop buttons
        function bindDetailsPaneHandlers()
        {
            var $pane = $("#detailsPane");
            if ($pane.length === 0) { return; }

            $pane.off("click.detailsActions").on("click.detailsActions", "[data-action]:not(form)", function ()
            {
                if (this.tagName === "FORM") { return; }

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

                switch (action)
                {
                    case "view":
                    {
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
                        if (kinds.isCode)
                        {
                            var raw = (typeof key === "string" && key.indexOf("code:") === 0) ? key.substring(5) : key;
                            openAction("EditCashCode", raw);
                            break;
                        }
                        if (kinds.data && kinds.data.categoryType === 1)
                        {
                            openAction("EditTotal", key);
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
                        openAction("AddCashCode", "", key);
                        break;
                    }

                    case "createTotal":
                    case "newTotal":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node.folder) { alert("Select a category"); break; }
                        openAction("CreateTotal", "", key);
                        break;
                    }

                    case "createCategory":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node.folder) { alert("Select a category"); break; }
                        openAction("CreateCategory", "", key);
                        break;
                    }

                    case "createCashCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        var targetCategory = node.folder ? key : parentKey;
                        if (!targetCategory)
                        {
                            alert("Select a parent category");
                            break;
                        }
                        if (kinds.isCode)
                        {
                            var rawSibling = (key.indexOf("code:") === 0) ? key.substring(5) : key;
                            openAction("CreateCashCode", targetCategory, null, { siblingCashCode: rawSibling });
                        }
                        else
                        {
                            openAction("CreateCashCode", targetCategory);
                        }
                        break;
                    }

                    case "toggleEnabled":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        var makeEnabled = (node.data && node.data.isEnabled === 1) ? 0 : 1;

                        postToggleEnabled(node, makeEnabled)
                            .done(function (res)
                            {
                                if (res && (res.success === true || typeof res.success === "undefined"))
                                {
                                    setNodeEnabledInUi(node, !!makeEnabled, !kinds.isCode);
                                    setToggleEnabledLabel($pane, (node.data && node.data.isEnabled === 1));
                                    loadDetails(node);
                                    notify((makeEnabled ? "Enabled" : "Disabled"), "success");
                                }
                                else
                                {
                                    alert((res && res.message) || "Update failed");
                                }
                            })
                            .fail(function (xhr)
                            {
                                var msg = (xhr && xhr.status === 200) ? (xhr.responseText || "Parse error") : ("Server error (" + xhr.status + ")");
                                alert(msg);
                            });
                        break;
                    }

                    case "delete":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        openDeleteFor(node, parentKey || "");
                        break;
                    }

                    case "move":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node.folder) { alert("Select a category"); break; }
                        openAction("Move", key, parentKey);
                        break;
                    }

                    case "setProfitRoot":
                    case "setVatRoot":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node || !node.folder) { alert("Select a category"); break; }

                        var kind = (action === "setProfitRoot") ? "Profit" : "VAT";
                        var name = plainNodeTitle(node);

                        if (!confirm("Set '" + name + "' as the " + kind + " root?"))
                        {
                            break;
                        }

                        postJsonGlobal("SetPrimaryRoot", { kind: kind, categoryCode: key })
                            .done(function (res)
                            {
                                if (res && res.success)
                                {
                                    try
                                    {
                                        refreshTopAnchors();
                                    }
                                    catch (_) {}
                                    try
                                    {
                                        loadDetails(node);
                                    }
                                    catch (_) {}
                                    notify(kind + " root updated", "success");
                                }
                                else
                                {
                                    alert((res && res.message) || ("Failed to set " + kind + " root."));
                                }
                            })
                            .fail(function (xhr)
                            {
                                alert("Server error (" + xhr.status + ")");
                            });
                        break;
                    }
                    case "editExpression":
                    {
                        if (!isAdmin)
                        {
                            alert("Insufficient privileges");
                            break;
                        }
                        openAction("EditExpression", key);
                        break;
                    }

                    case "deleteExpression":
                    {
                        if (!isAdmin)
                        {
                            alert("Insufficient privileges");
                            break;
                        }
                        openAction("DeleteExpression", key);
                        break;
                    }

                    case "resetExpressionError":
                    {
                        var $btn = $(this);
                        var $card = $btn.closest("#expressionDetails");
                        var key = $card.length ? $card.data("key") : null;

                        if (!key)
                        {
                            notify("Expression key not found.", "warning");
                            break;
                        }

                        resetExpressionError(key, function (ok)
                        {
                            if (!ok)
                            {
                                return;
                            }

                            try
                            {
                                var tree = getTree();

                                if (!tree)
                                {
                                    return;
                                }

                                var node = tree.getNodeByKey(key);

                                if (!node)
                                {
                                    return;
                                }

                                if (isMobile())
                                {
                                    updateActionBar(node);
                                }
                                else
                                {
                                    loadDetails(node);
                                }
                            }
                            catch (_)
                            {
                                // swallow
                            }
                        });

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

            // Make the container focusable so keydown handlers fire reliably
            $(treeSel).attr("tabindex", "0");

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

                        var parent = node.getParent ? node.getParent() : null;
                        if (parent && parent.key === EXPR_ROOT_KEY)
                        {                            
                            return true;    // expression leaf reorder allowed
                        }

                        var kinds = getNodeKinds(node);
                        if (!kinds.isCat)
                        {
                            return false;   // categories only
                        } 
                        return true;
                    },

                    dragEnter: function (node, data)
                    {
                        if (isMobile()) { return false; }
                        if (!isAdmin) { return false; }

                        if (node.getParent && data.otherNode)
                        {
                            var srcParent = data.otherNode.getParent ? data.otherNode.getParent() : null;
                            if (srcParent && srcParent.key === EXPR_ROOT_KEY
                                && node.getParent && node.getParent().key === EXPR_ROOT_KEY)
                            {
                                return ["before", "after"]; // only sibling ordering
                            }
                        }
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

                        if (data.otherNode
                            && data.otherNode.getParent
                            && node.getParent
                            && data.otherNode.getParent().key === EXPR_ROOT_KEY
                            && node.getParent().key === EXPR_ROOT_KEY
                            && (data.hitMode === "before" || data.hitMode === "after"))
                        {
                            postJsonGlobal("ReorderExpression",
                                {
                                    key: data.otherNode.key,
                                    anchorKey: node.key,
                                    mode: data.hitMode
                                })
                                .done(function (res)
                                {
                                    if (res && res.success)
                                    {
                                        try
                                        {
                                            data.otherNode.moveTo(node, data.hitMode);
                                        }
                                        catch (_) {}
                                        data.otherNode.setActive(true);
                                        persistActiveKey(data.otherNode);
                                        notify("Expression order updated", "success");
                                        if (!isMobile())
                                        {
                                            loadDetails(data.otherNode);
                                            resizeColumns();
                                        }
                                    }
                                    else
                                    {
                                        alert((res && res.message) || "Reorder failed");
                                    }
                                })
                                .fail(function (xhr)
                                {
                                    alert("Server error (" + xhr.status + ")");
                                });
                            return;
                        }

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
            bindEmbedMarkerObserver();

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

                if (codeKey.indexOf("code:") !== 0)
                {
                    codeKey = "code:" + codeKey;
                }

                try
                {
                    var u = new URL(window.location.href);
                    u.searchParams.set("select", codeKey);
                    u.searchParams.set("key", codeKey);
                    u.searchParams.set("expand", parentKey);
                    history.replaceState({}, "", u.toString());
                }
                catch (_) {}

                var tree = getTree();
                if (tree && tree.getNodeByKey(codeKey))
                {
                    // Node already present: just activate without pipeline storm
                    activateNodeFinal(tree.getNodeByKey(codeKey));
                    return;
                }

                try
                {
                    querySelectionApplied = false;
                    selectionRetryCount = 0;
                    applyQuerySelection();
                }
                catch (_) {}
            }
            catch (_) {}
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
