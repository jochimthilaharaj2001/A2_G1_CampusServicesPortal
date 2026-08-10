const API_URL = "/api/Hostel";

async function authorizedFetch(endpoint, options = {}) {
    const token = Auth.getToken();
    if (!token) {
        Auth.logout();
        throw new Error("Please sign in again.");
    }

    const headers = new Headers(options.headers || {});
    headers.set("Authorization", `Bearer ${token}`);
    return fetch(`${API_BASE}${endpoint}`, { ...options, headers });
}
let editingHostelId = null;


// ================================
// LOAD HOSTELS
// ================================

async function loadHostels() {

    const tableBody = document.getElementById("hostelTableBody");

    tableBody.innerHTML = `
        <tr>
            <td colspan="7" class="loading">
                Loading hostels...
            </td>
        </tr>
    `;

    try {

        const response = await authorizedFetch(API_URL);

        if (!response.ok) {
            throw new Error("Failed to load hostels.");
        }

        const hostels = await response.json();

        displayHostels(hostels);

    } catch (error) {

        console.error(error);

        tableBody.innerHTML = `
            <tr>
                <td colspan="7" class="empty">
                    Failed to load hostels.
                </td>
            </tr>
        `;

        showMessage(error.message, "error");
    }
}


// ================================
// DISPLAY HOSTELS
// ================================

function displayHostels(hostels) {

    const tableBody = document.getElementById("hostelTableBody");

    tableBody.innerHTML = "";

    if (!hostels || hostels.length === 0) {

        tableBody.innerHTML = `
            <tr>
                <td colspan="7" class="empty">
                    No hostels found.
                </td>
            </tr>
        `;

        return;
    }

    hostels.forEach(hostel => {

        const row = document.createElement("tr");

        row.innerHTML = `
            <td>${hostel.hostelId}</td>

            <td>
                <strong>${escapeHtml(hostel.hostelName)}</strong>
            </td>

            <td>${escapeHtml(hostel.gender)}</td>

            <td>${escapeHtml(hostel.location)}</td>

            <td>
                ${hostel.description
                ? escapeHtml(hostel.description)
                : "-"
            }
            </td>

            <td>
                <span class="status ${hostel.isActive ? "active" : "inactive"}">
                    ${hostel.isActive ? "Active" : "Inactive"}
                </span>
            </td>

            <td>

                <button
                    class="action-btn edit-btn"
                    onclick="editHostel(${hostel.hostelId})"
                >
                    Edit
                </button>

                <button
                    class="action-btn delete-btn"
                    onclick="deleteHostel(${hostel.hostelId})"
                >
                    Delete
                </button>

            </td>
        `;

        tableBody.appendChild(row);
    });
}


// ================================
// OPEN ADD MODAL
// ================================

function openAddModal() {

    editingHostelId = null;

    document.getElementById("modalTitle").textContent =
        "Add Hostel";

    document.getElementById("hostelForm").reset();

    document.getElementById("hostelId").value = "";

    document.getElementById("isActive").checked = true;

    document.getElementById("hostelModal").style.display =
        "flex";
}


// ================================
// EDIT HOSTEL
// ================================

async function editHostel(id) {

    try {

        const response = await authorizedFetch(`${API_URL}/${id}`);

        if (!response.ok) {
            throw new Error("Failed to load hostel.");
        }

        const hostel = await response.json();

        editingHostelId = id;

        document.getElementById("modalTitle").textContent =
            "Edit Hostel";

        document.getElementById("hostelId").value =
            hostel.hostelId;

        document.getElementById("hostelName").value =
            hostel.hostelName || "";

        document.getElementById("gender").value =
            hostel.gender || "";

        document.getElementById("location").value =
            hostel.location || "";

        document.getElementById("description").value =
            hostel.description || "";

        document.getElementById("isActive").checked =
            hostel.isActive;

        document.getElementById("hostelModal").style.display =
            "flex";

    } catch (error) {

        console.error(error);

        showMessage(error.message, "error");
    }
}


// ================================
// SAVE HOSTEL
// ================================

document
    .getElementById("hostelForm")
    .addEventListener("submit", async function (event) {

        event.preventDefault();

        const hostelData = {

            hostelName:
                document.getElementById("hostelName").value.trim(),

            gender:
                document.getElementById("gender").value,

            location:
                document.getElementById("location").value.trim(),

            description:
                document.getElementById("description").value.trim()
                || null,

            isActive:
                document.getElementById("isActive").checked
        };

        try {

            let response;

            if (editingHostelId === null) {

                // CREATE

                response = await authorizedFetch(API_URL, {

                    method: "POST",

                    headers: {
                        "Content-Type": "application/json"
                    },

                    body: JSON.stringify(hostelData)
                });

            } else {

                // UPDATE

                response = await authorizedFetch(
                    `${API_URL}/${editingHostelId}`,
                    {
                        method: "PUT",

                        headers: {
                            "Content-Type": "application/json"
                        },

                        body: JSON.stringify(hostelData)
                    }
                );
            }

            if (!response.ok) {

                let errorMessage =
                    "Failed to save hostel.";

                try {

                    const errorData =
                        await response.json();

                    if (errorData.message) {
                        errorMessage =
                            errorData.message;
                    }

                } catch {
                    // Ignore JSON parsing errors
                }

                throw new Error(errorMessage);
            }

            closeModal();

            showMessage(
                editingHostelId === null
                    ? "Hostel created successfully!"
                    : "Hostel updated successfully!",
                "success"
            );

            await loadHostels();

        } catch (error) {

            console.error(error);

            showMessage(
                error.message,
                "error"
            );
        }
    });


// ================================
// DELETE HOSTEL
// ================================

async function deleteHostel(id) {

    const confirmed = confirm(
        "Are you sure you want to delete this hostel?"
    );

    if (!confirmed) {
        return;
    }

    try {

        const response = await authorizedFetch(
            `${API_URL}/${id}`,
            {
                method: "DELETE"
            }
        );

        if (!response.ok) {

            let errorMessage =
                "Failed to delete hostel.";

            try {

                const errorData =
                    await response.json();

                if (errorData.message) {
                    errorMessage =
                        errorData.message;
                }

            } catch {
                // Ignore JSON parsing errors
            }

            throw new Error(errorMessage);
        }

        showMessage(
            "Hostel deleted successfully!",
            "success"
        );

        await loadHostels();

    } catch (error) {

        console.error(error);

        showMessage(
            error.message,
            "error"
        );
    }
}


// ================================
// CLOSE MODAL
// ================================

function closeModal() {

    document.getElementById("hostelModal").style.display =
        "none";

    document.getElementById("hostelForm").reset();

    editingHostelId = null;
}


// ================================
// SHOW MESSAGE
// ================================

function showMessage(message, type) {

    const messageBox =
        document.getElementById("message");

    messageBox.textContent = message;

    messageBox.className =
        `message ${type}`;

    messageBox.style.display = "block";

    setTimeout(() => {

        messageBox.style.display = "none";

    }, 4000);
}


// ================================
// HTML ESCAPE
// ================================

function escapeHtml(value) {

    if (value === null || value === undefined) {
        return "";
    }

    return String(value)
        .replace(/&/g, "&amp;")
        .replace(/</g, "&lt;")
        .replace(/>/g, "&gt;")
        .replace(/"/g, "&quot;")
        .replace(/'/g, "&#039;");
}


// ================================
// CLOSE MODAL WHEN CLICKING OUTSIDE
// ================================

window.addEventListener("click", function (event) {

    const modal =
        document.getElementById("hostelModal");

    if (event.target === modal) {
        closeModal();
    }
});


// ================================
// INITIAL LOAD
// ================================

document.addEventListener(
    "DOMContentLoaded",
    loadHostels
);