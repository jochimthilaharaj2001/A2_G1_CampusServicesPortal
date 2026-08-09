/**
 * Campus Services Portal — Admin: Lab Management
 */

const AdminLabs = {
    state: {
        labs: [],
        bookings: [],
        currentLabId: null,
        seats: [] // For manage seats modal
    },

    init() {
        if (!Auth.requireAdmin()) return;
        UI.initNavbar();

        this.cacheDOM();
        this.bindEvents();
        this.loadLabs();
        this.loadBookings();
    },

    cacheDOM() {
        this.dom = {
            // Tabs
            tabBtns: document.querySelectorAll('.tab-btn'),
            tabPanes: document.querySelectorAll('.tab-pane'),
            
            // Labs Table
            labsTableBody: document.querySelector('#labs-table tbody'),
            labsLoading: document.getElementById('labs-loading'),
            
            // Bookings Table
            bookingsTableBody: document.querySelector('#bookings-table tbody'),
            bookingsLoading: document.getElementById('bookings-loading'),
            
            // Modal: Create/Edit
            labModal: document.getElementById('lab-modal'),
            labForm: document.getElementById('lab-form'),
            modalTitle: document.getElementById('modal-title'),
            fId: document.getElementById('lab-id'),
            fName: document.getElementById('lab-name'),
            fType: document.getElementById('lab-type'),
            fRoom: document.getElementById('lab-room'),
            fCapacity: document.getElementById('lab-capacity'),
            fActive: document.getElementById('lab-active'),
            btnSave: document.getElementById('btn-save-lab'),
            
            // Modal: Seats
            seatsModal: document.getElementById('seats-modal'),
            seatsLabName: document.getElementById('seats-lab-name'),
            seatInput: document.getElementById('new-seat-number'),
            btnAddSeat: document.getElementById('btn-add-seat'),
            seatsContainer: document.getElementById('seats-container'),
            seatsLoading: document.getElementById('seats-list-loading'),
            noSeatsMsg: document.getElementById('no-seats-msg')
        };
    },

    bindEvents() {
        // Tab switching
        this.dom.tabBtns.forEach(btn => {
            btn.addEventListener('click', (e) => {
                this.dom.tabBtns.forEach(b => b.classList.remove('active'));
                this.dom.tabPanes.forEach(p => p.classList.remove('active'));
                e.target.classList.add('active');
                document.getElementById(e.target.dataset.target).classList.add('active');
            });
        });

        document.getElementById('btn-new-lab').addEventListener('click', () => this.openModal());
        
        this.dom.labForm.addEventListener('submit', (e) => {
            e.preventDefault();
            this.saveLab();
        });

        this.dom.btnAddSeat.addEventListener('click', () => this.addSeat());
    },

    // ── Labs CRUD ────────────────────────────────────────────────────────────

    async loadLabs() {
        this.dom.labsLoading.style.display = 'block';
        this.dom.labsTableBody.innerHTML = '';
        
        const { ok, data } = await api.get('/api/labs', true);
        this.dom.labsLoading.style.display = 'none';

        if (ok && data) {
            this.state.labs = data;
            this.renderLabs();
        } else {
            UI.toast('error', 'Failed to load labs.');
        }
    },

    renderLabs() {
        if (this.state.labs.length === 0) {
            this.dom.labsTableBody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-secondary);">No labs found.</td></tr>';
            return;
        }

        this.dom.labsTableBody.innerHTML = this.state.labs.map(lab => `
            <tr>
                <td>#${lab.labId}</td>
                <td style="font-weight:500;">${lab.name}</td>
                <td>${lab.roomNumber || '-'}</td>
                <td>
                    <span class="badge badge-${lab.labType === 0 ? 'Computer' : 'Science'}">
                        ${lab.labType === 0 ? 'Computer' : 'Science'}
                    </span>
                </td>
                <td>${lab.capacity}</td>
                <td>
                    <span class="badge badge-${lab.isActive ? 'active' : 'inactive'}">
                        ${lab.isActive ? 'Active' : 'Inactive'}
                    </span>
                </td>
                <td style="text-align:right;">
                    ${lab.labType === 0 ? 
                        `<button class="btn btn-ghost btn-sm" onclick="AdminLabs.openSeatsModal(${lab.labId})">Seats</button>` : ''}
                    <button class="btn btn-ghost btn-sm" onclick="AdminLabs.openModal(${lab.labId})">Edit</button>
                    ${lab.isActive ? 
                        `<button class="btn btn-ghost btn-sm" style="color:var(--error-color)" onclick="AdminLabs.deactivateLab(${lab.labId})">Deactivate</button>` : ''}
                </td>
            </tr>
        `).join('');
    },

    openModal(id = null) {
        UI.hideAlert('lab-alert');
        Validate.clearAll(this.dom.labForm);
        
        if (id) {
            const lab = this.state.labs.find(l => l.labId === id);
            if (!lab) return;
            this.dom.modalTitle.textContent = 'Edit Lab';
            this.dom.fId.value = lab.labId;
            this.dom.fName.value = lab.name;
            this.dom.fType.value = lab.labType;
            this.dom.fRoom.value = lab.roomNumber || '';
            this.dom.fCapacity.value = lab.capacity;
            this.dom.fActive.value = lab.isActive.toString();
            this.dom.fType.disabled = true; // Don't allow changing type after creation
        } else {
            this.dom.modalTitle.textContent = 'Create Lab';
            this.dom.fId.value = '';
            this.dom.fName.value = '';
            this.dom.fType.value = '0';
            this.dom.fType.disabled = false;
            this.dom.fRoom.value = '';
            this.dom.fCapacity.value = '30';
            this.dom.fActive.value = 'true';
        }
        
        this.dom.labModal.classList.add('show');
    },

    closeModal() {
        this.dom.labModal.classList.remove('show');
    },

    async saveLab() {
        const id = this.dom.fId.value;
        const payload = {
            name: this.dom.fName.value.trim(),
            roomNumber: this.dom.fRoom.value.trim() || null,
            capacity: parseInt(this.dom.fCapacity.value)
        };
        
        if (!payload.name) {
            UI.showAlert('lab-alert', 'error', 'Lab name is required.');
            return;
        }

        UI.setLoading(this.dom.btnSave, true);
        
        let ok, data;
        if (id) {
            // Update
            payload.isActive = this.dom.fActive.value === 'true';
            // Note: PUT UpdateLabDto does not have LabType.
            const res = await api.put(`/api/labs/${id}`, payload, true);
            ok = res.ok; data = res.data;
        } else {
            // Create
            payload.labType = parseInt(this.dom.fType.value);
            const res = await api.post('/api/labs', payload, true);
            ok = res.ok; data = res.data;
        }
        
        UI.setLoading(this.dom.btnSave, false);
        
        if (ok) {
            UI.toast('success', id ? 'Lab updated successfully.' : 'Lab created successfully.');
            this.closeModal();
            this.loadLabs();
        } else {
            UI.showAlert('lab-alert', 'error', data?.message || 'Failed to save lab.');
        }
    },

    async deactivateLab(id) {
        if (!confirm('Are you sure you want to deactivate this lab?')) return;
        const { ok, data } = await api.delete(`/api/labs/${id}`, true);
        if (ok) {
            UI.toast('success', 'Lab deactivated.');
            this.loadLabs();
        } else {
            UI.toast('error', data?.message || 'Failed to deactivate lab.');
        }
    },

    // ── Seats Management ──────────────────────────────────────────────────────

    async openSeatsModal(labId) {
        this.state.currentLabId = labId;
        const lab = this.state.labs.find(l => l.labId === labId);
        this.dom.seatsLabName.textContent = lab ? lab.name : '';
        this.dom.seatInput.value = '';
        UI.hideAlert('seats-alert');
        this.dom.seatsModal.classList.add('show');
        
        await this.loadSeats();
    },

    closeSeatsModal() {
        this.dom.seatsModal.classList.remove('show');
        this.state.currentLabId = null;
    },

    async loadSeats() {
        if (!this.state.currentLabId) return;
        
        this.dom.seatsContainer.innerHTML = '';
        this.dom.noSeatsMsg.style.display = 'none';
        this.dom.seatsLoading.style.display = 'block';

        // Wait, the API for retrieving ALL seats of a lab is NOT GET /api/labs/{id}/seats?date=...
        // Ah, the API GetSeatAvailability requires date and slot. 
        // Wait, is there a GET /api/labs/{id} that includes LabSeats? 
        // Let's check GET /api/labs/{id}.
        const { ok, data } = await api.get(`/api/labs/${this.state.currentLabId}`, true);
        this.dom.seatsLoading.style.display = 'none';

        if (ok && data) {
            // Assuming the GetById returns the lab with ActiveSeatCount or we can use it.
            // Wait, does the API expose a way to list ALL seats for admin?
            // The BRD says: GET /api/labs/{id}/seats (Wait, the controller didn't have a parameter-less GET seats).
            // Let's try to just fetch availability for a dummy future date to see all seats.
            const dummyDate = new Date(); dummyDate.setFullYear(dummyDate.getFullYear() + 1);
            const dummyDateStr = dummyDate.toISOString().split('T')[0];
            const dummySlot = "09:00:00";
            
            const seatsRes = await api.get(`/api/labs/${this.state.currentLabId}/seats?date=${dummyDateStr}&slot=${dummySlot}`, true);
            if (seatsRes.ok && seatsRes.data) {
                this.state.seats = seatsRes.data;
                this.renderSeats();
            } else {
                this.dom.noSeatsMsg.style.display = 'block';
            }
        }
    },

    renderSeats() {
        if (!this.state.seats || this.state.seats.length === 0) {
            this.dom.noSeatsMsg.style.display = 'block';
            return;
        }

        this.dom.seatsContainer.innerHTML = this.state.seats.map(seat => `
            <div class="seat-tag">
                ${seat.seatNumber}
                <button onclick="AdminLabs.removeSeat(${seat.seatId})" title="Remove Seat">&times;</button>
            </div>
        `).join('');
    },

    async addSeat() {
        const seatNum = this.dom.seatInput.value.trim();
        if (!seatNum) return;
        
        UI.setLoading(this.dom.btnAddSeat, true);
        const { ok, data } = await api.post(`/api/labs/${this.state.currentLabId}/seats`, { seatNumber: seatNum }, true);
        UI.setLoading(this.dom.btnAddSeat, false);

        if (ok) {
            this.dom.seatInput.value = '';
            this.loadSeats();
            this.loadLabs(); // Update active seat count on main table
        } else {
            UI.showAlert('seats-alert', 'error', data?.message || 'Failed to add seat.');
        }
    },

    async removeSeat(seatId) {
        if (!confirm('Remove this seat?')) return;
        
        const { ok, data } = await api.delete(`/api/labs/${this.state.currentLabId}/seats/${seatId}`, true);
        if (ok) {
            this.loadSeats();
            this.loadLabs();
        } else {
            UI.showAlert('seats-alert', 'error', data?.message || 'Failed to remove seat. It might be booked.');
        }
    },

    // ── All Bookings ─────────────────────────────────────────────────────────

    async loadBookings() {
        this.dom.bookingsLoading.style.display = 'block';
        this.dom.bookingsTableBody.innerHTML = '';
        
        const { ok, data } = await api.get('/api/lab-bookings', true);
        this.dom.bookingsLoading.style.display = 'none';

        if (ok && data) {
            this.state.bookings = data;
            // Sort by date descending
            this.state.bookings.sort((a, b) => new Date(b.reservationDate) - new Date(a.reservationDate));
            this.renderBookings();
        } else {
            this.dom.bookingsTableBody.innerHTML = '<tr><td colspan="7" style="text-align:center;">Failed to load bookings.</td></tr>';
        }
    },

    formatTimeSpan(ts) {
        if (!ts) return "";
        const [h, m] = ts.split(':');
        let hours = parseInt(h);
        const ampm = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12;
        hours = hours ? hours : 12; 
        return `${hours}:${m} ${ampm}`;
    },

    renderBookings() {
        if (this.state.bookings.length === 0) {
            this.dom.bookingsTableBody.innerHTML = '<tr><td colspan="7" style="text-align:center;color:var(--text-secondary);">No bookings found.</td></tr>';
            return;
        }

        this.dom.bookingsTableBody.innerHTML = this.state.bookings.map(b => {
            const dateStr = new Date(b.reservationDate).toLocaleDateString(undefined, { month: 'short', day: 'numeric', year: 'numeric' });
            const timeStr = `${this.formatTimeSpan(b.startTime)} - ${this.formatTimeSpan(b.endTime)}`;
            const isCancellable = b.status !== 'Cancelled';

            return `
            <tr>
                <td>#${b.labReservationId}</td>
                <td>
                    <div style="font-weight:500;">${b.userName || 'Student ' + b.userId}</div>
                </td>
                <td>
                    <div>${b.labName || 'Lab ' + b.labId}</div>
                    ${b.seatNumber ? `<div style="font-size:0.75rem; color:var(--text-secondary);">Seat: ${b.seatNumber}</div>` : ''}
                </td>
                <td>${dateStr}</td>
                <td>${timeStr}</td>
                <td>
                    <span class="badge badge-${b.status === 'Approved' ? 'active' : (b.status === 'Cancelled' ? 'inactive' : 'Computer')}">
                        ${b.status}
                    </span>
                </td>
                <td style="text-align:right;">
                    ${isCancellable ? `<button class="btn btn-ghost btn-sm" style="color:var(--error-color);" onclick="AdminLabs.cancelBooking(${b.labReservationId})">Cancel</button>` : ''}
                </td>
            </tr>
            `;
        }).join('');
    },

    async cancelBooking(id) {
        if (!confirm('Are you sure you want to cancel this student\'s booking?')) return;
        
        const { ok, data } = await api.delete(`/api/lab-bookings/${id}`, true);
        if (ok) {
            UI.toast('success', 'Booking cancelled successfully.');
            this.loadBookings();
        } else {
            UI.toast('error', data?.message || 'Failed to cancel booking.');
        }
    }
};

document.addEventListener('DOMContentLoaded', () => AdminLabs.init());
