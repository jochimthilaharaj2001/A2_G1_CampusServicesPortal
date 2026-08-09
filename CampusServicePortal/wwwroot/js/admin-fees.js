const AdminFees = {
  state: { fees: [], types: [] },
  money(value) { return new Intl.NumberFormat('en-LK', { style: 'currency', currency: 'LKR' }).format(value); },
  esc(value) { const node = document.createElement('span'); node.textContent = value || ''; return node.innerHTML; },
  badge(status) { return `<span class="badge ${status === 'Paid' ? 'badge-success' : status === 'Outstanding' ? 'badge-warning' : 'badge-gray'}">${this.esc(status)}</span>`; },

  async init() {
    if (!Auth.requireAdmin()) return;
    UI.initNavbar();
    document.getElementById('logout-btn').onclick = Auth.logout;
    document.getElementById('fee-status-filter').onchange = () => this.loadFees();
    document.getElementById('fee-type-filter').onchange = () => this.loadFees();
    document.getElementById('new-type').onclick = () => this.openType();
    document.getElementById('assign-fee').onclick = () => this.openAssign();
    document.getElementById('fee-type-form').onsubmit = event => this.saveType(event);
    document.getElementById('assign-form').onsubmit = event => this.assign(event);
    await Promise.all([this.loadTypes(), this.loadFaculties()]);
    await this.loadFees();
  },

  async loadFaculties() {
    const result = await api.get('/api/faculties?activeOnly=true');
    const select = document.getElementById('assign-faculty');
    select.innerHTML = '<option value="">Select a faculty for bulk assignment</option>' + (result.ok
      ? result.data.map(faculty => `<option value="${this.esc(faculty.name)}">${this.esc(faculty.name)}</option>`).join('')
      : '<option value="">Unable to load faculties</option>');
  },

  async loadTypes() {
    const result = await api.get('/api/fee-types?includeInactive=true', true);
    this.state.types = result.ok ? result.data : [];
    document.getElementById('fee-type-filter').innerHTML = '<option value="">All fee types</option>' + this.state.types.map(type => `<option value="${type.feeTypeId}">${this.esc(type.name)}${type.isActive ? '' : ' (inactive)'}</option>`).join('');
    document.getElementById('assign-type').innerHTML = '<option value="">Select a fee type</option>' + this.state.types.filter(type => type.isActive).map(type => `<option value="${type.feeTypeId}">${this.esc(type.name)}</option>`).join('');
  },

  async loadFees() {
    const query = new URLSearchParams();
    const status = document.getElementById('fee-status-filter').value;
    const type = document.getElementById('fee-type-filter').value;
    if (status) query.set('status', status);
    if (type) query.set('feeTypeId', type);
    const result = await api.get(`/api/fee-payments${query.toString() ? `?${query}` : ''}`, true);
    this.state.fees = result.ok ? result.data : [];
    document.getElementById('fee-rows').innerHTML = !this.state.fees.length ? '<tr><td colspan="6" style="text-align:center">No fee assignments found.</td></tr>' : this.state.fees.map(fee => `<tr><td>${this.esc(fee.studentName)}<br><small>${this.esc(fee.indexNumber)}</small></td><td>${this.esc(fee.feeTypeName)}</td><td>${this.esc(fee.billingPeriod)}</td><td>${this.money(fee.amount)}</td><td>${this.badge(fee.status)}</td><td>${fee.status === 'Outstanding' ? `<button class="btn btn-ghost btn-sm" onclick="AdminFees.cancel(${fee.feePaymentId})">Cancel</button>` : '-'}</td></tr>`).join('');
  },

  openType() { document.getElementById('fee-type-form').reset(); document.getElementById('fee-type-modal').classList.add('show'); },
  closeType() { document.getElementById('fee-type-modal').classList.remove('show'); },
  openAssign() { document.getElementById('assign-form').reset(); document.getElementById('assign-modal').classList.add('show'); },
  closeAssign() { document.getElementById('assign-modal').classList.remove('show'); },

  async saveType(event) {
    event.preventDefault();
    const body = { name: document.getElementById('fee-type-name').value.trim(), description: document.getElementById('fee-type-description').value.trim(), isActive: true };
    const result = await api.post('/api/fee-types', body, true);
    if (!result.ok) return UI.showAlert('admin-fee-alert', 'error', result.data?.message || 'Unable to save fee type.');
    this.closeType(); await this.loadTypes(); UI.toast('success', 'Fee type created.');
  },

  async assign(event) {
    event.preventDefault();
    const studentIndexNumber = document.getElementById('assign-student').value.trim();
    const faculty = document.getElementById('assign-faculty').value;
    if (!studentIndexNumber && !faculty) return UI.showAlert('admin-fee-alert', 'error', 'Enter either a Student ID or select a Faculty.');
    const body = { studentIndexNumber: studentIndexNumber || null, faculty: faculty || null, feeTypeId: +document.getElementById('assign-type').value, billingPeriod: document.getElementById('assign-period').value.trim(), amount: +document.getElementById('assign-amount').value, notes: document.getElementById('assign-notes').value.trim() };
    const result = await api.post('/api/fee-payments/assign', body, true);
    if (!result.ok) return UI.showAlert('admin-fee-alert', 'error', result.data?.message || 'Unable to assign fee.');
    this.closeAssign(); await this.loadFees(); UI.toast('success', result.data.message);
  },

  async cancel(id) {
    if (!confirm('Cancel this unpaid fee assignment?')) return;
    const result = await api.delete(`/api/fee-payments/${id}`, true);
    if (!result.ok) return UI.showAlert('admin-fee-alert', 'error', result.data?.message || 'Unable to cancel fee.');
    await this.loadFees(); UI.toast('success', 'Fee assignment cancelled.');
  }
};

document.addEventListener('DOMContentLoaded', () => AdminFees.init());
