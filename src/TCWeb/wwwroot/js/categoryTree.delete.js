(function ()
{
    // Helpers
    function cfg()
    {
        var el = document.getElementById("categoryTreeConfig");
        return el || null;
    }

    function getTree()
    {
        try
        {
            if (!window.$ || !window.$.ui || !window.$.ui.fancytree) { return null; }
            var el = document.querySelector("#categoryTree");
            if (!el) { return null; }
            return $.ui.fancytree.getTree(el);
        }
        catch (e)
        {
            return null;
        }
    }

    function appendQuery(url, key, value)
    {
        var sep = url.indexOf("?") === -1 ? "?" : "&";
        return url + sep + encodeURIComponent(key) + "=" + encodeURIComponent(value);
    }

    function nocache(url)
    {
        try
        {
            return appendQuery(url, "_", Date.now());
        }
        catch (e)
        {
            return url;
        }
    }

    function loadDetailsFor(key)
    {
        try
        {
            var pane = document.getElementById("detailsPane");
            var cfgEl = cfg();
            if (!pane || !cfgEl) { return; }
            var detailsUrl = cfgEl.dataset.detailsUrl;
            if (!detailsUrl) { return; }
            var url = detailsUrl + "?key=" + encodeURIComponent(key) + "&embed=1";
            fetch(nocache(url), { credentials: "same-origin" })
                .then(function (r) { return r.text(); })
                .then(function (html)
                {
                    pane.innerHTML = html;
                })
                .catch(function ()
                {
                    pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                });
        }
        catch (e)
        {
            /* swallow */
        }
    }

    function activateNode(key)
    {
        try
        {
            var tree = getTree();
            if (!tree || !key) { return; }
            var n = tree.getNodeByKey(key);
            if (!n) { return; }
            try
            {
                n.makeVisible();
            }
            catch (e) { /* swallow */ }
            try
            {
                n.setActive(true);
            }
            catch (e) { /* swallow */ }
        }
        catch (e)
        {
            /* swallow */
        }
    }

    function reloadChildrenFor(parentKey)
    {
        try
        {
            var tree = getTree();
            if (!tree || !parentKey) { return; }
            var p = tree.getNodeByKey(parentKey);
            if (!p) { return; }

            var cfgEl = cfg();
            var nodesUrl = cfgEl ? cfgEl.dataset.nodesUrl : null;

            if (typeof p.reloadChildren === "function")
            {
                if (nodesUrl)
                {
                    var url = nocache(appendQuery(nodesUrl, "id", p.key));
                    p.reloadChildren({ url: url });
                }
                else
                {
                    p.reloadChildren();
                }
            }
        }
        catch (e)
        {
            /* swallow */
        }
    }

    function removeNodeIfPresent(key)
    {
        try
        {
            var tree = getTree();
            if (!tree || !key) { return; }
            var n = tree.getNodeByKey(key) || tree.getNodeByKey("code:" + key);
            if (n)
            {
                try
                {
                    n.remove();
                }
                catch (e)
                {
                    /* swallow */
                }
            }
        }
        catch (e)
        {
            /* swallow */
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
            /* swallow */
        }
    }

    function refreshActiveIfAvailable()
    {
        try
        {
            if (typeof window.tcRefreshActiveNode === "function")
            {
                window.tcRefreshActiveNode();
            }
        }
        catch (e)
        {
            /* swallow */
        }
    }

    // Enable/disable Delete buttons when a confirm checkbox exists on page
    function bindConfirmToggles()
    {
        try
        {
            // Delete Category
            var formCat = document.getElementById("deleteCategoryForm");
            if (formCat)
            {
                var chkCat = formCat.querySelector("#confirmDelete");
                var btnCat = document.getElementById("btnDelete") || formCat.querySelector('button[type="submit"]');
                var updateCat = function ()
                {
                    if (btnCat)
                    {
                        btnCat.disabled = !(chkCat && chkCat.checked === true);
                    }
                };
                if (chkCat)
                {
                    chkCat.addEventListener("change", updateCat, false);
                }
                updateCat();
            }

            // Delete Cash Code
            var formCode = document.getElementById("deleteCashCodeForm");
            if (formCode)
            {
                var chkCode = formCode.querySelector("#confirmDeleteCode");
                var btnCode = document.getElementById("btnDeleteCode") || formCode.querySelector('button[type="submit"]');
                var updateCode = function ()
                {
                    if (btnCode)
                    {
                        btnCode.disabled = !(chkCode && chkCode.checked === true);
                    }
                };
                if (chkCode)
                {
                    chkCode.addEventListener("change", updateCode, false);
                }
                updateCode();
            }

            // Delete Total has no confirm checkbox by design.
        }
        catch (e)
        {
            /* swallow */
        }
    }

    // Submit handlers
    function handleDeleteTotalSubmit(form, e)
    {
        e.preventDefault();
        var submitBtn = form.querySelector("button[type='submit']");
        if (submitBtn) { submitBtn.disabled = true; }

        var fd = new FormData(form);

        fetch(form.action, { method: "POST", body: fd, credentials: "same-origin" })
            .then(function (res) { return res.json().catch(function () { return null; }); })
            .then(function (json)
            {
                if (!json)
                {
                    alert("Unexpected server response.");
                    if (submitBtn) { submitBtn.disabled = false; }
                    return;
                }

                if (json.success)
                {
                    var pane = document.getElementById("detailsPane");
                    var parent = fd.get("parentKey") || "";
                    var child = fd.get("childKey") || "";

                    if (!pane) { goIndex(); return; }
                    if (!parent) { pane.innerHTML = "<div class='text-muted small p-2'>No details</div>"; return; }

                    // Reload details for parent, activate it, remove child, and force reload children
                    loadDetailsFor(parent);
                    activateNode(parent);
                    removeNodeIfPresent(child);
                    reloadChildrenFor(parent);
                    refreshActiveIfAvailable();
                }
                else
                {
                    alert((json && json.message) || "Delete failed");
                    if (submitBtn) { submitBtn.disabled = false; }
                }
            })
            .catch(function ()
            {
                alert("Server error");
                if (submitBtn) { submitBtn.disabled = false; }
            });
    }

    function handleDeleteCategorySubmit(form, e)
    {
        e.preventDefault();
        var submitBtn = form.querySelector("button[type='submit']");
        if (submitBtn) { submitBtn.disabled = true; }

        var fd = new FormData(form);

        fetch(form.action, { method: "POST", body: fd, credentials: "same-origin" })
            .then(function (res) { return res.json().catch(function () { return null; }); })
            .then(function (json)
            {
                if (!json)
                {
                    alert("Unexpected server response.");
                    if (submitBtn) { submitBtn.disabled = false; }
                    return;
                }

                if (json.success)
                {
                    // json.key is the deleted category
                    var victimKey = (json.key || "").toString();
                    var pane = document.getElementById("detailsPane");

                    if (!pane) { goIndex(); return; }

                    pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";

                    // Remove victim, reload its parent, and activate parent
                    var tree = getTree();
                    var parentKey = "";
                    if (tree)
                    {
                        var victim = victimKey ? tree.getNodeByKey(victimKey) : null;
                        var parent = victim && victim.getParent ? victim.getParent() : null;
                        if (victim)
                        {
                            try
                            {
                                victim.remove();
                            }
                            catch (e) { /* swallow */ }
                        }
                        parentKey = parent && parent.key ? parent.key : "";
                    }

                    if (parentKey)
                    {
                        reloadChildrenFor(parentKey);
                        activateNode(parentKey);
                    }
                }
                else
                {
                    alert((json && json.message) || "Delete failed");
                    if (submitBtn) { submitBtn.disabled = false; }
                }
            })
            .catch(function ()
            {
                alert("Server error");
                if (submitBtn) { submitBtn.disabled = false; }
            });
    }

    function handleDeleteCashCodeSubmit(form, e)
    {
        e.preventDefault();
        var submitBtn = form.querySelector("button[type='submit']");
        if (submitBtn) { submitBtn.disabled = true; }

        var fd = new FormData(form);

        fetch(form.action, { method: "POST", body: fd, credentials: "same-origin" })
            .then(function (res) { return res.json().catch(function () { return null; }); })
            .then(function (json)
            {
                if (!json)
                {
                    alert("Unexpected server response.");
                    if (submitBtn) { submitBtn.disabled = false; }
                    return;
                }

                if (json.success)
                {
                    // Reload parent details and tree branch
                    var pane = document.getElementById("detailsPane");
                    var parentKey = "";

                    try
                    {
                        var tree = getTree();
                        var active = tree && tree.getActiveNode ? tree.getActiveNode() : null;
                        parentKey = (active && active.key) ? active.key : parentKey;
                    }
                    catch (e)
                    {
                        /* swallow */
                    }

                    if (!pane) { goIndex(); return; }

                    if (parentKey)
                    {
                        loadDetailsFor(parentKey);
                        activateNode(parentKey);
                        reloadChildrenFor(parentKey);
                    }
                    else
                    {
                        pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                    }
                }
                else
                {
                    alert((json && json.message) || "Delete failed");
                    if (submitBtn) { submitBtn.disabled = false; }
                }
            })
            .catch(function ()
            {
                alert("Server error");
                if (submitBtn) { submitBtn.disabled = false; }
            });
    }

    // Strong Cancel binding: delegated + direct + mutation observer
    function bindInlineCancel()
    {
        try
        {
            var anchors = document.querySelectorAll("[data-embedded-cancel]");
            anchors.forEach(function (a)
            {
                a.addEventListener("click", function (ev)
                {
                    ev.preventDefault();
                    try
                    {
                        if (typeof window.tcRefreshActiveNode === "function")
                        {
                            window.tcRefreshActiveNode();
                            return;
                        }
                    }
                    catch (ex) { /* swallow */ }
                    goIndex();
                }, { capture: true, once: false });
            });
        }
        catch (e)
        {
            /* swallow */
        }
    }

    // Delegated listeners (works for embedded content and for direct navigation)
    document.addEventListener("submit", function (e)
    {
        var form = e.target;
        if (!form || form.tagName !== "FORM") { return; }

        var id = form.id || "";
        if (id === "deleteTotalForm")
        {
            handleDeleteTotalSubmit(form, e);
        }
        else if (id === "deleteCategoryForm")
        {
            handleDeleteCategorySubmit(form, e);
        }
        else if (id === "deleteCashCodeForm")
        {
            handleDeleteCashCodeSubmit(form, e);
        }
    }, true);

    document.addEventListener("click", function (e)
    {
        var el = e.target && e.target.closest ? e.target.closest("[data-embedded-cancel]") : null;
        if (!el) { return; }

        e.preventDefault();

        try
        {
            if (typeof window.tcRefreshActiveNode === "function")
            {
                window.tcRefreshActiveNode();
                return;
            }
        }
        catch (ex)
        {
            /* swallow */
        }

        goIndex();
    }, true);

    // Ensure binding covers dynamically loaded embedded content
    document.addEventListener("DOMContentLoaded", function ()
    {
        bindConfirmToggles();
        bindInlineCancel();

        var pane = document.getElementById("detailsPane");
        if (pane && window.MutationObserver)
        {
            try
            {
                var mo = new MutationObserver(function (mutations)
                {
                    mutations.forEach(function (m)
                    {
                        if (m.addedNodes && m.addedNodes.length)
                        {
                            // rebind toggles and cancel for freshly loaded embedded delete pages
                            bindConfirmToggles();
                            bindInlineCancel();
                        }
                    });
                });
                mo.observe(pane, { childList: true, subtree: true });
            }
            catch (e)
            {
                /* swallow */
            }
        }
    });

    // Escape to cancel (nice-to-have)
    document.addEventListener("keydown", function (e)
    {
        try
        {
            if (e.key === "Escape" || e.key === "Esc")
            {
                var hasDeleteForm = document.getElementById("deleteCategoryForm")
                                   || document.getElementById("deleteTotalForm")
                                   || document.getElementById("deleteCashCodeForm");
                if (hasDeleteForm)
                {
                    e.preventDefault();
                    if (typeof window.tcRefreshActiveNode === "function")
                    {
                        window.tcRefreshActiveNode();
                        return;
                    }
                    goIndex();
                }
            }
        }
        catch (ex)
        {
            /* swallow */
        }
    }, true);
})();
