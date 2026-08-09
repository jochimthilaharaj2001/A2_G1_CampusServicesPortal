/**
 * Campus Services Portal — Module 3: Lab Reservation (Student Frontend)
 */

const LabsModule = {
    state: {
        labs: [],
        selectedLab: null,
        selectedDate: null,
        slots: [],
        selectedSlot: null,
        seats: [],
        selectedSeat: null
    },

    init() {
        if (!Auth.requireLogin()) return;
        UI.initNavbar();

        this.cacheDOM();
        this.bindEvents();
        
        // Setup initial date to tomorrow
        const tomorrow = new Date();
        tomorrow.setDate(tomorrow.getDate() + 1);
        this.dom.dateInput.value = tomorrow.toISOString().split('T')[0];
        
        this.loadLabs();
        this.loadMyReservations();

        if (window.location.hash === '#my-reservations') {
            document.querySelector('.tab-btn[data-target="tab-my"]')?.click();
        }
    },

    cacheDOM() {
        this.dom = {
            tabBtns: document.querySelectorAll('.tab-btn'),
            tabPanes: document.querySelectorAll('.tab-pane'),
            
            // Step 1: Labs
            labList: document.getElementById('lab-list'),
            labsLoading: document.getElementById('labs-loading'),
            noLabsMsg: document.getElementById('no-labs-msg'),
            step1: document.getElementById('step-1'),
            
            // Step 2: Date & Time
            dateInput: document.getElementById('booking-date'),
            slotList: document.getElementById('slot-list'),
            slotsLoading: document.getElementById('slots-loading'),
            noSlotsMsg: document.getElementById('no-slots-msg'),
            step2: document.getElementById('step-2'),
            
            // Step 3: Details
            step3: document.getElementById('step-3'),
            seatSelectionContainer: document.getElementById('seat-selection-container'),
            seatList: document.getElementById('seat-list'),
            seatsLoading: document.getElementById('seats-loading'),
            purposeInput: document.getElementById('booking-purpose'),
            btnConfirm: document.getElementById('btn-confirm-booking'),
            btnReset: document.getElementById('btn-reset'),
            
            // My Reservations
            myReservationsList: document.getElementById('my-reservations-list'),
            reservationsLoading: document.getElementById('reservations-loading'),
            noReservationsMsg: document.getElementById('no-reservations-msg')
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

        // Date change
        this.dom.dateInput.addEventListener('change', () => {
            if (this.state.selectedLab) {
                this.loadSlots();
            }
        });

        this.dom.btnReset.addEventListener('click', () => this.resetFlow());
        this.dom.btnConfirm.addEventListener('click', () => this.confirmBooking());
    },

    async loadLabs() {
        this.dom.labsLoading.style.display = 'block';
        this.dom.labList.innerHTML = '';
        this.dom.noLabsMsg.style.display = 'none';

        const { ok, data } = await api.get('/api/labs', true);
        this.dom.labsLoading.style.display = 'none';

        if (ok && data && data.length > 0) {
            this.state.labs = data.filter(l => l.isActive);
            if (this.state.labs.length === 0) {
                this.dom.noLabsMsg.style.display = 'block';
                return;
            }
            this.renderLabs();
        } else {
            this.dom.noLabsMsg.style.display = 'block';
        }
    },

    renderLabs() {
        this.dom.labList.innerHTML = this.state.labs.map(lab => `
            <div class="card lab-card" data-id="${lab.labId}">
                <div class="lab-card-header">
                    <h3 class="lab-title">${lab.name}</h3>
                    <span class="lab-type-badge type-${lab.labType === 0 ? 'Computer' : 'Science'}">
                        ${lab.labType === 0 ? '💻 Computer' : '🔬 Science'}
                    </span>
                </div>
                <div style="color:var(--text-secondary);font-size:0.875rem;">
                    <div>📍 Room: ${lab.roomNumber || 'N/A'}</div>
                    <div style="margin-top:0.25rem;">👥 Capacity: ${lab.capacity}</div>
                </div>
            </div>
        `).join('');

        this.dom.labList.querySelectorAll('.lab-card').forEach(card => {
            card.addEventListener('click', () => {
                this.dom.labList.querySelectorAll('.lab-card').forEach(c => c.classList.remove('selected'));
                card.classList.add('selected');
                
                const labId = parseInt(card.dataset.id);
                this.state.selectedLab = this.state.labs.find(l => l.labId === labId);
                
                this.dom.step2.classList.add('active');
                this.resetStep3();
                this.loadSlots();
            });
        });
    },

    async loadSlots() {
        const date = this.dom.dateInput.value;
        if (!date || !this.state.selectedLab) return;
        
        this.state.selectedDate = date;
        this.state.selectedSlot = null;
        this.resetStep3();
        
        this.dom.slotsLoading.style.display = 'block';
        this.dom.slotList.innerHTML = '';
        this.dom.noSlotsMsg.style.display = 'none';

        const { ok, data } = await api.get(`/api/labs/${this.state.selectedLab.labId}/slots?date=${date}`, true);
        this.dom.slotsLoading.style.display = 'none';

        if (ok && data && data.length > 0) {
            this.state.slots = data;
            this.renderSlots();
        } else {
            this.dom.noSlotsMsg.style.display = 'block';
        }
    },

    formatTimeSpan(ts) {
        // ts is "HH:mm:ss"
        if (!ts) return "";
        const [h, m] = ts.split(':');
        let hours = parseInt(h);
        const ampm = hours >= 12 ? 'PM' : 'AM';
        hours = hours % 12;
        hours = hours ? hours : 12; 
        return `${hours}:${m} ${ampm}`;
    },

    renderSlots() {
        this.dom.slotList.innerHTML = this.state.slots.map(slot => {
            const timeLabel = this.formatTimeSpan(slot);
            return `<button class="slot-btn" data-slot="${slot}">${timeLabel}</button>`;
        }).join('');

        this.dom.slotList.querySelectorAll('.slot-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                this.dom.slotList.querySelectorAll('.slot-btn').forEach(b => b.classList.remove('selected'));
                btn.classList.add('selected');
                this.state.selectedSlot = btn.dataset.slot;
                
                this.dom.step3.classList.add('active');
                
                if (this.state.selectedLab.labType === 0) { // Computer
                    this.dom.seatSelectionContainer.style.display = 'block';
                    this.loadSeats();
                } else {
                    this.dom.seatSelectionContainer.style.display = 'none';
                    this.state.selectedSeat = null;
                }
            });
        });
    },

    async loadSeats() {
        this.state.selectedSeat = null;
        this.dom.seatsLoading.style.display = 'block';
        this.dom.seatList.innerHTML = '';
        
        const labId = this.state.selectedLab.labId;
        const date = this.state.selectedDate;
        const slot = this.state.selectedSlot;

        const { ok, data } = await api.get(`/api/labs/${labId}/seats?date=${date}&slot=${slot}`, true);
        this.dom.seatsLoading.style.display = 'none';

        if (ok && data) {
            this.state.seats = data;
            this.renderSeats();
        }
    },

    renderSeats() {
        this.dom.seatList.innerHTML = this.state.seats.map(seat => `
            <button class="seat-btn" data-id="${seat.seatId}" ${seat.isBooked ? 'disabled' : ''}>
                ${seat.seatNumber}
            </button>
        `).join('');

        this.dom.seatList.querySelectorAll('.seat-btn:not(:disabled)').forEach(btn => {
            btn.addEventListener('click', () => {
                this.dom.seatList.querySelectorAll('.seat-btn').forEach(b => b.classList.remove('selected'));
                btn.classList.add('selected');
                this.state.selectedSeat = parseInt(btn.dataset.id);
            });
        });
    },

    resetStep3() {
        this.dom.step3.classList.remove('active');
        this.state.selectedSeat = null;
        this.dom.seatList.innerHTML = '';
        this.dom.seatSelectionContainer.style.display = 'none';
        this.dom.purposeInput.value = '';
        UI.hideAlert('booking-alert');
    },

    resetFlow() {
        this.state.selectedLab = null;
        this.state.selectedDate = null;
        this.state.selectedSlot = null;
        this.state.selectedSeat = null;
        
        this.dom.labList.querySelectorAll('.lab-card').forEach(c => c.classList.remove('selected'));
        this.dom.step2.classList.remove('active');
        this.dom.slotList.innerHTML = '';
        this.resetStep3();
    },

    async confirmBooking() {
        if (!this.state.selectedLab || !this.state.selectedDate || !this.state.selectedSlot) {
            UI.showAlert('booking-alert', 'error', 'Please select a lab, date, and time slot.');
            return;
        }

        if (this.state.selectedLab.labType === 0 && !this.state.selectedSeat) {
            UI.showAlert('booking-alert', 'error', 'Please select an available seat.');
            return;
        }

        UI.setLoading(this.dom.btnConfirm, true);
        
        // Calculate EndTime (StartTime + 1 hour)
        const startParts = this.state.selectedSlot.split(':');
        const endHour = parseInt(startParts[0]) + 1;
        const endSlot = `${endHour.toString().padStart(2, '0')}:${startParts[1]}:${startParts[2]}`;

        const payload = {
            userId: 0, // Handled by API auth claims
            labId: this.state.selectedLab.labId,
            seatId: this.state.selectedSeat || null,
            reservationDate: this.state.selectedDate,
            startTime: this.state.selectedSlot,
            endTime: endSlot,
            purpose: this.dom.purposeInput.value.trim()
        };

        const { ok, data } = await api.post('/api/lab-bookings', payload, true);
        UI.setLoading(this.dom.btnConfirm, false);

        if (ok) {
            UI.toast('success', 'Reservation confirmed successfully!');
            this.resetFlow();
            this.loadMyReservations();
            document.querySelector('.tab-btn[data-target="tab-my"]').click(); // switch tab
        } else {
            UI.showAlert('booking-alert', 'error', data?.message || 'Failed to confirm reservation.');
        }
    },

    // ── My Reservations ───────────────────────────────────────────────────────

    async loadMyReservations() {
        const user = Auth.getUser();
        this.dom.reservationsLoading.style.display = 'block';
        this.dom.myReservationsList.innerHTML = '';
        this.dom.noReservationsMsg.style.display = 'none';

        // Auth stores the student's record ID as `studentId`, not `id` or `userId`.
        // The reservations endpoint expects that Student ID and resolves it to its User ID server-side.
        if (!user?.studentId) {
            this.dom.reservationsLoading.style.display = 'none';
            UI.showAlert('booking-alert', 'error', 'Your student profile could not be identified. Please sign in again.');
            return;
        }

        try {
            const { ok, data } = await api.get(`/api/lab-bookings/student/${user.studentId}`, true);

            if (ok && Array.isArray(data) && data.length > 0) {
                // Sort by date descending
                data.sort((a, b) => new Date(b.reservationDate) - new Date(a.reservationDate));
                this.renderMyReservations(data);
            } else {
                this.dom.noReservationsMsg.style.display = 'block';
            }
        } catch (error) {
            console.error('Failed to load lab reservations:', error);
            this.dom.noReservationsMsg.style.display = 'block';
            UI.toast('error', 'Unable to load your reservations. Please try again.');
        } finally {
            this.dom.reservationsLoading.style.display = 'none';
        }
    },

    renderMyReservations(reservations) {
        this.dom.myReservationsList.innerHTML = reservations.map(res => {
            const dateStr = new Date(res.reservationDate).toLocaleDateString(undefined, { weekday: 'short', month: 'short', day: 'numeric', year: 'numeric' });
            const timeStr = `${this.formatTimeSpan(res.startTime)} - ${this.formatTimeSpan(res.endTime)}`;
            const isCancellable = (res.status === 'Pending' || res.status === 'Approved') && (new Date(res.reservationDate) >= new Date(new Date().setHours(0,0,0,0)));

            return `
                <div class="reservation-item" id="res-${res.labReservationId}">
                    <div class="res-info">
                        <h3>${res.labName} ${res.seatNumber ? `(Seat ${res.seatNumber})` : ''}</h3>
                        <div class="res-meta">
                            <span>📅 ${dateStr}</span>
                            <span>⏰ ${timeStr}</span>
                            <span class="status-badge status-${res.status}">${res.status}</span>
                        </div>
                    </div>
                    <div>
                        ${isCancellable ? `<button class="btn btn-ghost btn-sm" onclick="LabsModule.cancelReservation(${res.labReservationId})" style="color:var(--error-color)">Cancel</button>` : ''}
                    </div>
                </div>
            `;
        }).join('');
    },

    async cancelReservation(id) {
        if (!confirm('Are you sure you want to cancel this reservation?')) return;
        
        const { ok, data } = await api.delete(`/api/lab-bookings/${id}`, true);
        if (ok) {
            UI.toast('success', 'Reservation cancelled.');
            this.loadMyReservations();
            
            // If viewing same date/lab, refresh slots
            if (this.dom.step2.classList.contains('active')) {
                this.loadSlots();
            }
        } else {
            UI.toast('error', data?.message || 'Failed to cancel reservation.');
        }
    }
};

document.addEventListener('DOMContentLoaded', () => LabsModule.init());
