const ROOM_API = "/api/Room";
const HOSTEL_API = "/api/Hostel";

let editingRoomId = null;
let hostels = [];


// ======================================
// INITIALIZE
// ======================================

document.addEventListener("DOMContentLoaded", async () => {

    await loadHostels();

    await loadRooms();

});


// ======================================
// LOAD HOSTELS
// ======================================

async function loadHostels() {

    const select =
        document.getElementById("hostelId");

    try {

        const response =
            await fetch(HOSTEL_API);

        if (!response.ok) {
            throw new Error("Failed to load hostels.");
        }

        hostels =
            await response.json();

        select.innerHTML =
            `<option value="">Select Hostel</option>`;

        hostels.forEach(hostel => {

            const option =
                document.createElement("option");

            option.value =
                hostel.hostelId;

            option.textContent =
                hostel.hostelName;

            select.appendChild(option);

        });

    } catch (error) {

        console.error(error);

        showMessage(
            "Failed to load hostels.",
            "error"
        );
    }
}


// ======================================
// LOAD ROOMS
// ======================================

async function loadRooms() {

    const tableBody =
        document.getElementById("roomTableBody");

    tableBody.innerHTML = `
        <tr>
            <td colspan="8" class="loading">
                Loading rooms...
            </td>
        </tr>
    `;

    try {

        const response =
            await fetch(ROOM_API);

        if (!response.ok) {
            throw new Error("Failed to load rooms.");
        }

        const rooms =
            await response.json();

        displayRooms(rooms);

    } catch (error) {

        console.error(error);

        tableBody.innerHTML = `
            <tr>
                <td colspan="8" class="empty">
                    Failed to load rooms.
                </td>
            </tr>
        `;

        showMessage(
            error.message,
            "error"
        );
    }
}


// ======================================
// DISPLAY ROOMS
// ======================================

function displayRooms(rooms) {

    const tableBody =
        document.getElementById("roomTableBody");

    tableBody.innerHTML = "";

    if (!rooms || rooms.length === 0) {

        tableBody.innerHTML = `
            <tr>
                <td colspan="8" class="empty">
                    No rooms found.
                </td>
            </tr>
        `;

        return;
    }


    rooms.forEach(room => {

        const hostel =
            hostels.find(
                h => h.hostelId === room.hostelId
            );

        const hostelName =
            hostel
                ? hostel.hostelName
                : `Hostel ${room.hostelId}`;


        const occupancy =
            Number(room.currentOccupancy || 0);

        const capacity =
            Number(room.capacity || 0);

        const isFull =
            occupancy >= capacity;


        const row =
            document.createElement("tr");


        row.innerHTML = `

            <td>
                ${room.roomId}
            </td>

            <td>
                <strong>
                    ${escapeHtml(hostelName)}
                </strong>
            </td>

            <td>
                ${escapeHtml(room.roomNumber)}
            </td>

            <td>
                ${capacity}
            </td>

            <td class="occupancy ${isFull ? "full" : "available"}">
                ${occupancy} / ${capacity}
            </td>

            <td>
                ${room.roomType
                ? escapeHtml(room.roomType)
                : "-"
            }
            </td>

            <td>

                <span class="status ${room.isActive
                ? "active"
                : "inactive"
            }">

                    ${room.isActive
                ? "Active"
                : "Inactive"
            }

                </span>

            </td>

            <td>

                <button
                    class="action-btn edit-btn"
                    onclick="editRoom(${room.roomId})">
                    Edit
                </button>

                <button
                    class="action-btn delete-btn"
                    onclick="deleteRoom(${room.roomId})">
                    Delete
                </button>

            </td>
        `;

        tableBody.appendChild(row);

    });
}


// ======================================
// OPEN ADD MODAL
// ======================================

function openAddModal() {

    editingRoomId = null;

    document.getElementById("modalTitle")
        .textContent = "Add Room";

    document.getElementById("roomForm")
        .reset();

    document.getElementById("roomId")
        .value = "";

    document.getElementById("currentOccupancy")
        .value = 0;

    document.getElementById("occupancyGroup")
        .style.display = "none";

    document.getElementById("isActive")
        .checked = true;

    document.getElementById("roomModal")
        .style.display = "flex";
}


// ======================================
// EDIT ROOM
// ======================================

async function editRoom(id) {

    try {

        const response =
            await fetch(`${ROOM_API}/${id}`);

        if (!response.ok) {

            throw new Error(
                "Failed to load room."
            );
        }

        const room =
            await response.json();

        editingRoomId = id;


        document.getElementById("modalTitle")
            .textContent = "Edit Room";


        document.getElementById("roomId")
            .value = room.roomId;


        document.getElementById("hostelId")
            .value = room.hostelId;


        document.getElementById("roomNumber")
            .value = room.roomNumber || "";


        document.getElementById("capacity")
            .value = room.capacity;


        document.getElementById("currentOccupancy")
            .value =
            room.currentOccupancy || 0;


        document.getElementById("roomType")
            .value = room.roomType || "";


        document.getElementById("isActive")
            .checked = room.isActive;


        document.getElementById("occupancyGroup")
            .style.display = "block";


        document.getElementById("roomModal")
            .style.display = "flex";


    } catch (error) {

        console.error(error);

        showMessage(
            error.message,
            "error"
        );
    }
}


// ======================================
// SAVE ROOM
// ======================================

document
    .getElementById("roomForm")
    .addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const roomData = {

                hostelId:
                    Number(
                        document.getElementById(
                            "hostelId"
                        ).value
                    ),

                roomNumber:
                    document.getElementById(
                        "roomNumber"
                    ).value.trim(),

                capacity:
                    Number(
                        document.getElementById(
                            "capacity"
                        ).value
                    ),

                roomType:
                    document.getElementById(
                        "roomType"
                    ).value.trim() || null,

                isActive:
                    document.getElementById(
                        "isActive"
                    ).checked

            };


            try {

                let response;


                if (editingRoomId === null) {

                    // CREATE

                    response =
                        await fetch(
                            ROOM_API,
                            {
                                method: "POST",

                                headers: {
                                    "Content-Type":
                                        "application/json"
                                },

                                body:
                                    JSON.stringify(
                                        roomData
                                    )
                            }
                        );

                } else {

                    // UPDATE

                    roomData.currentOccupancy =
                        Number(
                            document.getElementById(
                                "currentOccupancy"
                            ).value
                        );


                    response =
                        await fetch(
                            `${ROOM_API}/${editingRoomId}`,
                            {
                                method: "PUT",

                                headers: {
                                    "Content-Type":
                                        "application/json"
                                },

                                body:
                                    JSON.stringify(
                                        roomData
                                    )
                            }
                        );
                }


                if (!response.ok) {

                    let message =
                        "Failed to save room.";

                    try {

                        const data =
                            await response.json();

                        if (data.message) {
                            message =
                                data.message;
                        }

                    } catch {
                        // Ignore JSON parsing error
                    }

                    throw new Error(message);
                }


                closeModal();


                showMessage(
                    editingRoomId === null
                        ? "Room created successfully!"
                        : "Room updated successfully!",
                    "success"
                );


                await loadRooms();


            } catch (error) {

                console.error(error);

                showMessage(
                    error.message,
                    "error"
                );
            }

        }
    );


// ======================================
// DELETE ROOM
// ======================================

async function deleteRoom(id) {

    const confirmed =
        confirm(
            "Are you sure you want to delete this room?"
        );


    if (!confirmed) {
        return;
    }


    try {

        const response =
            await fetch(
                `${ROOM_API}/${id}`,
                {
                    method: "DELETE"
                }
            );


        if (!response.ok) {

            let message =
                "Failed to delete room.";

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


        showMessage(
            "Room deleted successfully!",
            "success"
        );


        await loadRooms();


    } catch (error) {

        console.error(error);

        showMessage(
            error.message,
            "error"
        );
    }
}


// ======================================
// CLOSE MODAL
// ======================================

function closeModal() {

    document.getElementById("roomModal")
        .style.display = "none";

    document.getElementById("roomForm")
        .reset();

    editingRoomId = null;
}


// ======================================
// MESSAGE
// ======================================

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


// ======================================
// ESCAPE HTML
// ======================================

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


// ======================================
// CLOSE MODAL OUTSIDE CLICK
// ======================================

window.addEventListener(
    "click",
    function (event) {

        const modal =
            document.getElementById("roomModal");

        if (event.target === modal) {
            closeModal();
        }

    }
);