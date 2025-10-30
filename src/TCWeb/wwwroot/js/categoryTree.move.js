// Handles the Move page when loaded into #detailsPane (embedded) or full-page.
// Requires: jQuery, fancytree, and the Category Tree page's #categoryTreeConfig.

(function ()
{
    function onReady(fn)
    {
        if (document.readyState === "loading")
        {
            document.addEventListener("DOMContentLoaded", fn);
        }
        else
        {
            fn();
        }
    }

    function awaitify(p)
    {
        return new Promise(function (resolve)
        {
            if (p && typeof p.then === "function")
            {
                p.then(resolve, resolve);
            }
            else if (p && typeof p.done === "function")
            {
                p.done(resolve).fail(resolve);
            }
            else
            {
                resolve();
            }
        });
    }

    function expandNode(n)
    {
        if (!n)
        {
            return Promise.resolve();
        }
        return awaitify(n.setExpanded(true));
    }

    function reloadChildren(n)
    {
        if (!n)
        {
            return Promise.resolve();
        }
        return awaitify(n.reloadChildren());
    }

    function reloadNodeByKey(tree, key)
    {
        if (!tree || !key)
        {
            return Promise.resolve();
        }

        var n = tree.getNodeByKey(key);
        if (!n)
        {
            return Promise.resolve();
        }

        return reloadChildren(n);
    }

    function reloadAnchors(tree)
    {
        if (!tree)
        {
            return Promise.resolve();
        }

        var top = tree.getRootNode();
        if (!top || !top.children)
        {
            return Promise.resolve();
        }

        var chain = Promise.resolve();
        for (var i = 0; i < top.children.length; i++)
        {
            (function (n)
            {
                if (n && n.expanded)
                {
                    chain = chain.then(function ()
                    {
                        return reloadChildren(n);
                    });
                }
            })(top.children[i]);
        }
        return chain;
    }

    function waitForNode(tree, key, attempts)
    {
        attempts = attempts || 60;
        return new Promise(function (resolve, reject)
        {
            var n = tree.getNodeByKey(key);
            if (n)
            {
                resolve(n);
                return;
            }

            if (attempts <= 0)
            {
                reject();
                return;
            }

            setTimeout(function ()
            {
                waitForNode(tree, key, attempts - 1).then(resolve).catch(reject);
            }, 150);
        });
    }

    function nocache(u)
    {
        if (!u)
        {
            return u;
        }
        var sep = u.indexOf("?") === -1 ? "?" : "&";
        return u + sep + "_=" + Date.now();
    }

    // Try to activate node via FancyTree's loadKeyPath using [parentKey, movedKey]
    function activateViaPath(tree, parentKey, movedKey)
    {
        return new Promise(function (resolve)
        {
            try
            {
                var path = [];
                if (parentKey) { path.push(parentKey); }
                path.push(movedKey);

                var done = false;

                tree.loadKeyPath(path, function (node, status)
                {
                    // Called for each segment
                    if (status === "loaded")
                    {
                        try
                        {
                            node.setExpanded(true);
                        }
                        catch (ex)
                        {
                        }
                    }
                    if (status === "ok" && !done)
                    {
                        done = true;
                        try
                        {
                            tree.setFocus(true);
                        }
                        catch (ex)
                        {
                        }
                        try
                        {
                            node.makeVisible();
                        }
                        catch (ex)
                        {
                        }
                        try
                        {
                            node.setActive(true);
                        }
                        catch (ex)
                        {
                        }
                        try
                        {
                            tree.activateKey(movedKey);
                        }
                        catch (ex)
                        {
                        }

                        resolve(node);
                    }
                }, "activate");

                // Failsafe: if loadKeyPath doesn't resolve within a short time, continue
                setTimeout(function ()
                {
                    if (!done)
                    {
                        resolve(null);
                    }
                }, 1500);
            }
            catch (ex)
            {
                resolve(null);
            }
        });
    }

    onReady(function ()
    {
        var pane = document.getElementById("detailsPane");
        if (!pane)
        {
            return;
        }

        // Embedded Move handler (delegated)
        pane.addEventListener("submit", function (e)
        {
            var form = e.target;
            if (!form || form.id !== "moveForm")
            {
                return;
            }

            // Only intercept when embedded (tree present)
            var $treeEl = window.$ && window.$("#categoryTree");
            var embedded = !!($treeEl && $treeEl.length);
            if (!embedded)
            {
                return;
            }

            e.preventDefault();

            var url = form.action || window.location.href;
            url = nocache(url + (url.indexOf("?") === -1 ? "?embed=1" : "&embed=1"));

            var fd = new FormData(form);

            fetch(url, {
                method: "POST",
                body: fd,
                credentials: "same-origin"
            }).then(function (resp)
            {
                return resp.text();
            })
            .then(function (html)
            {
                var parser = new DOMParser();
                var doc = parser.parseFromString(html, "text/html");
                var marker = doc.getElementById("moveResult");

                // If no success marker, just render returned HTML (validation/errors)
                if (!marker)
                {
                    pane.innerHTML = html;
                    return;
                }

                var oldKey = marker.getAttribute("data-old") || "";
                var parentKey = marker.getAttribute("data-parent") || "";
                var movedKey = marker.getAttribute("data-key") || "";

                var pathAttr = marker.getAttribute("data-path") || "";
                var fullPath = [];
                if (pathAttr && pathAttr.length > 0)
                {
                    // data-path is root->...->parent (pipe-delimited); append movedKey
                    var parts = pathAttr.split("|");
                    for (var i = 0; i < parts.length; i++)
                    {
                        if (parts[i]) { fullPath.push(parts[i]); }
                    }
                    fullPath.push(movedKey);
                }

                // Ensure we have a tree instance before using it
                if (!window.$ || !window.$.ui || !window.$.fn.fancytree)
                {
                    // No tree present: render the returned HTML and bail
                    pane.innerHTML = html;
                    return;
                }

                var tree = window.$("#categoryTree").fancytree("getTree");
                if (!tree)
                {
                    pane.innerHTML = html;
                    return;
                }

                // Clear current active node
                var curActive = tree.getActiveNode && tree.getActiveNode();
                if (curActive)
                {
                    try
                    {
                        curActive.setActive(false);
                    }
                    catch (ex)
                    {
                    }
                }

                // If we have a full path, use loadKeyPath directly (most reliable)
                if (fullPath.length > 0 && typeof tree.loadKeyPath === "function")
                {
                    var lpDone = false;
                    try
                    {
                        tree.loadKeyPath(fullPath, function (node, status)
                        {
                            if (status === "loaded")
                            {
                                try { node.setExpanded(true); } catch (ex) { }
                            }
                            if (status === "ok" && !lpDone)
                            {
                                lpDone = true;

                                try { tree.setFocus(true); } catch (ex) { }
                                try { node.makeVisible(); } catch (ex) { }
                                try { node.setActive(true); } catch (ex) { }
                                try { tree.activateKey(movedKey); } catch (ex) { }

                                try
                                {
                                    var sp = (node && typeof node.getEventTarget === "function") ? node.getEventTarget() : null;
                                    if (sp && sp.scrollIntoView)
                                    {
                                        sp.scrollIntoView({ block: "nearest", inline: "nearest" });
                                    }
                                }
                                catch (ex)
                                {
                                }

                                // Keep headers coherent, then stop
                                reloadAnchors(tree);
                            }
                        }, "activate");

                        // If loadKeyPath didn’t resolve promptly, fall through to the existing fallback
                        setTimeout(function ()
                        {
                            if (!lpDone)
                            {
                                runFallback(); // defined below
                            }
                        }, 1500);
                    }
                    catch (ex)
                    {
                        runFallback();
                    }
                }
                else
                {
                    runFallback();
                }

                // Existing fallback logic extracted into a function
                function runFallback()
                {
                    // First attempt: directly activate via parentKey->movedKey if available
                    activateViaPath(tree, parentKey, movedKey)
                        .then(function (node)
                        {
                            if (node)
                            {
                                try
                                {
                                    var span = (node && typeof node.getEventTarget === "function") ? node.getEventTarget() : null;
                                    if (span && span.scrollIntoView)
                                    {
                                        span.scrollIntoView({ block: "nearest", inline: "nearest" });
                                    }
                                }
                                catch (ex) { }
                                return reloadAnchors(tree);
                            }

                            // Deterministic reload/expand flow
                            return reloadAnchors(tree)
                                .then(function ()
                                {
                                    if (oldKey && oldKey !== parentKey)
                                    {
                                        var oldParent = tree.getNodeByKey(oldKey);
                                        if (oldParent && oldParent.expanded)
                                        {
                                            oldParent.setExpanded(false);
                                        }
                                        return reloadNodeByKey(tree, oldKey);
                                    }
                                })
                                .then(function () { return reloadNodeByKey(tree, parentKey); })
                                .then(function ()
                                {
                                    var targetParent = parentKey ? tree.getNodeByKey(parentKey) : null;
                                    try
                                    {
                                        if (targetParent && typeof targetParent.makeVisible === "function")
                                        {
                                            targetParent.makeVisible();
                                        }
                                    }
                                    catch (ex) { }

                                    if (!targetParent)
                                    {
                                        return null;
                                    }

                                    return expandNode(targetParent)
                                        .then(function () { return reloadChildren(targetParent); })
                                        .then(function () { return targetParent; });
                                })
                                .then(function () { return waitForNode(tree, movedKey, 60); })
                                .then(function (moved)
                                {
                                    try { tree.setFocus(true); } catch (ex) { }
                                    try { moved.makeVisible(); } catch (ex) { }
                                    try { moved.setActive(true); } catch (ex) { }
                                    try { tree.activateKey(movedKey); } catch (ex) { }

                                    try
                                    {
                                        var span2 = (moved && typeof moved.getEventTarget === "function") ? moved.getEventTarget() : null;
                                        if (span2 && span2.scrollIntoView)
                                        {
                                            span2.scrollIntoView({ block: "nearest", inline: "nearest" });
                                        }
                                    }
                                    catch (ex) { }

                                    return reloadAnchors(tree);
                                })
                                .catch(function ()
                                {
                                    // Heavy fallback: reload whole tree once, then try to activate
                                    try
                                    {
                                        if (nodesUrl && tree && typeof tree.reload === "function")
                                        {
                                            tree.reload({ url: nocache(nodesUrl) })
                                                .done(function ()
                                                {
                                                    var m = tree.getNodeByKey(movedKey);
                                                    if (m)
                                                    {
                                                        try { tree.setFocus(true); } catch (ex) { }
                                                        try { m.makeVisible(); } catch (ex) { }
                                                        try { m.setActive(true); } catch (ex) { }
                                                        try { tree.activateKey(movedKey); } catch (ex) { }
                                                    }
                                                });
                                        }
                                    }
                                    catch (ex) { }
                                });
                        });
                }
            })
            .catch(function ()
            {
                alert("Failed to submit Move action.");
            });
        }, true);

        // Embedded Cancel handler (delegated)
        pane.addEventListener("click", function (e)
        {
            var btn = e.target && e.target.closest && e.target.closest("#btnCancel");
            if (!btn)
            {
                return;
            }

            var $tree = window.$ && window.$("#categoryTree");
            var embedded = !!($tree && $tree.length);
            if (!embedded)
            {
                return;
            }

            e.preventDefault();

            try
            {
                // Ensure we have a tree instance before using it
                if (!window.$ || !window.$.ui || !window.$.fn.fancytree)
                {
                    pane.innerHTML = "";
                    return;
                }

                var tree = window.$("#categoryTree").fancytree("getTree");
                if (!tree)
                {
                    pane.innerHTML = "";
                    return;
                }

                var node = tree.getActiveNode ? tree.getActiveNode() : null;
                var key = node ? (node.key || "") : "";
                var parentKey = "";
                if (node && node.getParent)
                {
                    var p = node.getParent();
                    parentKey = (p && p.key) ? p.key : "";
                }

                var cfg = document.getElementById("categoryTreeConfig");
                var detailsUrl = cfg ? cfg.getAttribute("data-details-url") : null;
                if (!detailsUrl || !key)
                {
                    pane.innerHTML = "";
                    return;
                }

                var url = detailsUrl + "?key=" + encodeURIComponent(key);
                if (parentKey)
                {
                    url += "&parentKey=" + encodeURIComponent(parentKey);
                }

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
            catch (ex)
            {
                // ignore
            }
        }, true);
    });
})();