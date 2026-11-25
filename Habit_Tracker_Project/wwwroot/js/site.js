// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

(function () {
    const notificationMenu = document.querySelector('[data-notification-menu]');

    if (notificationMenu) {
        const toggle = notificationMenu.querySelector('[data-notification-toggle]');
        const panel = notificationMenu.querySelector('[data-notification-panel]');
        const badge = notificationMenu.querySelector('[data-notification-badge]');
        const count = notificationMenu.querySelector('[data-notification-count]');
        const feed = notificationMenu.querySelector('[data-notification-feed]');
        const emptyText = feed.dataset.emptyText || 'You have no notifications.';
        const newNotificationText = feed.dataset.newNotification || 'New notification';
        const unreadText = feed.dataset.unreadSuffix || 'unread';
        const seenStorageKey = 'habit-tracker-notification-ids';

        const getSeenIds = () => {
            try {
                const ids = JSON.parse(sessionStorage.getItem(seenStorageKey) || '[]');
                return new Set(Array.isArray(ids) ? ids.map(String) : []);
            } catch {
                return new Set();
            }
        };

        const saveSeenIds = (ids) => {
            sessionStorage.setItem(seenStorageKey, JSON.stringify(Array.from(ids).slice(-50)));
        };

        const formatDate = (value) => {
            const date = new Date(value);
            return Number.isNaN(date.getTime()) ? '' : date.toLocaleString();
        };

        const showToast = (item) => {
            const toast = document.createElement('div');
            toast.className = 'ht-notification-toast';
            toast.setAttribute('role', 'status');

            const title = document.createElement('strong');
            title.textContent = item.title || newNotificationText;
            const message = document.createElement('span');
            message.textContent = item.message || '';
            toast.append(title, message);
            document.body.appendChild(toast);
            window.setTimeout(() => toast.remove(), 6000);
        };

        const renderFeed = (items) => {
            feed.replaceChildren();
            if (!items.length) {
                const empty = document.createElement('p');
                empty.className = 'ht-notification-empty';
                empty.textContent = emptyText;
                feed.appendChild(empty);
                return;
            }

            items.slice(0, 8).forEach((item) => {
                const article = document.createElement('article');
                article.className = `ht-notification-feed-item${item.isRead ? '' : ' is-unread'}`;

                const title = document.createElement('strong');
                title.textContent = item.title || '';
                const message = document.createElement('p');
                message.textContent = item.message || '';
                const date = document.createElement('time');
                date.textContent = formatDate(item.createdAt);
                article.append(title, message, date);
                feed.appendChild(article);
            });
        };

        const loadNotifications = async () => {
            try {
                const response = await fetch('/Notifications/Feed', {
                    headers: { Accept: 'application/json' },
                    credentials: 'same-origin'
                });
                if (!response.ok) return;

                const data = await response.json();
                const items = Array.isArray(data.items) ? data.items : [];
                const seenIds = getSeenIds();
                const isFirstLoad = sessionStorage.getItem(seenStorageKey) === null;
                const newItems = items.filter((item) => item.id != null && !seenIds.has(String(item.id)));

                items.forEach((item) => {
                    if (item.id != null) seenIds.add(String(item.id));
                });
                saveSeenIds(seenIds);
                renderFeed(items);

                const unreadCount = Number(data.unreadCount) || 0;
                badge.textContent = unreadCount > 99 ? '99+' : String(unreadCount);
                badge.hidden = unreadCount === 0;
                count.textContent = unreadCount ? `${unreadCount} ${unreadText}` : '';

                if (!isFirstLoad && newItems.length) showToast(newItems[0]);
            } catch {
                // Notification polling is non-critical and should not interrupt the page.
            }
        };

        toggle.addEventListener('click', () => {
            const isOpen = !panel.hidden;
            panel.hidden = isOpen;
            toggle.setAttribute('aria-expanded', String(!isOpen));
        });

        document.addEventListener('click', (event) => {
            if (!notificationMenu.contains(event.target)) {
                panel.hidden = true;
                toggle.setAttribute('aria-expanded', 'false');
            }
        });

        loadNotifications();
        window.setInterval(loadNotifications, 20000);
    }

    const telegramButton = document.querySelector('[data-connect-telegram]');
    if (telegramButton) {
        const result = document.querySelector('[data-telegram-result]');
        const code = document.querySelector('[data-telegram-code]');
        const link = document.querySelector('[data-telegram-link]');
        const status = document.querySelector('[data-telegram-status]');
        const buttonLabel = telegramButton.textContent.trim();

        telegramButton.addEventListener('click', async () => {
            const telegramWindow = window.open('', '_blank');
            telegramButton.disabled = true;
            telegramButton.textContent = telegramButton.dataset.connectingText || 'Connecting...';
            try {
                const response = await fetch('/Users/ConnectTelegram', {
                    method: 'POST',
                    headers: { Accept: 'application/json' },
                    credentials: 'same-origin'
                });
                if (!response.ok) throw new Error(telegramButton.dataset.connectError || 'Unable to connect Telegram.');
                const data = await response.json();
                const botUrl = new URL(data.botUrl || '', window.location.origin);
                if (botUrl.protocol !== 'https:' || botUrl.hostname !== 't.me') {
                    throw new Error(telegramButton.dataset.invalidUrl || 'Invalid Telegram bot URL.');
                }

                code.textContent = data.code || '';
                link.href = botUrl.href;
                link.textContent = botUrl.href;
                link.hidden = false;
                result.hidden = false;
                status.textContent = status.dataset.pendingText || 'Connection pending';
                if (telegramWindow) telegramWindow.location.href = botUrl.href;
            } catch {
                if (telegramWindow) telegramWindow.close();
                status.textContent = telegramButton.dataset.generationError || 'Could not generate a Telegram code. Please try again.';
            } finally {
                telegramButton.disabled = false;
                telegramButton.textContent = buttonLabel;
            }
        });
    }
})();


(function () {
    const table = document.querySelector('[data-user-sort-table]');
    if (!table) {
        return;
    }

    const collator = new Intl.Collator(document.documentElement.lang, { sensitivity: 'base' });
    const body = table.tBodies[0];

    table.querySelectorAll('[data-sort-column]').forEach(function (button) {
        button.addEventListener('click', function () {
            const column = Number(button.dataset.sortColumn);
            const header = button.closest('th');
            const ascending = header.getAttribute('aria-sort') !== 'ascending';
            const rows = Array.from(body.querySelectorAll('[data-user-row]'), function (row) {
                const cell = row.cells[column];
                const key = cell.dataset.sortValue === undefined
                    ? cell.textContent.replace(/\s+/g, ' ').trim()
                    : Number(cell.dataset.sortValue);
                return { row, key };
            });

            rows.sort(function (a, b) {
                const comparison = typeof a.key === 'number'
                    ? a.key - b.key
                    : collator.compare(a.key, b.key);
                return ascending ? comparison : -comparison;
            });

            body.append(...rows.map(entry => entry.row));
            table.querySelector('[aria-sort]')?.removeAttribute('aria-sort');
            header.setAttribute('aria-sort', ascending ? 'ascending' : 'descending');
        });
    });
})();
