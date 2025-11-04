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

        // Derive the base path from detailsUrl, e.g. ".../CategoryTree/Details" -> ".../CategoryTree"
        function actionsBasePath()
        {
            if (!detailsUrl) 
            {
                return "";
            }
            // Trim the last path segment (Details or anything else)
            return detailsUrl.replace(/\/[^\/?#]+(\?.*)?$/i, "");
        }

        // Open an action page in RHS (desktop) or navigate (mobile)
        function openAction(actionName, key, parentKey, extras)
        {
            var base = actionsBasePath();
            if (!base)
            {
                alert("Action endpoint not configured.");
                return;
            }

            // Build query parameters robustly
            var parts = [];
            parts.push("key=" + encodeURIComponent(key || ""));

            if (parentKey)
            {
                parts.push("parentKey=" + encodeURIComponent(parentKey));
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

            if (isMobile())
            {
                window.location.href = url;
            }
            else
            {
                // Mark as embedded to suppress the full layout/navigation
                url = appendQuery(url, "embed", "1");

                var $pane = $("#detailsPane");
                if ($pane.length)
                {
                    $pane.css("overflow", "auto");
                    $.get(nocache(url))
                        .done(function (html)
                        {
                            $pane.html(html);
                        })
                        .fail(function ()
                        {
                            $pane.html("<div class='text-danger small p-2'>Failed to load action.</div>");
                        });
                }
                else
                {
                    window.location.href = url;
                }
            }
        }
        // Load details into RHS pane (desktop only)
        function loadDetails(node)
        {
            if (!detailsUrl) {return;}
            if (isMobile()) {return;}

            var $pane = $("#detailsPane");
            if ($pane.length)
            {
                $pane.css("overflow", "auto");
            }

            if (!node)
            {
                $pane.empty();
                return;
            }

            var key = node.key || "";
            var parentKey = "";
            if (node && typeof node.getParent === "function")
            {
                var p = node.getParent();
                parentKey = (p && p.key) ? p.key : "";
            }

            var url = detailsUrl + "?key=" + encodeURIComponent(key);
            if (parentKey)
            {
                url += "&parentKey=" + encodeURIComponent(parentKey);
            }

            $.get(nocache(url))
                .done(function (html)
                {
                    $pane.html(html);

                    // Hide Move button outside Totals (i.e., in Type subtree or Disconnected)
                    try
                    {
                        // Use same context detection as menus
                        var kinds = getNodeKinds(node); // { isCat, isDisconnect, isRoot, ... }
                        var data = kinds.data || {};
                        var parentKeyNow = "";
                        if (node && node.getParent)
                        {
                            var pp = node.getParent();
                            parentKeyNow = (pp && pp.key) ? pp.key : "";
                        }

                        // In Cash Type view if node or its context signals 'type'
                        var inTypeCtx = !!(data && (data.isTypeContext === true || data.syntheticKind === "type"))
                                        || (typeof parentKeyNow === "string" && parentKeyNow.indexOf("type:") === 0);

                        // In Disconnected view if node is the disc root or under it
                        var isDiscRoot = (typeof kinds.key === "string" && kinds.key === DISC_KEY);
                        var isDiscCategory = !!(kinds.isCat && parentKeyNow === DISC_KEY);

                        // Show only for real categories under Totals; also honor admin
                        var showMove = !!isAdmin && !!kinds.isCat && !inTypeCtx && !isDiscRoot && !isDiscCategory;

                        // Details pane uses data-action="move" hooks
                        var $moveBtn = $pane.find("[data-action='move']");
                        if ($moveBtn.length)
                        {
                            $moveBtn.toggle(showMove);
                        }
                    }
                    catch (ex)
                    {
                        // no-op
                    }
                })
                .fail(function ()
                {
                    $pane.html("<div class='text-muted small p-2'>No details</div>");
                });
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

         function showContextMenu(x, y, node)
        {
            var $menu = $(menuSel);
            $menu.find(".admin-only").toggle(!!isAdmin);

            var kinds = getNodeKinds(node);
            var key = kinds.key, data = kinds.data, isSynthetic = kinds.isSynthetic, isCode = kinds.isCode, isCat = kinds.isCat, isRoot = kinds.isRoot, isDisconnect = kinds.isDisconnect;

            var parentKeyNow = "";
            if (node && node.getParent)
            {
                var p = node.getParent();
                parentKeyNow = (p && p.key) ? p.key : "";
            }

            var inTypeCtx = (data && (data.isTypeContext === true || data.syntheticKind === "type")) ? true : false;
            if (!inTypeCtx && typeof parentKeyNow === "string" && parentKeyNow.indexOf("type:") === 0)
            {
                inTypeCtx = true;
            }

            var isTypeKey = (typeof key === "string" && key.indexOf("type:") === 0);
            var isTypeNode = isTypeKey || (data && (data.syntheticKind === "type" || data.nodeType === "synthetic")) ? true : false;
            var isDiscRoot = (typeof key === "string" && key === DISC_KEY);
            var isDiscCategory = !!(isCat && parentKeyNow === DISC_KEY);

            if (isDiscRoot)
            {
                isSynthetic = false;
            }

            function showFirst(action)
            {
                return $menu.find("[data-action='" + action + "']").hide().first().show();
            }

            $menu.removeClass("type-ctx synthetic-ctx");
            $menu.find(".dropdown-item").show();
            $menu.find(".admin-only").toggle(!!isAdmin);

            $menu.find("[data-action='view']").toggle(!isSynthetic && isMobile());

            $menu.find(".cat-only").toggle(isCat && !isRoot && !isDisconnect && !inTypeCtx);

            // decide visibility for createTotal and createCategory
            // showCreateCategory should be shown for Disconnected root or for categories (not root)
            var showCreateCategory = isAdmin && !inTypeCtx && (isDiscRoot || (isCat && !isRoot));
            // showCreateTotal should be shown for root or for category totals, but NOT for the disconnected root.
            var showCreateTotal = isAdmin && !inTypeCtx && ((isRoot && !isDiscRoot) || (isCat && !isDisconnect && !isRoot));

            // toggle menu items
            $menu.find("[data-action='createTotal']").toggle(showCreateTotal);
            $menu.find("[data-action='createCategory']").toggle(showCreateCategory);

            // adjust labels deterministically (keep createTotal label behavior for roots/categories)
            var $createTotal = $menu.find("[data-action='createTotal']");
            if ($createTotal.length)
            {
                if (isRoot) { $createTotal.text("New Total…"); }
                else { $createTotal.text("New Total…"); }
            }

            var $createCategory = $menu.find("[data-action='createCategory']");
            if ($createCategory.length)
            {
                // label consistent for category creation
                $createCategory.text("New Category…");
            }

            $menu.find(".code-only").toggle(isCode && !isDisconnect);

            var showMove = isAdmin && isCat && !isRoot && !isDisconnect && !isDiscCategory;
            $menu.find("[data-action='moveUp'], [data-action='moveDown']").toggle(showMove);

            // Hide Expand/Collapse for Cash Codes (and any non-folder leaf)
            $menu.find("[data-action='expandSelected'], [data-action='collapseSelected']")
                 .toggle(!!(node && node.folder));

            var canToggle = !!isAdmin && !!node && !isSynthetic && typeof data.isEnabled !== "undefined";
            var $toggle = $menu.find("[data-action='toggleEnabled']");
            if (canToggle)
            {
                var label = (data.isEnabled === 1) ? "Disable" : "Enable";
                $toggle.text(label).show();
            }
            else
            {
                $toggle.hide();
            }

            if (inTypeCtx)
            {
                $menu.addClass("type-ctx");
                var blockSelectors = [
                    "[data-action='addExistingTotal']",
                    "[data-action='createTotal']",
                    "[data-action='addExistingCode']",
                    "[data-action='createCode']",
                    "[data-action='move']",
                    "[data-action='delete']"
                ];
                $menu.find(blockSelectors.join(",")).hide();
            }

            if (isRoot || isTypeNode)
            {
                $menu.addClass("synthetic-ctx");
            }

            var $createCode = $menu.find("[data-action='createCode']");
            if ($createCode.length)
            {
                // Server-provided node payload includes `data.categoryType` (numeric).
                // Allow create when categoryType === 0 (CashCode). Treat value as numeric/string defensively.
                var isCashCodeCategory = !!(data && typeof data.categoryType !== "undefined" && Number(data.categoryType) === 0);

                if (isDiscCategory)
                {
                    $createCode.text("New Cash Code…").show();
                }
                else if (isCode)
                {
                    $createCode.text("New Cash Code like this…").show();
                }
                else
                {
                    $createCode.text("New Code…");
                    // Show when admin, not in type-context, is a category, not synthetic root/disconnected,
                    // and the category's CategoryType == CashCode (0).
                    $createCode.toggle(isAdmin && !inTypeCtx && isCat && !isRoot && !isDisconnect && isCashCodeCategory);
                }
            }

            var $createTotal = $menu.find("[data-action='createTotal']");
            if ($createTotal.length)
            {
                if (isDiscRoot)
                {
                    $createTotal.text("New Category…");
                }
                else if (isRoot)
                {
                    $createTotal.text("New Total…");
                }
            }

            // Remove maintenance actions from context menu
            $menu.find("[data-action='makePrimary'], [data-action='setProfitRoot'], [data-action='setVatRoot']").hide();

            if (isDiscCategory)
            {
                $menu.find(".dropdown-item").hide();

                if (isMobile())
                {
                    showFirst("view");
                }

                if (isAdmin)
                {
                    var cc = showFirst("createCode");
                    cc.text("New Code…");

                    showFirst("edit");
                    var del = showFirst("delete");
                    del.text("Delete");

                    if (typeof data.isEnabled !== "undefined")
                    {
                        var en = showFirst("toggleEnabled");
                        en.text((data.isEnabled === 1) ? "Disable" : "Enable");
                    }
                }
            }

            if (isDiscRoot)
            {
                var hideRootSelectors = [
                    "[data-action='addExistingTotal']",
                    "[data-action='addExistingCode']",
                    "[data-action='createCode']",
                    "[data-action='move']",
                    "[data-action='moveUp']",
                    "[data-action='moveDown']",
                    "[data-action='edit']",
                    "[data-action='delete']",
                    "[data-action='toggleEnabled']"
                ];
                $menu.find(hideRootSelectors.join(",")).hide();
            }

            normalizeDividers($menu);

            if (isMobile())
            {
                hideActionBar();
            }

            if (isMobile())
            {
                $menu.addClass("mobile-sheet").css({ top: "", left: "" }).show();
            }
            else
            {
                $menu.removeClass("mobile-sheet").css({ top: y + "px", left: x + "px" }).show();
            }

            $menu.data("nodeKey", key).data("parentKey", isSynthetic ? "" : parentKeyNow);

            setTimeout(function ()
            {
                $(document).one("click.treeCtx", function ()
                {
                    $menu.hide().data("nodeKey", null).data("parentKey", null);
                    if (isMobile() && node && !isSynthetic)
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
				var keyCode = e.which || e.keyCode;

				// Non-shift navigation: Left/Right/Home/End
				if (!e.shiftKey)
				{
					var treeNav = getTree();
					var cur = treeNav && treeNav.getActiveNode ? treeNav.getActiveNode() : null;
					if (!cur) { return; }

					// Left: collapse, or go to parent if already collapsed
					if (keyCode === 37)
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
					if (keyCode === 39)
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
					if (keyCode === 36)
					{
						var first = cur.getParent && cur.getParent() && cur.getParent().getFirstChild && cur.getParent().getFirstChild();
						if (first) { first.setActive(true); persistActiveKey(first); }
						e.preventDefault();
						e.stopPropagation();
						return;
					}

					// End: last sibling
					if (keyCode === 35)
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
				var isUp = (keyCode === 38);
				var isDown = (keyCode === 40);
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
				var token = antiXsrf();

				// Cash Type view => ReorderType
				if (isTypeContainer(parent))
				{
					$.ajax({
						type: "POST",
						url: handlerUrl("ReorderType"),
						data: { key: node.key, anchorKey: anchor.key, mode: mode },
						headers: token ? { "RequestVerificationToken": token } : {},
						dataType: "json"
					}).done(function (res)
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
					$.ajax({
						type: "POST",
						url: handlerUrl("ReorderSiblings"),
						data: { parentKey: parent.key || "", key: node.key, anchorKey: anchor.key, mode: mode },
						headers: token ? { "RequestVerificationToken": token } : {},
						dataType: "json"
					}).done(function (res)
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

							var t = getTree();
							var root = t.getNodeByKey(ROOT_KEY);
							var disc = t.getNodeByKey(DISC_KEY);
							reloadIfExpandedNode(root);
						 reloadIfExpandedNode(disc);

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

                function handlerUrl(name)
                {
                    var sep = basePageUrl.indexOf('?') === -1 ? '?' : '&';
                    return basePageUrl + sep + 'handler=' + name;
                }

                function postJson(handler, data)
                {
                    return $.ajax({
                        type: "POST",
                        url: handlerUrl(handler),
                        data: data,
                        headers: token ? { "RequestVerificationToken": token } : {},
                        dataType: "json"
                    });
                }

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

                        // Expand the selected node and all descendants (lazy-safe)
                        expandSubtree(node)
                            .then(function ()
                            {
                                resizeColumns();
                            })
                            .catch(function ()
                            {
                                resizeColumns();
                            });
                        break;
                    }

                    case "collapseSelected":
                    {
                        if (!node || !node.folder)
                        {
                            alert("Select a folder");
                            break;
                        }

                        // Collapse the selected node and all descendants
                        collapseSubtree(node);
                        resizeColumns();
                        break;
                    }
                    case "view":
                    {
                        postJson("View", { key: key, parentKey: parentKey })
                            .done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                            .fail(alertFail);
                        break;
                    }

                    case "edit":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        postJson("Edit", { key: key })
                            .done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                            .fail(alertFail);
                        break;
                    }

                    case "addExistingTotal":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        var targetParent = (node && node.folder) ? key : parentKey;
                        if (!targetParent)
                        {
                            alert("Select a parent category");
                            break;
                        }

                        var childKey = prompt("Existing Category Code to add under " + targetParent + ":");
                        if (!childKey) { break; }

                        postJson("AddExistingTotal", { parentKey: targetParent, childKey: childKey })
                            .done(function (res)
                            {
                                alert((res && res.message) || "Not Yet Implemented");
                                if (res && res.success)
                                {
                                    refreshNode(targetParent);
                                    var root = tree.getNodeByKey(ROOT_KEY);
                                    var disc = tree.getNodeByKey(DISC_KEY);
                                    reloadIfExpandedNode(root);
                                    reloadIfExpandedNode(disc);
                                }
                            })
                            .fail(alertFail);
                        break;
                    }

                    case "addExistingCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        // Code must be added under a category; use current node if it's a category, else its parent
                        var targetCategory = (node && node.folder) ? key : parentKey;
                        if (!targetCategory)
                        {
                            alert("Select a category to add an existing code under");
                            break;
                        }

                        var codeKey = prompt("Existing Cash Code to add under " + targetCategory + ":");
                        if (!codeKey) { break; }

                        postJson("AddExistingCode", { parentKey: targetCategory, codeKey: codeKey })
                            .done(function (res)
                            {
                                alert((res && res.message) || "Not Yet Implemented");
                                if (res && res.success)
                                {
                                    refreshNode(targetCategory);
                                }
                            })
                            .fail(alertFail);
                        break;
                    }

                    case "move":
                    {
                        if (!isAdmin) 
                        {
                            alert("Insufficient privileges");
                            break;
                        }
                        if (!node || !node.folder) 
                        {
                            alert("Select a category");
                            break;
                        }

                        openAction("Move", key, parentKey);
                        break;
                    }

                    case "createTotal":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        // Determine parent target (use current node if folder, otherwise parentKey)
                        var targetParent = (node && node.folder) ? key : parentKey;
                        if (!targetParent)
                        {
                            alert("Select a parent category");
                            break;
                        }

                        var isDisc = (key === DISC_KEY) || (parentKey === DISC_KEY);

                        // If creating under Disconnected root, open the full CreateCategory page (embedded)
                        if (isDisc && key === DISC_KEY)
                        {
                            openAction("CreateCategory", "", DISC_KEY);
                        }
                        else
                        {
                            // Open embedded CreateTotal with parentKey set
                            openAction("CreateTotal", "", targetParent);
                        }
                        break;
                    }  

                    case "createCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var isCodeNode = !!(node.data && node.data.nodeType === "code") || (key && typeof key === "string" && key.indexOf("code:") === 0);
                        var discCat = isDiscCategoryNode(node);

                        // Determine category target: if node is a category use its key, otherwise use parentKey
                        var targetCategory = (node && node.folder) ? key : parentKey;
                        if (!targetCategory)
                        {
                            alert("Select a category to add a code under");
                            break;
                        }

                        // If the current node is a code, offer quick-create "like this" using sibling template
                        if (isCodeNode)
                        {
                            var siblingCash = (key && key.indexOf("code:") === 0) ? key.substring(5) : (node && node.data && node.data.cashCode) || "";
                            openAction("CreateCode", targetCategory, null, { siblingCashCode: siblingCash });
                            break;
                        }
                        else
                        {
                            // Default: open embedded CreateCode and pre-fill the Category via parentKey
                            openAction("CreateCode", targetCategory);
                            break;
                        }
                    }

                    case "createTotal":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        var isDisc = (key === DISC_KEY) || (parentKey === DISC_KEY);
                        if (isDisc && key === DISC_KEY)
                        {
                            // Open embedded CreateCategory when invoked from disconnected root
                            openAction("CreateCategory", "", DISC_KEY);
                        }
                        else
                        {
                            // Open embedded CreateTotal with parentKey
                            openAction("CreateTotal", "", key || parentKey);
                        }
                        break;
                    }

                    case "createCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var isCodeNode = !!(node.data && node.data.nodeType === "code") || (key && typeof key === "string" && key.indexOf("code:") === 0);

                        var targetCategory = node.folder ? key : parentKey;
                        if (!targetCategory) { alert("Select a category"); break; }

                        if (isCodeNode)
                        {
                            var siblingCash2 = (key && key.indexOf("code:") === 0) ? key.substring(5) : (node && node.data && node.data.cashCode) || "";
                            openAction("CreateCode", targetCategory, null, { siblingCashCode: siblingCash2 });
                            break;
                        }
                        else
                        {
                            // Fall back to opening embedded CreateCode
                            openAction("CreateCode", targetCategory);
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
                            alert("Cannot reorder code nodes");
                            break;
                        }

                        var handler = action === "moveUp" ? "MoveUp" : "MoveDown";
                        postJson(handler, { key: key, parentKey: parentKey })
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
                        postJson("SetEnabled", { key: key, enabled: makeEnabled })
                            .done(function (res)
                            {
                                if (res && res.success)
                                {
                                    var isCodeNode2 = (node.data && node.data.nodeType === "code") || (key && key.indexOf("code:") === 0);
                                    setNodeEnabledInUi(node, !!makeEnabled, !isCodeNode2);
                                    if (!isMobile()) { loadDetails(node); }

                                    var disc = tree.getNodeByKey(DISC_KEY);
                                    var root = tree.getNodeByKey(ROOT_KEY);
                                    reloadIfExpandedNode(disc);
                                    reloadIfExpandedNode(root);
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

                        var discCat = isDiscCategoryNode(node);
                        var recursive = !!(node && node.folder && node.data && node.data.nodeType === "category" && !discCat);

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Delete"),
                            data: { key: key, recursive: recursive },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                if (parentKey) { refreshNode(parentKey); }
                                var root = tree.getNodeByKey(ROOT_KEY);
                                var disc = tree.getNodeByKey(DISC_KEY);
                                reloadIfExpandedNode(root);
                                reloadIfExpandedNode(disc);
                            }
                        }).fail(alertFail);
                        break;
                    }

                    case "makePrimary":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!parentKey) { alert("Open from a parent context to make primary."); break; }

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("MakePrimary"),
                            data: { key: key, parentKey: parentKey },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                var p = tree.getNodeByKey(parentKey);
                                reloadIfExpandedNode(p);
                                var root = tree.getNodeByKey(ROOT_KEY);
                                reloadIfExpandedNode(root);
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

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("SetPrimaryRoot"),
                            data: { kind: kind, categoryCode: key },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                // Refresh top-level anchors and details to reflect new primary paths/badges
                                var root = tree.getNodeByKey(ROOT_KEY);
                                var disc = tree.getNodeByKey(DISC_KEY);
                                reloadIfExpandedNode(root);
                                reloadIfExpandedNode(disc);
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

                        var isDisc = (key === DISC_KEY) || (parentKey === DISC_KEY);

                        // If invoked from the Disconnected root open CreateCategory with DISC_KEY
                        if (isDisc && key === DISC_KEY)
                        {
                            openAction("CreateCategory", "", DISC_KEY);
                        }
                        else
                        {
                            // Standard case: open embedded CreateCategory with parentKey set
                            openAction("CreateCategory", "", targetParent);
                        }
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
                var token = antiXsrf();

                function handlerUrl(name)
                {
                    var sep = basePageUrl.indexOf('?') === -1 ? '?' : '&';
                    return basePageUrl + sep + 'handler=' + name;
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
                    $.ajax({
                        type: "POST",
                        url: handlerUrl(handler),
                        data: Object.assign({ key: key, parentKey: parentKey }, extra || {}),
                        headers: token ? { "RequestVerificationToken": token } : {},
                        dataType: "json"
                    }).done(function (res)
                    {
                        alert((res && res.message) || "Not Yet Implemented");
                    }).fail(function (xhr)
                    {
                        alert("Server error (" + xhr.status + ")");
                    });
                }

                switch (action)
                {
                    case "view":
                    {
                        callStub("View");
                        break;
                    }

                    case "edit":
                    {
                        if (!isAdmin)
                        {
                            alert("Insufficient privileges");
                            break;
                        }
                        callStub("Edit");
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
                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Delete"),
                            data: { key: key, recursive: (node.folder && node.data && node.data.nodeType === "category") },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                        }).fail(function (xhr)
                        {
                            alert("Server error (" + xhr.status + ")");
                        });
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

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("SetEnabled"),
                            data: { key: key, enabled: makeEnabled },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            if (res && res.success)
                            {
                                setNodeEnabledInUi(node, !!makeEnabled, !kinds.isCode);
                                updateActionBar(node);
                                if (!isMobile())
                                {
                                    loadDetails(node);
                                }
                                refreshAnchors();
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
                var token = antiXsrf();

                function handlerUrl(name)
                {
                    var sep = basePageUrl.indexOf('?') === -1 ? '?' : '&';
                    return basePageUrl + sep + 'handler=' + name;
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
                    $.ajax({
                        type: "POST",
                        url: handlerUrl(handler),
                        data: Object.assign({ key: key, parentKey: parentKey }, extra || {}),
                        headers: token ? { "RequestVerificationToken": token } : {},
                        dataType: "json"
                    }).done(function (res)
                    {
                        alert((res && res.message) || "Not Yet Implemented");
                    }).fail(function (xhr)
                    {
                        alert("Server error (" + xhr.status + ")");
                    });
                }

                switch (action)
                {
                    case "view": { callStub("View"); break; }
                    case "edit": { if (!isAdmin) { alert("Insufficient privileges"); break; } callStub("Edit"); break; }
                    case "addExistingTotal":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node.folder) { alert("Select a category"); break; }

                        var childKey = prompt("Existing Category Code to add under " + key + ":");
                        if (!childKey) { break; }

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("AddExistingTotal"),
                            data: { parentKey: key, childKey: childKey },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                reloadIfExpandedNode(node);
                                var disc = tree.getNodeByKey(DISC_KEY);
                                var root = tree.getNodeByKey(ROOT_KEY);
                                reloadIfExpandedNode(disc);
                                reloadIfExpandedNode(root);
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }
                    case "addExistingCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        var targetCategory = node.folder ? key : parentKey;
                        if (!targetCategory) { alert("Select a category"); break; }

                        var codeKey = prompt("Existing Cash Code to add under " + targetCategory + ":");
                        if (!codeKey) { break; }

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("AddExistingCode"),
                            data: { parentKey: targetCategory, codeKey: codeKey },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                var tgt = tree.getNodeByKey(targetCategory);
                                reloadIfExpandedNode(tgt);
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
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

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("SetEnabled"),
                            data: { key: key, enabled: makeEnabled },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            if (res && res.success)
                            {
                                setNodeEnabledInUi(node, !!makeEnabled, !kinds.isCode);
                                loadDetails(node);
                                var disc = tree.getNodeByKey(DISC_KEY);
                                var root = tree.getNodeByKey(ROOT_KEY);
                                reloadIfExpandedNode(disc);
                                reloadIfExpandedNode(root);
                            }
                            else
                            {
                                alert((res && res.message) || "Update failed");
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "delete":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        var discCat = false;
                        if (node && node.getParent)
                        {
                            var pp = node.getParent();
                            discCat = !!(pp && pp.key === DISC_KEY);
                        }
                        var recursive = !!(node && node.folder && node.data && node.data.nodeType === "category" && !discCat);

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Delete"),
                            data: { key: key, recursive: recursive },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                if (parentKey) { var p = tree.getNodeByKey(parentKey); reloadIfExpandedNode(p); }
                                var root = tree.getNodeByKey(ROOT_KEY);
                                var disc = tree.getNodeByKey(DISC_KEY);
                                reloadIfExpandedNode(root);
                                reloadIfExpandedNode(disc);
                                loadDetails(tree.getActiveNode());
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "makePrimary":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!parentKey) { alert("Open from a parent context to make primary."); break; }

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("MakePrimary"),
                            data: { key: key, parentKey: parentKey },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                var p = tree.getNodeByKey(parentKey);
                                reloadIfExpandedNode(p);
                                var root = tree.getNodeByKey(ROOT_KEY);
                                reloadIfExpandedNode(root);
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

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("SetPrimaryRoot"),
                            data: { kind: kind, categoryCode: key },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                // Refresh top-level anchors and details to reflect new primary paths/badges
                                var root = tree.getNodeByKey(ROOT_KEY);
                                var disc = tree.getNodeByKey(DISC_KEY);
                                reloadIfExpandedNode(root);
                                reloadIfExpandedNode(disc);
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

                        var isDisc = (key === DISC_KEY) || (parentKey === DISC_KEY);

                        // If invoked from the Disconnected root open CreateCategory with DISC_KEY
                        if (isDisc && key === DISC_KEY)
                        {
                            openAction("CreateCategory", "", DISC_KEY);
                        }
                        else
                        {
                            // Standard case: open embedded CreateCategory with parentKey set
                            openAction("CreateCategory", "", targetParent);
                        }
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
                        if (root && !root.expanded) { root.setExpanded(true); }
                    }

                    // Restore expanded state for this branch (handles subsequent lazy loads too)
                    var set = loadExpandedSet();
                    var scope = data.node ? data.node : data.tree.getRootNode();
                    restoreExpandedForNode(scope, set);

                    // Try restore active selection
                    var activeKey = loadActiveKey();
                    if (activeKey)
                    {
                        var n = data.tree.getNodeByKey(activeKey);
                        if (n) { n.setActive(true); }
                    }

                    ensureTreeContainerSizing();
                    resizeColumns();
                },
                activate: function (event, data)
                {
                    persistActiveKey(data.node);
                    loadDetails(data.node);
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
                        // If target has explicit categoryType and it's not CashTotal, disallow "over".
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
                        console.log("categoryTree.dragDrop", { nodeKey: node && node.key, hitMode: data && data.hitMode, srcKey: data && data.otherNode && data.otherNode.key });

                        if (isMobile()) { return false; }
                        if (!isAdmin) { return false; }

                        var src = data.otherNode;
                        if (!src)
                        {
                            console.log("dragDrop: no source node");
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
                                var tokenA = antiXsrf();
                                console.log("POST ReorderType", { key: src.key, anchorKey: node.key, mode: data.hitMode });

                                $.ajax({
                                    type: "POST",
                                    url: handlerUrl("ReorderType"),
                                    data: { key: src.key, anchorKey: node.key, mode: data.hitMode },
                                    headers: tokenA ? { "RequestVerificationToken": tokenA } : {},
                                    dataType: "json"
                                }).done(function (res)
                                {
                                    if (res && res.success)
                                    {
                                        try
                                        {
                                            src.moveTo(node, data.hitMode);
                                        }
                                        catch (ex)
                                        {
                                            console.warn("moveTo failed (ReorderType UI move)", ex);
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
                                        console.warn("ReorderType response:", res);
                                        alert((res && res.message) || "Reorder failed");
                                    }
                                }).fail(function (xhr)
                                {
                                    console.error("ReorderType AJAX failed", xhr);
                                    alert("Server error (" + xhr.status + ")");
                                });

                                return;
                            }

                            // Totals/Disconnected sibling reorder -> ReorderSiblings
                            if (parent && !isTypeContainer(parent))
                            {
                                var tokenB = antiXsrf();
                                console.log("POST ReorderSiblings", { parentKey: parent.key || "", key: src.key, anchorKey: node.key, mode: data.hitMode });

                                $.ajax({
                                    type: "POST",
                                    url: handlerUrl("ReorderSiblings"),
                                    data: { parentKey: parent.key || "", key: src.key, anchorKey: node.key, mode: data.hitMode },
                                    headers: tokenB ? { "RequestVerificationToken": tokenB } : {},
                                    dataType: "json"
                                }).done(function (res)
                                {
                                    if (res && res.success)
                                    {
                                        try
                                        {
                                            src.moveTo(node, data.hitMode);
                                        }
                                        catch (ex)
                                        {
                                            console.warn("moveTo failed (ReorderSiblings UI move)", ex);
                                        }

                                        reloadIfExpandedNode(parent);

                                        src.setActive(true);
                                        persistActiveKey(src);
                                        announce("Moved " + (src.title || src.key) + (data.hitMode === "before" ? " before " : " after ") + (node.title || node.key));
                                        notify("Order updated", "success");

                                        // Refresh top anchors
                                        var root = t.getNodeByKey(ROOT_KEY);
                                        var disc = t.getNodeByKey(DISC_KEY);
                                        reloadIfExpandedNode(root);
                                        reloadIfExpandedNode(disc);

                                        if (!isMobile())
                                        {
                                            loadDetails(src);
                                            resizeColumns();
                                        }
                                    }
                                    else
                                    {
                                        console.warn("ReorderSiblings response:", res);
                                        alert((res && res.message) || "Reorder failed");
                                    }
                                }).fail(function (xhr)
                                {
                                    console.error("ReorderSiblings AJAX failed", xhr);
                                    alert("Server error (" + xhr.status + ")");
                                });

                                return;
                            }
                        }

                        // Fallback: child drop (move under parent) - call Move handler when hitMode === "over"
                        if (data.hitMode !== "over") { return false; }

                        var token2 = antiXsrf();
                        var oldParent = src.getParent ? (src.getParent() && src.getParent().key) || "" : "";

                        console.log("POST Move", { key: src.key, targetParentKey: node.key });

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Move"),
                            data: { key: src.key, targetParentKey: node.key },
                            headers: token2 ? { "RequestVerificationToken": token2 } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            if (res && res.success)
                            {
                                try
                                {
                                    src.moveTo(node, "child");
                                }
                                catch (ex)
                                {
                                    console.warn("moveTo failed (Move UI move)", ex);
                                }

                                // reload relevant parents
                                reloadIfExpandedNode(oldParent);
                                reloadIfExpandedNode(node.key);

                                var root = t.getNodeByKey(ROOT_KEY);
                                var disc = t.getNodeByKey(DISC_KEY);
                                reloadIfExpandedNode(root);
                                reloadIfExpandedNode(disc);

                                src.setActive(true);
                                persistActiveKey(src);

                                if (!isMobile()) { loadDetails(src); resizeColumns(); }
                                notify("Moved", "success");
                                announce("Moved " + (src.title || src.key) + " under " + (node.title || node.key));
                            }
                            else
                            {
                                console.warn("Move response:", res);
                                alert((res && res.message) || "Move failed");
                            }
                        }).fail(function (xhr)
                        {
                            console.error("Move AJAX failed", xhr);
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

            // Empty-area context menu (desktop)
            $(treeSel).on("contextmenu", function (e)
            {
                if (isMobile()) {return;}
                if ($(e.target).closest(".fancytree-node").length === 0)
                {
                    e.preventDefault();
                    showContextMenu(e.pageX, e.pageY, null);
                }
            });

			bindContextMenuHandlers();
			bindActionBarHandlers();
			bindDetailsPaneHandlers();
			bindKeyboardHandlers();
			bindAutoscrollHandlers();

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

        // Initialize the tree after cfg is available
        initTree();
    } 

    return { init: init };
})();