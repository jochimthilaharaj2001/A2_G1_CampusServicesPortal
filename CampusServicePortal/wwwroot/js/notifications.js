const NOTIFICATION_API = "/api/Notification";

let editingNotificationId = null;


// ==========================================
// INITIALIZE
// ==========================================

document.addEventListener("DOMContentLoaded", () => {

    loadNotifications();

});


// ==========================================
// LOAD NOTIFICATIONS
// ==========================================

async function loadNotifications() {

    const table =
        document.getElementById(
            "notificationTableBody"
        );

    table.innerHTML = `
        <tr>
            <td colspan="7" class="loading">
                Loading notifications...
            </td>
        </tr>
    `;

    try {

        const response =
            await fetch(NOTIFICATION_API);

        if (!response.ok) {

            throw new Error(
                "Failed to load notifications."
            );
        }

        const notifications =
            await response.json();

        displayNotifications(
            notifications
        );

    } catch (error) {

        console.error(error);

        table.innerHTML = `
            <tr>
                <td colspan="7" class="empty">
                    Failed to load notifications.
                </td>
            </tr>
        `;

        showMessage(
            error.message,
            "error"
        );
    }
}


// ==========================================
// DISPLAY
// ==========================================

function displayNotifications(
    notifications
) {

    const table =
        document.getElementById(
            "notificationTableBody"
        );

    table.innerHTML = "";


    if (
        !notifications ||
        notifications.length === 0
    ) {

        table.innerHTML = `
            <tr>
                <td colspan="7" class="empty">
                    No notifications found.
                </td>
            </tr>
        `;

        return;
    }


    notifications.forEach(notification => {

        const statusClass =
            notification.isRead
                ? "read"
                : "unread";


        const statusText =
            notification.isRead
                ? "Read"
                : "Unread";


        const createdDate =
            notification.createdDate
                ? new Date(
                    notification.createdDate
                ).toLocaleDateString()
                : "-";


        const row =
            document.createElement("tr");


        row.innerHTML = `

            <td>
                ${notification.notificationId}
            </td>

            <td>
                Student ${notification.studentId}
            </td>

            <td>
                <strong>
                    ${escapeHtml(
            notification.title
        )}
                </strong>
            </td>

            <td>
                ${escapeHtml(
            notification.message
        )}
            </td>

            <td>
                ${createdDate}
            </td>

            <td>

                <span class="status ${statusClass}">
                    ${statusText}
                </span>

            </td>

            <td>

                <button
                    class="action-btn edit-btn"
                    onclick="editNotification(
                        ${notification.notificationId}
                    )">
                    Edit
                </button>

                <button
                    class="action-btn delete-btn"
                    onclick="deleteNotification(
                        ${notification.notificationId}
                    )">
                    Delete
                </button>

            </td>

        `;

        table.appendChild(row);

    });
}


// ==========================================
// ADD
// ==========================================

function openAddModal() {

    editingNotificationId = null;


    document.getElementById("modalTitle")
        .textContent =
        "New Notification";


    document.getElementById("notificationForm")
        .reset();


    document.getElementById("studentGroup")
        .style.display = "block";


    document.getElementById("readGroup")
        .style.display = "none";


    document.getElementById("notificationModal")
        .style.display = "flex";
}


// ==========================================
// EDIT
// ==========================================

async function editNotification(id) {

    try {

        const response =
            await fetch(
                `${NOTIFICATION_API}/${id}`
            );


        if (!response.ok) {

            throw new Error(
                "Failed to load notification."
            );
        }


        const notification =
            await response.json();


        editingNotificationId = id;


        document.getElementById("modalTitle")
            .textContent =
            "Edit Notification";


        document.getElementById("studentId")
            .value =
            notification.studentId;


        document.getElementById("title")
            .value =
            notification.title || "";


        document.getElementById(
            "notificationMessage"
        ).value =
            notification.message || "";


        document.getElementById("isRead")
            .checked =
            notification.isRead;


        document.getElementById("studentGroup")
            .style.display = "none";


        document.getElementById("readGroup")
            .style.display = "flex";


        document.getElementById("notificationModal")
            .style.display = "flex";


    } catch (error) {

        console.error(error);

        showMessage(
            error.message,
            "error"
        );
    }
}


// ==========================================
// SAVE
// ==========================================

document
    .getElementById("notificationForm")
    .addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const wasEditing =
                editingNotificationId !== null;


            try {

                let response;


                if (!wasEditing) {

                    const createData = {

                        studentId:
                            Number(
                                document.getElementById(
                                    "studentId"
                                ).value
                            ),

                        title:
                            document.getElementById(
                                "title"
                            ).value.trim(),

                        message:
                            document.getElementById(
                                "notificationMessage"
                            ).value.trim()

                    };


                    response =
                        await fetch(
                            NOTIFICATION_API,
                            {
                                method: "POST",

                                headers: {
                                    "Content-Type":
                                        "application/json"
                                },

                                body:
                                    JSON.stringify(
                                        createData
                                    )
                            }
                        );

                } else {

                    const updateData = {

                        title:
                            document.getElementById(
                                "title"
                            ).value.trim(),

                        message:
                            document.getElementById(
                                "notificationMessage"
                            ).value.trim(),

                        isRead:
                            document.getElementById(
                                "isRead"
                            ).checked

                    };


                    response =
                        await fetch(
                            `${NOTIFICATION_API}/${editingNotificationId}`,
                            {
                                method: "PUT",

                                headers: {
                                    "Content-Type":
                                        "application/json"
                                },

                                body:
                                    JSON.stringify(
                                        updateData
                                    )
                            }
                        );
                }


                if (!response.ok) {

                    let message =
                        "Failed to save notification.";

                    try {

                        const data =
                            await response.json();

                        if (data.message) {
                            message =
                                data.message;
                        }

                    } catch {
                        // Ignore
                    }

                    throw new Error(message);
                }


                closeModal();


                showMessage(
                    wasEditing
                        ? "Notification updated successfully!"
                        : "Notification created successfully!",
                    "success"
                );


                await loadNotifications();


            } catch (error) {

                console.error(error);

                showMessage(
                    error.message,
                    "error"
                );
            }

        }
    );


// ==========================================
// DELETE
// ==========================================

async function deleteNotification(id) {

    if (
        !confirm(
            "Are you sure you want to delete this notification?"
        )
    ) {
        return;
    }


    try {

        const response =
            await fetch(
                `${NOTIFICATION_API}/${id}`,
                {
                    method: "DELETE"
                }
            );


        if (!response.ok) {

            throw new Error(
                "Failed to delete notification."
            );
        }


        showMessage(
            "Notification deleted successfully!",
            "success"
        );


        await loadNotifications();


    } catch (error) {

        console.error(error);

        showMessage(
            error.message,
            "error"
        );
    }
}


// ==========================================
// CLOSE MODAL
// ==========================================

function closeModal() {

    document.getElementById(
        "notificationModal"
    ).style.display = "none";

    document.getElementById(
        "notificationForm"
    ).reset();

    editingNotificationId = null;
}


// ==========================================
// MESSAGE
// ==========================================

function showMessage(message, type) {

    const box =
        document.getElementById("message");

    box.textContent = message;

    box.className =
        `message ${type}`;

    box.style.display = "block";


    setTimeout(() => {

        box.style.display = "none";

    }, 4000);
}


// ==========================================
// ESCAPE HTML
// ==========================================

function escapeHtml(value) {

    if (
        value === null ||
        value === undefined
    ) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}


// ==========================================
// CLOSE MODAL WHEN CLICKING OUTSIDE
// ==========================================

window.addEventListener(
    "click",
    event => {

        const modal =
            document.getElementById(
                "notificationModal"
            );

        if (event.target === modal) {
            closeModal();
        }

    }
);