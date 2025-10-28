// Bootstraps the CategoryTree page (kept separate from the core tree logic)
(function ()
{
    function isDesktop()
    {
        return window.matchMedia && window.matchMedia("(min-width: 992px)").matches;
    }

    function initSplitter()
    {
        if (typeof Split !== "function") { return; }
        var splitInstance = null;

        function ensureSplit()
        {
            if (!isDesktop())
            {
                if (splitInstance) { splitInstance.destroy(true, true); splitInstance = null; }
                return;
            }
            if (!splitInstance)
            {
                splitInstance = Split(["#leftCol", "#rightCol"], {
                    sizes: [60, 40],
                    minSize: [300, 300],
                    gutterSize: 8,
                    snapOffset: 0,
                    onDragEnd: function ()
                    {
                        if (window.CategoryTree && CategoryTree._resizeColumns)
                        {
                            CategoryTree._resizeColumns();
                        }
                        else
                        {
                            var ev = document.createEvent("Event"); ev.initEvent("resize", true, true); window.dispatchEvent(ev);
                        }
                    }
                });
            }
        }

        window.addEventListener("resize", ensureSplit);
        ensureSplit();
    }

    function initPage()
    {
        var cfgEl = document.getElementById("categoryTreeConfig");
        if (!cfgEl || !window.CategoryTree) { return; }

        CategoryTree.init({
            treeSelector: "#categoryTree",
            menuSelector: "#treeContextMenu",
            actionBarSelector: "#mobileActionBar",
            nodesUrl: cfgEl.dataset.nodesUrl,
            basePageUrl: cfgEl.dataset.baseUrl,
            detailsUrl: cfgEl.dataset.detailsUrl,
            rootKey: cfgEl.dataset.root,
            discKey: cfgEl.dataset.disc,
            isAdmin: cfgEl.dataset.isAdmin === "true"
        });

        initSplitter();
    }

    if (document.readyState === "loading")
    {
        document.addEventListener("DOMContentLoaded", initPage);
    }
    else
    {
        initPage();
    }
})();