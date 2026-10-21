(() => {
    'use strict';
    const root = document.getElementById('omidDashboard');
    const refresh = document.getElementById('dashboardRefresh');
    const status = document.getElementById('dashboardStatus');
    const error = document.getElementById('dashboardError');
    const bars = document.getElementById('dashboardBars');
    const pieContent = document.getElementById('dashboardPieContent');
    const pie = document.getElementById('dashboardPie');
    const legend = document.getElementById('dashboardPieLegend');
    const pieGroups = [
        ['PieAwaitingInquiry', 'ارسال سالم، در انتظار استعلام', '#287ac0'],
        ['PieInquiryErrors', 'خطای استعلام', '#a86b12'],
        ['PieAccepted', 'استعلام موفق', '#17805f'],
        ['PieOmidErrors', 'خطای امید', '#c34857'],
        ['PieOther', 'سایر وضعیت‌ها', '#8795a5']
    ];
    const number = new Intl.NumberFormat('fa-IR');
    const keys = ['SentSuccessfully', 'InquiryErrors', 'InquirySucceeded', 'OmidErrors'];
    document.querySelectorAll('.sidebar-menu li.active').forEach(item => item.classList.remove('active'));
    document.getElementById('menu-omid-dashboard')?.classList.add('active');
    async function load() {
        if (refresh.disabled) return;
        refresh.disabled = true;
        root.setAttribute('aria-busy', 'true');
        error.hidden = true;
        bars.hidden = true;
        pieContent.hidden = true;
        root.querySelectorAll('[data-value]').forEach(item => { item.textContent = '—'; });
        status.textContent = 'در حال دریافت آمار…';
        try {
            const response = await fetch(root.dataset.url, { credentials: 'same-origin', cache: 'no-store' });
            const data = await response.json();
            if (!response.ok) throw new Error(data.Message || 'دریافت آمار ناموفق بود.');
            if (![...keys, 'Total', ...pieGroups.map(group => group[0])].every(key => Number.isSafeInteger(data[key]) && data[key] >= 0) || !Number.isFinite(Date.parse(data.CheckedAt)) ||
                pieGroups.reduce((sum, group) => sum + data[group[0]], 0) !== data.Total)
                throw new Error('پاسخ آمار معتبر نیست.');
            const max = Math.max(1, ...keys.map(key => data[key]));
            root.querySelectorAll('[data-value]').forEach(item => { item.textContent = number.format(data[item.dataset.value]); });
            root.querySelectorAll('[data-bar]').forEach(item => {
                const count = data[item.dataset.bar];
                item.style.width = `${count / max * 100}%`;
                item.setAttribute('aria-hidden', 'true');
            });
            bars.hidden = false;
            let offset = 0;
            const segments = [];
            legend.replaceChildren();
            for (const [key, label, color] of pieGroups) {
                const percent = data.Total ? data[key] / data.Total * 100 : 0;
                if (data[key] > 0) segments.push(`${color} ${offset}% ${offset + percent}%`);
                offset += percent;
                const row = document.createElement('li');
                const swatch = document.createElement('i');
                swatch.style.background = color;
                swatch.setAttribute('aria-hidden', 'true');
                const title = document.createElement('span');
                title.textContent = label;
                const value = document.createElement('strong');
                value.textContent = `${number.format(data[key])} (${new Intl.NumberFormat('fa-IR', { maximumFractionDigits: 1 }).format(percent)}٪)`;
                row.append(swatch, title, value);
                legend.appendChild(row);
            }
            pie.style.background = segments.length ? `conic-gradient(${segments.join(',')})` : '#e5ebf0';
            document.getElementById('dashboardPieTotal').textContent = number.format(data.Total);
            pie.setAttribute('aria-label', data.Total ? 'توزیع وضعیت فاکتورها؛ تعداد و درصد در راهنما' : 'هنوز فاکتوری ثبت نشده است');
            pieContent.hidden = false;
            const time = new Date(data.CheckedAt).toLocaleString('fa-IR', { timeZone: 'Asia/Tehran' });
            status.textContent = data.Total === 0 ? `هنوز فاکتوری ثبت نشده است. آخرین بررسی: ${time}` :
                `کل فاکتورهای ثبت‌شده: ${number.format(data.Total)} — آخرین بررسی: ${time} (به وقت تهران)`;
        } catch (e) {
            error.textContent = e instanceof SyntaxError ? 'پاسخ سرور معتبر نیست؛ دوباره تلاش کنید.' : e.message;
            error.hidden = false;
            status.textContent = 'آمار در دسترس نیست؛ برای تلاش مجدد «به‌روزرسانی آمار» را بزنید.';
        } finally {
            refresh.disabled = false;
            root.setAttribute('aria-busy', 'false');
        }
    }
    refresh.addEventListener('click', load);
    load();
})();
