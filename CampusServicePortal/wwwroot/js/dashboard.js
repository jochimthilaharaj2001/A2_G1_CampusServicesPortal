/**
 * Campus Services Portal - Student Dashboard
 * Presents profile details and live Lab Reservation data available to the signed-in student.
 */

const StudentDashboard = {
    dom: {},

    init() {
        if (!Auth.requireLogin()) return;
        if (Auth.isAdmin()) {
            window.location.href = '/admin/index.html';
            return;
        }

        UI.initNavbar();
        this.cacheDOM();
        this.load();
    },

    cacheDOM() {
        this.dom = {
            alert: document.getElementById('dashboard-alert'),
            firstName: document.getElementById('dashboard-first-name'),
            fullName: document.getElementById('dashboard-full-name'),
            profileAvatar: document.getElementById('dashboard-profile-avatar'),
            indexNumber: document.getElementById('dashboard-index-number'),
            degree: document.getElementById('dashboard-degree'),
            enrollmentYear: document.getElementById('dashboard-enrollment-year'),
            email: document.getElementById('dashboard-email'),
            upcomingLabs: document.getElementById('stat-upcoming-labs'),
            totalBookings: document.getElementById('stat-total-bookings'),
            faculty: document.getElementById('stat-faculty'),
            accountStatus: document.getElementById('stat-account-status'),
            bookingsLoading: document.getElementById('upcoming-bookings-loading'),
            bookingsList: document.getElementById('upcoming-bookings-list'),
            bookingsEmpty: document.getElementById('upcoming-bookings-empty')
        };
    },

    async load() {
        try {
            const profileResult = await api.get('/api/students/me', true);
            if (!profileResult?.ok || !profileResult.data) {
                throw new Error(profileResult?.data?.message || 'Unable to load your student profile.');
            }

            const profile = profileResult.data;
            this.renderProfile(profile);

            const user = Auth.getUser() || {};
            Auth.setUser({ ...user, studentId: profile.studentId, fullName: profile.fullName });
            UI.initNavbar();

            const bookingsResult = await api.get(`/api/lab-bookings/student/${profile.studentId}`, true);
            const bookings = bookingsResult?.ok && Array.isArray(bookingsResult.data)
                ? bookingsResult.data
                : [];
            this.renderBookings(bookings);
        } catch (error) {
            console.error('Failed to load student dashboard:', error);
            UI.showAlert('dashboard-alert', 'error', error.message || 'Unable to load the dashboard. Please try again.');
            this.renderBookings([]);
        }
    },

    renderProfile(profile) {
        const name = profile.fullName || 'Student';
        const firstName = name.trim().split(/\s+/)[0] || 'Student';
        const accountStatus = profile.isActive ? 'Active' : 'Inactive';

        this.dom.firstName.textContent = firstName;
        this.dom.fullName.textContent = name;
        this.dom.profileAvatar.textContent = firstName.charAt(0).toUpperCase();
        this.dom.indexNumber.textContent = profile.indexNumber || 'Student index unavailable';
        this.dom.degree.textContent = profile.degreeProgram || 'Not provided';
        this.dom.enrollmentYear.textContent = profile.enrollmentYear || 'Not provided';
        this.dom.email.textContent = profile.email || 'Not provided';
        this.dom.faculty.textContent = profile.faculty || 'Not provided';
        this.dom.accountStatus.textContent = accountStatus;
        this.dom.accountStatus.classList.toggle('status-negative', !profile.isActive);
    },

    renderBookings(bookings) {
        const today = new Date();
        today.setHours(0, 0, 0, 0);

        const upcoming = bookings
            .filter(booking => booking.status !== 'Cancelled' && this.bookingDate(booking) >= today)
            .sort((left, right) => this.bookingDate(left) - this.bookingDate(right));

        this.dom.upcomingLabs.textContent = upcoming.length;
        this.dom.totalBookings.textContent = bookings.length;
        this.dom.bookingsLoading.hidden = true;

        if (upcoming.length === 0) {
            this.dom.bookingsEmpty.hidden = false;
            this.dom.bookingsList.hidden = true;
            return;
        }

        this.dom.bookingsEmpty.hidden = true;
        this.dom.bookingsList.hidden = false;
        this.dom.bookingsList.innerHTML = upcoming.slice(0, 4).map(booking => `
            <article class="upcoming-booking-item">
              <div class="booking-date-box">
                <span>${this.bookingDate(booking).toLocaleDateString(undefined, { month: 'short' })}</span>
                <strong>${this.bookingDate(booking).getDate()}</strong>
              </div>
              <div class="booking-details">
                <h3>${this.escapeHtml(booking.labName || `Lab ${booking.labId}`)}</h3>
                <p>${booking.seatNumber ? `Seat: ${this.escapeHtml(booking.seatNumber)} · ` : ''}${this.formatTime(booking.startTime)} - ${this.formatTime(booking.endTime)}</p>
              </div>
              <span class="badge ${booking.status === 'Approved' ? 'badge-success' : 'badge-warning'}">${this.escapeHtml(booking.status || 'Pending')}</span>
            </article>`).join('');
    },

    bookingDate(booking) {
        const dateValue = String(booking.reservationDate || '').slice(0, 10);
        return new Date(`${dateValue}T00:00:00`);
    },

    formatTime(value) {
        if (!value) return '-';
        const [hourText, minutes] = String(value).split(':');
        let hour = Number.parseInt(hourText, 10);
        if (Number.isNaN(hour)) return value;
        const suffix = hour >= 12 ? 'PM' : 'AM';
        hour = hour % 12 || 12;
        return `${hour}:${minutes || '00'} ${suffix}`;
    },

    escapeHtml(value) {
        return String(value)
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;')
            .replace(/'/g, '&#039;');
    }
};

document.addEventListener('DOMContentLoaded', () => StudentDashboard.init());
