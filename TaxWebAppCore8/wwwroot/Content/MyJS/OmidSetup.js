(() => {
    'use strict';
    const root = document.getElementById('omidSetup');
    const rows = document.getElementById('setupRows');
    const summary = document.getElementById('setupSummary');
    const message = document.getElementById('setupMessage');
    const refresh = document.getElementById('setupRefresh');
    let busy = false;
    function cell(row, text) {
        const td = document.createElement('td');
        td.textContent = text;
        row.appendChild(td);
        return td;
    }
    function setBusy(value) {
        busy = value;
        refresh.disabled = value;
        rows.querySelectorAll('button').forEach(button => { button.disabled = value; });
        root.setAttribute('aria-busy', String(value));
    }
    async function request(url, options) {
        const response = await fetch(url, { credentials: 'same-origin', ...options });
        let body;
        try { body = await response.json(); }
        catch { throw new Error('پاسخ معتبر دریافت نشد؛ صفحه را تازه‌سازی کنید.'); }
        if (!response.ok) throw new Error(body.Message || 'درخواست انجام نشد.');
        return body;
    }
    async function load() {
        rows.replaceChildren();
        summary.textContent = 'در حال بررسی…';
        const body = await request(root.dataset.checkUrl, { cache: 'no-store' });
        const ready = body.Items.filter(item => item.Ready).length;
        summary.textContent = ` ${ready} از ${body.Items.length} مورد آماده است؛ ${body.Items.length - ready} مورد نیاز به بررسی دارد.`;
        for (const item of body.Items) {
            const row = document.createElement('tr');
            row.className = item.Ready ? 'success' : 'warning';
            cell(row, item.Ready ? '✓ آماده' : item.Exists ? '✕ ناسازگار' : '□ موجود نیست');
            const name = cell(row, `dbo.${item.Name} (${({ U: 'جدول', V: 'ویو', P: 'روال' })[item.Kind]})`);
            name.dir = 'ltr';
            cell(row, item.Description);
            const details = cell(row, item.Ready ? `بررسی موفق (${item.ColumnCount} ستون موردنیاز)` : item.Errors.join('\n'));
            details.style.whiteSpace = 'pre-wrap';
            const action = cell(row, item.Ready ? 'تأیید شد' : item.Exists ? 'نیاز به اصلاح ساختار' : 'ابتدا وابستگی‌ها / نصب سامانه فروش');
            if (item.CanCreate) {
                action.textContent = '';
                const button = document.createElement('button');
                button.type = 'button';
                button.className = 'btn btn-success btn-sm';
                button.textContent = 'ایجاد';
                button.addEventListener('click', () => create(item.Name));
                action.appendChild(button);
            }
            rows.appendChild(row);
        }
    }
    async function run(action) {
        if (busy) return;
        setBusy(true);
        message.textContent = '';
        try { await action(); }
        catch (error) { message.textContent = error.message; summary.textContent = 'بررسی کامل نشد.'; }
        finally { setBusy(false); }
    }
    async function create(name) {
        await run(async () => {
            const data = new URLSearchParams();
            data.set('name', name);
            data.set('__RequestVerificationToken', root.querySelector('input[name="__RequestVerificationToken"]').value);
            const body = await request(root.dataset.createUrl, { method: 'POST', body: data });
            message.textContent = body.Message;
            await load();
        });
    }
    refresh.addEventListener('click', () => run(load));
    run(load);
})();
