(function () {
    const root = document.documentElement;
    const storageKey = 'habit-tracker-theme';
    let theme = 'light';

    try {
        if (localStorage.getItem(storageKey) === 'dark') {
            theme = 'dark';
        }
    } catch {
        // The toggle still works on the current page when storage is unavailable.
    }

    root.dataset.bsTheme = theme;

    document.addEventListener('DOMContentLoaded', function () {
        document.querySelectorAll('[data-theme-toggle]').forEach(function (button) {
            button.hidden = false;
            button.addEventListener('click', function () {
                theme = theme === 'dark' ? 'light' : 'dark';
                root.dataset.bsTheme = theme;

                try {
                    localStorage.setItem(storageKey, theme);
                } catch {
                    // Keep the current page's theme even when it cannot be saved.
                }
            });
        });
    });
})();
