export function measurePopup(element, preferUpwards)
{
    if (!element)
    {
        return {
            left: 0,
            top: 0,
            width: 520,
            maxHeight: 256
        };
    }

    const rect = element.getBoundingClientRect();
    const viewportWidth = window.innerWidth || document.documentElement.clientWidth || 1280;
    const viewportHeight = window.innerHeight || document.documentElement.clientHeight || 720;
    const margin = 8;
    const desiredHeight = 256;
    const minimumWidth = 520;

    const spaceAbove = Math.max(0, rect.top - margin);
    const spaceBelow = Math.max(0, viewportHeight - rect.bottom - margin);

    const openUpwards = preferUpwards
        ? spaceAbove > 160 || spaceAbove > spaceBelow
        : spaceBelow < 160 && spaceAbove > spaceBelow;

    const maxHeight = Math.max(
        160,
        Math.min(
            desiredHeight,
            openUpwards ? spaceAbove : spaceBelow));

    const desiredWidth = Math.max(rect.width, minimumWidth);
    const width = Math.min(desiredWidth, viewportWidth - (margin * 2));
    const left = Math.max(margin, Math.min(rect.left, viewportWidth - width - margin));
    const top = openUpwards
        ? Math.max(margin, rect.top - maxHeight - 4)
        : Math.min(viewportHeight - maxHeight - margin, rect.bottom + 4);

    return {
        left,
        top,
        width,
        maxHeight
    };
}
