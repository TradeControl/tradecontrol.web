// Ensure dropdown toggles work even if delegated handlers are missed.
// Adds a lightweight fallback: delegated click -> bootstrap.Dropdown.toggle()
document.addEventListener('DOMContentLoaded', function () {
  document.body.addEventListener('click', function (e) {
    try {
      var toggle = e.target.closest('[data-bs-toggle="dropdown"]');
      if (!toggle) return;
      // Prevent default navigation for href="#"
      e.preventDefault();

      // Use existing instance if available, otherwise create one
      var inst = (bootstrap.Dropdown.getInstance && bootstrap.Dropdown.getInstance(toggle)) || new bootstrap.Dropdown(toggle);
      inst.toggle();
    } catch (err) {
      // Fail silently but log for debugging
      if (window.console) console.warn('Dropdown fallback error:', err);
    }
  }, { passive: false });
});