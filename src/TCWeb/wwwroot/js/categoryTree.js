// TCWeb: Cash/CategoryTree page script
// Usage: CategoryTree.init({ treeSelector, menuSelector, nodesUrl, basePageUrl, rootKey, discKey, isAdmin, actionBarSelector });

window.CategoryTree = (function () {
    function appendQuery(url, key, value) {
        var sep = url.indexOf('?') === -1 ? '?' : '&';
        return url + sep + encodeURIComponent(key) + '=' + encodeURIComponent(value);
    }
    function nocache(url) { return appendQuery(url, "_", Date.now()); }

    function init(cfg) {
        var treeSel = cfg.treeSelector || "#categoryTree";
        var menuSel = cfg.menuSelector || "#treeContextMenu";
        var actionBarSel = cfg.actionBarSelector || "#mobileActionBar";
        var nodesUrl = cfg.nodesUrl;
        var basePageUrl = cfg.basePageUrl;
        var ROOT_KEY = cfg.rootKey;
        var DISC_KEY = cfg.discKey;
        var isAdmin = !!cfg.isAdmin;

        function handlerUrl(handlerName) {
            var sep = basePageUrl.indexOf('?') === -1 ? '?' : '&';
            return basePageUrl + sep + 'handler=' + handlerName;
        }
        function antiXsrf() {
            return document.querySelector("meta[name='request-verification-token']")?.getAttribute("content")
                || document.querySelector("input[name='__RequestVerificationToken']")?.value;
        }
        function isMobile() {
            return window.matchMedia && window.matchMedia("(max-width: 991.98px)").matches;
        }

        // Helpers to classify node
        function getNodeKinds(node) {
            var key = node ? node.key : null;
            var data = (node && node.data) || {};
            var nodeType = data.nodeType; // "category" | "code"
            var isSynthetic = !node || key === ROOT_KEY || key === DISC_KEY;
            var isCode = nodeType === "code" || (key && typeof key === "string" && key.startsWith("code:"));
            var isCat = node && node.folder && !isCode;
            var isRoot = key === ROOT_KEY;
            var isDisconnect = key === DISC_KEY;
            return { key, data, nodeType, isSynthetic, isCode, isCat, isRoot, isDisconnect };
        }

        // Allow moving leafs under Disconnected; restrict to folder-only elsewhere
        function moveNodeInUi(node, direction) {
            if (!node) return;
            var parent = node.getParent && node.getParent();
            var parentKey = parent ? parent.key : "";
            var cursor = direction === "up" ? node.getPrevSibling() : node.getNextSibling();

            if (parentKey !== DISC_KEY) {
                while (cursor && !cursor.folder) {
                    cursor = direction === "up" ? cursor.getPrevSibling() : cursor.getNextSibling();
                }
            }
            if (cursor) {
                node.moveTo(cursor, direction === "up" ? "before" : "after");
            }
        }

        // Recursively expand selected node and all descendants (handles lazy nodes)
        function expandSubtree(node) {
            function expandNode(n) {
                return new Promise(function (resolve, reject) {
                    if (!n || !n.folder) { resolve(); return; }
                    var res = n.setExpanded(true);

                    function afterExpand() {
                        var children = n.children || [];
                        var chain = Promise.resolve();
                        children.forEach(function (ch) {
                            chain = chain.then(function () { return expandNode(ch); });
                        });
                        chain.then(resolve).catch(reject);
                    }

                    if (res && typeof res.then === "function") {
                        res.then(afterExpand, reject);
                    } else if (res && typeof res.done === "function") {
                        res.done(afterExpand).fail(reject);
                    } else {
                        afterExpand();
                    }
                });
            }
            return expandNode(node);
        }

        // Recursively collapse the selected node and all descendants (no lazy-load needed)
        function collapseSubtree(node) {
            if (!node) return;
            node.visit(function (n) {
                if (n !== node && n.folder && n.expanded) {
                    n.setExpanded(false);
                }
            });
            if (node.folder && node.expanded) {
                node.setExpanded(false);
            }
        }

        // Update UI enabled state; optionally cascade to descendant categories
        // Hard guard to never apply to code nodes
        function setNodeEnabledInUi(node, enabled, cascadeCategories) {
            function isCategory(n) { return !!(n && n.folder && n.data && n.data.nodeType === "category"); }
            function apply(n) {
                if (!n) return;
                n.data = n.data || {};
                n.data.isEnabled = enabled ? 1 : 0;
                n.toggleClass("tc-disabled", !enabled);
            }

            if (!cascadeCategories || !isCategory(node)) {
                if (isCategory(node) || (node && node.data && node.data.nodeType === "code")) {
                    apply(node);
                }
                return;
            }

            apply(node);
            node.visit(function (n) {
                if (n !== node && isCategory(n)) {
                    apply(n);
                }
            });
        }

        // Remove leading/trailing and duplicate adjacent separators
        function normalizeDividers($menu) {
            var $children = $menu.children().filter(":visible");
            $menu.find(".dropdown-divider").hide();
            $children = $menu.children().filter(":visible");
            if ($children.length === 0) return;

            var prevWasItem = false;
            $menu.children().each(function () {
                var $el = $(this);
                if (!$el.is(":visible")) return;

                if ($el.hasClass("dropdown-item")) { prevWasItem = true; return; }

                if ($el.hasClass("dropdown-divider")) {
                    var $nextVisible = $el.nextAll(":visible").first();
                    var nextIsItem = $nextVisible.length && $nextVisible.hasClass("dropdown-item");
                    var show = prevWasItem && nextIsItem;
                    $el.toggle(show);
                }
            });

            var lastWasDivider = false;
            $menu.children().each(function () {
                var $el = $(this);
                if (!$el.is(":visible")) return;
                if ($el.hasClass("dropdown-divider")) {
                    if (lastWasDivider) { $el.hide(); }
                    lastWasDivider = $el.is(":visible");
                } else {
                    lastWasDivider = false;
                }
            });

            var $firstVisible = $menu.children(":visible").first();
            if ($firstVisible.hasClass("dropdown-divider")) { $firstVisible.hide(); }
            var $lastVisible = $menu.children(":visible").last();
            if ($lastVisible.hasClass("dropdown-divider")) { $lastVisible.hide(); }
        }

        // Show mobile action bar
        function updateActionBar(node) {
            var bar = document.querySelector(actionBarSel);
            if (!bar) return;

            // Desktop: never show
            if (!isMobile()) { bar.classList.remove("tc-visible"); return; }

            var kinds = getNodeKinds(node);
            var key = kinds.key, data = kinds.data, isSynthetic = kinds.isSynthetic, isCode = kinds.isCode, isCat = kinds.isCat;

            // Hide for synthetic (root/disconnected) or no node
            if (!node || isSynthetic) { bar.classList.remove("tc-visible"); return; }

            // Toggle admin-only visibility
            bar.querySelectorAll(".admin-only").forEach(function (el) {
                el.style.display = isAdmin ? "" : "none";
            });

            // Button visibility per type (keep it simple: primary actions only)
            // View: always for both
            setBarButtonVisible("view", true);
            // Edit/Delete: admin only; both types
            setBarButtonVisible("edit", isAdmin);
            setBarButtonVisible("delete", isAdmin);
            // Move: admin; both types (codes may be moved to totals)
            setBarButtonVisible("move", isAdmin);
            // Toggle: admin; both types; label reflects current state
            if (isAdmin && typeof data.isEnabled !== "undefined") {
                var toggleBtn = bar.querySelector("[data-action='toggleEnabled']");
                toggleBtn.style.display = "";
                toggleBtn.querySelector("span")?.remove(); // safety: in case prior span
                var currentLabel = (data.isEnabled === 1) ? "Disable" : "Enable";
                toggleBtn.innerHTML = "<i class='bi bi-power'></i> " + currentLabel;
            } else {
                setBarButtonVisible("toggleEnabled", false);
            }

            bar.classList.add("tc-visible");

            function setBarButtonVisible(action, visible) {
                var btn = bar.querySelector("[data-action='" + action + "']");
                if (!btn) return;
                btn.style.display = visible ? "" : "none";
            }
        }
        function hideActionBar() {
            var bar = document.querySelector(actionBarSel);
            if (bar) bar.classList.remove("tc-visible");
        }

        function showContextMenu(x, y, node) {
            var $menu = $(menuSel);
            $menu.find(".admin-only").toggle(!!isAdmin);

            var kinds = getNodeKinds(node);
            var key = kinds.key, data = kinds.data, isSynthetic = kinds.isSynthetic, isCode = kinds.isCode, isCat = kinds.isCat, isRoot = kinds.isRoot, isDisconnect = kinds.isDisconnect;

            // Reset menu visibility; we'll toggle per rules
            $menu.find(".dropdown-item").show();
            $menu.find(".admin-only").toggle(!!isAdmin);

            // View: mobile-only for non-synthetic nodes
            $menu.find("[data-action='view']").toggle(!isSynthetic && isMobile());

            // Category actions
            $menu.find(".cat-only").toggle(isCat && !isRoot && !isDisconnect);
            // Root allows only createTotal
            $menu.find("[data-action='createTotal']").toggle(isAdmin && ((isCat && !isDisconnect && !isRoot) || isRoot));

            // Code actions
            $menu.find(".code-only").toggle(isCode && !isDisconnect);

            // Move Up/Down: hide for codes; allowed for categories except root/disconnect
            $menu.find("[data-action='moveUp'], [data-action='moveDown']").toggle(isAdmin && isCat && !isRoot && !isDisconnect);

            // Toggle Enabled (single item)
            var canToggle = !!isAdmin && !!node && !isSynthetic && typeof data.isEnabled !== "undefined";
            var $toggle = $menu.find("[data-action='toggleEnabled']");
            if (canToggle) {
                var label = (data.isEnabled === 1) ? "Disable" : "Enable";
                $toggle.text(label).show();
            } else {
                $toggle.hide();
            }

            // Clean up dividers
            normalizeDividers($menu);

            // Hide action bar while bottom-sheet is open (mobile)
            if (isMobile()) hideActionBar();

            // Positioning: bottom-sheet on mobile, absolute near pointer on desktop
            if (isMobile()) {
                $menu.addClass("mobile-sheet").css({ top: "", left: "" }).show();
            } else {
                $menu.removeClass("mobile-sheet").css({ top: y + "px", left: x + "px" }).show();
            }

            $menu.data("nodeKey", key).data("parentKey", kinds.isSynthetic ? "" : (node && node.getParent ? (node.getParent()?.key || "") : ""));

            setTimeout(function () {
                $(document).one("click.treeCtx", function () {
                    $menu.hide().data("nodeKey", null).data("parentKey", null);
                    // Restore action bar after sheet closes
                    if (isMobile() && node && !isSynthetic) updateActionBar(node);
                });
            }, 10);
        }

        function bindContextMenuHandlers() {
            var $menu = $(menuSel);
            $menu.on("click", "[data-action]", function () {
                var action = $(this).data("action");
                var key = $menu.data("nodeKey");
                var parentKey = $menu.data("parentKey");
                var node = key ? $(treeSel).fancytree("getTree").getNodeByKey(key) : null;
                var token = antiXsrf();

                $menu.hide();

                function callStub(handler, extra) {
                    $.ajax({
                        type: "POST",
                        url: handlerUrl(handler),
                        data: Object.assign({ key: key, parentKey: parentKey }, extra || {}),
                        headers: token ? { "RequestVerificationToken": token } : {},
                        dataType: "json"
                    }).done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                      .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                }

                switch (action) {
                    case "moveUp":
                    case "moveDown": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }
                        if (key.startsWith("code:")) { alert("Cannot reorder code nodes"); break; }

                        var handler = action === "moveUp" ? "MoveUp" : "MoveDown";
                        $.ajax({
                            type: "POST",
                            url: handlerUrl(handler),
                            data: { key: key, parentKey: parentKey },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res) {
                            if (res && res.success) {
                                moveNodeInUi(node, action === "moveUp" ? "up" : "down");
                            } else {
                                alert((res && res.message) || (handler + " failed"));
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "expandSelected": {
                        if (!node) { alert("No node selected"); break; }
                        expandSubtree(node).catch(function (err) { console && console.error && console.error("Expand failed:", err); });
                        break;
                    }
                    case "collapseSelected": {
                        if (!node) { alert("No node selected"); break; }
                        collapseSubtree(node);
                        break;
                    }

                    case "toggleEnabled": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }
                        var makeEnabled = (node.data && node.data.isEnabled === 1) ? 0 : 1;
                        $.ajax({
                            type: "POST",
                            url: handlerUrl("SetEnabled"),
                            data: { key: key, enabled: makeEnabled },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res) {
                            if (res && res.success) {
                                var kinds = getNodeKinds(node);
                                setNodeEnabledInUi(node, !!makeEnabled, !kinds.isCode /* cascade categories only */);
                                // Reflect in mobile action bar
                                if (isMobile()) updateActionBar(node);
                            } else {
                                alert((res && res.message) || "Update failed");
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "view": {
                        if (!node) { alert("Select a node first"); break; }
                        callStub("View");
                        break;
                    }

                    // Category actions
                    case "addExistingTotal": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node || !node.folder) { alert("Select a category"); break; }
                        callStub("AddExistingTotal", {});
                        break;
                    }
                    case "createTotal": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        callStub("CreateTotal", {});
                        break;
                    }
                    case "addExistingCode": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node || !node.folder) { alert("Select a category"); break; }
                        callStub("AddExistingCode", {});
                        break;
                    }
                    case "createCode": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node || !node.folder) { alert("Select a category"); break; }
                        callStub("CreateCode", {});
                        break;
                    }
                    case "move": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }
                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Move"),
                            data: { key: key, targetParentKey: "" },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                          .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }
                    case "edit": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }
                        callStub("Edit");
                        break;
                    }
                    case "delete": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }
                        var recursive = (node.folder && node.data && node.data.nodeType === "category") ? true : false;
                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Delete"),
                            data: { key: key, recursive: recursive },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                          .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }
                }
            });
        }

        function bindActionBarHandlers() {
            var bar = document.querySelector(actionBarSel);
            if (!bar) return;
            bar.addEventListener("click", function (e) {
                var target = e.target.closest("[data-action]");
                if (!target) return;
                var action = target.getAttribute("data-action");
                var tree = $(treeSel).fancytree("getTree");
                var node = tree && tree.getActiveNode ? tree.getActiveNode() : null;
                if (!node) { alert("Select a node first"); return; }
                var kinds = getNodeKinds(node);
                var key = kinds.key;
                var parentKey = node.getParent ? (node.getParent()?.key || "") : "";
                var token = antiXsrf();

                function callStub(handler, extra) {
                    $.ajax({
                        type: "POST",
                        url: handlerUrl(handler),
                        data: Object.assign({ key: key, parentKey: parentKey }, extra || {}),
                        headers: token ? { "RequestVerificationToken": token } : {},
                        dataType: "json"
                    }).done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                      .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                }

                switch (action) {
                    case "view": {
                        callStub("View"); break;
                    }
                    case "edit": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        callStub("Edit"); break;
                    }
                    case "move": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Move"),
                            data: { key: key, targetParentKey: "" },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                          .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }
                    case "delete": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        var recursive = (node.folder && node.data && node.data.nodeType === "category");
                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Delete"),
                            data: { key: key, recursive: recursive },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                          .fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }
                    case "toggleEnabled": {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        var makeEnabled = (node.data && node.data.isEnabled === 1) ? 0 : 1;
                        $.ajax({
                            type: "POST",
                            url: handlerUrl("SetEnabled"),
                            data: { key: key, enabled: makeEnabled },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res) {
                            if (res && res.success) {
                                setNodeEnabledInUi(node, !!makeEnabled, !kinds.isCode /* categories only */);
                                updateActionBar(node); // refresh label
                            } else {
                                alert((res && res.message) || "Update failed");
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }
                }
            });
        }

        function initTree() {
            if (typeof $.ui === "undefined" || typeof $.fn.fancytree !== "function") return;

            $.ajaxSetup({ cache: false });

            $(treeSel).fancytree({
                source: { url: nocache(nodesUrl) },
                escapeTitles: false,
                minExpandLevel: 1,
                clickFolderMode: 3, // friendlier on touch: activate & expand on click
                lazyLoad: function (event, data) {
                    var node = data.node;
                    data.result = { url: nocache(appendQuery(nodesUrl, 'id', node.key)) };
                },
                init: function (event, data) {
                    var root = data.tree.getNodeByKey(ROOT_KEY);
                    if (root && !root.expanded) root.setExpanded(true);
                },
                load: function (event, data) {
                    if (!data.node) {
                        var root = data.tree.getNodeByKey(ROOT_KEY);
                        if (root && !root.expanded) root.setExpanded(true);
                    }
                },
                activate: function (event, data) {
                    // Update mobile action bar on active node change
                    updateActionBar(data.node);
                }
            });

            // Desktop: right click
            $(treeSel).on("contextmenu", ".fancytree-node", function (e) {
                if (isMobile()) return; // let touch handlers manage mobile
                e.preventDefault();
                var node = $.ui.fancytree.getNode(this);
                if (!node) return;
                node.setActive(true);
                showContextMenu(e.pageX, e.pageY, node);
            });

            // Mobile: long-press (500ms) to open menu
            (function bindLongPress() {
                var pressTimer = null;
                var startX = 0, startY = 0;

                $(treeSel).on("touchstart", ".fancytree-node", function (e) {
                    if (!isMobile()) return;
                    var node = $.ui.fancytree.getNode(this);
                    if (!node) return;
                    var touch = e.originalEvent.touches && e.originalEvent.touches[0];
                    if (!touch) return;

                    startX = touch.clientX + window.scrollX;
                    startY = touch.clientY + window.scrollY;

                    pressTimer = setTimeout(function () {
                        node.setActive(true);
                        showContextMenu(startX, startY, node);
                    }, 500);
                });

                function cancelPress() { if (pressTimer) { clearTimeout(pressTimer); pressTimer = null; } }

                $(treeSel).on("touchend touchcancel touchmove", ".fancytree-node", function () { cancelPress(); });
                $(window).on("scroll", cancelPress);
            })();

            // Allow opening menu when right-clicking the empty area (desktop)
            $(treeSel).on("contextmenu", function (e) {
                if (isMobile()) return;
                if ($(e.target).closest(".fancytree-node").length === 0) {
                    e.preventDefault();
                    showContextMenu(e.pageX, e.pageY, null);
                }
            });

            bindContextMenuHandlers();
            bindActionBarHandlers();

            // Initial state
            if (isMobile()) {
                var tree = $(treeSel).fancytree("getTree");
                updateActionBar(tree.getActiveNode());
            } else {
                hideActionBar();
            }
        }

        initTree();
    }

    return { init: init };
})();