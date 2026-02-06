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
