const StudentNotifications = {
    dom: {},

    init() {
        if (!Auth.requireLogin()) return;
        if (Auth.isAdmin()) {
            window.location.href = "/admin/index.html";
            return;
        }

        UI.initNavbar();
        this.dom = {
            alert: document.getElementById("notifications-alert"),
            loading: document.getElementById("notifications-loading"),
            list: document.getElementById("notifications-list"),
            empty: document.getElementById("notifications-empty"),
            unread: document.getElementById("unread-count")
        };
        this.load();
    },

    async load() {
        const result = await api.get("/api/notifications/mine", true);
        this.dom.loading.hidden = true;

        if (!result?.ok) {
            UI.showAlert("notifications-alert", "error", result?.data?.message || "Unable to load notifications.");
            this.render([]);
            return;
        }

        this.render(Array.isArray(result.data) ? result.data : []);
    },

    render(notifications) {
        const unread = notifications.filter(item => !item.isRead).length;
        this.dom.unread.textContent = `${unread} unread`;
        this.dom.list.replaceChildren();

        if (!notifications.length) {
            this.dom.list.hidden = true;
            this.dom.empty.hidden = false;
            return;
        }

        this.dom.empty.hidden = true;
        this.dom.list.hidden = false;

        notifications.forEach(notification => {
            const item = document.createElement("article");
            item.className = `notification-item${notification.isRead ? "" : " notification-unread"}`;
            const date = notification.createdDate
                ? new Date(notification.createdDate).toLocaleString()
                : "";

            item.innerHTML = `
                <div class="notification-item-heading">
                  <div>
                    <span class="notification-type">${this.escape(notification.type || "System")}</span>
                    <h2>${this.escape(notification.title)}</h2>
                  </div>
                  <time>${this.escape(date)}</time>
                </div>
                <p>${this.escape(notification.message)}</p>
                ${notification.isRead
                    ? '<span class="notification-read">Read</span>'
                    : `<button type="button" class="btn btn-outline btn-sm mark-read" data-id="${notification.notificationId}">Mark as read</button>`}
            `;
            this.dom.list.appendChild(item);
        });

        this.dom.list.querySelectorAll(".mark-read").forEach(button => {
            button.addEventListener("click", () => this.markRead(Number(button.dataset.id)));
        });
    },

    async markRead(id) {
        const result = await api.put(`/api/notifications/${id}/read`, {}, true);
        if (!result?.ok) {
            UI.showAlert("notifications-alert", "error", result?.data?.message || "Unable to mark notification as read.");
            return;
        }
        await this.load();
    },

    escape(value) {
        return String(value ?? "")
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }
};

document.addEventListener("DOMContentLoaded", () => StudentNotifications.init());
