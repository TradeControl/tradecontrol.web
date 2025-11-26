(function ()
{
    // Shared tree utilities and constants (lightweight, global-safe)
    window.tcTree = window.tcTree || {};

    // Debug flag
    if (typeof window.tcTree.debug === "undefined")
    {
        window.tcTree.debug = false;
    }

    // Anchors/constants shared across scripts
    window.tcTree.anchors = window.tcTree.anchors || {
        ROOT: "__ROOT__",
        DISC: "__DISCONNECTED__"
    };

    // Adaptive retry/backoff helper
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
                if (work() === true)
                {
                    return;
                }
            }
            catch (_) { }

            if (--attempts <= 0)
            {
                return;
            }
            timeoutId = setTimeout(attempt, delay);
            delay = Math.ceil(delay * factor);
        }

        attempt();

        return function cancel()
        {
            if (timeoutId)
            {
                clearTimeout(timeoutId);
                timeoutId = null;
            }
        };
    };

    // Canonical action normalization (now includes expressions)
    function normalizeActionName(name)
    {
        if (!name)
        {
            return "";
        }
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
            case "createexpression":
            case "newexpression":
                return "createExpression";
            case "editexpression":
                return "editExpression";
            case "deleteexpression":
                return "deleteExpression";
            case "viewexpression":
                return "viewExpression";
            case "view":
                return "view";
            case "cancel":
                return "cancel";
            default:
                return String(name).trim();
        }
    }

    // Expose for other scripts (expressions file reuses)
    window.tcTree.normalizeActionName = normalizeActionName;

    // Bind details pane action normalization
    function bindDetailsActionAliases()
    {
        var pane = document.getElementById("detailsPane");
        if (!pane)
        {
            return;
        }

        document.addEventListener("click", function (e)
        {
            var btn = e.target.closest("#detailsPane [data-action]");
            if (!btn)
            {
                return;
            }
            var raw = btn.getAttribute("data-action") || "";
            var norm = normalizeActionName(raw);
            if (!norm)
            {
                return;
            }
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

        document.addEventListener("submit", function (e)
        {
            var form = e.target;
            if (!form || !form.matches("#detailsPane form"))
            {
                return;
            }
            if (form.hasAttribute("data-no-auto-embed"))
            {
                return;
            }
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

    // Mobile full-page actions (unchanged except uses normalizeActionName above)
    (function mobileFullPageUnifiedActions()
    {
        if (window.__tcMobileUnifiedBound)
        {
            return;
        }

        var isMobileViewport = window.matchMedia && window.matchMedia("(max-width: 991.98px)").matches;
        var hasDesktopPane = !!document.getElementById("detailsPane");

        if (!isMobileViewport || hasDesktopPane)
        {
            return;
        }

        var hasCard = document.getElementById("categoryDetails") || document.getElementById("cashCodeDetails") || document.getElementById("expressionDetails");
        if (!hasCard)
        {
            var mo = new MutationObserver(function ()
            {
                if (document.getElementById("categoryDetails") || document.getElementById("cashCodeDetails") || document.getElementById("expressionDetails"))
                {
                    mo.disconnect();
                    mobileFullPageUnifiedActions();
                }
            });
            try
            {
                mo.observe(document.body, { childList: true, subtree: true });
            }
            catch (_) { }
            return;
        }

        window.__tcMobileUnifiedBound = true;

        function dbg()
        {
            return !!(window.tcTree && window.tcTree.debug);
        }

        function antiXsrf()
        {
            var meta = document.querySelector("meta[name='request-verification-token']");
            if (meta && meta.content)
            {
                return meta.content;
            }
            var inp = document.querySelector("input[name='__RequestVerificationToken']");
            return inp && inp.value ? inp.value : "";
        }

        var basePath = "/Cash/CategoryTree";

        function currentContext()
        {
            var el = document.getElementById("expressionDetails")
                || document.getElementById("cashCodeDetails")
                || document.getElementById("categoryDetails");

            if (!el)
            {
                return { key: "", parentKey: "", nodeType: "" };
            }
            return {
                key: el.getAttribute("data-key") || "",
                parentKey: el.getAttribute("data-parent-key") || "",
                nodeType: el.getAttribute("data-node-type") || ""
            };
        }

        function nav(pageName, key, parentKey, extras)
        {
            var parts = [];
            if (key)
            {
                parts.push("key=" + encodeURIComponent(key));
            }
            if (parentKey)
            {
                parts.push("parentKey=" + encodeURIComponent(parentKey));
            }
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
            if (dbg())
            {
                console.log("[mobile/nav]", url);
            }
            window.location.href = url;
        }

        function toggleEnabledAjax(key, parentKey)
        {
            if (!key)
            {
                alert("No key");
                return;
            }
            var badge = document.querySelector("#cashCodeDetails .badge, #categoryDetails .badge");
            var currentlyEnabled = !!(badge && /Enabled/i.test(badge.textContent || ""));
            var newEnabled = currentlyEnabled ? 0 : 1;

            var xhr = new XMLHttpRequest();
            xhr.open("POST", basePath + "?handler=SetEnabled", true);
            xhr.setRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=UTF-8");
            xhr.setRequestHeader("X-Requested-With", "XMLHttpRequest");
            var token = antiXsrf();
            if (token)
            {
                xhr.setRequestHeader("RequestVerificationToken", token);
            }

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
                    var selKey = ctx.key;
                    var expKey = ctx.parentKey || ctx.key;
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

                case "edit":
                    if (ctx.nodeType === "code")
                    {
                        nav("EditCashCode", ctx.key.replace(/^code:/, ""), ctx.parentKey);
                    }
                    else if (ctx.nodeType === "expression")
                    {
                        nav("EditExpression", ctx.key);
                    }
                    else
                    {
                        nav("EditCategory", ctx.key, ctx.parentKey);
                    }
                    return;

                case "delete":
                    if (ctx.nodeType === "code")
                    {
                        nav("DeleteCashCode", ctx.key);
                    }
                    else if (ctx.nodeType === "expression")
                    {
                        nav("DeleteExpression", ctx.key);
                    }
                    else
                    {
                        nav("DeleteCategory", ctx.key);
                    }
                    return;

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

                case "createExpression":
                    nav("CreateExpression");
                    return;

                case "editExpression":
                    nav("EditExpression", ctx.key);
                    return;

                case "deleteExpression":
                    nav("DeleteExpression", ctx.key);
                    return;

                case "viewExpression":
                    nav("Details", ctx.key, "__EXPRESSIONS__");
                    return;
            }
        }

        document.addEventListener("click", function (e)
        {
            var btn = e.target.closest("#expressionDetails [data-action], #cashCodeDetails [data-action], #categoryDetails [data-action]");
            if (!btn)
            {
                return;
            }
            var action = normalizeActionName(btn.getAttribute("data-action") || "");
            if (!action)
            {
                return;
            }
            e.preventDefault();
            e.stopPropagation();
            var ctx = currentContext();
            if (!ctx.key && action !== "cancel" && action !== "createExpression")
            {
                if (dbg())
                {
                    console.warn("[mobile/actions] Missing key for action:", action);
                }
                return;
            }
            handleAction(action, ctx);
        }, { passive: false });
    })();

    // Init
    if (window.jQuery && window.jQuery.fn)
    {
        window.jQuery(function ()
        {
            try
            {
                bindDetailsActionAliases();
            }
            catch (_) { }
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
            catch (_) { }
        });
    }

    window.tcTree.bindDetailsActionAliases = bindDetailsActionAliases;
})();
