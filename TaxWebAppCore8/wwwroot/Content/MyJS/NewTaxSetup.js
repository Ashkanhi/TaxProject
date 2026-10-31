(() => {
    'use strict';
    const root = document.getElementById('newtaxSetup');
    if (!root) return;
    const rows = document.getElementById('newtaxSetupRows');
    const summary = document.getElementById('newtaxSetupSummary');
    const message = document.getElementById('newtaxSetupMessage');
    const refresh = document.getElementById('newtaxSetupRefresh');
    const number = new Intl.NumberFormat('fa-IR');
    function cell(row, value) {
        const td = document.createElement('td'); td.textContent = value; row.append(td); return td;
    }
    async function load() {
        if (refresh.disabled) return;
        refresh.disabled = true; root.setAttribute('aria-busy', 'true');
        rows.replaceChildren(); message.textContent = ''; summary.textContent = 'در حال بررسی…';
        try {
            const response = await fetch(root.dataset.checkUrl, { credentials: 'same-origin', cache: 'no-store' });
            if (!(response.headers.get('content-type') || '').includes('application/json')) throw new Error('پاسخ معتبر نیست؛ از منوی ابزارهای سیستم دوباره وارد شوید.');
            const body = await response.json();
            if (!response.ok || !Array.isArray(body.Items)) throw new Error(body.Message || 'بررسی انجام نشد.');
            const ready = body.Items.filter(item => item.Ready).length;
            summary.textContent = `${number.format(ready)} از ${number.format(body.Items.length)} مورد بررسی‌شده تأیید شد.`;
            for (const item of body.Items) {
                const row = document.createElement('tr'); row.className = item.Ready ? 'success' : 'warning';
                cell(row, item.Ready ? '✓ تأیید شد' : item.Exists ? '✕ نیازمند بررسی' : '□ موجود نیست / قابل مشاهده نیست');
                cell(row, `dbo.${item.Name} (${({ U: 'جدول', V: 'ویو', P: 'رویه' })[item.Kind] || item.Kind})`).dir = 'ltr';
                cell(row, item.Description);
                cell(row, item.Ready ? (item.Kind === 'P' ? 'وجود، تعداد پارامترها، مجوز اجرا و وابستگی‌ها تأیید شد؛ رویه اجرا نشده است.' : `${number.format(item.ColumnCount)} ستون مورد نیاز و قابلیت خواندن تأیید شد.`) : item.Errors.join('\n')).style.whiteSpace = 'pre-wrap';
                rows.append(row);
            }
        } catch (error) { summary.textContent = 'بررسی کامل نشد.'; message.textContent = error.message; }
        finally { refresh.disabled = false; root.setAttribute('aria-busy', 'false'); }
    }
    document.querySelectorAll('.sidebar-menu li.active').forEach(item => item.classList.remove('active'));
    document.getElementById('menu-newtax-setup')?.classList.add('active');
    refresh.addEventListener('click', load);
    load();
})();
