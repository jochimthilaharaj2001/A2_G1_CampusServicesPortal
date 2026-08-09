if (document.getElementById('admin-student-detail-page')) {
    let currentStudent = null;
    let editMode = false;
    let studentId = null;
    let faculties = [];

    (async () => {
        if (!Auth.requireAdmin()) return;
        UI.initNavbar();

        const urlParams = new URLSearchParams(window.location.search);
        studentId = urlParams.get('id');
        
        if (!studentId) {
            UI.toast('error', 'Student ID missing');
            setTimeout(() => window.location.href = '/admin/students.html', 1500);
            return;
        }

        await loadFaculties();
        await loadProfile();
    })();

    async function loadFaculties() {
        try {
            const res = await api.get('/api/faculties?activeOnly=true', true);
            if (res.ok) faculties = res.data || [];
        } catch {
            faculties = [];
        }
    }

    async function loadProfile() {
        const loadingEl = document.getElementById('loading');
        const contentEl = document.getElementById('content');

        loadingEl.style.display = 'block';
        contentEl.style.display = 'none';

        try {
            const res = await api.get(`/api/students/${studentId}`, true);
            if (res.ok) {
                currentStudent = res.data;
                renderProfile(currentStudent);
                loadingEl.style.display = 'none';
                contentEl.style.display = 'block';
            } else {
                UI.toast('error', res.data?.message || 'Failed to load profile.');
            }
        } catch {
            UI.toast('error', 'Server error. Could not load profile.');
        }
    }

    function renderProfile(student) {
        document.getElementById('avatar').textContent = student.fullName?.[0]?.toUpperCase() || '?';
        document.getElementById('name').textContent   = student.fullName;
        document.getElementById('index').textContent  = `Index: ${student.indexNumber}`;

        const statusBadge   = document.getElementById('status-badge');
        const verifiedBadge = document.getElementById('verified-badge');

        statusBadge.textContent   = student.isActive ? '● Active' : '● Inactive';
        statusBadge.className     = `badge ${student.isActive ? 'badge-success' : 'badge-error'}`;
        verifiedBadge.textContent = student.emailVerified ? '✓ Verified' : '! Unverified';
        verifiedBadge.className   = `badge ${student.emailVerified ? 'badge-info' : 'badge-warning'}`;

        setField('view-faculty', student.faculty);
        setField('view-degree', student.degreeProgram);
        setField('view-year', student.enrollmentYear);
        setField('view-email', student.email);
        setField('view-phone', student.phoneNumber || '—');
        setField('view-contact', student.contactNumber || '—');
        setField('view-address', student.address || '—');
        setField('view-deactivated', student.deactivatedAt ? formatDate(student.deactivatedAt) : '—');

        document.getElementById('edit-fullname').value = student.fullName || '';
        document.getElementById('edit-email').value = student.email || '';
        populateFacultySelect(student.faculty);
        document.getElementById('edit-degree').value = student.degreeProgram || '';
        document.getElementById('edit-year').value = student.enrollmentYear || '';
        document.getElementById('edit-phone').value = student.phoneNumber || '';
        document.getElementById('edit-contact').value = student.contactNumber || '';
        document.getElementById('edit-address').value = student.address || '';
        document.getElementById('edit-active').checked = student.isActive;
        document.getElementById('edit-verified').checked = student.emailVerified;
    }

    function populateFacultySelect(currentValue) {
        const select = document.getElementById('edit-faculty');
        select.innerHTML = '';

        const names = new Set(faculties.map(f => f.name));
        if (currentValue) names.add(currentValue);

        names.forEach(name => {
            const opt = document.createElement('option');
            opt.value = name;
            opt.textContent = name;
            select.appendChild(opt);
        });

        select.value = currentValue || '';
    }

    function setField(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    function formatDate(dateStr) {
        if (!dateStr) return '—';
        return new Date(dateStr).toLocaleDateString('en-GB', {
            day: '2-digit', month: 'short', year: 'numeric',
            hour: '2-digit', minute: '2-digit'
        });
    }

    document.getElementById('edit-btn')?.addEventListener('click', () => {
        editMode = true;
        document.getElementById('view-mode').style.display  = 'none';
        document.getElementById('edit-mode').style.display  = 'block';
    });

    document.getElementById('cancel-btn')?.addEventListener('click', () => {
        editMode = false;
        document.getElementById('edit-mode').style.display  = 'none';
        document.getElementById('view-mode').style.display  = 'block';
        UI.hideAlert('edit-alert');
        renderProfile(currentStudent); // Reset form
    });

    document.getElementById('edit-form')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const btnEl = document.getElementById('save-btn');
        UI.hideAlert('edit-alert');

        const dto = {
            fullName: document.getElementById('edit-fullname').value.trim(),
            email: document.getElementById('edit-email').value.trim(),
            faculty: document.getElementById('edit-faculty').value,
            degreeProgram: document.getElementById('edit-degree').value.trim(),
            enrollmentYear: parseInt(document.getElementById('edit-year').value),
            phoneNumber: document.getElementById('edit-phone').value.trim() || null,
            contactNumber: document.getElementById('edit-contact').value.trim() || null,
            address: document.getElementById('edit-address').value.trim() || null,
            isActive: document.getElementById('edit-active').checked,
            emailVerified: document.getElementById('edit-verified').checked
        };

        UI.setLoading(btnEl, true);
        try {
            const res = await api.put(`/api/admin/students/${studentId}`, dto);
            if (res.ok) {
                currentStudent = res.data;
                renderProfile(currentStudent);
                document.getElementById('edit-mode').style.display = 'none';
                document.getElementById('view-mode').style.display = 'block';
                editMode = false;
                UI.toast('success', 'Student record updated successfully!');
            } else {
                UI.showAlert('edit-alert', 'error', res.data?.message || 'Update failed.');
            }
        } catch {
            UI.showAlert('edit-alert', 'error', 'Server error. Please try again.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });

    document.getElementById('logout-btn')?.addEventListener('click', () => Auth.logout());
}
