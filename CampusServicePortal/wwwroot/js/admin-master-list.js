/**
 * Campus Services Portal — Admin Master List Module
 * Handles: View master list, Search, CSV Import
 * BRD Module 1 — Student Registration Validation
 */

if (document.getElementById('master-list-page')) {

    (async () => {
        if (!Auth.requireAdmin()) return;
        UI.initNavbar();
        await loadMasterList();
    })();

    // Search debounce
    let debounce;
    document.getElementById('search-input')?.addEventListener('input', () => {
        clearTimeout(debounce);
        debounce = setTimeout(loadMasterList, 400);
    });

    async function loadMasterList() {
        const search   = document.getElementById('search-input')?.value.trim() || '';
        const tbody    = document.getElementById('master-tbody');
        const countEl  = document.getElementById('result-count');

        if (tbody) {
            tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;padding:2rem;color:var(--text-muted)">
                <div class="spinner" style="width:28px;height:28px;margin:0 auto 0.5rem;"></div>Loading…</td></tr>`;
        }

        try {
            const params = search ? `?search=${encodeURIComponent(search)}` : '';
            const res = await api.get(`/api/student-master${params}`, true);

            if (!res.ok) { UI.toast('error', res.data?.message || 'Failed to load master list.'); return; }

            const records = res.data;
            if (countEl) countEl.textContent = `${records.length} record${records.length !== 1 ? 's' : ''}`;

            if (!records.length) {
                tbody.innerHTML = `<tr><td colspan="6" style="text-align:center;padding:2rem;color:var(--text-muted)">
                    No records found.</td></tr>`;
                return;
            }

            tbody.innerHTML = records.map(r => `
                <tr>
                    <td><code style="color:var(--accent);font-size:0.82rem;">${escHtml(r.indexNumber)}</code></td>
                    <td style="font-weight:500;">${escHtml(r.fullName)}</td>
                    <td>${escHtml(r.faculty)}</td>
                    <td>${escHtml(r.degreeProgram)}</td>
                    <td>${r.enrollmentYear}</td>
                    <td>
                        <span class="badge ${r.isRegistered ? 'badge-success' : 'badge-gray'}">
                            ${r.isRegistered ? '✓ Registered' : '○ Not Registered'}
                        </span>
                    </td>
                </tr>`).join('');
        } catch {
            UI.toast('error', 'Server error. Could not load master list.');
        }
    }

    // CSV Import
    const importBtn  = document.getElementById('import-btn');
    const fileInput  = document.getElementById('csv-file');
    const dropZone   = document.getElementById('drop-zone');

    // Drag & Drop
    if (dropZone) {
        dropZone.addEventListener('dragover', (e) => {
            e.preventDefault();
            dropZone.classList.add('drag-over');
        });
        dropZone.addEventListener('dragleave', () => dropZone.classList.remove('drag-over'));
        dropZone.addEventListener('drop', (e) => {
            e.preventDefault();
            dropZone.classList.remove('drag-over');
            const file = e.dataTransfer.files[0];
            if (file) handleFileSelected(file);
        });
        dropZone.addEventListener('click', () => fileInput?.click());
    }

    fileInput?.addEventListener('change', () => {
        if (fileInput.files[0]) handleFileSelected(fileInput.files[0]);
    });

    function handleFileSelected(file) {
        const nameEl = document.getElementById('selected-filename');
        if (!file.name.endsWith('.csv')) {
            UI.toast('error', 'Please select a .csv file.');
            return;
        }
        if (nameEl) nameEl.textContent = `📄 ${file.name}`;
        document.getElementById('import-confirm-btn').style.display = 'inline-flex';
    }

    document.getElementById('import-confirm-btn')?.addEventListener('click', async () => {
        const file = fileInput?.files[0];
        if (!file) { UI.toast('warning', 'Please select a CSV file first.'); return; }

        const btnEl = document.getElementById('import-confirm-btn');
        UI.setLoading(btnEl, true);
        UI.hideAlert('import-alert');

        const formData = new FormData();
        formData.append('file', file);

        try {
            const token = Auth.getToken();
            const response = await fetch(`${API_BASE}/api/student-master/import`, {
                method: 'POST',
                headers: { 'Authorization': `Bearer ${token}` },
                body: formData
            });
            const data = await response.json();

            if (response.ok) {
                UI.showAlert('import-alert', 'success', data.message || 'Import successful!');
                document.getElementById('import-confirm-btn').style.display = 'none';
                document.getElementById('selected-filename').textContent = '';
                if (fileInput) fileInput.value = '';
                await loadMasterList();
            } else {
                UI.showAlert('import-alert', 'error', data.message || 'Import failed.');
            }
        } catch {
            UI.showAlert('import-alert', 'error', 'Server error during import. Please try again.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });

    // Logout
    document.getElementById('logout-btn')?.addEventListener('click', () => Auth.logout());
}

function escHtml(str) {
    if (!str) return '';
    return String(str)
        .replace(/&/g, '&amp;')
        .replace(/</g, '&lt;')
        .replace(/>/g, '&gt;')
        .replace(/"/g, '&quot;');
}
