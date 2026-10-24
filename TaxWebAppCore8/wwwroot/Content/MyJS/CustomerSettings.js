(function () {
    'use strict';
    const form = document.getElementById('customerSettingsForm');
    if (!form) return;
    const service = document.getElementById('setting-ServiceType');
    const tabs = Array.from(form.querySelectorAll('[data-settings-tab]'));
    function selectTab(tab) {
        tabs.forEach(item => {
            const selected = item === tab;
            item.setAttribute('aria-selected', String(selected));
            item.tabIndex = selected ? 0 : -1;
            document.getElementById(item.dataset.settingsTab).hidden = !selected;
        });
    }
    tabs.forEach((tab, index) => {
        tab.addEventListener('click', () => selectTab(tab));
        tab.addEventListener('keydown', event => {
            let next;
            if (event.key === 'ArrowLeft') next = tabs[(index + 1) % tabs.length];
            if (event.key === 'ArrowRight') next = tabs[(index + tabs.length - 1) % tabs.length];
            if (event.key === 'Home') next = tabs[0];
            if (event.key === 'End') next = tabs[tabs.length - 1];
            if (next) { event.preventDefault(); selectTab(next); next.focus(); }
        });
    });
    const revealButtons = Array.from(form.querySelectorAll('[data-reveal]'));
    function mask(button) {
        document.getElementById(button.dataset.reveal).type = 'password';
        button.setAttribute('aria-pressed', 'false');
        button.setAttribute('aria-label', 'نمایش مقدار');
        button.querySelector('i').className = 'fa fa-eye';
    }
    revealButtons.forEach(button => button.addEventListener('click', () => {
        const input = document.getElementById(button.dataset.reveal);
        const reveal = input.type === 'password';
        input.type = reveal ? 'text' : 'password';
        button.setAttribute('aria-pressed', String(reveal));
        button.setAttribute('aria-label', reveal ? 'پنهان کردن مقدار' : 'نمایش مقدار');
        button.querySelector('i').className = reveal ? 'fa fa-eye-slash' : 'fa fa-eye';
    }));
    function updateService() {
        revealButtons.forEach(mask);
        document.getElementById('omidConnectionTest').hidden = service.value !== 'Omid';
        form.querySelectorAll('[data-services]').forEach(field => {
            const supported = field.dataset.services.split(' ');
            const visible = !field.dataset.services || supported.includes(service.value);
            field.hidden = !visible;
            field.querySelectorAll('input, select').forEach(input => { input.disabled = !visible; });
        });
        form.querySelectorAll('[data-settings-group]').forEach(group => {
            group.hidden = !Array.from(group.querySelectorAll('[data-services]')).some(field => !field.hidden);
        });
        const direct = service.value === 'Tax';
        form.querySelector('[data-field-label="CertificateName"]').textContent = direct ? 'نام گواهی' : 'نام کاربری سرویس';
        form.querySelector('[data-field-label="PASSWORD"]').textContent = direct ? 'رمز گواهی / توکن' : 'رمز عبور سرویس';
        form.querySelector('[data-field-label="FILE_KEY_PRIVATE"]').textContent = direct ? 'مسیر فایل کلید خصوصی روی سرور' : 'توکن سرویس';
    }
    service.addEventListener('change', updateService);
    updateService();
    window.addEventListener('pageshow', () => { updateService(); });
    document.addEventListener('visibilitychange', () => { if (document.hidden) revealButtons.forEach(mask); });

    const test = document.getElementById('testDatabase');
    const result = document.getElementById('databaseTestResult');
    let testRevision = 0;
    function clearResult() { testRevision++; result.hidden = true; }
    document.getElementById('databasePanel').addEventListener('input', clearResult);
    test.addEventListener('click', async () => {
        const revision = ++testRevision;
        test.disabled = true;
        test.setAttribute('aria-busy', 'true');
        result.hidden = false;
        result.className = 'settings-test-result';
        result.textContent = 'در حال بررسی اتصال…';
        const data = new FormData(form);
        // Send only database values and the anti-forgery/version tokens to this endpoint.
        for (const key of Array.from(data.keys())) {
            if (key.startsWith('Values[') && !document.getElementById('databasePanel').querySelector('[name="' + key + '"]')) data.delete(key);
        }
        const controller = new AbortController();
        const timer = setTimeout(() => controller.abort(), 16000);
        try {
            const response = await fetch(form.dataset.testUrl, { method: 'POST', body: data, credentials: 'same-origin', signal: controller.signal });
            const contentType = response.headers.get('content-type') || '';
            if (!contentType.includes('application/json')) throw new Error('invalid-response');
            const body = await response.json();
            if (revision !== testRevision) return;
            result.className = 'settings-test-result ' + (body.Success ? 'is-success' : 'is-error');
            result.textContent = body.Message || 'تست اتصال ناموفق بود.';
        } catch (error) {
            if (revision !== testRevision) return;
            result.className = 'settings-test-result is-error';
            result.textContent = error.name === 'AbortError' ? 'مهلت تست اتصال پایان یافت.' : 'پاسخ تست دریافت نشد؛ اتصال برنامه را بررسی کنید.';
        } finally {
            clearTimeout(timer);
            test.disabled = false;
            test.removeAttribute('aria-busy');
        }
    });
    const omidTest = document.getElementById('testOmid');
    const omidResult = document.getElementById('omidTestResult');
    let omidRevision = 0;
    let omidRequest;
    function clearOmidResult() {
        omidRevision++;
        omidResult.hidden = true;
        if (omidRequest) omidRequest.abort();
    }
    document.getElementById('sendingPanel').addEventListener('input', clearOmidResult);
    service.addEventListener('change', clearOmidResult);
    window.addEventListener('pageshow', clearOmidResult);
    omidTest.addEventListener('click', async () => {
        if (service.value !== 'Omid') return;
        const revision = ++omidRevision;
        omidTest.disabled = true;
        omidTest.setAttribute('aria-busy', 'true');
        omidResult.hidden = false;
        omidResult.className = 'settings-test-result';
        omidResult.textContent = 'در حال بررسی ارتباط و ورود به امید…';
        const data = new FormData(form);
        for (const key of Array.from(data.keys())) {
            if (key.startsWith('Values[') && key !== 'Values[ServiceType]' && !key.startsWith('Values[OMID_')) data.delete(key);
        }
        omidRequest = new AbortController();
        const timer = setTimeout(() => omidRequest.abort(), 24000);
        try {
            const response = await fetch(form.dataset.omidTestUrl, { method: 'POST', body: data, credentials: 'same-origin', signal: omidRequest.signal });
            const contentType = response.headers.get('content-type') || '';
            if (!contentType.includes('application/json')) throw new Error('invalid-response');
            const body = await response.json();
            if (revision !== omidRevision) return;
            omidResult.className = 'settings-test-result ' + (body.Success ? 'is-success' : 'is-error');
            omidResult.textContent = body.Message || 'تست ارتباط با امید ناموفق بود.';
        } catch (error) {
            if (revision !== omidRevision) return;
            omidResult.className = 'settings-test-result is-error';
            omidResult.textContent = error.name === 'AbortError' ? 'مهلت تست ارتباط با امید پایان یافت.' : 'پاسخ تست دریافت نشد؛ اتصال برنامه را بررسی کنید.';
        } finally {
            clearTimeout(timer);
            omidRequest = null;
            omidTest.disabled = false;
            omidTest.removeAttribute('aria-busy');
        }
    });
}());
