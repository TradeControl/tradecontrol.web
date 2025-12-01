/* Expression root + node context menu integration (Allman style)
   - Adds menu items for expressions root (__EXPRESSIONS__)
   - Adds menu items for expression nodes (expr:<CategoryCode>)
   - Navigates to Create/Edit/Delete/Details Razor Pages (desktop/mobile agnostic)
*/
(function ()
{
    if (!window.jQuery)
    {
        return;
    }

    var EXP_ROOT = "__EXPRESSIONS__";
    var PREFIX = "expr:";

    function isExpressionNode(key)
    {
        return !!key && key.indexOf(PREFIX) === 0;
    }

    function navigate(page, params)
    {
        var base = "/Cash/CategoryTree/" + page;
        if (params && typeof params === "object")
        {
            var qs = Object.keys(params)
                .filter(function (k) { return params[k] != null && params[k] !== ""; })
                .map(function (k) { return encodeURIComponent(k) + "=" + encodeURIComponent(params[k]); })
                .join("&");
            if (qs.length)
            {
                base += "?" + qs;
            }
        }
        window.location.href = base;
    }

    function handleAction(action, key)
    {
        switch (action)
        {
            case "createExpression":
                navigate("CreateExpression");
                return;

            case "editExpression":
                navigate("EditExpression", { key: key });
                return;

            case "deleteExpression":
                navigate("DeleteExpression", { key: key });
                return;

            case "viewExpression":
                navigate("Details", { key: key, parentKey: EXP_ROOT });
                return;
        }
    }

    // Core action name normalization (extends existing without patch file)
    if (!window.tcTree)
    {
        window.tcTree = {};
    }

    var prevNormalize = window.tcTree.normalizeActionName;
    window.tcTree.normalizeActionName = function (name)
    {
        var raw = (name || "").trim().toLowerCase();
        switch (raw)
        {
            case "newexpression":
            case "createexpression":
                return "createExpression";
            case "editexpression":
                return "editExpression";
            case "deleteexpression":
                return "deleteExpression";
            case "viewexpression":
                return "viewExpression";
            default:
                return prevNormalize ? prevNormalize(name) : name;
        }
    };

    // Context menu extension
    function installContextMenuExtension()
    {
        if (!window.CategoryTree)
        {
            return;
        }

        if (typeof window.CategoryTree.extendContextMenu === "function")
        {
            window.CategoryTree.extendContextMenu(function (ctx)
            {
                var node = ctx && ctx.node;
                if (!node)
                {
                    return null;
                }

                if (node.key === EXP_ROOT)
                {
                    return [
                        { action: "createExpression", text: "New Expression" }
                    ];
                }

                if (isExpressionNode(node.key))
                {
                    return [
                        { action: "viewExpression", text: "View" },
                        { action: "editExpression", text: "Edit" },
                        { action: "deleteExpression", text: "Delete" }
                    ];
                }

                return null;
            });
        }
        else
        {
            // Fallback: intercept menu open if a generic builder exists
            // (Non-invasive; if project later provides extendContextMenu use that instead)
            var $doc = jQuery(document);
            $doc.on("tcTree.buildMenu", function (e, data)
            {
                var items = data && data.items;
                var node = data && data.node;
                if (!node || !items)
                {
                    return;
                }

                if (node.key === EXP_ROOT)
                {
                    items.push({ action: "createExpression", text: "New Expression" });
                }
                else if (isExpressionNode(node.key))
                {
                    items.push({ action: "viewExpression", text: "View" });
                    items.push({ action: "editExpression", text: "Edit" });
                    items.push({ action: "deleteExpression", text: "Delete" });
                }
            });
        }
    }

    // Global click handler for custom menu actions
    function bindActionClicks()
    {
        jQuery(document).on("click", ".tc-context-menu [data-action]", function (e)
        {
            var $item = jQuery(this);
            var action = window.tcTree.normalizeActionName($item.data("action"));
            var key = $item.closest(".tc-context-menu").data("nodeKey");
            if (!action)
            {
                return;
            }
            if (action.indexOf("Expression") >= 0)
            {
                e.preventDefault();
                e.stopPropagation();
                handleAction(action, key);
            }
        });
    }

    jQuery(function ()
    {
        installContextMenuExtension();
        bindActionClicks();
    });
})();
