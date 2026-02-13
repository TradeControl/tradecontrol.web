window.adminManager = window.adminManager || {};

window.adminManager.getFileTransferPageSize = function ()
{
    try
    {
        const raw = localStorage.getItem("tcFileTransferPageSize");
        const ps = Number(raw);
        if (!Number.isFinite(ps) || ps <= 0)
        {
            return 25;
        }
        return ps;
    }
    catch
    {
        return 25;
    }
};

window.adminManager.registerTemplateRefreshListener = function (dotNetRef)
{
    window.adminManager._templateRefreshDotNetRef = dotNetRef || null;

    if (window.adminManager._templateRefreshListenerAttached)
    {
        return;
    }

    window.adminManager._templateRefreshListenerAttached = true;

    window.addEventListener("message", function (e)
    {
        const data = e && e.data ? e.data : null;
        if (!data || typeof data !== "object")
        {
            return;
        }

        if (data.type !== "tc.template.refresh")
        {
            return;
        }

        const ref = window.adminManager._templateRefreshDotNetRef;
        if (!ref)
        {
            return;
        }

        if (data.scope === "invoiceType.templates" || data.scope === "invoiceType.attachments")
        {
            const code = Number(data.invoiceTypeCode);
            if (!Number.isFinite(code))
            {
                return;
            }

            const method = data.scope === "invoiceType.templates"
                ? "RefreshInvoiceTypeTemplates"
                : "RefreshInvoiceTypeAttachments";

            try
            {
                ref.invokeMethodAsync(method, code)
                    .catch(function () { });
            }
            catch
            {
            }

            return;
        }

        if (data.scope === "template.images")
        {
            const templateId = Number(data.templateId);
            if (!Number.isFinite(templateId))
            {
                return;
            }

            try
            {
                ref.invokeMethodAsync("RefreshTemplateImages", templateId)
                    .catch(function () { });
            }
            catch
            {
            }
        }
    });
};

window.adminManager.unregisterTemplateRefreshListener = function ()
{
    window.adminManager._templateRefreshDotNetRef = null;
};
