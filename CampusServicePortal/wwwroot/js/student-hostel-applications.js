const StudentHostelApplications = {
    student: null,
    hostels: [],

    async init() {
        if (!Auth.requireLogin()) return;
        if (Auth.isAdmin()) {
            window.location.href = "/admin/applications.html";
            return;
        }

        UI.initNavbar();
        document.getElementById("hostel-application-form")
            .addEventListener("submit", event => this.submit(event));

        try {
            const profileResponse = await api.get("/api/students/me", true);
            if (!profileResponse?.ok) {
                throw new Error(profileResponse?.data?.message || "Unable to load your student profile.");
            }

            this.student = profileResponse.data;
            await Promise.all([this.loadHostels(), this.loadApplications()]);
        } catch (error) {
            console.error(error);
            UI.showAlert("hostel-application-alert", "error", error.message);
        }
    },

    escape(value) {
        const node = document.createElement("span");
        node.textContent = value ?? "";
        return node.innerHTML;
    },

    statusBadge(status) {
        const normalized = String(status || "Pending").toLowerCase();
        const badgeClass = normalized === "approved"
            ? "badge-success"
            : normalized === "rejected"
                ? "badge-danger"
                : "badge-warning";
        return `<span class="badge ${badgeClass}">${this.escape(status || "Pending")}</span>`;
    },

    async loadHostels() {
        const response = await api.get("/api/Hostel", true);
        if (!response?.ok) {
            throw new Error("Unable to load hostels.");
        }

        this.hostels = Array.isArray(response.data)
            ? response.data.filter(hostel => hostel.isActive)
            : [];

        const select = document.getElementById("hostel-id");
        select.innerHTML = '<option value="">Select a hostel</option>' + this.hostels
            .map(hostel => `<option value="${hostel.hostelId}">${this.escape(hostel.hostelName)}</option>`)
            .join("");
    },

    async loadApplications() {
        const body = document.getElementById("my-hostel-applications");
        body.innerHTML = '<tr><td colspan="5" style="text-align:center">Loading applications...</td></tr>';

        const response = await api.get("/api/HostelApplication/mine", true);
        if (!response?.ok) {
            throw new Error("Unable to load hostel applications.");
        }

        const applications = (Array.isArray(response.data) ? response.data : [])
            .sort((first, second) => new Date(second.appliedDate) - new Date(first.appliedDate));

        if (!applications.length) {
            body.innerHTML = '<tr><td colspan="5" style="text-align:center">No hostel applications yet.</td></tr>';
            return;
        }

        body.innerHTML = applications.map(application => {
            const hostel = this.hostels.find(item => item.hostelId === application.hostelId);
            const hostelName = application.hostel?.hostelName || hostel?.hostelName || "-";
            const room = application.room?.roomNumber || "Not assigned";
            const date = application.appliedDate
                ? new Date(application.appliedDate).toLocaleDateString()
                : "-";
            return `<tr><td>${this.escape(hostelName)}</td><td>${this.escape(application.semester)}</td><td>${date}</td><td>${this.escape(room)}</td><td>${this.statusBadge(application.status)}</td></tr>`;
        }).join("");
    },

    async submit(event) {
        event.preventDefault();
        const hostelId = Number(document.getElementById("hostel-id").value);
        const semester = document.getElementById("semester").value.trim();
        const specialRequirements = document.getElementById("special-requirements").value.trim();

        if (!hostelId || !semester) {
            UI.showAlert("hostel-application-alert", "error", "Choose a hostel and enter your semester.");
            return;
        }

        const button = document.getElementById("submit-hostel-application");
        UI.setLoading(button, true);
        try {
            const response = await api.post("/api/HostelApplication/mine", {
                hostelId,
                semester,
                specialRequirements: specialRequirements || null
            }, true);

            if (!response?.ok) {
                throw new Error(response?.data?.message || "Unable to submit the hostel application.");
            }

            event.target.reset();
            UI.showAlert("hostel-application-alert", "success", "Hostel application submitted successfully.");
            await this.loadApplications();
        } catch (error) {
            console.error(error);
            UI.showAlert("hostel-application-alert", "error", error.message);
        } finally {
            UI.setLoading(button, false);
        }
    }
};

document.addEventListener("DOMContentLoaded", () => StudentHostelApplications.init());
