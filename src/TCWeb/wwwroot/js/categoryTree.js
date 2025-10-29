// TCWeb: Cash/CategoryTree page script
// Usage: CategoryTree.init({ treeSelector, menuSelector, nodesUrl, basePageUrl, rootKey, discKey, isAdmin, actionBarSelector, detailsUrl });

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
            var nodeType = data.nodeType; // "category" | "code" | "synthetic"
            // Treat synthetic (By Cash Type root/type nodes) as synthetic along with root/disconnected
            var isSynthetic = !node || key === ROOT_KEY || key === DISC_KEY || nodeType === "synthetic";
            var isCode = nodeType === "code" || (key && typeof key === "string" && key.startsWith("code:"));
            // A category is a folder that is not a code and not synthetic
            var isCat = node && node.folder && !isCode && !isSynthetic;
            var isRoot = key === ROOT_KEY;
            var isDisconnect = key === DISC_KEY;
            return { key, data, nodeType, isSynthetic, isCode, isCat, isRoot, isDisconnect };
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

            var showCreateTotal = isAdmin && !inTypeCtx && (isRoot || isDiscRoot || (isCat && !isDisconnect && !isRoot));
            $menu.find("[data-action='createTotal']").toggle(showCreateTotal);

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
                if (isDiscCategory)
                {
                    $createCode.text("New Code…").show();
                }
                else if (isCode)
                {
                    $createCode.text("New Code like this…").show();
                }
                else
                {
                    $createCode.text("New Code…");
                    $createCode.toggle(isAdmin && !inTypeCtx && isCat && !isRoot && !isDisconnect);
                }
                if (!isDiscCategory && !isCode)
                {
                    $createCode.hide();
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

        function bindContextMenuHandlers()
        {
            var $menu = $(menuSel);

            $menu.off("click.categoryActions").on("click.categoryActions", "[data-action]", function ()
            {
                var action = $(this).data("action");
                var key = $menu.data("nodeKey");
                var parentKey = $menu.data("parentKey");

                var tree = $(treeSel).fancytree("getTree");
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
                        n.reloadChildren();
                    }
                    else
                    {
                        var ex = n.setExpanded(true);
                        if (ex && typeof ex.done === "function")
                        {
                            ex.done(function () { n.reloadChildren(); });
                        }
                        else
                        {
                            n.reloadChildren();
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
                                    if (root && root.expanded) { root.reloadChildren(); }
                                    var disc = tree.getNodeByKey(DISC_KEY);
                                    if (disc && disc.expanded) { disc.reloadChildren(); }
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
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var targetParentKey = prompt("Move to parent Category Code:");
                        if (!targetParentKey) { break; }

                        var cycleDetected = false;
                        if (node && typeof node.visit === "function")
                        {
                            node.visit(function (n)
                            {
                                if (n && n.key === targetParentKey)
                                {
                                    cycleDetected = true;
                                    return false;
                                }
                                return true;
                            });
                        }
                        if (cycleDetected)
                        {
                            alert("Invalid move: the selected target is a descendant of this category.");
                            break;
                        }

                        postJson("Move", { key: key, targetParentKey: targetParentKey })
                            .done(function (res)
                            {
                                alert((res && res.message) || "Not Yet Implemented");
                                if (res && res.success)
                                {
                                    if (parentKey) { refreshNode(parentKey); }
                                    refreshNode(targetParentKey);
                                    var root = tree.getNodeByKey(ROOT_KEY);
                                    if (root && root.expanded) { root.reloadChildren(); }
                                    var disc = tree.getNodeByKey(DISC_KEY);
                                    if (disc && disc.expanded) { disc.reloadChildren(); }
                                }
                            })
                            .fail(alertFail);
                        break;
                    }

                    case "createTotal":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }

                        var isDisc = (key === DISC_KEY) || (parentKey === DISC_KEY);
                        if (isDisc && key === DISC_KEY)
                        {
                            var categoryCode = prompt("New Category Code:");
                            if (!categoryCode) { break; }
                            var categoryName = prompt("Category Name:");
                            if (!categoryName) { break; }

                            var cashTypeCodeStr = prompt("Cash Type Code (number):");
                            if (!cashTypeCodeStr) { break; }
                            var cashTypeCode = parseInt(cashTypeCodeStr, 10);
                            if (isNaN(cashTypeCode)) { alert("Invalid Cash Type Code"); break; }

                            var polStr = prompt("Cash Polarity (0=Expense, 1=Income, 2=Neutral):", "2");
                            if (!polStr && polStr !== "0") { break; }
                            var cashPolarityCode = parseInt(polStr, 10);
                            if (isNaN(cashPolarityCode) || cashPolarityCode < 0 || cashPolarityCode > 2)
                            {
                                alert("Invalid Cash Polarity"); break;
                            }

                            postJson("CreateCategory", {
                                parentKey: DISC_KEY,
                                categoryCode: categoryCode,
                                category: categoryName,
                                cashTypeCode: cashTypeCode,
                                cashPolarityCode: cashPolarityCode,
                                isEnabled: 1
                            }).done(function (res)
                            {
                                if (res && res.success)
                                {
                                    refreshNode(DISC_KEY);
                                    alert("Category created");
                                }
                                else
                                {
                                    alert((res && res.message) || "Create failed");
                                }
                            }).fail(alertFail);
                        }
                        else
                        {
                            postJson("CreateTotal", { parentKey: key || parentKey })
                                .done(function (res) { alert((res && res.message) || "Not Yet Implemented"); })
                                .fail(alertFail);
                        }
                        break;
                    }

                    case "createCode":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

                        var isCodeNode = !!(node.data && node.data.nodeType === "code") || (key && typeof key === "string" && key.indexOf("code:") === 0);
                        var discCat = isDiscCategoryNode(node);

                        var newCashCode = prompt("New Cash Code:");
                        if (!newCashCode) { break; }
                        var newDesc = prompt("Description:");
                        if (!newDesc) { break; }

                        if (discCat)
                        {
                            postJson("CreateCodeByCategory", {
                                categoryCode: key,
                                taxCode: "",
                                cashCode: newCashCode,
                                cashDescription: newDesc,
                                templateCode: ""
                            }).done(function (res)
                            {
                                alert((res && res.message) || "Not Yet Implemented");
                                if (res && res.success) { refreshNode(key); }
                            }).fail(alertFail);
                        }
                        else if (isCodeNode)
                        {
                            var siblingCashCode = key.indexOf("code:") === 0 ? key.substring("code:".length) : key;
                            postJson("CreateCodeByCashCode", {
                                siblingCashCode: siblingCashCode,
                                cashCode: newCashCode,
                                cashDescription: newDesc
                            }).done(function (res)
                            {
                                alert((res && res.message) || "Not Yet Implemented");
                            }).fail(alertFail);
                        }
                        else
                        {
                            postJson("CreateCodeByCategory", {
                                categoryCode: key,
                                taxCode: "",
                                cashCode: newCashCode,
                                cashDescription: newDesc,
                                templateCode: ""
                            }).done(function (res)
                            {
                                alert((res && res.message) || "Not Yet Implemented");
                                if (res && res.success) { refreshNode(key); }
                            }).fail(alertFail);
                        }
                        break;
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
                                    if (disc && disc.expanded) { disc.reloadChildren(); }
                                    var root = tree.getNodeByKey(ROOT_KEY);
                                    if (root && root.expanded) { root.reloadChildren(); }
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
                                if (root && root.expanded) { root.reloadChildren(); }
                                var disc = tree.getNodeByKey(DISC_KEY);
                                if (disc && disc.expanded) { disc.reloadChildren(); }
                            }
                        }).fail(alertFail);
                        break;
                    }

                    case "expandSelected":
                    {
                        if (!node) { alert("Select a node first"); break; }
                        expandSubtree(node)
                            .then(function () { resizeColumns(); })
                            .catch(function () { });
                        break;
                    }

                    case "collapseSelected":
                    {
                        if (!node) { alert("Select a node first"); break; }
                        collapseSubtree(node);
                        resizeColumns();
                        break;
                    }

                    default:
                        break;
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
                var tree = $(treeSel).fancytree("getTree");
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
                    var t = $(treeSel).fancytree("getTree");
                    if (!t) { return; }
                    var top = t.getRootNode();
                    if (!top || !top.children) { return; }
                    for (var i = 0; i < top.children.length; i++)
                    {
                        var n = top.children[i];
                        if (n && n.expanded)
                        {
                            n.reloadChildren({ url: nocache(appendQuery(nodesUrl, 'id', n.key)) });
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
                        callStub("Move");
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
                var tree = $(treeSel).fancytree("getTree");
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
                    var t = $(treeSel).fancytree("getTree");
                    if (!t) { return; }
                    var top = t.getRootNode();
                    if (!top || !top.children) { return; }
                    for (var i = 0; i < top.children.length; i++)
                    {
                        var n = top.children[i];
                        if (n && n.expanded)
                        {
                            n.reloadChildren({ url: nocache(appendQuery(nodesUrl, 'id', n.key)) });
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
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        callStub("Edit");
                        break;
                    }

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
                                if (node.expanded) { node.reloadChildren(); }
                                var disc = tree.getNodeByKey(DISC_KEY);
                                if (disc && disc.expanded) { disc.reloadChildren(); }
                                var root = tree.getNodeByKey(ROOT_KEY);
                                if (root && root.expanded) { root.reloadChildren(); }
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
                                if (tgt && tgt.expanded) { tgt.reloadChildren(); }
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "move":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node.folder) { alert("Select a category"); break; }

                        var targetParentKey = prompt("Move to parent Category Code:");
                        if (!targetParentKey) { break; }

                        var cycleDetected = false;
                        node.visit(function (n)
                        {
                            if (n && n.key === targetParentKey)
                            {
                                cycleDetected = true;
                                return false;
                            }
                            return true;
                        });
                        if (cycleDetected) { alert("Invalid move: the selected target is a descendant of this category."); break; }

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Move"),
                            data: { key: key, targetParentKey: targetParentKey },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            alert((res && res.message) || "Not Yet Implemented");
                            if (res && res.success)
                            {
                                if (parentKey) { var p = tree.getNodeByKey(parentKey); if (p && p.expanded) { p.reloadChildren(); } }
                                var tgt = tree.getNodeByKey(targetParentKey);
                                if (tgt && tgt.expanded) { tgt.reloadChildren(); }
                                var root = tree.getNodeByKey(ROOT_KEY);
                                if (root && root.expanded) { root.reloadChildren(); }
                                var disc = tree.getNodeByKey(DISC_KEY);
                                if (disc && disc.expanded) { disc.reloadChildren(); }
                                loadDetails(tree.getActiveNode());
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
                        break;
                    }

                    case "toggleEnabled":
                    {
                        if (!isAdmin) { alert("Insufficient privileges"); break; }
                        if (!node) { alert("Select a node first"); break; }

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
                                if (disc && disc.expanded) { disc.reloadChildren(); }
                                var root = tree.getNodeByKey(ROOT_KEY);
                                if (root && root.expanded) { root.reloadChildren(); }
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
                                if (parentKey) { var p = tree.getNodeByKey(parentKey); if (p && p.expanded) { p.reloadChildren(); } }
                                var root = tree.getNodeByKey(ROOT_KEY);
                                if (root && root.expanded) { root.reloadChildren(); }
                                var disc = tree.getNodeByKey(DISC_KEY);
                                if (disc && disc.expanded) { disc.reloadChildren(); }
                                loadDetails(tree.getActiveNode());
                            }
                        }).fail(function (xhr) { alert("Server error (" + xhr.status + ")"); });
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
                load: function (event, data)
                {
                    if (!data.node)
                    {
                        var root = data.tree.getNodeByKey(ROOT_KEY);
                        if (root && !root.expanded) {root.setExpanded(true);}
                    }
                    ensureTreeContainerSizing();
                    resizeColumns();
                },
                activate: function (event, data)
                {
                    loadDetails(data.node);
                },
                dnd5: {
                    dragStart: function (node, data)
                    {
                        if (isMobile()) return false;
                        if (!isAdmin) return false;

                        var kinds = getNodeKinds(node);
                        if (!kinds.isCat) return false; // categories only
                        return true;
                    },
                    dragEnter: function (node, data)
                    {
                        if (isMobile()) return false;
                        if (!isAdmin) return false;

                        var src = data.otherNode;
                        if (!src) return false;

                        if (node === src || node.isDescendantOf(src)) return false;

                        var srcKinds = getNodeKinds(src);
                        var tgtKinds = getNodeKinds(node);
                        if (!srcKinds.isCat) return false;
                        if (!tgtKinds.isCat) return false;

                        var parentKeyNow = node.getParent ? (node.getParent()?.key || "") : "";
                        var inTypeCtx = !!(node.data && (node.data.isTypeContext === true || node.data.syntheticKind === "type"))
                                        || (typeof parentKeyNow === "string" && parentKeyNow.indexOf("type:") === 0);
                        if (inTypeCtx) return false;

                        return ["over"];
                    },
                    dragDrop: function (node, data)
                    {
                        if (isMobile()) return;
                        if (!isAdmin) return;
                        if (data.hitMode !== "over") return;

                        var src = data.otherNode;
                        if (!src) return;

                        if (node === src || node.isDescendantOf(src)) { alert("Invalid move: cannot move a category under itself or its descendant."); return; }

                        var token = antiXsrf();
                        var oldParent = src.getParent ? (src.getParent()?.key || "") : "";

                        $.ajax({
                            type: "POST",
                            url: handlerUrl("Move"),
                            data: { key: src.key, targetParentKey: node.key },
                            headers: token ? { "RequestVerificationToken": token } : {},
                            dataType: "json"
                        }).done(function (res)
                        {
                            if (res && res.success)
                            {
                                try { src.moveTo(node, "child"); } catch {}

                                var tree = $(treeSel).fancytree("getTree");
                                function reloadIfExpanded(k) { if (!k) return; var n = tree.getNodeByKey(k); if (n && n.expanded) { n.reloadChildren(); } }
                                reloadIfExpanded(oldParent);
                                reloadIfExpanded(node.key);

                                var root = tree.getNodeByKey(ROOT_KEY);
                                if (root && root.expanded) { root.reloadChildren(); }
                                var disc = tree.getNodeByKey(DISC_KEY);
                                if (disc && disc.expanded) { disc.reloadChildren(); }

                                if (!isMobile()) { loadDetails(src); resizeColumns(); }
                            }
                            else
                            {
                                alert((res && res.message) || "Move failed");
                            }
                        }).fail(function (xhr)
                        {
                            alert("Server error (" + xhr.status + ")");
                        });
                    },
                    dragEnd: function () { }
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

            if (!isMobile())
            {
                var tree = $(treeSel).fancytree("getTree");
                loadDetails(tree.getActiveNode());
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