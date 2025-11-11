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
            if (!pane || !cfgEl)
            {
                return;
            }
            var detailsUrl = cfgEl.dataset.detailsUrl;
            if (!detailsUrl)
            {
                return;
            }
            var url = detailsUrl + "?key=" + encodeURIComponent(key) + "&embed=1";
            fetch(nocache(url), { credentials: "same-origin" })
                .then(function (r)
                {
                    return r.text();
                })
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
            if (!tree || !key)
            {
                return;
            }
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
            if (!tree || !parentKey)
            {
                return;
            }
            var p = tree.getNodeByKey(parentKey);
            if (!p)
            {
                return;
            }

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
            if (!tree || !key)
            {
                return;
            }
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

    function isSynthetic(key)
    {
        return !key
            || key === "__DISCONNECTED__"
            || key === "__ROOT__"
            || /^root_\d+$/i.test(key)
            || (typeof key === "string" && key.indexOf("type:") === 0);
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
                    if (!btnCat)
                    {
                        return;
                    }

                    // Only enforce disabled when a checkbox exists; otherwise leave as-is (enabled).
                    if (chkCat)
                    {
                        btnCat.disabled = !(chkCat.checked === true);
                    }
                };
                if (chkCat)
                {
                    chkCat.addEventListener("change", updateCat, false);
                    updateCat();
                }
                // If no checkbox, do not disable the button here.
            }

            // Delete Cash Code
            var formCode = document.getElementById("deleteCashCodeForm");
            if (formCode)
            {
                var chkCode = formCode.querySelector("#confirmDeleteCode");
                var btnCode = document.getElementById("btnDeleteCode") || formCode.querySelector('button[type="submit"]');
                var updateCode = function ()
                {
                    if (!btnCode)
                    {
                        return;
                    }
                    if (chkCode)
                    {
                        btnCode.disabled = !(chkCode.checked === true);
                    }
                };
                if (chkCode)
                {
                    chkCode.addEventListener("change", updateCode, false);
                    updateCode();
                }
            }

            // Delete Total has no confirm checkbox by design.
        }
        catch (e)
        {
            /* swallow */
        }
    }

    function handleDeleteTotalSubmit(form, e)
    {
        e.preventDefault();
        var submitBtn = form.querySelector("button[type='submit']");
        if (submitBtn)
        {
            submitBtn.disabled = true;
        }

        var fd = new FormData(form);

        fetch(form.action, { method: "POST", body: fd, credentials: "same-origin" })
            .then(function (res)
            {
                // Prefer JSON; if HTML/plain text returned, treat HTTP 200 as success and continue.
                return res.json().catch(function ()
                {
                    return { success: res.ok, message: "non-json" };
                });
            })
            .then(function (json)
            {
                if (!json || json.success !== true)
                {
                    alert((json && json.message) || "Delete failed");
                    if (submitBtn)
                    {
                        submitBtn.disabled = false;
                    }
                    return;
                }

                var pane = document.getElementById("detailsPane");
                var parent = (fd.get("parentKey") || "").trim();
                var child = (fd.get("childKey") || "").trim();

                // Remove child from both root & disconnected duplicates if present
                removeNodeIfPresent(child);
                removeNodeIfPresent("code:" + child); // in case coded version exists

                // Reload anchors to purge duplicates
                if (typeof refreshTopAnchorsLocal === "function")
                {
                    try
                    {
                        refreshTopAnchorsLocal();
                    }
                    catch (ex) { /* swallow */ }
                }

                // Only load/activate parent if it is a real category code
                if (!isSynthetic(parent))
                {
                    loadDetailsFor(parent);
                    activateNode(parent);
                    reloadChildrenFor(parent);
                }
                else
                {
                    if (pane)
                    {
                        pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                    }
                }

                refreshActiveIfAvailable();
            })
            .catch(function ()
            {
                alert("Server error");
                if (submitBtn)
                {
                    submitBtn.disabled = false;
                }
            });
    }

    function handleDeleteCategorySubmit(form, e)
    {
        e.preventDefault();
        var submitBtn = form.querySelector("button[type='submit']");
        if (submitBtn)
        {
            submitBtn.disabled = true;
        }

        var fd = new FormData(form);

        fetch(form.action, { method: "POST", body: fd, credentials: "same-origin" })
            .then(function (res)
            {
                return res.json().catch(function ()
                {
                    return null;
                });
            })
            .then(function (json)
            {
                if (!json || json.success !== true)
                {
                    alert((json && json.message) || "Delete failed");
                    if (submitBtn)
                    {
                        submitBtn.disabled = false;
                    }
                    return;
                }

                // json.key is the deleted category
                var victimKey = (json.key || "").toString();
                var pane = document.getElementById("detailsPane");

                if (!pane)
                {
                    goIndex();
                    return;
                }

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
                        catch (e)
                        {
                            /* swallow */
                        }
                    }
                    parentKey = parent && parent.key ? parent.key : "";
                }

                if (parentKey)
                {
                    reloadChildrenFor(parentKey);
                    activateNode(parentKey);
                }
            })
            .catch(function ()
            {
                alert("Server error");
                if (submitBtn)
                {
                    submitBtn.disabled = false;
                }
            });
    }

    function handleDeleteCashCodeSubmit(form, e)
    {
        e.preventDefault();
        var submitBtn = form.querySelector("button[type='submit']");
        if (submitBtn)
        {
            submitBtn.disabled = true;
        }

        var fd = new FormData(form);

        fetch(form.action, { method: "POST", body: fd, credentials: "same-origin" })
            .then(function (res)
            {
                return res.json().catch(function ()
                {
                    return null;
                });
            })
            .then(function (json)
            {
                if (!json || json.success !== true)
                {
                    alert((json && json.message) || "Delete failed");
                    if (submitBtn)
                    {
                        submitBtn.disabled = false;
                    }
                    return;
                }

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

                if (!pane)
                {
                    goIndex();
                    return;
                }

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
            })
            .catch(function ()
            {
                alert("Server error");
                if (submitBtn)
                {
                    submitBtn.disabled = false;
                }
            });
    }

    function isDeleteTotalForm(form)
    {
        if (!form)
        {
            return false;
        }
        var id = (form.id || "").toLowerCase();
        if (id === "deletetotalform")
        {
            return true;
        }

        var action = (form.getAttribute("action") || "").toLowerCase();
        if (action.indexOf("deletetotal") >= 0)
        {
            return true;
        }
        if (action.indexOf("handler=deletetotal") >= 0)
        {
            return true;
        }

        // Heuristic: has both parentKey and childKey inputs
        try
        {
            if (form.querySelector('input[name="parentKey"]') && form.querySelector('input[name="childKey"]'))
            {
                return true;
            }
        }
        catch (e)
        {
            /* swallow */
        }

        return false;
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
                    catch (ex)
                    {
                        /* swallow */
                    }
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
        if (!form || form.tagName !== "FORM")
        {
            return;
        }

        var id = form.id || "";

        if (id === "deleteCategoryForm")
        {
            handleDeleteCategorySubmit(form, e);
            return;
        }

        if (id === "deleteCashCodeForm")
        {
            handleDeleteCashCodeSubmit(form, e);
            return;
        }

        // Broaden detection for Delete Total forms so we always intercept and prevent navigation to raw JSON.
        if (id === "deleteTotalForm" || isDeleteTotalForm(form))
        {
            handleDeleteTotalSubmit(form, e);
            return;
        }
    }, true);

    document.addEventListener("click", function (e)
    {
        var el = e.target && e.target.closest ? e.target.closest("[data-embedded-cancel]") : null;
        if (!el)
        {
            return;
        }

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
