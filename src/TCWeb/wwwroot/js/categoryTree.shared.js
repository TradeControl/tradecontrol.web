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
})();
