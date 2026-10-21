// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your Javascript code.

(() => {
    const storageKeys = {
        header: "tradecontrol.display.header-hidden",
        footer: "tradecontrol.display.footer-hidden"
    };

    const classNames = {
        header: "tc-header-hidden",
        footer: "tc-footer-hidden"
    };

    const readPreference = part => {
        try {
            return localStorage.getItem(storageKeys[part]) === "true";
        } catch {
            return false;
        }
    };

    const writePreference = (part, hidden) => {
        try {
            if (hidden) {
                localStorage.setItem(storageKeys[part], "true");
            } else {
                localStorage.removeItem(storageKeys[part]);
            }
        } catch {
            // The current page still updates even when browser storage is unavailable.
        }
    };

    const isHidden = part => document.body.classList.contains(classNames[part]);

    const applyPreference = part => {
        document.body.classList.toggle(classNames[part], readPreference(part));
    };

    const updateLabels = () => {
        Object.keys(storageKeys).forEach(part => {
            const label = document.querySelector(`[data-tc-label="${part}"]`);
            if (label) {
                label.textContent = isHidden(part)
                    ? `Show ${part}`
                    : `Hide ${part}`;
            }
        });
    };

    const notifyLayoutChanged = () => {
        window.dispatchEvent(new Event("resize"));
        window.dispatchEvent(new CustomEvent("tc:chromechange"));
    };

    document.addEventListener("DOMContentLoaded", () => {
        const hasFooter = Boolean(document.querySelector("body > footer"));
        document.body.classList.toggle("tc-footer-unavailable", !hasFooter);
        document.querySelector("[data-tc-footer-option]")?.classList.toggle("d-none", !hasFooter);

        Object.keys(storageKeys).forEach(applyPreference);
        updateLabels();

        document.querySelectorAll("[data-tc-toggle-chrome]").forEach(button => {
            button.addEventListener("click", () => {
                const part = button.dataset.tcToggleChrome;
                if (!storageKeys[part]) return;

                const hidden = !isHidden(part);
                writePreference(part, hidden);
                document.body.classList.toggle(classNames[part], hidden);
                updateLabels();
                notifyLayoutChanged();
            });
        });

        document.querySelector("[data-tc-show-chrome]")?.addEventListener("click", () => {
            Object.keys(storageKeys).forEach(part => {
                writePreference(part, false);
                document.body.classList.remove(classNames[part]);
            });
            updateLabels();
            notifyLayoutChanged();
        });
    });
})();
