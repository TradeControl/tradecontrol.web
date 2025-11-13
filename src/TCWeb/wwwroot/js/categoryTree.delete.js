(function ()
{
    // ----------- Tree helpers -----------
    function getTree()
    {
        try
        {
            if (!window.$ || !window.$.ui || !window.$.ui.fancytree)
            {
                return null;
            }

            var el = document.querySelector("#categoryTree");
            return el ? $.ui.fancytree.getTree(el) : null;
        }
        catch (_)
        {
            return null;
        }
    }

    function detailsUrlFor(key, parentKey)
    {
        var u = "/Cash/CategoryTree/Details?key=" + encodeURIComponent(key || "");
        u += "&embed=1";
        if (parentKey)
        {
            u += "&parentKey=" + encodeURIComponent(parentKey);
        }
        return u;
    }

    function loadDetails(key, parentKey)
    {
        try
        {
            var pane = document.getElementById("detailsPane");
            if (!pane || !key)
            {
                return;
            }

            fetch(detailsUrlFor(key, parentKey), { credentials: "same-origin" })
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
                    // silent
                });
        }
        catch (_)
        {
        }
    }

    // ----------- Confirm checkbox -> Delete button wiring -----------
    function bindConfirmToggle(chk)
    {
        if (!chk)
        {
            return;
        }

        var form = chk.closest("form");
        var id = chk.id || "";
        var targetButtonId = "";

        // Patterns we accept:
        // confirmDeleteCode -> btnDeleteCode
        // confirmDeleteCategory -> btnDeleteCategory
        // confirmDeleteTotal -> btnDeleteTotal
        // confirmDelete -> btnDelete
        if (/^confirmDelete(Code|Category|Total)?$/i.test(id))
        {
            targetButtonId = id.replace(/^confirm/i, "btn");
        }

        var buttons = [];
        if (targetButtonId)
        {
            var btn = document.getElementById(targetButtonId);
            if (btn)
            {
                buttons.push(btn);
            }
        }

        if (buttons.length === 0 && form)
        {
            buttons = Array.prototype.slice.call(form.querySelectorAll("button[type='submit'],input[type='submit']"));
        }

        function apply()
        {
            var enabled = !!chk.checked;
            for (var i = 0; i < buttons.length; i++)
            {
                var b = buttons[i];
                if (!b)
                {
                    continue;
                }
                try
                {
                    b.disabled = !enabled;
                }
                catch (_)
                {
                }
            }
        }

        apply();
        chk.addEventListener("change", apply);
    }

    function wireAllConfirmToggles()
    {
        // Explicit known ids plus generic pattern match
        var ids = ["confirmDeleteCode", "confirmDeleteCategory", "confirmDeleteTotal", "confirmDelete"];
        for (var i = 0; i < ids.length; i++)
        {
            var c = document.getElementById(ids[i]);
            if (c)
            {
                bindConfirmToggle(c);
            }
        }

        // Any checkbox with data-confirm attribute
        var generics = document.querySelectorAll("input[type='checkbox'][data-confirm]");
        for (var j = 0; j < generics.length; j++)
        {
            bindConfirmToggle(generics[j]);
        }
    }

    // ----------- Submission helpers -----------
    function postJson(form, payload, callback)
    {
        var token =
            (form.querySelector('input[name="__RequestVerificationToken"]') || {}).value
            || (document.querySelector('meta[name="request-verification-token"]') || {}).content
            || "";

        fetch(form.action, {
            method: "POST",
            body: payload,
            credentials: "same-origin",
            headers: token ? { "RequestVerificationToken": token } : {}
        })
            .then(function (resp)
            {
                var ct = (resp.headers.get("content-type") || "").toLowerCase();
                if (ct.indexOf("application/json") >= 0)
                {
                    return resp.json();
                }
                return resp.text().then(function (t)
                {
                    try
                    {
                        return JSON.parse(t);
                    }
                    catch (_)
                    {
                        return { success: false, message: "Unexpected response." };
                    }
                });
            })
            .then(function (data)
            {
                callback(null, data);
            })
            .catch(function (err)
            {
                callback(err);
            });
    }

    // ----------- Delete Cash Code -----------
    function handleDeleteCashCode()
    {
        var form = document.getElementById("deleteCashCodeForm");
        if (!form)
        {
            return;
        }

        form.addEventListener("submit", function (e)
        {
            e.preventDefault();
            e.stopPropagation();

            var keyInput = form.querySelector('input[name="key"]');
            var key = keyInput ? (keyInput.value || "").trim() : "";
            if (!key)
            {
                alert("Missing key.");
                return;
            }

            var fd = new FormData();
            fd.append("key", key);

            postJson(form, fd, function (err, data)
            {
                if (err || !data || data.success !== true)
                {
                    alert((data && data.message) || "Delete failed.");
                    return;
                }

                var cashCode = data.cashCode || key.replace(/^code:/, "");
                var nodeKey = (key.indexOf("code:") === 0) ? key : ("code:" + cashCode);

                var parentKey = "";
                try
                {
                    var tree = getTree();
                    if (tree)
                    {
                        var n = tree.getNodeByKey(nodeKey) || tree.getNodeByKey(cashCode);
                        var p = n && n.getParent && n.getParent();
                        parentKey = (p && p.key) ? p.key : "";
                    }
                }
                catch (_)
                {
                }

                if (!parentKey)
                {
                    var parentEl = document.getElementById("deleteCashCodeParent");
                    parentKey = parentEl ? (parentEl.value || "").trim() : "";
                }

                try
                {
                    var tree2 = getTree();
                    if (tree2)
                    {
                        var n1 = tree2.getNodeByKey(nodeKey);
                        var n2 = tree2.getNodeByKey(cashCode);
                        if (n1 && n1.remove)
                        {
                            n1.remove();
                        }
                        if (n2 && n2.remove)
                        {
                            n2.remove();
                        }

                        if (parentKey)
                        {
                            var p = tree2.getNodeByKey(parentKey);
                            if (p && p.reloadChildren)
                            {
                                try
                                {
                                    p.reloadChildren();
                                }
                                catch (_)
                                {
                                }
                            }
                            if (p && p.setActive)
                            {
                                try
                                {
                                    p.setActive(true);
                                }
                                catch (_)
                                {
                                }
                            }
                        }
                    }
                }
                catch (_)
                {
                }

                if (parentKey)
                {
                    loadDetails(parentKey, "");
                }
                else
                {
                    var pane = document.getElementById("detailsPane");
                    if (pane)
                    {
                        pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                    }
                }
            });
        });
    }

    // ----------- Delete Category -----------
    function handleDeleteCategory()
    {
        var form = document.getElementById("deleteCategoryForm");
        if (!form)
        {
            return;
        }

        form.addEventListener("submit", function (e)
        {
            e.preventDefault();
            e.stopPropagation();

            var keyInput = form.querySelector('input[name="key"]');
            var key = keyInput ? (keyInput.value || "").trim() : "";
            if (!key)
            {
                alert("Missing key.");
                return;
            }

            var fd = new FormData();
            fd.append("key", key);

            postJson(form, fd, function (err, data)
            {
                if (err || !data || data.success !== true)
                {
                    alert((data && data.message) || "Delete failed.");
                    return;
                }

                try
                {
                    var tree = getTree();
                    if (tree)
                    {
                        var node = tree.getNodeByKey(key);
                        if (node && node.remove)
                        {
                            node.remove();
                        }
                    }
                }
                catch (_)
                {
                }

                var fallbackKey = "";
                try
                {
                    var cfg = document.getElementById("categoryTreeConfig");
                    if (cfg && cfg.dataset)
                    {
                        fallbackKey = cfg.dataset.root || "";
                    }
                }
                catch (_)
                {
                }

                if (fallbackKey)
                {
                    loadDetails(fallbackKey, "");
                    try
                    {
                        var tree2 = getTree();
                        var fb = tree2 && tree2.getNodeByKey(fallbackKey);
                        if (fb && fb.setActive)
                        {
                            fb.setActive(true);
                        }
                    }
                    catch (_)
                    {
                    }
                }
                else
                {
                    var pane = document.getElementById("detailsPane");
                    if (pane)
                    {
                        pane.innerHTML = "<div class='text-muted small p-2'>No details</div>";
                    }
                }
            });
        });
    }

    // ----------- Init -----------
    function onReady()
    {
        wireAllConfirmToggles();
        handleDeleteCashCode();
        handleDeleteCategory();
    }

    if (document.readyState === "loading")
    {
        document.addEventListener("DOMContentLoaded", onReady);
    }
    else
    {
        onReady();
    }
})();
