const APPLICATION_API = "/api/HostelApplication";
const HOSTEL_API = "/api/Hostel";
const ROOM_API = "/api/Room";
const STUDENT_API = "/api/Students";

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
let applications = [];
let hostels = [];
let rooms = [];
let students = [];
let editingApplicationId = null;


// ==========================================
// INITIALIZE
// ==========================================

document.addEventListener("DOMContentLoaded", async () => {

    await Promise.all([
        loadHostels(),
        loadRooms(),
        loadStudents()
    ]);

    await loadApplications();

});

// ==========================================
// STUDENT INDEX NUMBER LOOKUP
// ==========================================

async function loadStudents() {
    try {
        const response = await api.get(
            `${STUDENT_API}?page=1&pageSize=100`,
            true
        );

        if (!response?.ok) {
            throw new Error("Failed to load student index numbers.");
        }

        students = Array.isArray(response.data?.items)
            ? response.data.items
            : [];

        const options = document.getElementById("studentIndexOptions");
        options.innerHTML = "";
        students.forEach(addStudentOption);
    } catch (error) {
        console.error(error);
        students = [];
        showMessage(
            "Student index numbers could not be loaded. Please sign in again.",
            "error"
        );
    }
}

function addStudentOption(student) {
    const options = document.getElementById("studentIndexOptions");
    if (!options || !student?.indexNumber) {
        return;
    }

    const hasOption = Array.from(options.options).some(option =>
        normalizeIndexNumber(option.value) === normalizeIndexNumber(student.indexNumber)
    );
    if (hasOption) {
        return;
    }

    const option = document.createElement("option");
    option.value = student.indexNumber;
    option.label = student.fullName || "";
    options.appendChild(option);
}

function normalizeIndexNumber(value) {
    return String(value || "").trim().toUpperCase();
}

function getCachedStudentByIndexNumber(indexNumber) {
    const normalizedIndexNumber = normalizeIndexNumber(indexNumber);
    return students.find(student =>
        normalizeIndexNumber(student.indexNumber) === normalizedIndexNumber
    ) || null;
}

function getCachedStudentById(studentId) {
    return students.find(student =>
        Number(student.studentId) === Number(studentId)
    ) || null;
}

async function resolveStudentByIndexNumber(indexNumber) {
    const cachedStudent = getCachedStudentByIndexNumber(indexNumber);
    if (cachedStudent) {
        return cachedStudent;
    }

    const normalizedIndexNumber = normalizeIndexNumber(indexNumber);
    const response = await api.get(
        `${STUDENT_API}?search=${encodeURIComponent(normalizedIndexNumber)}&page=1&pageSize=20`,
        true
    );

    if (!response?.ok) {
        throw new Error("Failed to validate the Student Index Number.");
    }

    const matchedStudent = (response.data?.items || []).find(student =>
        normalizeIndexNumber(student.indexNumber) === normalizedIndexNumber
    ) || null;

    if (matchedStudent && !getCachedStudentById(matchedStudent.studentId)) {
        students.push(matchedStudent);
        addStudentOption(matchedStudent);
    }

    return matchedStudent;
}

async function getStudentById(studentId) {
    const cachedStudent = getCachedStudentById(studentId);
    if (cachedStudent) {
        return cachedStudent;
    }

    const response = await api.get(`${STUDENT_API}/${studentId}`, true);
    if (!response?.ok) {
        return null;
    }

    const student = response.data;
    if (student && !getCachedStudentById(student.studentId)) {
        students.push(student);
        addStudentOption(student);
    }

    return student || null;
}

// ==========================================
// LOAD HOSTELS
// ==========================================

async function loadHostels() {

    try {

        const response = await authorizedFetch(HOSTEL_API);

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

        const response = await authorizedFetch(ROOM_API);

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
            await authorizedFetch(APPLICATION_API);

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

        const student =
            getCachedStudentById(
                application.studentId
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
                ${student
                ? `${escapeHtml(student.indexNumber)} — ${escapeHtml(student.fullName)}`
                : `Student ${application.studentId}`
            }
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
            await authorizedFetch(
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


        const student =
            await getStudentById(application.studentId);

        document.getElementById("studentIndexNumber")
            .value =
            student?.indexNumber || "";


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


            const indexNumber =
                normalizeIndexNumber(
                    document.getElementById(
                        "studentIndexNumber"
                    ).value
                );

            try {

                const student =
                    await resolveStudentByIndexNumber(
                        indexNumber
                    );

                if (!student) {

                    showMessage(
                        "Enter a valid registered Student Index Number.",
                        "error"
                    );

                    document.getElementById(
                        "studentIndexNumber"
                    ).focus();

                    return;
                }


                const baseData = {

                    studentId:
                        Number(student.studentId),

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

                let response;


                if (
                    editingApplicationId === null
                ) {

                    // CREATE

                    response =
                        await authorizedFetch(
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
                        await authorizedFetch(
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
            await authorizedFetch(
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