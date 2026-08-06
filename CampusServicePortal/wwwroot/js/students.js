/**
 * Campus Services Portal — Students Module
 * Handles: Student Profile (view/edit), Admin Directory (search, filter, deactivate/reactivate)
 * BRD Module 1 — Student Profile
 */

// ════════════════════════════════════════════════════════════════════════════
// STUDENT PROFILE PAGE (profile.html)
// ════════════════════════════════════════════════════════════════════════════
if (document.getElementById('profile-page')) {
    let currentStudent = null;
    let editMode = false;

    (async () => {
        if (!Auth.requireLogin()) return;
        UI.initNavbar();
        await loadProfile();
    })();

    async function loadProfile() {
        const user = Auth.getUser();
        const loadingEl = document.getElementById('profile-loading');
        const contentEl = document.getElementById('profile-content');

        if (loadingEl) loadingEl.style.display = 'flex';

        try {
            // Get student id from user's studentId stored at login
            // Since we only have userId in the token, we fetch by userId via index
            const res = await api.get(`/api/students/${user.userId}`, true);
            if (res.ok) {
                currentStudent = res.data;
                renderProfile(currentStudent);
                if (loadingEl) loadingEl.style.display = 'none';
                if (contentEl) contentEl.style.display = 'block';
            } else {
                UI.toast('error', res.data?.message || 'Failed to load profile.');
            }
        } catch {
            UI.toast('error', 'Server error. Could not load profile.');
        }
    }

    function renderProfile(student) {
        // Header
        document.getElementById('avatar-initial').textContent = student.fullName?.[0]?.toUpperCase() || '?';
        document.getElementById('profile-name').textContent   = student.fullName;
        document.getElementById('profile-index').textContent  = `Index: ${student.indexNumber}`;

        // Status badges
        const statusBadge   = document.getElementById('status-badge');
        const verifiedBadge = document.getElementById('verified-badge');

        statusBadge.textContent   = student.isActive ? '● Active' : '● Inactive';
        statusBadge.className     = `badge ${student.isActive ? 'badge-success' : 'badge-error'}`;
        verifiedBadge.textContent = student.emailVerified ? '✓ Verified' : '! Unverified';
        verifiedBadge.className   = `badge ${student.emailVerified ? 'badge-info' : 'badge-warning'}`;

        // Fields
        setField('field-email',    student.email);
        setField('field-phone',    student.phoneNumber || '—');
        setField('field-faculty',  student.faculty);
        setField('field-degree',   student.degreeProgram);
        setField('field-year',     student.enrollmentYear);
        setField('field-contact',  student.contactNumber || '—');
        setField('field-address',  student.address || '—');
        setField('field-joined',   formatDate(student.createdDate));

        // Form defaults
        if (document.getElementById('edit-fullname'))
            document.getElementById('edit-fullname').value  = student.fullName || '';
        if (document.getElementById('edit-phone'))
            document.getElementById('edit-phone').value     = student.phoneNumber || '';
        if (document.getElementById('edit-contact'))
            document.getElementById('edit-contact').value   = student.contactNumber || '';
        if (document.getElementById('edit-address'))
            document.getElementById('edit-address').value   = student.address || '';
        if (document.getElementById('edit-degree'))
            document.getElementById('edit-degree').value    = student.degreeProgram || '';
    }

    function setField(id, value) {
        const el = document.getElementById(id);
        if (el) el.textContent = value;
    }

    // Toggle edit mode
    document.getElementById('edit-btn')?.addEventListener('click', () => {
        editMode = true;
        document.getElementById('view-mode').style.display  = 'none';
        document.getElementById('edit-mode').style.display  = 'block';
    });

    document.getElementById('cancel-btn')?.addEventListener('click', () => {
        editMode = false;
        document.getElementById('edit-mode').style.display  = 'none';
        document.getElementById('view-mode').style.display  = 'block';
        Validate.clearAll(document.getElementById('edit-form'));
        UI.hideAlert('profile-alert');
    });

    // Save profile
    document.getElementById('edit-form')?.addEventListener('submit', async (e) => {
        e.preventDefault();
        const btnEl = document.getElementById('save-btn');
        Validate.clearAll(e.target);
        UI.hideAlert('profile-alert');

        const fullname = document.getElementById('edit-fullname');
        const contact  = document.getElementById('edit-contact');
        let valid = true;

        if (!Validate.required(fullname.value)) {
            Validate.showError(fullname, 'Full name is required.'); valid = false;
        }
        if (!Validate.phone(contact.value)) {
            Validate.showError(contact, 'Enter a valid phone number.'); valid = false;
        }
        if (!valid) return;

        UI.setLoading(btnEl, true);
        try {
            const res = await api.put(`/api/students/${currentStudent.studentId}`, {
                fullName:     fullname.value.trim(),
                phoneNumber:  document.getElementById('edit-phone').value.trim(),
                contactNumber:contact.value.trim(),
                address:      document.getElementById('edit-address').value.trim(),
                degreeProgram:document.getElementById('edit-degree').value.trim()
            });

            if (res.ok) {
                currentStudent = res.data;
                renderProfile(currentStudent);
                // Update stored user name
                const user = Auth.getUser();
                user.fullName = res.data.fullName;
                Auth.setUser(user);
                UI.initNavbar();
                document.getElementById('edit-mode').style.display = 'none';
                document.getElementById('view-mode').style.display = 'block';
                editMode = false;
                UI.toast('success', 'Profile updated successfully!');
            } else {
                UI.showAlert('profile-alert', 'error', res.data?.message || 'Update failed.');
            }
        } catch {
            UI.showAlert('profile-alert', 'error', 'Server error. Please try again.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });

    // Logout
    document.getElementById('logout-btn')?.addEventListener('click', () => Auth.logout());
}

// ════════════════════════════════════════════════════════════════════════════
// ADMIN — STUDENTS DIRECTORY (admin/students.html)
// ════════════════════════════════════════════════════════════════════════════
if (document.getElementById('students-admin-page')) {
    let currentPage = 1;
    const pageSize  = 15;
    let totalPages  = 1;
    let pendingDeactivateId = null;

    (async () => {
        if (!Auth.requireAdmin()) return;
        UI.initNavbar();
        await loadStudents();
    })();

    // Search on input change (debounced)
    let debounceTimer;
    document.getElementById('search-input')?.addEventListener('input', () => {
        clearTimeout(debounceTimer);
        debounceTimer = setTimeout(() => { currentPage = 1; loadStudents(); }, 400);
    });

    document.getElementById('faculty-filter')?.addEventListener('change', () => {
        currentPage = 1;
        loadStudents();
    });

    async function loadStudents() {
        const search  = document.getElementById('search-input')?.value.trim() || '';
        const faculty = document.getElementById('faculty-filter')?.value || '';
        const tableBody = document.getElementById('students-tbody');
        const countEl  = document.getElementById('result-count');

        if (tableBody) {
            tableBody.innerHTML = `<tr><td colspan="7" style="text-align:center;padding:2rem;color:var(--text-muted)">
                <div class="spinner" style="width:28px;height:28px;margin:0 auto 0.5rem;"></div>Loading…</td></tr>`;
        }

        try {
            const params = new URLSearchParams({ page: currentPage, pageSize });
            if (search)  params.set('search', search);
            if (faculty) params.set('faculty', faculty);

            const res = await api.get(`/api/students?${params}`, true);
            if (!res.ok) { UI.toast('error', res.data?.message || 'Failed to load students.'); return; }

            const { items, totalCount, totalPages: tp } = res.data;
            totalPages = tp;

            if (countEl) countEl.textContent = `${totalCount} student${totalCount !== 1 ? 's' : ''} found`;

            if (!items.length) {
                tableBody.innerHTML = `<tr><td colspan="7" style="text-align:center;padding:2rem;color:var(--text-muted)">
                    No students found matching your search.</td></tr>`;
                renderPagination();
                return;
            }

            tableBody.innerHTML = items.map(s => `
                <tr>
                    <td>
                        <div style="display:flex;align-items:center;gap:0.625rem;">
                            <div class="user-avatar" style="width:32px;height:32px;font-size:0.8rem;">
                                ${s.fullName?.[0]?.toUpperCase() || '?'}
                            </div>
                            <div>
                                <div style="font-weight:600;">${escHtml(s.fullName)}</div>
                                <div style="font-size:0.75rem;color:var(--text-muted);">${escHtml(s.email)}</div>
                            </div>
                        </div>
                    </td>
                    <td><code style="color:var(--accent);font-size:0.8rem;">${escHtml(s.indexNumber)}</code></td>
                    <td>${escHtml(s.faculty)}</td>
                    <td>${escHtml(s.degreeProgram)}</td>
                    <td>
                        <span class="badge ${s.isActive ? 'badge-success' : 'badge-error'}">
                            ${s.isActive ? '● Active' : '● Inactive'}
                        </span>
                    </td>
                    <td>
                        <span class="badge ${s.emailVerified ? 'badge-info' : 'badge-warning'}">
                            ${s.emailVerified ? '✓ Verified' : '! Unverified'}
                        </span>
                    </td>
                    <td>
                        <div class="table-actions">
                            <a href="/admin/student-detail.html?id=${s.studentId}" class="btn btn-ghost btn-sm" title="View Profile">
                                👁
                            </a>
                            ${s.isActive
                                ? `<button class="btn btn-danger btn-sm" onclick="confirmDeactivate(${s.studentId}, '${escHtml(s.fullName)}')" title="Deactivate">🚫</button>`
                                : `<button class="btn btn-success btn-sm" onclick="reactivateStudent(${s.studentId}, '${escHtml(s.fullName)}')" title="Reactivate">✅</button>`
                            }
                        </div>
                    </td>
                </tr>`).join('');

            renderPagination();
        } catch {
            UI.toast('error', 'Server error. Could not load students.');
        }
    }

    // Deactivation — check blockers first
    window.confirmDeactivate = async (id, name) => {
        pendingDeactivateId = id;
        document.getElementById('deactivate-name').textContent = name;
        const blockersEl = document.getElementById('deactivation-blockers');

        // Show modal while checking
        document.getElementById('deactivate-modal').classList.add('show');
        blockersEl.innerHTML = `<div class="flex-center" style="gap:0.75rem;padding:0.75rem 0;">
            <div class="spinner" style="width:20px;height:20px;"></div> Checking active commitments…</div>`;
        document.getElementById('confirm-deactivate-btn').disabled = true;

        try {
            const res = await api.get(`/api/admin/students/${id}/deactivation-check`, true);
            if (res.ok) {
                const { canDeactivate, blockingReasons } = res.data;
                if (canDeactivate) {
                    blockersEl.innerHTML = `<div class="alert alert-success" style="margin:0;">
                        <span class="alert-icon">✅</span><span>No active commitments. Safe to deactivate.</span></div>`;
                    document.getElementById('confirm-deactivate-btn').disabled = false;
                } else {
                    blockersEl.innerHTML = `
                        <div class="alert alert-warning" style="margin:0;flex-direction:column;gap:0.5rem;">
                            <div style="display:flex;align-items:center;gap:0.5rem;"><span>⚠️</span><strong>Cannot deactivate. Active commitments:</strong></div>
                            <ul style="margin-left:1rem;">${blockingReasons.map(r => `<li>${escHtml(r)}</li>`).join('')}</ul>
                        </div>`;
                    document.getElementById('confirm-deactivate-btn').disabled = true;
                }
            }
        } catch {
            blockersEl.innerHTML = `<div class="alert alert-error" style="margin:0;">
                <span>❌</span><span>Could not check deactivation status.</span></div>`;
        }
    };

    document.getElementById('confirm-deactivate-btn')?.addEventListener('click', async () => {
        if (!pendingDeactivateId) return;
        const btnEl = document.getElementById('confirm-deactivate-btn');
        UI.setLoading(btnEl, true);
        try {
            const res = await api.put(`/api/admin/students/${pendingDeactivateId}/deactivate`, {});
            if (res.ok) {
                closeDeactivateModal();
                UI.toast('success', 'Student account deactivated successfully.');
                await loadStudents();
            } else {
                UI.toast('error', res.data?.message || 'Deactivation failed.');
            }
        } catch {
            UI.toast('error', 'Server error during deactivation.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });

    window.reactivateStudent = async (id, name) => {
        if (!confirm(`Reactivate account for ${name}?`)) return;
        try {
            const res = await api.put(`/api/admin/students/${id}/reactivate`, {});
            if (res.ok) {
                UI.toast('success', `${name}'s account has been reactivated.`);
                await loadStudents();
            } else {
                UI.toast('error', res.data?.message || 'Reactivation failed.');
            }
        } catch {
            UI.toast('error', 'Server error during reactivation.');
        }
    };

    window.closeDeactivateModal = () => {
        document.getElementById('deactivate-modal')?.classList.remove('show');
        pendingDeactivateId = null;
    };

    function renderPagination() {
        const container = document.getElementById('pagination');
        if (!container) return;
        if (totalPages <= 1) { container.innerHTML = ''; return; }

        let html = `<button class="page-btn" ${currentPage===1?'disabled':''} onclick="goToPage(${currentPage-1})">‹</button>`;
        for (let i = 1; i <= totalPages; i++) {
            if (i === 1 || i === totalPages || Math.abs(i - currentPage) <= 1) {
                html += `<button class="page-btn ${i===currentPage?'active':''}" onclick="goToPage(${i})">${i}</button>`;
            } else if (Math.abs(i - currentPage) === 2) {
                html += `<span style="color:var(--text-muted);padding:0 0.25rem;">…</span>`;
            }
        }
        html += `<button class="page-btn" ${currentPage===totalPages?'disabled':''} onclick="goToPage(${currentPage+1})">›</button>`;
        container.innerHTML = html;
    }

    window.goToPage = (page) => { currentPage = page; loadStudents(); };

    // Logout
    document.getElementById('logout-btn')?.addEventListener('click', () => Auth.logout());
}

// ════════════════════════════════════════════════════════════════════════════
// Utility Helpers
// ════════════════════════════════════════════════════════════════════════════
function escHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#x27;');
}

function formatDate(dateStr) {
    if (!dateStr) return '—';
    return new Date(dateStr).toLocaleDateString('en-GB', {
        day: '2-digit', month: 'short', year: 'numeric'
    });
}
