(function ()
{
    // Shared tree utilities and constants (lightweight, global-safe)
    window.tcTree = window.tcTree || {};

    // Debug flag (can be toggled at runtime: window.tcTree.debug = true)
    if (typeof window.tcTree.debug === "undefined")
    {
        window.tcTree.debug = false;
    }

    // Anchors/constants shared across scripts
    window.tcTree.anchors = window.tcTree.anchors || {
        ROOT: "__ROOT__",
        DISC: "__DISCONNECTED__"
    };

    // Adaptive retry/backoff helper:
    // work(): should return true when finished; otherwise retry until attempts exhausted.
    // options: { attempts?: number, delayMs?: number, factor?: number }
    window.tcTree.retry = function (work, options)
    {
        var attempts = (options && options.attempts) ? options.attempts : 5;
        var delay = (options && options.delayMs) ? options.delayMs : 150;
        var factor = (options && options.factor) ? options.factor : 1.5;

        var timeoutId = null;

        function attempt()
        {
            try
            {
                var done = work();
                if (done === true)
                {
                    return;
                }
            }
            catch (e)
            {
                // swallow
            }

            attempts--;
            if (attempts <= 0)
            {
                return;
            }

            timeoutId = setTimeout(attempt, delay);
            delay = Math.ceil(delay * factor);
        }

        attempt();

        // Optional: cancel in-flight retries
        return function cancel()
        {
            if (timeoutId)
            {
                clearTimeout(timeoutId);
                timeoutId = null;
            }
        };
    };

    // ---------- New: Details actions alias normalization + embed form shim ----------

    // Map variants used by Details cards to canonical actions handled in categoryTree.js
    function normalizeActionName(name)
    {
        if (!name) { return ""; }
        var n = String(name).trim();

        // case-insensitive normalize
        var key = n.toLowerCase();

        switch (key)
        {
            case "disable":
            case "enable":
            case "toggle":
            case "toggleenabled":
                return "toggleEnabled";

            case "edit":
                return "edit";

            case "delete":
            case "remove":
                return "delete";

            case "addcategory":
            case "addexistingcategory":
                return "addExistingCategory";

            case "addcashcode":
            case "addexistingcashcode":
            case "addexistingcode":
                return "addExistingCashCode";

            case "newtotal":
            case "createtotal":
                return "createTotal";

            case "newcategory":
            case "createcategory":
                return "createCategory";

            case "newcashcode":
            case "createcashcode":
                return "createCashCode";

            case "view":
                return "view";

            case "move":
            case "moveup":
            case "movedown":
            case "setprofitroot":
            case "setvatroot":
                // Leave movement/maintenance to existing handlers
                return n; // preserve original casing for moveUp/moveDown detection upstream

            case "cancel":
                return "cancel";

            default:
                return n; // pass through anything unknown
        }
    }

    // Ensure clicks in #detailsPane use canonical action names expected by CategoryTree.bindDetailsPaneHandlers
   function bindDetailsActionAliases()
    {
        var pane = document.getElementById("detailsPane");
        if (!pane)
        {
            return;
        }

        function normalizeActionName(name)
        {
            if (!name) { return ""; }
            var key = String(name).trim().toLowerCase();
            switch (key)
            {
                case "disable":
                case "enable":
                case "toggle":
                case "toggleenabled":
                    return "toggleEnabled";
                case "edit":
                    return "edit";
                case "delete":
                case "remove":
                    return "delete";
                case "addcategory":
                case "addexistingcategory":
                    return "addExistingCategory";
                case "addcashcode":
                case "addexistingcashcode":
                case "addexistingcode":
                    return "addExistingCashCode";
                case "newtotal":
                case "createtotal":
                    return "createTotal";
                case "newcategory":
                case "createcategory":
                    return "createCategory";
                case "newcashcode":
                case "createcashcode":
                    return "createCashCode";
                case "view":
                    return "view";
                case "cancel":
                    return "cancel";
                default:
                    return String(name).trim();
            }
        }

        // If jQuery present, use existing logic (safe fall-through).
        if (window.jQuery && window.jQuery.fn)
        {
            var $doc = window.jQuery(document);

            // Remove prior handlers to avoid duplicates when script reloaded
            $doc.off("click.tcDetailsAliases")
                .on("click.tcDetailsAliases", "#detailsPane [data-action]", function (e)
                {
                    var $btn = window.jQuery(this);
                    var raw = $btn.attr("data-action") || $btn.data("action") || "";
                    var norm = normalizeActionName(raw);
                    if (!norm) { return; }

                    if (norm === "cancel")
                    {
                        e.preventDefault();
                        e.stopPropagation();
                        if (typeof window.tcCancel === "function")
                        {
                            window.tcCancel();
                        }
                        else
                        {
                            pane.innerHTML = "";
                        }
                        return;
                    }

                    if (norm !== raw)
                    {
                        $btn.attr("data-action", norm);
                        try
                        {
                            $btn.data("action", norm);
                        }
                        catch (_) {}
                    }
                });

            $doc.off("submit.tcEmbedForms")
                .on("submit.tcEmbedForms", "#detailsPane form", function (_e)
                {
                    var form = this;
                    if (form.hasAttribute("data-no-auto-embed")) { return; }
                    var hasEmbed = window.jQuery(form).find("input[name='embed'],input[name='Embed']").length > 0;
                    if (!hasEmbed)
                    {
                        var hidden = document.createElement("input");
                        hidden.type = "hidden";
                        hidden.name = "embed";
                        hidden.value = "1";
                        form.appendChild(hidden);
                    }
                });

            return;
        }

        // Vanilla JS fallback (no jQuery available).
        // Delegated click for action alias normalization + cancel.
        document.addEventListener("click", function (e)
        {
            var btn = e.target.closest("#detailsPane [data-action]");
            if (!btn) { return; }

            var raw = btn.getAttribute("data-action") || "";
            var norm = normalizeActionName(raw);
            if (!norm) { return; }

            if (norm === "cancel")
            {
                e.preventDefault();
                e.stopPropagation();
                if (typeof window.tcCancel === "function")
                {
                    window.tcCancel();
                }
                else
                {
                    pane.innerHTML = "";
                }
                return;
            }

            if (norm !== raw)
            {
                btn.setAttribute("data-action", norm);
            }
        }, { passive: false });

        // Form submit embed injection (vanilla)
        document.addEventListener("submit", function (e)
        {
            var form = e.target;
            if (!form || !form.matches("#detailsPane form")) { return; }
            if (form.hasAttribute("data-no-auto-embed")) { return; }

            var hasEmbed = form.querySelector("input[name='embed'],input[name='Embed']");
            if (!hasEmbed)
            {
                var hidden = document.createElement("input");
                hidden.type = "hidden";
                hidden.name = "embed";
                hidden.value = "1";
                form.appendChild(hidden);
            }
        }, true);
    }

    // Auto-bind on DOM ready
    if (window.jQuery && window.jQuery.fn)
    {
        window.jQuery(function ()
        {
            try
            {
                bindDetailsActionAliases();
            }
            catch (_) {}
        });
    }
    else
    {
        document.addEventListener("DOMContentLoaded", function ()
        {
            try
            {
                bindDetailsActionAliases();
            }
            catch (_) {}
        });
    }

    // Expose in case pages want to rebind after script reloads
    window.tcTree.bindDetailsActionAliases = bindDetailsActionAliases;

    (function mobileFullPageUnifiedActions()
    {
        // Avoid double install
        if (window.__tcMobileUnifiedBound) { return; }

        // HARD mobile gating: run only when width < 992px AND #detailsPane is NOT visible.
        var isMobileViewport = window.matchMedia && window.matchMedia("(max-width: 991.98px)").matches;
        var hasDesktopPane = !!document.getElementById("detailsPane");

        if (!isMobileViewport || hasDesktopPane)
        {
            return; // Do NOT bind on desktop; allow embedded openAction flow to work.
        }

        // Diagnostics helper (run window.__tcDiag() in console)
        window.__tcDiag = function ()
        {
            console.log({
                width: window.innerWidth,
                matchMediaMobile: !!(window.matchMedia && window.matchMedia("(max-width: 991.98px)").matches),
                locationSearch: window.location.search,
                hasEmbedParam: window.location.search.indexOf("embed=1") >= 0,
                hasCategoryDetails: !!document.getElementById("categoryDetails"),
                hasCashCodeDetails: !!document.getElementById("cashCodeDetails"),
                alreadyBound: !!window.__tcMobileUnifiedBound
            });
        };

        // Original mobile-only conditions often failed in emulation (width slightly > breakpoint or embed param still present).
        // We relax gating: if the standalone cards are present, we bind.
        var hasCard = document.getElementById("categoryDetails") || document.getElementById("cashCodeDetails");
        if (!hasCard)
        {
            // Defer bind until cards appear (mutation observer); stop after first success.
            var mo = new MutationObserver(function (muts)
            {
                if (document.getElementById("categoryDetails") || document.getElementById("cashCodeDetails"))
                {
                    mo.disconnect();
                    mobileFullPageUnifiedActions(); // re-enter to actually bind
                }
            });
            try
            {
                mo.observe(document.body, { childList: true, subtree: true });
            }
            catch (_) {}
            return;
        }

        // Mark bound now (prevents recursion)
        window.__tcMobileUnifiedBound = true;

        function dbg()
        {
            return !!(window.tcTree && window.tcTree.debug);
        }

        function antiXsrf()
        {
            var meta = document.querySelector("meta[name='request-verification-token']");
            if (meta && meta.content) { return meta.content; }
            var inp = document.querySelector("input[name='__RequestVerificationToken']");
            return inp && inp.value ? inp.value : "";
        }

        function norm(a)
        {
            if (typeof normalizeActionName === "function") { return normalizeActionName(a); }
            return (a || "").trim();
        }

        var basePath = "/Cash/CategoryTree";

        function currentContext()
        {
            var el = document.getElementById("cashCodeDetails") || document.getElementById("categoryDetails");
            if (!el) { return { key: "", parentKey: "", nodeType: "", isTotal: false }; }
            return {
                key: el.getAttribute("data-key") || "",
                parentKey: el.getAttribute("data-parent-key") || "",
                nodeType: el.getAttribute("data-node-type") || "",
                isTotal: el.getAttribute("data-total") === "1"
            };
        }

        function nav(pageName, key, parentKey, extras)
        {
            var parts = [];
            if (key) { parts.push("key=" + encodeURIComponent(key)); }
            if (parentKey) { parts.push("parentKey=" + encodeURIComponent(parentKey)); }
            if (extras)
            {
                for (var p in extras)
                {
                    if (Object.prototype.hasOwnProperty.call(extras, p) && extras[p] != null)
                    {
                        parts.push(encodeURIComponent(p) + "=" + encodeURIComponent(extras[p]));
                    }
                }
            }
            var url = basePath + "/" + pageName + (parts.length ? "?" + parts.join("&") : "");
            if (dbg()) { console.log("[mobile/nav]", url); }
            window.location.href = url;
        }

        function toggleEnabledAjax(key, parentKey)
        {
            if (!key) { alert("No key"); return; }
            var badge = document.querySelector("#cashCodeDetails .badge, #categoryDetails .badge");
            var currentlyEnabled = !!(badge && /Enabled/i.test(badge.textContent || ""));
            var newEnabled = currentlyEnabled ? 0 : 1;

            var xhr = new XMLHttpRequest();
            xhr.open("POST", basePath + "?handler=SetEnabled", true);
            xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
            var token = antiXsrf();
            if (token) { xhr.setRequestHeader("RequestVerificationToken", token); }

            xhr.onreadystatechange = function ()
            {
                if (xhr.readyState === 4)
                {
                    if (xhr.status === 200)
                    {
                        var redirect = basePath + "/Index?select=" + encodeURIComponent(key)
                            + "&key=" + encodeURIComponent(key)
                            + "&expand=" + encodeURIComponent(parentKey || "");
                        window.location.href = redirect;
                    }
                    else
                    {
                        alert("Toggle failed (" + xhr.status + ")");
                    }
                }
            };
            xhr.send("key=" + encodeURIComponent(key) + "&enabled=" + newEnabled);
        }

        function handleAction(action, ctx)
        {
            switch (action)
            {
                case "cancel":
                {
                    // Always send current context back so tree reselects node after reload
                    var selKey = ctx.key;
                    var expKey = ctx.parentKey || (ctx.nodeType === "category" ? ctx.key : ctx.parentKey);
                    var url = basePath + "/Index";
                    if (selKey)
                    {
                        url += "?select=" + encodeURIComponent(selKey)
                            + "&key=" + encodeURIComponent(selKey)
                            + "&expand=" + encodeURIComponent(expKey || "")
                            + "&returnKey=" + encodeURIComponent(selKey);
                    }
                    window.location.href = url;
                    return;
                }

                case "edit":
                    if (ctx.nodeType === "code")
                    {
                        nav("EditCashCode", ctx.key.replace(/^code:/, ""), ctx.parentKey);
                    }
                    else
                    {
                        nav(ctx.isTotal ? "EditTotal" : "EditCategory", ctx.key, ctx.parentKey);
                    }
                    return;
                case "delete":
                {
                    var parentSel = ctx.parentKey || "";
                    var deletePage = (ctx.nodeType === "code") ? "DeleteCashCode" : "DeleteCategory";
                    var delUrl = basePath + "/" + deletePage
                        + "?key=" + encodeURIComponent(ctx.key)
                        + "&parentKey=" + encodeURIComponent(parentSel)
                        + "&returnKey=" + encodeURIComponent(parentSel)
                        + "&expand=" + encodeURIComponent(parentSel);
                    window.location.href = delUrl;
                    return;
                }
                case "toggleEnabled":
                    toggleEnabledAjax(ctx.key, ctx.parentKey);
                    return;
                case "addExistingCategory":
                    nav("AddCategory", "", ctx.key);
                    return;
                case "addExistingCashCode":
                    nav("AddCashCode", "", ctx.key);
                    return;
                case "createTotal":
                    nav("CreateTotal", "", ctx.key);
                    return;
                case "createCategory":
                    nav("CreateCategory", "", ctx.key);
                    return;
                case "createCashCode":
                    var parentForCode = ctx.nodeType === "code" ? ctx.parentKey : ctx.key;
                    nav("CreateCashCode", parentForCode, ctx.parentKey);
                    return;
            }
        }

        document.addEventListener("click", function (e)
        {
            var btn = e.target.closest && e.target.closest("#cashCodeDetails [data-action], #categoryDetails [data-action]");
            if (!btn) { return; }

            var action = norm(btn.getAttribute("data-action") || "");
            if (!action) { return; }

            e.preventDefault();
            e.stopPropagation();

            var ctx = currentContext();
            if (!ctx.key && action !== "cancel")
            {
                if (dbg()) { console.warn("[mobile/actions] Missing key for action:", action); }
                return;
            }

            if (dbg()) { console.log("[mobile/actions] dispatch", action, ctx); }
            handleAction(action, ctx);
        }, { passive: false });

        if (dbg()) { console.log("[mobile/unified actions] bound (tolerant)"); }
    })();

})();
