const APPLICATION_API = "/api/HostelApplication";
const HOSTEL_API = "/api/Hostel";
const ROOM_API = "/api/Room";

let applications = [];
let hostels = [];
let rooms = [];
let editingApplicationId = null;


// ==========================================
// INITIALIZE
// ==========================================

document.addEventListener("DOMContentLoaded", async () => {

    await loadHostels();
    await loadRooms();
    await loadApplications();

});


// ==========================================
// LOAD HOSTELS
// ==========================================

async function loadHostels() {

    try {

        const response = await fetch(HOSTEL_API);

        if (!response.ok) {
            throw new Error("Failed to load hostels.");
        }

        hostels = await response.json();

        const select =
            document.getElementById("hostelId");

        select.innerHTML =
            `<option value="">Select Hostel</option>`;

        hostels.forEach(hostel => {

            select.innerHTML += `
                <option value="${hostel.hostelId}">
                    ${escapeHtml(hostel.hostelName)}
                </option>
            `;

        });

    } catch (error) {

        console.error(error);

        showMessage(
            "Failed to load hostels.",
            "error"
        );
    }
}


// ==========================================
// LOAD ROOMS
// ==========================================

async function loadRooms() {

    try {

        const response = await fetch(ROOM_API);

        if (!response.ok) {
            throw new Error("Failed to load rooms.");
        }

        rooms = await response.json();

    } catch (error) {

        console.error(error);

        rooms = [];
    }
}


// ==========================================
// LOAD APPLICATIONS
// ==========================================

async function loadApplications() {

    const table =
        document.getElementById(
            "applicationTableBody"
        );

    table.innerHTML = `
        <tr>
            <td colspan="8" class="loading">
                Loading applications...
            </td>
        </tr>
    `;

    try {

        const response =
            await fetch(APPLICATION_API);

        if (!response.ok) {
            throw new Error(
                "Failed to load applications."
            );
        }

        applications =
            await response.json();

        displayApplications(applications);

    } catch (error) {

        console.error(error);

        table.innerHTML = `
            <tr>
                <td colspan="8" class="empty">
                    Failed to load applications.
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
// DISPLAY APPLICATIONS
// ==========================================

function displayApplications(list) {

    const table =
        document.getElementById(
            "applicationTableBody"
        );

    table.innerHTML = "";

    if (!list || list.length === 0) {

        table.innerHTML = `
            <tr>
                <td colspan="8" class="empty">
                    No applications found.
                </td>
            </tr>
        `;

        return;
    }


    list.forEach(application => {

        const hostel =
            hostels.find(
                h => h.hostelId === application.hostelId
            );

        const room =
            rooms.find(
                r => r.roomId === application.roomId
            );


        const status =
            application.status || "Pending";


        const statusClass =
            status.toLowerCase();


        const row =
            document.createElement("tr");


        row.innerHTML = `

            <td>
                ${application.applicationId}
            </td>

            <td>
                Student ${application.studentId}
            </td>

            <td>
                ${hostel
                ? escapeHtml(hostel.hostelName)
                : `Hostel ${application.hostelId}`
            }
            </td>

            <td>
                ${room
                ? escapeHtml(room.roomNumber)
                : "-"
            }
            </td>

            <td>
                ${escapeHtml(application.semester)}
            </td>

            <td>
                <span class="status ${statusClass}">
                    ${escapeHtml(status)}
                </span>
            </td>

            <td>
                ${application.specialRequirements
                ? escapeHtml(
                    application.specialRequirements
                )
                : "-"
            }
            </td>

            <td>

                <button
                    class="action-btn edit-btn"
                    onclick="editApplication(
                        ${application.applicationId}
                    )">
                    Edit
                </button>

                <button
                    class="action-btn delete-btn"
                    onclick="deleteApplication(
                        ${application.applicationId}
                    )">
                    Delete
                </button>

            </td>
        `;

        table.appendChild(row);

    });
}


// ==========================================
// LOAD ROOM DROPDOWN
// ==========================================

function updateRoomDropdown(hostelId, selectedRoomId = null) {

    const select =
        document.getElementById("roomId");

    select.innerHTML =
        `<option value="">No Room Assigned</option>`;


    if (!hostelId) {
        return;
    }


    const hostelRooms =
        rooms.filter(
            room =>
                Number(room.hostelId) ===
                Number(hostelId)
        );


    hostelRooms.forEach(room => {

        const option =
            document.createElement("option");

        option.value =
            room.roomId;

        option.textContent =
            `${room.roomNumber} (${room.currentOccupancy || 0}/${room.capacity})`;

        if (
            selectedRoomId !== null &&
            Number(selectedRoomId) ===
            Number(room.roomId)
        ) {
            option.selected = true;
        }

        select.appendChild(option);

    });
}


// ==========================================
// HOSTEL CHANGE
// ==========================================

document
    .getElementById("hostelId")
    .addEventListener(
        "change",
        function () {

            updateRoomDropdown(
                this.value
            );

        }
    );


// ==========================================
// ADD APPLICATION
// ==========================================

function openAddModal() {

    editingApplicationId = null;

    document.getElementById("modalTitle")
        .textContent =
        "New Application";


    document.getElementById("applicationForm")
        .reset();


    document.getElementById("roomId")
        .innerHTML =
        `<option value="">
            No Room Assigned
        </option>`;


    document.getElementById("statusGroup")
        .style.display = "none";


    document.getElementById("applicationModal")
        .style.display = "flex";
}


// ==========================================
// EDIT APPLICATION
// ==========================================

async function editApplication(id) {

    try {

        const response =
            await fetch(
                `${APPLICATION_API}/${id}`
            );


        if (!response.ok) {

            throw new Error(
                "Failed to load application."
            );
        }


        const application =
            await response.json();


        editingApplicationId = id;


        document.getElementById("modalTitle")
            .textContent =
            "Edit Application";


        document.getElementById("studentId")
            .value =
            application.studentId;


        document.getElementById("hostelId")
            .value =
            application.hostelId;


        updateRoomDropdown(
            application.hostelId,
            application.roomId
        );


        document.getElementById("semester")
            .value =
            application.semester || "";


        document.getElementById(
            "specialRequirements"
        ).value =
            application.specialRequirements || "";


        document.getElementById("status")
            .value =
            application.status || "Pending";


        document.getElementById("statusGroup")
            .style.display = "block";


        document.getElementById("applicationModal")
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
// SAVE APPLICATION
// ==========================================

document
    .getElementById("applicationForm")
    .addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const baseData = {

                studentId:
                    Number(
                        document.getElementById(
                            "studentId"
                        ).value
                    ),

                hostelId:
                    Number(
                        document.getElementById(
                            "hostelId"
                        ).value
                    ),

                semester:
                    document.getElementById(
                        "semester"
                    ).value.trim(),

                specialRequirements:
                    document.getElementById(
                        "specialRequirements"
                    ).value.trim() || null

            };


            try {

                let response;


                if (
                    editingApplicationId === null
                ) {

                    // CREATE

                    response =
                        await fetch(
                            APPLICATION_API,
                            {
                                method: "POST",

                                headers: {
                                    "Content-Type":
                                        "application/json"
                                },

                                body:
                                    JSON.stringify(
                                        baseData
                                    )
                            }
                        );

                } else {

                    // UPDATE

                    const roomValue =
                        document.getElementById(
                            "roomId"
                        ).value;


                    const updateData = {

                        ...baseData,

                        roomId:
                            roomValue
                                ? Number(roomValue)
                                : null,

                        status:
                            document.getElementById(
                                "status"
                            ).value

                    };


                    response =
                        await fetch(
                            `${APPLICATION_API}/${editingApplicationId}`,
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
                        "Failed to save application.";

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
                    editingApplicationId === null
                        ? "Application created successfully!"
                        : "Application updated successfully!",
                    "success"
                );


                await loadApplications();


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
// DELETE APPLICATION
// ==========================================

async function deleteApplication(id) {

    if (
        !confirm(
            "Are you sure you want to delete this application?"
        )
    ) {
        return;
    }


    try {

        const response =
            await fetch(
                `${APPLICATION_API}/${id}`,
                {
                    method: "DELETE"
                }
            );


        if (!response.ok) {

            throw new Error(
                "Failed to delete application."
            );
        }


        showMessage(
            "Application deleted successfully!",
            "success"
        );


        await loadApplications();


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
        "applicationModal"
    ).style.display = "none";

    document.getElementById(
        "applicationForm"
    ).reset();

    editingApplicationId = null;
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
// OUTSIDE MODAL CLICK
// ==========================================

window.addEventListener(
    "click",
    event => {

        const modal =
            document.getElementById(
                "applicationModal"
            );

        if (event.target === modal) {
            closeModal();
        }

    }
);