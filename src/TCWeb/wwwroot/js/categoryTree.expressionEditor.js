(function ()
{
    if (window.cashExpressionEditorInitialised)
    {
        return;
    }
    window.cashExpressionEditorInitialised = true;

    function pickerFrom(element)
    {
        return element && element.closest
            ? element.closest("[data-expression-reference-picker]")
            : null;
    }

    function insertSelectedReference(picker)
    {
        if (!picker)
        {
            return;
        }

        var list = picker.querySelector("[data-reference-list]");
        var targetId = picker.getAttribute("data-expression-target");
        var editor = targetId ? document.getElementById(targetId) : null;
        var option = list && list.selectedIndex >= 0 ? list.options[list.selectedIndex] : null;
        if (!editor || !option || option.hidden)
        {
            return;
        }

        var token = "[" + option.value + "]";
        var start = typeof editor.selectionStart === "number" ? editor.selectionStart : editor.value.length;
        var end = typeof editor.selectionEnd === "number" ? editor.selectionEnd : start;
        editor.setRangeText(token, start, end, "end");
        editor.focus();
        editor.dispatchEvent(new Event("input", { bubbles: true }));
    }

    document.addEventListener("input", function (event)
    {
        if (!event.target.matches("[data-reference-filter]"))
        {
            return;
        }

        var picker = pickerFrom(event.target);
        var list = picker ? picker.querySelector("[data-reference-list]") : null;
        var query = String(event.target.value || "").trim().toLowerCase();
        if (!list)
        {
            return;
        }

        Array.prototype.forEach.call(list.options, function (option)
        {
            var search = String(option.getAttribute("data-search") || option.textContent || "").toLowerCase();
            option.hidden = query.length > 0 && search.indexOf(query) === -1;
        });
        list.selectedIndex = -1;
        var button = picker.querySelector("[data-reference-insert]");
        if (button)
        {
            button.disabled = true;
        }
    });

    document.addEventListener("change", function (event)
    {
        if (!event.target.matches("[data-reference-list]"))
        {
            return;
        }
        var picker = pickerFrom(event.target);
        var button = picker ? picker.querySelector("[data-reference-insert]") : null;
        if (button)
        {
            button.disabled = event.target.selectedIndex < 0;
        }
    });

    document.addEventListener("dblclick", function (event)
    {
        if (event.target.matches("[data-reference-list], [data-reference-list] option"))
        {
            insertSelectedReference(pickerFrom(event.target));
        }
    });

    document.addEventListener("keydown", function (event)
    {
        if (event.key === "Enter" && event.target.matches("[data-reference-list]"))
        {
            event.preventDefault();
            insertSelectedReference(pickerFrom(event.target));
        }
    });

    document.addEventListener("click", function (event)
    {
        var button = event.target.closest("[data-reference-insert]");
        if (button)
        {
            insertSelectedReference(pickerFrom(button));
        }
    });
})();
