/**
 * Campus Services Portal — Authentication Module
 * Handles: Login, Register (2-step), Verify Email, Resend Verification,
 *          Forgot Password, Reset Password
 * BRD Module 1 — Student Profile
 */

// ════════════════════════════════════════════════════════════════════════════
// LOGIN
// ════════════════════════════════════════════════════════════════════════════
if (document.getElementById('login-form')) {
    const form = document.getElementById('login-form');
    const btnEl = document.getElementById('login-btn');
    const alert = 'login-alert';

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        Validate.clearAll(form);
        UI.hideAlert(alert);

        const email = document.getElementById('email');
        const password = document.getElementById('password');
        const role = document.getElementById('selected-role').value;
        let valid = true;

        if (role === 'Student' && !Validate.email(email.value)) {
            Validate.showError(email, 'Please enter a valid email address.'); valid = false;
        } else if (role === 'Admin' && !email.value.trim()) {
            Validate.showError(email, 'Username is required.'); valid = false;
        }

        if (!Validate.required(password.value)) {
            Validate.showError(password, 'Password is required.'); valid = false;
        }
        if (!valid) return;

        UI.setLoading(btnEl, true);
        try {
            const payload = {
                password: password.value,
                role: role
            };

            if (role === 'Student') {
                payload.email = email.value.trim();
            } else {
                payload.username = email.value.trim();
            }

            const res = await api.post('/api/auth/login', payload);

            if (res.ok) {
                Auth.setToken(res.data.token);
                Auth.setUser({
                    userId: res.data.userId,
                    studentId: res.data.studentId || null,
                    fullName: res.data.fullName,
                    email: res.data.email,
                    role: res.data.role
                });
                window.location.href = res.data.role === 'Admin'
                    ? '/admin/index.html'
                    : '/dashboard.html';
            } else {
                const msg = res.data?.message || 'Invalid email or password.';
                UI.showAlert(alert, 'error', msg);
            }
        } catch (err) {
            UI.showAlert(alert, 'error', 'Unable to connect to the server. Please try again.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });
}

// ════════════════════════════════════════════════════════════════════════════
// REGISTER — Step 1: Verify Index Number
// ════════════════════════════════════════════════════════════════════════════
let masterRecord = null; // pre-filled from master list

if (document.getElementById('step1-form')) {
    const step1Form = document.getElementById('step1-form');
    const step2Form = document.getElementById('step2-form');
    const verifyBtn = document.getElementById('verify-btn');
    const indexInput = document.getElementById('index-number');

    step1Form.addEventListener('submit', async (e) => {
        e.preventDefault();
        Validate.clearAll(step1Form);
        UI.hideAlert('step1-alert');

        const indexNumber = indexInput.value.trim().toUpperCase();
        if (!indexNumber) {
            Validate.showError(indexInput, 'Index number is required.');
            return;
        }

        UI.setLoading(verifyBtn, true);
        try {
            const res = await api.get(`/api/student-master/${encodeURIComponent(indexNumber)}`);

            if (res.ok) {
                masterRecord = res.data;
                // Show step 2 and pre-fill known fields
                step1Form.closest('.step-panel').classList.add('hidden');
                step2Form.closest('.step-panel').classList.remove('hidden');
                updateStepIndicator(2);
                document.getElementById('prefill-index').textContent = masterRecord.indexNumber;
                document.getElementById('prefill-name').textContent = masterRecord.fullName;
                document.getElementById('prefill-faculty').textContent = masterRecord.faculty;
                document.getElementById('prefill-degree').textContent = masterRecord.degreeProgram;
                document.getElementById('reg-index').value = masterRecord.indexNumber;
                document.getElementById('reg-faculty').value = masterRecord.faculty;
                document.getElementById('reg-fullname').value = masterRecord.fullName;
                document.getElementById('reg-degree').value = masterRecord.degreeProgram;
                document.getElementById('reg-year').value = masterRecord.enrollmentYear;
            } else {
                const msg = res.data?.message || 'Index number not found in university master list.';
                UI.showAlert('step1-alert', 'error', msg);
            }
        } catch (err) {
            UI.showAlert('step1-alert', 'error', 'Unable to connect to the server. Please try again.');
        } finally {
            UI.setLoading(verifyBtn, false);
        }
    });
}

// ════════════════════════════════════════════════════════════════════════════
// REGISTER — Step 2: Complete Registration
// ════════════════════════════════════════════════════════════════════════════
if (document.getElementById('step2-form')) {
    const form = document.getElementById('step2-form');
    const btnEl = document.getElementById('register-btn');
    const pwdEl = document.getElementById('reg-password');
    const fillEl = document.getElementById('strength-fill');
    const textEl = document.getElementById('strength-text');

    // Password strength meter
    if (pwdEl) {
        pwdEl.addEventListener('input', () => {
            const s = checkPasswordStrength(pwdEl.value);
            if (fillEl) { fillEl.style.width = s.width; fillEl.style.background = s.color; }
            if (textEl) { textEl.textContent = s.label; textEl.style.color = s.color; }
        });
    }

    form.addEventListener('submit', async (e) => {
        e.preventDefault();
        Validate.clearAll(form);
        UI.hideAlert('step2-alert');

        const email = document.getElementById('reg-email');
        const password = document.getElementById('reg-password');
        const confirm = document.getElementById('reg-confirm');
        const contact = document.getElementById('reg-contact');
        const address = document.getElementById('reg-address');
        let valid = true;

        if (!Validate.email(email.value)) {
            Validate.showError(email, 'Please enter a valid email address.'); valid = false;
        }
        if (!Validate.minLen(password.value, 8)) {
            Validate.showError(password, 'Password must be at least 8 characters.'); valid = false;
        }
        if (password.value !== confirm.value) {
            Validate.showError(confirm, 'Passwords do not match.'); valid = false;
        }
        if (!Validate.phone(contact.value)) {
            Validate.showError(contact, 'Enter a valid phone number (7–20 digits).'); valid = false;
        }
        if (!valid) return;

        UI.setLoading(btnEl, true);
        try {
            const res = await api.post('/api/auth/register', {
                fullName: document.getElementById('reg-fullname').value.trim(),
                email: email.value.trim(),
                password: password.value,
                indexNumber: document.getElementById('reg-index').value.trim(),
                faculty: document.getElementById('reg-faculty').value.trim(),
                degreeProgram: document.getElementById('reg-degree').value.trim(),
                enrollmentYear: parseInt(document.getElementById('reg-year').value),
                phoneNumber: contact.value.trim(),
                address: address.value.trim()
            });

            if (res.ok || res.status === 201) {
                // Show success panel
                form.closest('.step-panel').classList.add('hidden');
                document.getElementById('success-panel').classList.remove('hidden');
                updateStepIndicator(3);
            } else {
                const msg = res.data?.message || 'Registration failed. Please try again.';
                UI.showAlert('step2-alert', 'error', msg);
            }
        } catch (err) {
            UI.showAlert('step2-alert', 'error', 'Unable to connect to the server. Please try again.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });
}

function updateStepIndicator(activeStep) {
    document.querySelectorAll('.step').forEach((el, i) => {
        el.classList.remove('active', 'done');
        const stepNum = i + 1;
        if (stepNum < activeStep) el.classList.add('done');
        else if (stepNum === activeStep) el.classList.add('active');
    });
}

// ════════════════════════════════════════════════════════════════════════════
// VERIFY EMAIL
// ════════════════════════════════════════════════════════════════════════════
if (document.getElementById('verify-status')) {
    (async () => {
        const params = new URLSearchParams(window.location.search);
        const token = params.get('token');
        const statusEl = document.getElementById('verify-status');
        const resendSection = document.getElementById('resend-section');

        if (!token) {
            statusEl.innerHTML = `
                <div class="alert alert-error">
                    <span class="alert-icon">❌</span>
                    <span>No verification token found. Please use the link from your email.</span>
                </div>`;
            if (resendSection) resendSection.style.display = 'block';
            return;
        }

        statusEl.innerHTML = `<div class="flex-center" style="gap:1rem;padding:1rem;">
            <div class="spinner" style="width:32px;height:32px;"></div>
            <span style="color:var(--text-secondary)">Verifying your email…</span>
        </div>`;

        try {
            const res = await api.post('/api/auth/verify-email', { token });
            if (res.ok) {
                statusEl.innerHTML = `
                    <div class="alert alert-success">
                        <span class="alert-icon">✅</span>
                        <span>${res.data?.message || 'Email verified successfully! You can now log in.'}</span>
                    </div>`;
                setTimeout(() => window.location.href = '/login.html', 3000);
            } else {
                const msg = res.data?.message || 'Verification failed.';
                statusEl.innerHTML = `
                    <div class="alert alert-error">
                        <span class="alert-icon">❌</span>
                        <span>${msg}</span>
                    </div>`;
                if (resendSection) resendSection.style.display = 'block';
            }
        } catch (err) {
            statusEl.innerHTML = `
                <div class="alert alert-error">
                    <span class="alert-icon">❌</span>
                    <span>Unable to connect to the server. Please try again.</span>
                </div>`;
        }
    })();
}

// Resend Verification
if (document.getElementById('resend-form')) {
    document.getElementById('resend-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const emailEl = document.getElementById('resend-email');
        const btnEl = document.getElementById('resend-btn');
        UI.hideAlert('resend-alert');

        if (!Validate.email(emailEl.value)) {
            Validate.showError(emailEl, 'Enter a valid email address.'); return;
        }

        UI.setLoading(btnEl, true);
        try {
            const res = await api.post('/api/auth/resend-verification', { email: emailEl.value.trim() });
            if (res.ok) {
                UI.showAlert('resend-alert', 'success', 'A new verification email has been sent. Please check your inbox.');
                emailEl.value = '';
            } else {
                UI.showAlert('resend-alert', 'error', res.data?.message || 'Failed to resend email.');
            }
        } catch {
            UI.showAlert('resend-alert', 'error', 'Server error. Please try again.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });
}

// ════════════════════════════════════════════════════════════════════════════
// FORGOT PASSWORD
// ════════════════════════════════════════════════════════════════════════════
if (document.getElementById('forgot-form')) {
    document.getElementById('forgot-form').addEventListener('submit', async (e) => {
        e.preventDefault();
        const emailEl = document.getElementById('forgot-email');
        const btnEl = document.getElementById('forgot-btn');
        UI.hideAlert('forgot-alert');
        Validate.clearAll(e.target);

        if (!Validate.email(emailEl.value)) {
            Validate.showError(emailEl, 'Please enter a valid email address.'); return;
        }

        UI.setLoading(btnEl, true);
        try {
            // API always returns 200 (never reveals if email exists — BRD rule)
            await api.post('/api/auth/forgot-password', { email: emailEl.value.trim() });
            UI.showAlert('forgot-alert', 'success',
                'If your email is registered, you will receive a reset link shortly. Please check your inbox.');
            emailEl.value = '';
        } catch {
            UI.showAlert('forgot-alert', 'error', 'Server error. Please try again.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });
}

// ════════════════════════════════════════════════════════════════════════════
// RESET PASSWORD
// ════════════════════════════════════════════════════════════════════════════
if (document.getElementById('reset-form')) {
    const form = document.getElementById('reset-form');
    const btnEl = document.getElementById('reset-btn');
    const pwdEl = document.getElementById('new-password');
    const fillEl = document.getElementById('strength-fill');
    const textEl = document.getElementById('strength-text');
    const token = new URLSearchParams(window.location.search).get('token');

    if (!token) {
        UI.showAlert('reset-alert', 'error', 'No reset token found. Please use the link from your email.');
        if (form) form.style.display = 'none';
    }

    if (pwdEl) {
        pwdEl.addEventListener('input', () => {
            const s = checkPasswordStrength(pwdEl.value);
            if (fillEl) { fillEl.style.width = s.width; fillEl.style.background = s.color; }
            if (textEl) { textEl.textContent = s.label; textEl.style.color = s.color; }
        });
    }

    form?.addEventListener('submit', async (e) => {
        e.preventDefault();
        Validate.clearAll(form);
        UI.hideAlert('reset-alert');

        const newPwd = document.getElementById('new-password');
        const confirm = document.getElementById('confirm-password');
        let valid = true;

        if (!Validate.minLen(newPwd.value, 8)) {
            Validate.showError(newPwd, 'Password must be at least 8 characters.'); valid = false;
        }
        if (newPwd.value !== confirm.value) {
            Validate.showError(confirm, 'Passwords do not match.'); valid = false;
        }
        if (!valid) return;

        UI.setLoading(btnEl, true);
        try {
            const res = await api.post('/api/auth/reset-password', {
                token,
                newPassword: newPwd.value,
                confirmPassword: confirm.value
            });

            if (res.ok) {
                UI.showAlert('reset-alert', 'success',
                    'Password reset successful! Redirecting to login…');
                form.style.display = 'none';
                setTimeout(() => window.location.href = '/login.html', 3000);
            } else {
                UI.showAlert('reset-alert', 'error', res.data?.message || 'Reset failed. Please try again.');
            }
        } catch {
            UI.showAlert('reset-alert', 'error', 'Server error. Please try again.');
        } finally {
            UI.setLoading(btnEl, false);
        }
    });
}
