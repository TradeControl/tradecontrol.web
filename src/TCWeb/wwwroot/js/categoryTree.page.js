// Bootstraps the CategoryTree page (kept separate from the core tree logic)
(function ()
{
    function isDesktop()
    {
        return window.matchMedia && window.matchMedia("(min-width: 992px)").matches;
    }

    function initSplitter()
    {
        if (typeof Split !== "function") { return; }
        var splitInstance = null;

        function ensureSplit()
        {
            if (!isDesktop())
            {
                if (splitInstance) { splitInstance.destroy(true, true); splitInstance = null; }
                return;
            }
            if (!splitInstance)
            {
                splitInstance = Split(["#leftCol", "#rightCol"], {
                    sizes: [60, 40],
                    minSize: [300, 300],
                    gutterSize: 8,
                    snapOffset: 0,
                    onDragEnd: function ()
                    {
                        if (window.CategoryTree && CategoryTree._resizeColumns)
                        {
                            CategoryTree._resizeColumns();
                        }
                        else
                        {
                            var ev = document.createEvent("Event"); ev.initEvent("resize", true, true); window.dispatchEvent(ev);
                        }
                    }
                });
            }
        }

        window.addEventListener("resize", ensureSplit);
        ensureSplit();
    }

    // Mobile selection helper: ensure we pick the node under the intended parent (__ROOT__ hierarchy)
    function forceSelectIfRequested()
    {
        if (isDesktop()) { return; }

        var params = new URLSearchParams(window.location.search);
        var targetRaw = params.get("select") || params.get("key");
        if (!targetRaw) { return; }
        var parentKey = params.get("parentKey") || params.get("expand") || "";

        // Normalize code key variants
        var targetVariants = [];
        if (targetRaw.indexOf("code:") === 0)
        {
            var rawOnly = targetRaw.substring(5);
            targetVariants = [targetRaw, rawOnly];
        }
        else
        {
            targetVariants = ["code:" + targetRaw, targetRaw];
        }

        var attempts = 0;
        var maxAttempts = 32;

        function nocache(url)
        {
            if (!url) { return url; }
            var sep = url.indexOf("?") === -1 ? "?" : "&";
            return url + sep + "_=" + Date.now();
        }

        function findUnderParent(parentNode)
        {
            if (!parentNode) { return null; }
            // Direct children check first (fast)
            if (parentNode.children)
            {
                for (var i = 0; i < parentNode.children.length; i++)
                {
                    var c = parentNode.children[i];
                    if (!c) { continue; }
                    for (var k = 0; k < targetVariants.length; k++)
                    {
                        if (c.key === targetVariants[k]) { return c; }
                    }
                }
            }
            // BFS limited
            var q = [];
            if (parentNode.children)
            {
                for (var j = 0; j < parentNode.children.length; j++)
                {
                    var ch = parentNode.children[j];
                    if (ch && ch.folder) { q.push(ch); }
                }
            }
            var guard = 0;
            while (q.length && guard++ < 250)
            {
                var n = q.shift();
                if (n && n.children)
                {
                    for (var x = 0; x < n.children.length; x++)
                    {
                        var c2 = n.children[x];
                        if (!c2) { continue; }
                        for (var v = 0; v < targetVariants.length; v++)
                        {
                            if (c2.key === targetVariants[v]) { return c2; }
                        }
                        if (c2.folder) { q.push(c2); }
                    }
                }
            }
            return null;
        }

        function preferRootInstance(tree)
        {
            try
            {
                var cfg = document.getElementById("categoryTreeConfig");
                var rootKey = (cfg && cfg.dataset && cfg.dataset.root) ? cfg.dataset.root : "";
                if (!rootKey) { return null; }
                var rootAnchor = tree.getNodeByKey(rootKey);
                if (!rootAnchor) { return null; }

                var best = null;
                var first = null;
                rootAnchor.visit(function (n)
                {
                    if (!n || !n.key) { return; }
                    for (var i = 0; i < targetVariants.length; i++)
                    {
                        if (n.key === targetVariants[i])
                        {
                            if (!first) { first = n; }
                            // Ensure ancestry contains the intended parentKey if provided
                            if (parentKey)
                            {
                                var a = n;
                                while (a)
                                {
                                    if (a.key === parentKey) { best = n; return false; }
                                    a = a.parent;
                                }
                            }
                            else
                            {
                                best = n; return false;
                            }
                        }
                    }
                });
                return best || first;
            }
            catch (_)
            {
                return null;
            }
        }

        function activate(node)
        {
            if (!node) { return false; }
            try
            {
                node.setActive(true);
            }
            catch (_){}
            try
            {
                node.makeVisible();
            }
            catch (_){}
            return true;
        }

        function ensureParentReload(tree, pNode, done)
        {
            if (!pNode || typeof pNode.reloadChildren !== "function") { done(); return; }
            if (!pNode.expanded && typeof pNode.setExpanded === "function")
            {
                var ex = pNode.setExpanded(true);
                var afterExpand = function ()
                {
                    var cfg = document.getElementById("categoryTreeConfig");
                    var nodesUrl = (cfg && cfg.dataset && cfg.dataset.nodesUrl) ? cfg.dataset.nodesUrl : "";
                    if (nodesUrl)
                    {
                        try
                        {
                            var r = pNode.reloadChildren({ url: nocache(nodesUrl + "?id=" + encodeURIComponent(pNode.key)) });
                            if (r && typeof r.then === "function") { r.then(done, done); return; }
                            if (r && r.done) { r.done(done).fail(done); return; }
                        }
                        catch (_){}
                    }
                    // Fallback
                    try
                    {
                        var r2 = pNode.reloadChildren();
                        if (r2 && typeof r2.then === "function") { r2.then(done, done); return; }
                        if (r2 && r2.done) { r2.done(done).fail(done); return; }
                    }
                    catch (_){}
                    setTimeout(done, 120);
                };
                if (ex && typeof ex.then === "function") { ex.then(afterExpand, afterExpand); }
                else if (ex && ex.done) { ex.done(afterExpand).fail(afterExpand); }
                else { afterExpand(); }
                return;
            }

            // Already expanded: force a nocache reload once
            var cfg2 = document.getElementById("categoryTreeConfig");
            var nodesUrl2 = (cfg2 && cfg2.dataset && cfg2.dataset.nodesUrl) ? cfg2.dataset.nodesUrl : "";
            try
            {
                var r3 = nodesUrl2
                    ? pNode.reloadChildren({ url: nocache(nodesUrl2 + "?id=" + encodeURIComponent(pNode.key)) })
                    : pNode.reloadChildren();
                if (r3 && typeof r3.then === "function") { r3.then(done, done); return; }
                if (r3 && r3.done) { r3.done(done).fail(done); return; }
            }
            catch (_){}
            setTimeout(done, 120);
        }

        function attempt()
        {
            attempts++;
            try
            {
                if (!window.$ || !window.$.ui || !window.$.ui.fancytree)
                {
                    if (attempts < maxAttempts) { setTimeout(attempt, 140); }
                    return;
                }
                var treeEl = document.querySelector("#categoryTree");
                if (!treeEl) { if (attempts < maxAttempts) { setTimeout(attempt, 140); } return; }
                var tree = $.ui.fancytree.getTree(treeEl);
                if (!tree) { if (attempts < maxAttempts) { setTimeout(attempt, 140); } return; }

                // Parent-first strategy
                if (parentKey)
                {
                    var pNode = tree.getNodeByKey(parentKey);
                    if (!pNode)
                    {
                        // Try again later; parent may not yet be loaded
                        if (attempts < maxAttempts) { setTimeout(attempt, 160); }
                        return;
                    }

                    ensureParentReload(tree, pNode, function ()
                    {
                        var found = findUnderParent(pNode);
                        if (found && activate(found))
                        {
                            attempts = maxAttempts;
                            return;
                        }

                        // If not found after reloads, fall back to root preference
                        var preferred = preferRootInstance(tree);
                        if (activate(preferred))
                        {
                            attempts = maxAttempts;
                            return;
                        }

                        // Global variants lookup (last resort)
                        for (var i = 0; i < targetVariants.length; i++)
                        {
                            var n = tree.getNodeByKey(targetVariants[i]);
                            if (activate(n))
                            {
                                attempts = maxAttempts;
                                return;
                            }
                        }

                        if (attempts < maxAttempts) { setTimeout(attempt, 170); }
                    });

                    return;
                }

                // No parentKey: prefer root-instance directly
                var preferred2 = preferRootInstance(tree);
                if (activate(preferred2))
                {
                    attempts = maxAttempts;
                    return;
                }

                // Fallback: any variant
                for (var j = 0; j < targetVariants.length; j++)
                {
                    var n2 = tree.getNodeByKey(targetVariants[j]);
                    if (activate(n2))
                    {
                        attempts = maxAttempts;
                        return;
                    }
                }

                if (attempts < maxAttempts) { setTimeout(attempt, 170); }
            }
            catch (_)
            {
                if (attempts < maxAttempts) { setTimeout(attempt, 180); }
            }
        }

        // Start after short delay to allow initial source load
        setTimeout(attempt, 260);
    }

    function initPage()
    {
        var cfgEl = document.getElementById("categoryTreeConfig");
        if (!cfgEl || !window.CategoryTree) { return; }

        CategoryTree.init({
            treeSelector: "#categoryTree",
            menuSelector: "#treeContextMenu",
            actionBarSelector: "#mobileActionBar",
            nodesUrl: cfgEl.dataset.nodesUrl,
            basePageUrl: cfgEl.dataset.baseUrl,
            detailsUrl: cfgEl.dataset.detailsUrl,
            rootKey: cfgEl.dataset.root,
            discKey: cfgEl.dataset.disc,
            isAdmin: cfgEl.dataset.isAdmin === "true"
        });

        initSplitter();
        forceSelectIfRequested();
    }

    if (document.readyState === "loading")
    {
        document.addEventListener("DOMContentLoaded", initPage);
    }
    else
    {
        initPage();
    }
})();
