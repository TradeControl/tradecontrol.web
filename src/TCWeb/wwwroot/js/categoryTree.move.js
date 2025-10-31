// Handles the Move page when loaded into #detailsPane (embedded) or full-page.
// Requires: jQuery, FancyTree, and the Category Tree page's #categoryTreeConfig.

(function ()
{
    // Allman + ES5-safe helpers

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

    function safeInvoke(target, methodName)
    {
        try
        {
            if (target && typeof target[methodName] === "function")
            {
                var args = Array.prototype.slice.call(arguments, 2);
                return target[methodName].apply(target, args);
            }
        }
        catch (ex)
        {
        }
        return undefined;
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
        if (!n || typeof n.reloadChildren !== "function")
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

    // Totals vs Type helpers

    function isTypeNode(n)
    {
        if (!n) { return false; }
        var d = n.data || {};
        if (d.syntheticKind === "type" || d.isTypeContext === true)
        {
            return true;
        }
        var k = n.key;
        return (typeof k === "string" && k.indexOf("type:") === 0);
    }

    function isInTypeSubtree(n)
    {
        var p = n;
        while (p)
        {
            if (isTypeNode(p)) { return true; }
            p = (p.getParent && p.getParent()) || null;
        }
        return false;
    }

    function getChildByKey(parent, key)
    {
        if (!parent || !parent.children) { return null; }
        for (var i = 0; i < parent.children.length; i++)
        {
            var ch = parent.children[i];
            if (ch && ch.key === key) { return ch; }
        }
        return null;
    }

    // BFS limited to a given subtree (single start) and time budget
    function findInSubtreeBfs(startNode, targetKey, budgetMs)
    {
        return new Promise(function (resolve)
        {
            var deadline = Date.now() + (budgetMs || 2500);
            var q = [];
            if (startNode) { q.push(startNode); }

            function step()
            {
                if (Date.now() > deadline)
                {
                    resolve(null);
                    return;
                }
                if (q.length === 0)
                {
                    resolve(null);
                    return;
                }

                var n = q.shift();
                if (n && n.key === targetKey)
                {
                    resolve(n);
                    return;
                }

                if (!n || !n.folder)
                {
                    setTimeout(step, 0);
                    return;
                }

                var p = awaitify(n.setExpanded(true));
                p.then(function ()
                {
                    return reloadChildren(n);
                })
                .then(function ()
                {
                    var kids = n.children || [];
                    for (var i = 0; i < kids.length; i++)
                    {
                        var ch = kids[i];
                        if (ch && ch.key === targetKey)
                        {
                            resolve(ch);
                            return;
                        }
                    }
                    for (var j = 0; j < kids.length; j++)
                    {
                        var ch2 = kids[j];
                        if (ch2 && ch2.folder) { q.push(ch2); }
                    }
                    setTimeout(step, 0);
                })
                .catch(function ()
                {
                    setTimeout(step, 0);
                });
            }

            setTimeout(step, 0);
        });
    }

    // Sequential search across Totals roots: try each start one-by-one (avoids expanding all roots)
    function findInStartsSequential(starts, key, budgetMs)
    {
        return new Promise(function (resolve)
        {
            var i = 0;

            function next()
            {
                if (!starts || i >= starts.length)
                {
                    resolve(null);
                    return;
                }

                var s = starts[i++];
                findInSubtreeBfs(s, key, budgetMs || 2500)
                    .then(function (node)
                    {
                        if (node)
                        {
                            resolve(node);
                        }
                        else
                        {
                            setTimeout(next, 0);
                        }
                    })
                    .catch(function ()
                    {
                        setTimeout(next, 0);
                    });
            }

            next();
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

                // Optional: parent chain (Totals): topCategory|...|parent
                var pathAttr = marker.getAttribute("data-path") || "";
                var parentPath = [];
                if (pathAttr && pathAttr.length > 0)
                {
                    var parts = pathAttr.split("|");
                    for (var i = 0; i < parts.length; i++)
                    {
                        if (parts[i]) { parentPath.push(parts[i]); }
                    }
                }

                // Replace RHS immediately with moved node details (visible feedback)
                try
                {
                    var cfg0 = document.getElementById("categoryTreeConfig");
                    var detailsUrl0 = cfg0 ? cfg0.getAttribute("data-details-url") : null;
                    if (detailsUrl0 && movedKey)
                    {
                        var durl = detailsUrl0 + "?key=" + encodeURIComponent(movedKey);
                        if (parentKey)
                        {
                            durl += "&parentKey=" + encodeURIComponent(parentKey);
                        }

                        fetch(nocache(durl), { credentials: "same-origin" })
                            .then(function (r) { return r.text(); })
                            .then(function (detailsHtml)
                            {
                                pane.innerHTML = detailsHtml;
                            })
                            .catch(function (ex)
                            {
                            });
                    }
                }
                catch (ex)
                {
                }

                // Ensure we have a tree instance before using it
                if (!window.$ || !window.$.ui || !window.$.fn.fancytree)
                {
                    pane.innerHTML = html;
                    return;
                }

                var tree = ($.ui && $.ui.fancytree && $.ui.fancytree.getTree)
                    ? $.ui.fancytree.getTree("#categoryTree")
                    : window.$("#categoryTree").fancytree("getTree");
                if (!tree)
                {
                    pane.innerHTML = html;
                    return;
                }

                // Clear any current active node
                var curActive = tree.getActiveNode && tree.getActiveNode();
                if (curActive)
                {
                    safeInvoke(curActive, "setActive", false);
                }

                // Reload the whole tree once (deterministic), then select under Totals without expanding unrelated roots
                var cfgEl = document.getElementById("categoryTreeConfig");
                var nodesUrl = cfgEl ? cfgEl.getAttribute("data-nodes-url") : null;

                function selectAndScroll(n)
                {
                    safeInvoke(tree, "setFocus", true);
                    safeInvoke(n, "makeVisible");
                    safeInvoke(n, "setActive", true);
                    safeInvoke(tree, "activateKey", n.key);
                    try
                    {
                        var span = (n && typeof n.getEventTarget === "function") ? n.getEventTarget() : null;
                        if (span && span.scrollIntoView)
                        {
                            span.scrollIntoView({ block: "nearest", inline: "nearest" });
                        }
                    }
                    catch (ex)
                    {
                    }
                }

                function buildTotalsStarts()
                {
                    var root = tree.getRootNode();
                    var starts = [];
                    if (root && root.children && root.children.length)
                    {
                        for (var i = 0; i < root.children.length; i++)
                        {
                            var ch = root.children[i];
                            if (ch && !isTypeNode(ch))
                            {
                                starts.push(ch); // only Totals roots
                            }
                        }
                    }
                    return starts;
                }

                function expandPathUnderTotals(starts, parentPathArr)
                {
                    // Sequentially: find top node in one Totals root, then walk the chain under that subtree only
                    if (!parentPathArr || parentPathArr.length === 0)
                    {
                        return Promise.resolve(null);
                    }

                    var topKey = parentPathArr[0];
                    return findInStartsSequential(starts, topKey, 2500)
                        .then(function (current)
                        {
                            if (!current)
                            {
                                return null;
                            }

                            var idx = 1;

                            function step()
                            {
                                if (idx >= parentPathArr.length)
                                {
                                    return Promise.resolve(current);
                                }

                                var nextKey = parentPathArr[idx++];
                                return expandNode(current)
                                    .then(function () { return reloadChildren(current); })
                                    .then(function ()
                                    {
                                        var next = getChildByKey(current, nextKey);
                                        if (next)
                                        {
                                            current = next;
                                            return step();
                                        }
                                        return findInSubtreeBfs(current, nextKey, 2000)
                                            .then(function (found)
                                            {
                                                if (!found)
                                                {
                                                    return null;
                                                }
                                                current = found;
                                                return step();
                                            });
                                    });
                            }

                            return step();
                        });
                }

                if (nodesUrl && tree && typeof tree.reload === "function")
                {
                    tree.reload({ url: nocache(nodesUrl) })
                        .done(function ()
                        {
                            setTimeout(function ()
                            {
                                var starts = buildTotalsStarts();

                                // Preferred: use parentPath (precise, minimal expansion)
                                var chain = null;
                                if (parentPath && parentPath.length > 0)
                                {
                                    chain = expandPathUnderTotals(starts, parentPath)
                                        .then(function (parentNode)
                                        {
                                            if (!parentNode)
                                            {
                                                return false;
                                            }

                                            return expandNode(parentNode)
                                                .then(function () { return reloadChildren(parentNode); })
                                                .then(function ()
                                                {
                                                    var movedChild = getChildByKey(parentNode, movedKey);
                                                    if (movedChild)
                                                    {
                                                        selectAndScroll(movedChild);
                                                        return true;
                                                    }
                                                    return findInSubtreeBfs(parentNode, movedKey, 2500)
                                                        .then(function (found)
                                                        {
                                                            if (found)
                                                            {
                                                                selectAndScroll(found);
                                                                return true;
                                                            }
                                                            return false;
                                                        });
                                                });
                                        });
                                }
                                else
                                {
                                    // Fallback: find the parentKey by sequentially probing Totals roots
                                    if (parentKey)
                                    {
                                        chain = findInStartsSequential(starts, parentKey, 2500)
                                            .then(function (parentNode2)
                                            {
                                                if (!parentNode2)
                                                {
                                                    return false;
                                                }
                                                return expandNode(parentNode2)
                                                    .then(function () { return reloadChildren(parentNode2); })
                                                    .then(function ()
                                                    {
                                                        var movedChild2 = getChildByKey(parentNode2, movedKey);
                                                        if (movedChild2)
                                                        {
                                                            selectAndScroll(movedChild2);
                                                            return true;
                                                        }
                                                        return findInSubtreeBfs(parentNode2, movedKey, 2500)
                                                            .then(function (found2)
                                                            {
                                                                if (found2)
                                                                {
                                                                    selectAndScroll(found2);
                                                                    return true;
                                                                }
                                                                return false;
                                                            });
                                                    });
                                            });
                                    }
                                    else
                                    {
                                        // Last resort: look for movedKey by probing each Totals root one-by-one
                                        chain = new Promise(function (resolve)
                                        {
                                            var i = 0;
                                            function next()
                                            {
                                                if (!starts || i >= starts.length)
                                                {
                                                    resolve(false);
                                                    return;
                                                }
                                                var s = starts[i++];
                                                findInSubtreeBfs(s, movedKey, 2500)
                                                    .then(function (found3)
                                                    {
                                                        if (found3)
                                                        {
                                                            selectAndScroll(found3);
                                                            resolve(true);
                                                        }
                                                        else
                                                        {
                                                            setTimeout(next, 0);
                                                        }
                                                    })
                                                    .catch(function () { setTimeout(next, 0); });
                                            }
                                            next();
                                        });
                                    }
                                }

                                (chain || Promise.resolve(false))
                                    .then(function ()
                                    {
                                        return reloadAnchors(tree);
                                    })
                                    .catch(function ()
                                    {
                                    });
                            }, 30);
                        });
                }
                else
                {
                    // No reload available: try parent-targeted selection with current tree
                    var startsNow = buildTotalsStarts();

                    var chain2 = null;
                    if (parentPath && parentPath.length > 0)
                    {
                        chain2 = expandPathUnderTotals(startsNow, parentPath)
                            .then(function (parentNode3)
                            {
                                if (!parentNode3) { return false; }
                                return expandNode(parentNode3)
                                    .then(function () { return reloadChildren(parentNode3); })
                                    .then(function ()
                                    {
                                        var movedChild3 = getChildByKey(parentNode3, movedKey);
                                        if (movedChild3)
                                        {
                                            selectAndScroll(movedChild3);
                                            return true;
                                        }
                                        return findInSubtreeBfs(parentNode3, movedKey, 2500)
                                            .then(function (found4)
                                            {
                                                if (found4)
                                                {
                                                    selectAndScroll(found4);
                                                    return true;
                                                }
                                                return false;
                                            });
                                    });
                            });
                    }
                    else if (parentKey)
                    {
                        chain2 = findInStartsSequential(startsNow, parentKey, 2500)
                            .then(function (parentNode4)
                            {
                                if (!parentNode4) { return false; }
                                return expandNode(parentNode4)
                                    .then(function () { return reloadChildren(parentNode4); })
                                    .then(function ()
                                    {
                                        var movedChild4 = getChildByKey(parentNode4, movedKey);
                                        if (movedChild4)
                                        {
                                            selectAndScroll(movedChild4);
                                            return true;
                                        }
                                        return findInSubtreeBfs(parentNode4, movedKey, 2500)
                                            .then(function (found5)
                                            {
                                                if (found5)
                                                {
                                                    selectAndScroll(found5);
                                                    return true;
                                                }
                                                return false;
                                            });
                                    });
                            });
                    }
                    else
                    {
                        // Sequentially probe Totals roots for movedKey
                        chain2 = new Promise(function (resolve)
                        {
                            var i2 = 0;
                            function next2()
                            {
                                if (!startsNow || i2 >= startsNow.length)
                                {
                                    resolve(false);
                                    return;
                                }
                                var s2 = startsNow[i2++];
                                findInSubtreeBfs(s2, movedKey, 2500)
                                    .then(function (found6)
                                    {
                                        if (found6)
                                        {
                                            selectAndScroll(found6);
                                            resolve(true);
                                        }
                                        else
                                        {
                                            setTimeout(next2, 0);
                                        }
                                    })
                                    .catch(function () { setTimeout(next2, 0); });
                            }
                            next2();
                        });
                    }

                    (chain2 || Promise.resolve(false))
                        .then(function ()
                        {
                            return reloadAnchors(tree);
                        })
                        .catch(function ()
                        {
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
            }
        }, true);
    });
})();