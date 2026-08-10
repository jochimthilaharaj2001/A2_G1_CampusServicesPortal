const NOTIFICATION_API = "/api/notifications";
let editingNotificationId = null;

document.addEventListener("DOMContentLoaded", () => {
    if (!Auth.requireAdmin()) return;
    UI.initNavbar();
    loadNotifications();
});

async function loadNotifications() {
    const table = document.getElementById("notificationTableBody");
    table.innerHTML = '<tr><td colspan="7" class="loading">Loading notifications...</td></tr>';

    const result = await api.get(NOTIFICATION_API, true);
    if (!result?.ok) {
        table.innerHTML = '<tr><td colspan="7" class="empty">Unable to load notifications.</td></tr>';
        showMessage(result?.data?.message || "Unable to load notifications.", "error");
        return;
    }
    displayNotifications(Array.isArray(result.data) ? result.data : []);
}

function displayNotifications(notifications) {
    const table = document.getElementById("notificationTableBody");
    table.replaceChildren();

    if (!notifications.length) {
        table.innerHTML = '<tr><td colspan="7" class="empty">No notifications found.</td></tr>';
        return;
    }

    notifications.forEach(notification => {
        const row = document.createElement("tr");
        const status = notification.isRead ? "Read" : "Unread";
        const created = notification.createdDate
            ? new Date(notification.createdDate).toLocaleString()
            : "-";

        row.innerHTML = `
            <td>${notification.notificationId}</td>
            <td>Student ${notification.studentId}</td>
            <td><strong>${escapeHtml(notification.title)}</strong></td>
            <td>${escapeHtml(notification.message)}</td>
            <td>${created}</td>
            <td><span class="status ${notification.isRead ? "read" : "unread"}">${status}</span></td>
            <td>
                <button class="action-btn edit-btn" onclick="editNotification(${notification.notificationId})">Edit</button>
                <button class="action-btn delete-btn" onclick="deleteNotification(${notification.notificationId})">Delete</button>
            </td>`;
        table.appendChild(row);
    });
}

function openAddModal() {
    editingNotificationId = null;
    document.getElementById("modalTitle").textContent = "New Notification";
    document.getElementById("notificationForm").reset();
    document.getElementById("studentGroup").style.display = "block";
    document.getElementById("readGroup").style.display = "none";
    document.getElementById("notificationModal").style.display = "flex";
}

async function editNotification(id) {
    const result = await api.get(`${NOTIFICATION_API}/${id}`, true);
    if (!result?.ok) {
        showMessage(result?.data?.message || "Unable to load notification.", "error");
        return;
    }

    const notification = result.data;
    editingNotificationId = id;
    document.getElementById("modalTitle").textContent = "Edit Notification";
    document.getElementById("studentId").value = notification.studentId;
    document.getElementById("title").value = notification.title || "";
    document.getElementById("notificationMessage").value = notification.message || "";
    document.getElementById("isRead").checked = Boolean(notification.isRead);
    document.getElementById("studentGroup").style.display = "none";
    document.getElementById("readGroup").style.display = "flex";
    document.getElementById("notificationModal").style.display = "flex";
}

document.getElementById("notificationForm").addEventListener("submit", async event => {
    event.preventDefault();
    const editing = editingNotificationId !== null;
    const body = editing
        ? {
            title: document.getElementById("title").value.trim(),
            message: document.getElementById("notificationMessage").value.trim(),
            isRead: document.getElementById("isRead").checked
        }
        : {
            studentId: Number(document.getElementById("studentId").value),
            title: document.getElementById("title").value.trim(),
            message: document.getElementById("notificationMessage").value.trim(),
            type: "AdminMessage"
        };

    const result = editing
        ? await api.put(`${NOTIFICATION_API}/${editingNotificationId}`, body, true)
        : await api.post(NOTIFICATION_API, body, true);

    if (!result?.ok) {
        showMessage(result?.data?.message || "Unable to save notification.", "error");
        return;
    }

    closeModal();
    showMessage(editing ? "Notification updated successfully." : "Notification created successfully.", "success");
    await loadNotifications();
});

async function deleteNotification(id) {
    if (!confirm("Are you sure you want to delete this notification?")) return;

    const result = await api.delete(`${NOTIFICATION_API}/${id}`, true);
    if (!result?.ok) {
        showMessage(result?.data?.message || "Unable to delete notification.", "error");
        return;
    }

    showMessage("Notification deleted successfully.", "success");
    await loadNotifications();
}

function closeModal() {
    document.getElementById("notificationModal").style.display = "none";
    document.getElementById("notificationForm").reset();
    editingNotificationId = null;
}

function showMessage(message, type) {
    const box = document.getElementById("message");
    box.textContent = message;
    box.className = `message ${type}`;
    box.style.display = "block";
    setTimeout(() => box.style.display = "none", 4000);
}

function escapeHtml(value) {
    return String(value ?? "")
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}

window.addEventListener("click", event => {
    const modal = document.getElementById("notificationModal");
    if (event.target === modal) closeModal();
});
