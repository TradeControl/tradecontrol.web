(function ()
{
    function cfg()
    {
        try
        {
            return document.getElementById("categoryTreeConfig") || null;
        }
        catch (e)
        {
            return null;
        }
    }

    function getTree()
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

    function isDisconnectedKey(k)
    {
        try
        {
            if (!k) { return true; }
            var disc = (cfg() && cfg().dataset && cfg().dataset.disc) ? String(cfg().dataset.disc) : "";
            var kc = String(k).toLowerCase();
            return kc === "" || (disc && k === disc) || kc === "disc" || kc === "disconnected" || kc === "__root__";
        }
        catch (e)
        {
            return false;
        }
    }

    function getReturnKeyFromPage()
    {
        try
        {
            // Priority 1: hidden input named ParentKey
            var parentInput = document.querySelector('input[name="ParentKey"]');
            if (parentInput && typeof parentInput.value === "string")
            {
                return parentInput.value || "";
            }

            // Priority 2: form wrapper data-parent (optional convention)
            var form = document.querySelector("form");
            if (form && form.dataset && typeof form.dataset.parent === "string")
            {
                return form.dataset.parent || "";
            }
        }
        catch (e)
        {
            // swallow
        }
        return "";
    }

    function getActiveParentFromTree()
    {
        try
        {
            var tree = getTree();
            if (!tree || !tree.getActiveNode) { return ""; }
            var active = tree.getActiveNode();
            if (!active || !active.getParent) { return ""; }
            var p = active.getParent();
            return (p && p.key) ? p.key : "";
        }
        catch (e)
        {
            return "";
        }
    }

    function activateNode(key)
    {
        try
        {
            var tree = getTree();
            if (!tree || !key) { return; }
            var n = tree.getNodeByKey(key);

            if (!n)
            {
                return;
            }

            try
            {
                n.makeVisible();
            }
            catch (e)
            {
                /* swallow */
            }

            try
            {
                n.setActive(true);
            }
            catch (e)
            {
                /* swallow */
            }

            try
            {
                var el = (typeof n.getEventTarget === "function") ? n.getEventTarget() : null;
                if (el && el.scrollIntoView)
                {
                    el.scrollIntoView({ block: "nearest", inline: "nearest" });
                }
            }
            catch (e)
            {
                // swallow
            }
        }
        catch (e)
        {
            // swallow
        }
    }

    function loadDetailsFor(key)
    {
        try
        {
            var pane = document.getElementById("detailsPane");
            var c = cfg();
            if (!pane || !c) { return false; }
            var detailsUrl = c.dataset.detailsUrl;
            if (!detailsUrl) { return false; }
            var url = detailsUrl + "?key=" + encodeURIComponent(key) + "&embed=1";
            fetch(url, { credentials: "same-origin" })
                .then(function (r)
                {
                    if (!r.ok)
                    {
                        throw new Error("bad status");
                    }
                    return r.text();
                })
                .then(function (html)
                {
                    pane.innerHTML = html;
                    activateNode(key);
                })
                .catch(function ()
                {
                    pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                });
            return true;
        }
        catch (e)
        {
            return false;
        }
    }

    function goIndex()
    {
        try
        {
            window.location.href = "/Cash/CategoryTree/Index";
        }
        catch (e)
        {
            // swallow
        }
    }


    var _tcCancelReturnKey = "";

    // Expose setter for CategoryTree to record the return target before opening an action
    window.tcSetCancelReturn = function (key)
    {
        try
        {
            _tcCancelReturnKey = (typeof key === "string") ? key : "";
        }
        catch (e)
        {
            _tcCancelReturnKey = "";
        }
    };

    window.tcCancel = function ()
    {
        try
        {
            var pane = document.getElementById("detailsPane");

            // Not embedded -> prefer history.back, else index
            if (!pane)
            {
                try
                {
                    if (window.history && window.history.length > 1)
                    {
                        window.history.back();
                        return;
                    }
                }
                catch (e)
                {
                    // swallow
                }

                try
                {
                    window.location.href = "/Cash/CategoryTree/Index";
                }
                catch (_)
                {
                    // swallow
                }
                return;
            }

            // Try host refresh, but do NOT return here; still run explicit restore below.
            try
            {
                if (typeof window.tcRefreshActiveNode === "function")
                {
                    window.tcRefreshActiveNode();
                }
            }
            catch (e)
            {
                // swallow
            }

            // Local helpers for explicit restore
            function _cfg()
            {
                try
                {
                    return document.getElementById("categoryTreeConfig") || null;
                }
                catch (e)
                {
                    return null;
                }
            }

            function _getTree()
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

            function _getActiveKey()
            {
                try
                {
                    var t = _getTree();
                    var a = t && t.getActiveNode ? t.getActiveNode() : null;
                    return (a && a.key) ? a.key : "";
                }
                catch (e)
                {
                    return "";
                }
            }

            function _getParentKeyOf(key)
            {
                try
                {
                    var t = _getTree();
                    if (!t || !key)
                    {
                        return "";
                    }

                    var n = t.getNodeByKey(key);
                    if (!n || !n.getParent)
                    {
                        return "";
                    }

                    var p = n.getParent();
                    return (p && p.key) ? p.key : "";
                }
                catch (e)
                {
                    return "";
                }
            }

            function _nocache(url)
            {
                try
                {
                    var sep = url.indexOf("?") === -1 ? "?" : "&";
                    return url + sep + "_=" + Date.now();
                }
                catch (e)
                {
                    return url;
                }
            }

            function _loadDetailsFor(key, parentKey)
            {
                try
                {
                    var c = _cfg();
                    if (!c)
                    {
                        return false;
                    }

                    var detailsUrl = c.dataset.detailsUrl;
                    if (!detailsUrl)
                    {
                        return false;
                    }

                    var url = detailsUrl + "?key=" + encodeURIComponent(key) + "&embed=1";
                    if (parentKey)
                    {
                        url += "&parentKey=" + encodeURIComponent(parentKey);
                    }

                    fetch(_nocache(url), { credentials: "same-origin" })
                        .then(function (r)
                        {
                            if (!r.ok)
                            {
                                throw new Error("bad status");
                            }
                            return r.text();
                        })
                        .then(function (html)
                        {
                            pane.innerHTML = html;

                            // Re-activate node in tree (keeps selection consistent)
                            try
                            {
                                var t = _getTree();
                                if (t)
                                {
                                    var n = t.getNodeByKey(key);
                                    if (n)
                                    {
                                        try
                                        {
                                            n.makeVisible();
                                        }
                                        catch (_) { }

                                        try
                                        {
                                            n.setActive(true);
                                        }
                                        catch (_) { }
                                    }
                                }
                            }
                            catch (_)
                            {
                            }
                        })
                        .catch(function ()
                        {
                            pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                        });

                    return true;
                }
                catch (e)
                {
                    return false;
                }
            }

            // Use recorded key if available (set by openAction), otherwise active key
            var retKey = (_tcCancelReturnKey && typeof _tcCancelReturnKey === "string") ? _tcCancelReturnKey : _getActiveKey();

            // Clear recorded key after use
            if (_tcCancelReturnKey && typeof _tcCancelReturnKey === "string")
            {
                _tcCancelReturnKey = "";
            }

            if (retKey)
            {
                var retParent = _getParentKeyOf(retKey);
                if (!_loadDetailsFor(retKey, retParent))
                {
                    pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                }
                return;
            }

            // Fallback
            pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
        }
        catch (ex)
        {
            try
            {
                window.location.href = "/Cash/CategoryTree/Index";
            }
            catch (_)
            {
                // swallow
            }
        }
    };


    // Delegated handler for anchors/buttons that declare data-embedded-cancel
    document.addEventListener("click", function (e)
    {
        try
        {
            var el = e.target && e.target.closest ? e.target.closest("[data-embedded-cancel]") : null;
            if (!el) { return; }
            e.preventDefault();
            window.tcCancel();
        }
        catch (ex)
        {
            // last resort
            goIndex();
        }
    }, true);

    // Escape to cancel when a cancel-capable page is visible
    document.addEventListener("keydown", function (e)
    {
        try
        {
            if (e.key === "Escape" || e.key === "Esc")
            {
                var pane = document.getElementById("detailsPane");
                if (!pane)
                {
                    // direct page
                    e.preventDefault();
                    window.tcCancel();
                    return;
                }

                // Embedded: only cancel if the pane currently hosts a form (create/edit/delete)
                if (pane.querySelector("form"))
                {
                    e.preventDefault();
                    window.tcCancel();
                }
            }
        }
        catch (ex)
        {
            // swallow
        }
    }, true);
})();
