window.taxHubHmrc = {
    showSubmissionStep: function (element) {
        if (!element) return;
        element.scrollIntoView({ behavior: 'smooth', block: 'start' });
        element.focus({ preventScroll: true });
    },
    signOut: async function () {
        const token = document.querySelector('meta[name="request-verification-token"]')?.content;
        if (!token) throw new Error('The request verification token is unavailable.');
        const response = await fetch('/Identity/Account/Logout?returnUrl=%2FTax%2FHub', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'RequestVerificationToken': token }
        });
        if (!response.ok) throw new Error('Trade Control could not sign out.');
        window.location.assign(response.url || '/');
    },
    captureClientFacts: async function () {
        const key = 'trade-control-tax-hub-device-id';
        let deviceId = localStorage.getItem(key);
        if (!deviceId) {
            deviceId = crypto.randomUUID();
            localStorage.setItem(key, deviceId);
        }
        const offset = -new Date().getTimezoneOffset();
        const sign = offset >= 0 ? '+' : '-';
        const pad = value => String(Math.abs(value)).padStart(2, '0');
        const token = document.querySelector('meta[name="request-verification-token"]')?.content;
        if (!token) throw new Error('The request verification token is unavailable.');
        const response = await fetch('/TaxHub/Hmrc/ClientFacts', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': token },
            body: JSON.stringify({
                javascriptUserAgent: navigator.userAgent,
                deviceId,
                screens: [{ width: screen.width, height: screen.height,
                    scalingFactor: devicePixelRatio, colourDepth: screen.colorDepth }],
                timezone: `UTC${sign}${pad(Math.trunc(offset / 60))}:${pad(offset % 60)}`,
                windowSize: { width: window.innerWidth, height: window.innerHeight }
            })
        });
        if (!response.ok) throw new Error('HMRC client facts could not be captured.');
    },
    disconnect: async function () {
        const token = document.querySelector('meta[name="request-verification-token"]')?.content;
        if (!token) throw new Error('The request verification token is unavailable.');
        const response = await fetch('/TaxHub/Hmrc/Disconnect', {
            method: 'POST',
            credentials: 'same-origin',
            headers: { 'RequestVerificationToken': token }
        });
        if (!response.ok) throw new Error('HMRC could not be disconnected.');
    }
};
