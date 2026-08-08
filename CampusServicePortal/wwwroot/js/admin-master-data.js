/**
 * Admin Master Data — Faculties & Certificate Types (BRD Module 9)
 */
if (document.getElementById('master-data-page')) {
    if (!Auth.requireAdmin()) { /* redirected */ }
    else {
        UI.initNavbar();
        document.getElementById('logout-btn')?.addEventListener('click', () => Auth.logout());

        document.querySelectorAll('.tab-btn').forEach(btn => {
            btn.addEventListener('click', () => {
                document.querySelectorAll('.tab-btn').forEach(b => b.classList.remove('active'));
                document.querySelectorAll('.panel').forEach(p => p.classList.remove('active'));
                btn.classList.add('active');
                document.getElementById(`panel-${btn.dataset.tab}`)?.classList.add('active');
            });
        });

        loadFaculties();
        loadCerts();

        document.getElementById('faculty-form')?.addEventListener('submit', async (e) => {
            e.preventDefault();
            const btn = document.getElementById('faculty-submit');
            UI.hideAlert('faculty-alert');
            UI.setLoading(btn, true);
            try {
                const res = await api.post('/api/faculties', {
                    name: document.getElementById('faculty-name').value.trim(),
                    code: document.getElementById('faculty-code').value.trim() || null
                }, true);
                if (res.ok) {
                    e.target.reset();
                    UI.toast('success', 'Faculty created.');
                    await loadFaculties();
                } else {
                    UI.showAlert('faculty-alert', 'error', res.data?.message || 'Create failed.');
                }
            } catch {
                UI.showAlert('faculty-alert', 'error', 'Server error.');
            } finally {
                UI.setLoading(btn, false);
            }
        });

        document.getElementById('cert-form')?.addEventListener('submit', async (e) => {
            e.preventDefault();
            const btn = document.getElementById('cert-submit');
            UI.hideAlert('cert-alert');
            UI.setLoading(btn, true);
            try {
                const res = await api.post('/api/certificate-types', {
                    name: document.getElementById('cert-name').value.trim(),
                    description: document.getElementById('cert-desc').value.trim() || null
                }, true);
                if (res.ok) {
                    e.target.reset();
                    UI.toast('success', 'Certificate type created.');
                    await loadCerts();
                } else {
                    UI.showAlert('cert-alert', 'error', res.data?.message || 'Create failed.');
                }
            } catch {
                UI.showAlert('cert-alert', 'error', 'Server error.');
            } finally {
                UI.setLoading(btn, false);
            }
        });
    }
}

async function loadFaculties() {
    const tbody = document.getElementById('faculties-tbody');
    if (!tbody) return;
    try {
        const res = await api.get('/api/faculties', true);
        if (!res.ok) {
            tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;padding:1.5rem;color:var(--error);">Failed to load</td></tr>`;
            return;
        }
        const items = res.data || [];
        if (!items.length) {
            tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;padding:1.5rem;color:var(--text-muted);">No faculties yet.</td></tr>`;
            return;
        }
        tbody.innerHTML = items.map(f => `
            <tr>
              <td style="font-weight:600;">${esc(f.name)}</td>
              <td><code>${esc(f.code || '—')}</code></td>
              <td>${f.studentCount}</td>
              <td><span class="badge ${f.isActive ? 'badge-success' : 'badge-error'}">${f.isActive ? 'Active' : 'Inactive'}</span></td>
              <td>
                ${f.isActive
                    ? `<button class="btn btn-danger btn-sm" onclick="deactivateFaculty(${f.facultyId}, '${esc(f.name)}')">Deactivate</button>`
                    : `<button class="btn btn-success btn-sm" onclick="reactivateFaculty(${f.facultyId}, '${esc(f.name)}')">Reactivate</button>`}
              </td>
            </tr>`).join('');
    } catch {
        tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;padding:1.5rem;color:var(--error);">Server error</td></tr>`;
    }
}

async function loadCerts() {
    const tbody = document.getElementById('certs-tbody');
    if (!tbody) return;
    try {
        const res = await api.get('/api/certificate-types', true);
        if (!res.ok) {
            tbody.innerHTML = `<tr><td colspan="4" style="text-align:center;padding:1.5rem;color:var(--error);">Failed to load</td></tr>`;
            return;
        }
        const items = res.data || [];
        if (!items.length) {
            tbody.innerHTML = `<tr><td colspan="4" style="text-align:center;padding:1.5rem;color:var(--text-muted);">No certificate types yet.</td></tr>`;
            return;
        }
        tbody.innerHTML = items.map(c => `
            <tr>
              <td style="font-weight:600;">${esc(c.name)}</td>
              <td style="color:var(--text-secondary);">${esc(c.description || '—')}</td>
              <td><span class="badge ${c.isActive ? 'badge-success' : 'badge-error'}">${c.isActive ? 'Active' : 'Inactive'}</span></td>
              <td>
                ${c.isActive
                    ? `<button class="btn btn-danger btn-sm" onclick="deactivateCert(${c.certificateTypeId}, '${esc(c.name)}')">Deactivate</button>`
                    : `<button class="btn btn-success btn-sm" onclick="reactivateCert(${c.certificateTypeId}, '${esc(c.name)}')">Reactivate</button>`}
              </td>
            </tr>`).join('');
    } catch {
        tbody.innerHTML = `<tr><td colspan="4" style="text-align:center;padding:1.5rem;color:var(--error);">Server error</td></tr>`;
    }
}

window.deactivateFaculty = async (id, name) => {
    if (!confirm(`Deactivate faculty "${name}"? Linked students are retained.`)) return;
    const res = await api.delete(`/api/faculties/${id}`, true);
    if (res.ok) { UI.toast('success', 'Faculty deactivated.'); await loadFaculties(); }
    else UI.toast('error', res.data?.message || 'Failed.');
};

window.reactivateFaculty = async (id, name) => {
    const res = await api.put(`/api/faculties/${id}`, { name, isActive: true }, true);
    if (res.ok) { UI.toast('success', 'Faculty reactivated.'); await loadFaculties(); }
    else UI.toast('error', res.data?.message || 'Failed.');
};

window.deactivateCert = async (id, name) => {
    if (!confirm(`Deactivate certificate type "${name}"?`)) return;
    const res = await api.delete(`/api/certificate-types/${id}`, true);
    if (res.ok) { UI.toast('success', 'Certificate type deactivated.'); await loadCerts(); }
    else UI.toast('error', res.data?.message || 'Failed.');
};

window.reactivateCert = async (id, name) => {
    const res = await api.put(`/api/certificate-types/${id}`, { name, isActive: true }, true);
    if (res.ok) { UI.toast('success', 'Certificate type reactivated.'); await loadCerts(); }
    else UI.toast('error', res.data?.message || 'Failed.');
};

function esc(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;')
        .replace(/'/g, '&#x27;');
}
