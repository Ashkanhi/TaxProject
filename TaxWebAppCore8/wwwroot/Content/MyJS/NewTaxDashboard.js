(() => {
    'use strict';
    const root = document.getElementById('newtaxDashboard');
    if (!root) return;
    const byId = id => document.getElementById(id);
    const number = new Intl.NumberFormat('fa-IR');
    const groups = [ ['Unsent', 'ارسال نشده', '#287ac0'], ['Pending', 'در انتظار استعلام', '#a86b12'],
        ['Accepted', 'ثبت موفق', '#17805f'], ['Failed', 'ارسال‌شده با خطا', '#c34857'], ['Other', 'سایر وضعیت‌ها', '#8795a5'] ];
    async function load() {
        const button = byId('dashboardRefresh');
        if (button.disabled) return;
        button.disabled = true; root.setAttribute('aria-busy', 'true');
        byId('dashboardError').hidden = true; byId('dashboardBars').hidden = true; byId('dashboardPieContent').hidden = true;
        root.querySelectorAll('[data-value]').forEach(item => { item.textContent = '—'; });
        byId('dashboardStatus').textContent = 'در حال دریافت آمار…';
        try {
            const response = await fetch(root.dataset.url, { credentials: 'same-origin', cache: 'no-store' });
            const data = await response.json();
            if (!response.ok) throw new Error(data.Message || 'آمار دریافت نشد.');
            if (!['Total', ...groups.map(group => group[0])].every(key => Number.isSafeInteger(data[key]) && data[key] >= 0)
                || groups.reduce((sum, group) => sum + data[group[0]], 0) !== data.Total || !Number.isFinite(Date.parse(data.CheckedAt)))
                throw new Error('پاسخ آمار معتبر نیست.');
            root.querySelectorAll('[data-value]').forEach(item => { item.textContent = number.format(data[item.dataset.value]); });
            const max = Math.max(1, ...groups.map(group => data[group[0]]));
            root.querySelectorAll('[data-bar]').forEach(item => { item.style.width = `${data[item.dataset.bar] / max * 100}%`; });
            let offset = 0;
            const segments = [], legend = byId('dashboardPieLegend'); legend.replaceChildren();
            for (const [key, label, color] of groups) {
                const percent = data.Total ? data[key] / data.Total * 100 : 0;
                if (data[key]) segments.push(`${color} ${offset}% ${offset + percent}%`);
                offset += percent;
                const row = document.createElement('li'), swatch = document.createElement('i'), name = document.createElement('span'), value = document.createElement('strong');
                swatch.style.background = color; swatch.setAttribute('aria-hidden', 'true'); name.textContent = label;
                value.textContent = `${number.format(data[key])} (${new Intl.NumberFormat('fa-IR', { maximumFractionDigits: 1 }).format(percent)}٪)`;
                row.append(swatch, name, value); legend.append(row);
            }
            byId('dashboardPie').style.background = segments.length ? `conic-gradient(${segments.join(',')})` : '#e5ebf0';
            byId('dashboardPieTotal').textContent = number.format(data.Total);
            byId('dashboardBars').hidden = false; byId('dashboardPieContent').hidden = false;
            const time = new Date(data.CheckedAt).toLocaleString('fa-IR', { timeZone: 'Asia/Tehran' });
            byId('dashboardStatus').textContent = `${number.format(data.Total)} صورتحساب · آخرین بررسی: ${time} (به وقت تهران)`;
        } catch (error) {
            byId('dashboardError').textContent = error instanceof SyntaxError ? 'پاسخ سرور معتبر نیست؛ دوباره تلاش کنید.' : error.message;
            byId('dashboardError').hidden = false; byId('dashboardStatus').textContent = 'آمار در دسترس نیست؛ به‌روزرسانی آمار را بزنید.';
        } finally { button.disabled = false; root.setAttribute('aria-busy', 'false'); }
    }
    document.querySelectorAll('.sidebar-menu li.active').forEach(item => item.classList.remove('active'));
    byId('menu-newtax-dashboard')?.classList.add('active');
    byId('dashboardRefresh').addEventListener('click', load);
    load();
})();
