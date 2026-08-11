/**
 * Campus Services Portal — API Configuration & Utilities
 * Module 1: Student Profile
 * One file per module per BRD frontend requirements.
 */

// Dynamically detect API origin or fall back to ASP.NET Core dev port 7133
const API_BASE = (window.location.protocol.startsWith('http') && window.location.port !== '5500')
    ? ''
    : 'https://localhost:7133';

// ── Token Management ────────────────────────────────────────────────────────
const Auth = {
    getToken:     () => localStorage.getItem('csp_token'),
    setToken:     (t) => localStorage.setItem('csp_token', t),
    removeToken:  () => localStorage.removeItem('csp_token'),

    getUser:      () => JSON.parse(localStorage.getItem('csp_user') || 'null'),
    setUser:      (u) => localStorage.setItem('csp_user', JSON.stringify(u)),
    removeUser:   () => localStorage.removeItem('csp_user'),

    isLoggedIn:   () => !!localStorage.getItem('csp_token'),

    isAdmin() {
        const u = this.getUser();
        return u?.role === 'Admin';
    },

    logout(reason = null) {
        this.removeToken();
        this.removeUser();
        const loginUrl = reason
            ? `/login.html?reason=${encodeURIComponent(reason)}`
            : '/login.html';
        window.location.replace(loginUrl);
    },

    requireLogin() {
        if (!this.isLoggedIn()) {
            window.location.href = '/login.html';
            return false;
        }
        return true;
    },

    requireAdmin() {
        if (!this.isLoggedIn()) { window.location.href = '/login.html'; return false; }
        if (!this.isAdmin())    { window.location.href = '/profile.html'; return false; }
        return true;
    }
};

// ── HTTP Client ─────────────────────────────────────────────────────────────
async function apiRequest(method, endpoint, body = null, requiresAuth = false) {
    const headers = { 'Content-Type': 'application/json' };

    if (requiresAuth) {
        const token = Auth.getToken();
        if (!token) { Auth.logout(); return; }
        headers['Authorization'] = `Bearer ${token}`;
    }

    const options = { method, headers };
    if (body) options.body = JSON.stringify(body);

    const response = await fetch(`${API_BASE}${endpoint}`, options);

    // Parse body (may or may not be JSON)
    let data = null;
    const contentType = response.headers.get('Content-Type') || '';
    if (contentType.includes('application/json')) {
        data = await response.json();
    } else {
        data = await response.text();
    }

    // A stored token can expire while the user is on the dashboard, or become
    // invalid after a development server/database reset. Clear it immediately
    // so protected pages do not stay open with profile widgets stuck loading.
    if (requiresAuth && response.status === 401) {
        Auth.logout('session-expired');
        return {
            ok: false,
            status: response.status,
            data: { message: 'Your session has expired. Please sign in again.' }
        };
    }

    return { ok: response.ok, status: response.status, data };
}

// Convenience wrappers
const api = {
    get:    (url, auth = false)       => apiRequest('GET',    url, null, auth),
    post:   (url, body, auth = false) => apiRequest('POST',   url, body, auth),
    put:    (url, body, auth = true)  => apiRequest('PUT',    url, body, auth),
    delete: (url, auth = true)        => apiRequest('DELETE', url, null, auth),
};

// ── Form Validation Helpers ─────────────────────────────────────────────────
const Validate = {
    required: (val) => val?.trim().length > 0,
    email:    (val) => /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(val?.trim()),
    minLen:   (val, n) => val?.trim().length >= n,
    phone:    (val) => !val || /^[\d\s\+\-\(\)]{7,20}$/.test(val),

    showError(inputEl, msg) {
        inputEl.classList.add('invalid');
        inputEl.classList.remove('valid');
        const err = inputEl.closest('.form-group')?.querySelector('.field-error');
        if (err) { err.textContent = msg; err.classList.add('show'); }
    },

    clearError(inputEl) {
        inputEl.classList.remove('invalid');
        inputEl.classList.add('valid');
        const err = inputEl.closest('.form-group')?.querySelector('.field-error');
        if (err) err.classList.remove('show');
    },

    clearAll(formEl) {
        formEl.querySelectorAll('input, select').forEach(el => {
            el.classList.remove('invalid', 'valid');
        });
        formEl.querySelectorAll('.field-error').forEach(el => {
            el.classList.remove('show');
        });
    }
};

// ── Alert / Toast UI ────────────────────────────────────────────────────────
const UI = {
    showAlert(containerId, type, message) {
        const el = document.getElementById(containerId);
        if (!el) return;
        const icons = { success: '✅', error: '❌', warning: '⚠️', info: 'ℹ️' };
        el.className = `alert alert-${type} global-alert show`;
        el.innerHTML = `<span class="alert-icon">${icons[type]}</span><span>${message}</span>`;
    },

    hideAlert(containerId) {
        const el = document.getElementById(containerId);
        if (el) el.classList.remove('show');
    },

    toast(type, message, duration = 4000) {
        let container = document.querySelector('.toast-container');
        if (!container) {
            container = document.createElement('div');
            container.className = 'toast-container';
            document.body.appendChild(container);
        }
        const icons = { success: '✅', error: '❌', warning: '⚠️', info: 'ℹ️' };
        const bgMap = {
            success: 'var(--success-bg)', error: 'var(--error-bg)',
            warning: 'var(--warning-bg)', info: 'var(--info-bg)'
        };
        const colorMap = {
            success: '#34d399', error: '#f87171', warning: '#fbbf24', info: '#22d3ee'
        };
        const toast = document.createElement('div');
        toast.className = 'toast';
        toast.style.background = bgMap[type];
        toast.style.border = `1px solid ${colorMap[type]}40`;
        toast.style.color = colorMap[type];
        toast.innerHTML = `<span>${icons[type]}</span><span>${message}</span>`;
        toast.onclick = () => toast.remove();
        container.appendChild(toast);
        setTimeout(() => toast.remove(), duration);
    },

    setLoading(btnEl, loading) {
        if (!btnEl) return;
        if (loading) {
            btnEl.disabled = true;
            btnEl.classList.add('loading');
        } else {
            btnEl.disabled = false;
            btnEl.classList.remove('loading');
        }
    },

    /** Add a persistent collapse/expand control to the desktop sidebar. */
    initSidebar() {
        const navbar = document.querySelector('.navbar');
        if (!navbar) return;

        let toggle = navbar.querySelector('#sidebar-toggle');
        if (!toggle) {
            toggle = document.createElement('button');
            toggle.id = 'sidebar-toggle';
            toggle.className = 'sidebar-toggle';
            toggle.type = 'button';
            navbar.appendChild(toggle);
        }

        navbar.querySelectorAll('.nav-link').forEach(link => {
            const label = link.textContent.trim();
            if (!label) return;
            link.dataset.short = label.charAt(0).toUpperCase();
            link.title = label;
        });

        const collapsed = localStorage.getItem('csp_sidebar_collapsed') === '1';
        document.body.classList.toggle('sidebar-collapsed', collapsed);
        toggle.setAttribute('aria-expanded', String(!collapsed));
        toggle.setAttribute('aria-label', collapsed ? 'Expand navigation' : 'Collapse navigation');
        toggle.title = collapsed ? 'Expand navigation' : 'Collapse navigation';
        toggle.textContent = collapsed ? '»' : '«';

        if (!toggle.dataset.bound) {
            toggle.addEventListener('click', () => {
                const nextCollapsed = !document.body.classList.contains('sidebar-collapsed');
                document.body.classList.toggle('sidebar-collapsed', nextCollapsed);
                localStorage.setItem('csp_sidebar_collapsed', nextCollapsed ? '1' : '0');
                toggle.setAttribute('aria-expanded', String(!nextCollapsed));
                toggle.setAttribute('aria-label', nextCollapsed ? 'Expand navigation' : 'Collapse navigation');
                toggle.title = nextCollapsed ? 'Expand navigation' : 'Collapse navigation';
                toggle.textContent = nextCollapsed ? '»' : '«';
            });
            toggle.dataset.bound = 'true';
        }
    },
    /** Populate navbar with logged-in user info */
    initNavbar() {
        const user = Auth.getUser();
        if (!user) return;
        this.normalizeNavigation();
        this.initSidebar();
        const nameEl = document.getElementById('nav-name');
        const avatarEl = document.getElementById('nav-avatar');
        const adminLinks = document.querySelectorAll('.admin-only');
        const logoutBtn = document.getElementById('logout-btn');
        if (nameEl) nameEl.textContent = user.fullName?.split(' ')[0] || 'User';
        if (avatarEl) avatarEl.textContent = (user.fullName?.[0] || 'U').toUpperCase();
        if (Auth.isAdmin()) adminLinks.forEach(el => el.style.display = '');
        else adminLinks.forEach(el => el.style.display = 'none');

        // Every page uses the shared navbar, so the logout action belongs here.
        // This prevents pages that only call UI.initNavbar() from rendering a non-functional button.
        if (logoutBtn && !logoutBtn.dataset.logoutBound) {
            logoutBtn.addEventListener('click', () => Auth.logout());
            logoutBtn.dataset.logoutBound = 'true';
        }
    },

    /** Keep each role on a consistent navigation menu. */
    normalizeNavigation() {
        const navbar = document.querySelector('.navbar');
        const links = navbar?.querySelector('.nav-links');
        if (!navbar || !links) return;

        const isAdmin = Auth.isAdmin();
        navbar.classList.toggle('admin-navbar', isAdmin);
        const currentPath = window.location.pathname.toLowerCase();
        const adminItems = [
            ['/admin/index.html', 'Dashboard'],
            ['/admin/students.html', 'Students'],
            ['/admin/labs.html', 'Labs'],
            ['/admin/events.html', 'Events'],
            ['/admin/master-list.html', 'Master List'],
            ['/admin/master-data.html', 'Master Data'],
            ['/admin/complaints.html', 'Complaints'],
            ['/admin/certificates.html', 'Certificates'],
            ['/admin/fees.html', 'Fees']
        ];
        const studentItems = [
            ['/dashboard.html', 'Dashboard'],
            ['/hostel-applications.html', 'Hostel Applications'],
            ['/notifications.html', 'Notifications'],
            ['/labs.html', 'Lab Reservations'],
            ['/events.html', 'Events'],
            ['/complaints.html', 'Complaints'],
            ['/certificates.html', 'Certificates'],
            ['/fees.html', 'Fees']
        ];
        const items = isAdmin ? adminItems : studentItems;

        links.innerHTML = items.map(([href, label]) => {
            const active = currentPath === href || (href === '/admin/students.html' && currentPath === '/admin/student-detail.html');
            return `<a href="${href}" class="nav-link${active ? ' active' : ''}">${label}</a>`;
        }).join('');
    }
};

// Faculty-specific programme options used by the admin student forms.
// The selected value remains a string, matching the existing Student.DegreeProgram field.
window.DegreePrograms = {
    catalog: {
        Computing: ['BSc (Hons) Computer Science', 'BSc (Hons) Software Engineering', 'BSc (Hons) Information Technology', 'BSc (Hons) Cyber Security', 'BSc (Hons) Data Science'],
        Engineering: ['BSc Engineering', 'BSc (Hons) Civil Engineering', 'BSc (Hons) Electrical & Electronic Engineering', 'BSc (Hons) Mechanical Engineering', 'BSc (Hons) Software Engineering'],
        Business: ['BBA Management', 'BBA Accounting', 'BBA Finance', 'BBA Marketing', 'BBA Business Analytics'],
        Science: ['BSc Computer Science', 'BSc Mathematics', 'BSc Physics', 'BSc Chemistry', 'BSc Biology'],
        Arts: ['BA Economics', 'BA English', 'BA Psychology', 'BA International Relations', 'BA History'],
        Medicine: ['MBBS', 'BSc Nursing', 'BSc Medical Laboratory Science'],
        Law: ['LLB (Hons)'],
        Education: ['BEd (Hons)', 'BA Education']
    },

    key(faculty) {
        return String(faculty || '').replace(/^Faculty of\s+/i, '').trim();
    },

    populate(select, faculty, currentValue = '') {
        if (!select) return;
        select.replaceChildren();

        if (!faculty) {
            const option = new Option('Select a faculty first...', '');
            select.add(option);
            select.disabled = true;
            return;
        }

        select.disabled = false;
        select.add(new Option('Select degree program...', ''));
        const options = this.catalog[this.key(faculty)] || ['General / Other Programme'];
        if (currentValue && !options.includes(currentValue)) options.unshift(currentValue);
        options.forEach(program => select.add(new Option(program, program)));
        select.value = currentValue || '';
    }
};

// ── Password Strength ────────────────────────────────────────────────────────
function checkPasswordStrength(password) {
    let score = 0;
    if (password.length >= 6)  score++;
    if (password.length >= 10) score++;
    if (/[A-Z]/.test(password)) score++;
    if (/[0-9]/.test(password)) score++;
    if (/[^A-Za-z0-9]/.test(password)) score++;
    const levels = [
        { label: 'Too short',  color: '#ef4444', width: '10%' },
        { label: 'Weak',       color: '#f97316', width: '25%' },
        { label: 'Fair',       color: '#eab308', width: '50%' },
        { label: 'Good',       color: '#84cc16', width: '75%' },
        { label: 'Strong',     color: '#22c55e', width: '90%' },
        { label: 'Very Strong',color: '#10b981', width: '100%' },
    ];
    return levels[Math.min(score, 5)];
}
