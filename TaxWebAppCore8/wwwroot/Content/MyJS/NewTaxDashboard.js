(() => {
    'use strict';
    const root = document.getElementById('newtaxDashboard');
    if (!root) return;
    const byId = id => document.getElementById(id);
    const number = new Intl.NumberFormat('fa-IR');
    const demoCounts = { 1: 180, 2: 240, 3: 96, 4: 684 };
    const labels = { 1: 'ارسال نشده', 2: 'در انتظار استعلام', 3: 'ارسال‌شده با خطا', 4: 'ثبت موفق' };
    const groupStates = { Unsent: 1, Pending: 2, Failed: 3, Accepted: 4 };
    let detailTable, activeState, detailRequest = 0;
    let detailRows = [], retrying = false;
    const demoRows = {};
    const selected = new Set();
    const canSend = () => root.dataset.demo === 'true' && [1, 3].includes(activeState);
    function updateSelection() {
        byId('dashboardSendActions').hidden = !canSend();
        byId('dashboardSelectedCount').textContent = number.format(selected.size);
        byId('dashboardSendSelected').disabled = retrying || !canSend() || !selected.size;
        const selectAll = byId('dashboardSelectAll');
        selectAll.disabled = retrying || !canSend() || !detailRows.length
            || (!selected.size && detailTable && !detailTable.rows({ search: 'applied' }).data().toArray().length);
        selectAll.innerHTML = `<i class="fa fa-check-square-o" aria-hidden="true"></i> ${selected.size ? 'پاک کردن انتخاب‌ها' : 'انتخاب همه'}`;
        const header = byId('dashboardSelectPage');
        header.hidden = !canSend();
        const page = detailTable ? detailTable.rows({ page: 'current', search: 'applied' }).data().toArray() : [];
        const count = page.filter(row => selected.has(row.RealInvoiceNumber)).length;
        header.disabled = retrying || !canSend() || !page.length;
        header.checked = !!page.length && count === page.length;
        header.indeterminate = count > 0 && count < page.length;
        root.querySelectorAll('[data-dashboard-select]').forEach(box => {
            box.checked = selected.has(detailRows[Number(box.dataset.dashboardSelect)]?.RealInvoiceNumber);
            box.disabled = retrying;
        });
    }
    function sampleInvoices(status) {
        if (demoRows[status]) return demoRows[status];
        return demoRows[status] = Array.from({ length: demoCounts[status] }, (_, index) => ({
            RealInvoiceNumber: `DEMO-${status}-${String(index + 1).padStart(4, '0')}`,
            StoreName: ['شعبه مرکزی تهران', 'شعبه اصفهان', 'شعبه شیراز'][index % 3],
            CustomerName: ['فروشگاه بهار', 'شرکت توسعه سپهر', 'بازرگانی پارسیان', 'شرکت نوین تجارت'][index % 4],
            PersianCreateDate: `۱۴۰۵/۰۸/${number.format(index % 7 + 1).padStart(2, '۰')}`,
            NetPrice: (index + 1) * 1250000, TotalTax: (index + 1) * 125000,
            ReferenceNumber: status === 1 ? '' : `DEMO-REF-${status}-${String(index + 1).padStart(5, '0')}`,
            Caption: status === 3 ? ['شناسه اقتصادی خریدار نیازمند بررسی است', 'شناسه کالا معتبر نیست', 'جمع مالیات نیازمند بررسی است'][index % 3] : labels[status]
        }));
    }
    function invoiceTable() {
        if (detailTable) return detailTable;
        const safeText = $.fn.dataTable.render.text();
        detailTable = $('#dashboardInvoices').DataTable({
            data: [], pageLength: 10, lengthMenu: [10, 25, 50], order: [], autoWidth: false, deferRender: true,
            language: { emptyTable: 'صورتحسابی در این وضعیت وجود ندارد.', zeroRecords: 'نتیجه‌ای پیدا نشد.',
                search: 'جستجو در فهرست:', lengthMenu: 'نمایش _MENU_ ردیف', info: 'نمایش _START_ تا _END_ از _TOTAL_ صورتحساب',
                infoEmpty: 'بدون نتیجه', infoFiltered: '(از _MAX_ صورتحساب)', paginate: { next: 'بعدی', previous: 'قبلی' } },
            columns: [{ data: null, orderable: false, searchable: false,
                render: (_, type, row, meta) => type === 'display' && canSend()
                    ? `<input type="checkbox" data-dashboard-select="${meta.row}" aria-label="انتخاب صورتحساب" />` : ''
            }].concat(['RealInvoiceNumber', 'StoreName', 'CustomerName', 'PersianCreateDate', 'NetPrice', 'TotalTax', 'ReferenceNumber', 'Caption'].map(key => ({
                data: key, defaultContent: '', render: ['NetPrice', 'TotalTax'].includes(key)
                    ? (value, type) => type === 'display' ? number.format(Number(value) || 0) : value : safeText
            }))), drawCallback: updateSelection
        });
        return detailTable;
    }
    function markState(status) {
        root.querySelectorAll('[data-dashboard-state]').forEach(item => item.setAttribute('aria-pressed', String(Number(item.dataset.dashboardState) === status)));
    }
    async function showInvoices(status) {
        if (!labels[status]) return;
        const requestId = ++detailRequest;
        activeState = status; markState(status);
        const panel = byId('dashboardDetails'), hint = byId('dashboardDetailsStatus');
        panel.hidden = false; panel.setAttribute('aria-busy', 'true');
        byId('dashboardDetailsTitle').textContent = `صورتحساب‌های ${labels[status]}`;
        hint.textContent = 'در حال دریافت صورتحساب‌ها…';
        detailRows = []; selected.clear(); updateSelection();
        const table = invoiceTable(); table.search('').clear().draw();
        try {
            let rows;
            if (root.dataset.demo === 'true') rows = sampleInvoices(status);
            else {
                const response = await fetch(`${root.dataset.detailsUrl}?status=${status}`, { credentials: 'same-origin', cache: 'no-store' });
                const data = await response.json();
                if (!response.ok || !Array.isArray(data.Rows)) throw new Error(data.Message || 'فهرست دریافت نشد.');
                rows = data.Rows;
            }
            if (requestId !== detailRequest) return;
            detailRows = rows;
            table.clear().rows.add(rows).draw(); table.columns.adjust();
            updateSelection();
            hint.textContent = `${number.format(rows.length)} صورتحساب · ${labels[status]}${root.dataset.demo === 'true' ? ' · پیش‌نمایش' : ''}`;
        } catch (error) {
            if (requestId === detailRequest) hint.textContent = 'فهرست دریافت نشد؛ دوباره روی وضعیت کلیک کنید.';
        } finally { if (requestId === detailRequest) panel.setAttribute('aria-busy', 'false'); }
    }
    async function sendSelected() {
        if (retrying || !canSend() || !selected.size) return;
        const state = activeState, source = sampleInvoices(state);
        const invoices = source.filter(row => selected.has(row.RealInvoiceNumber));
        if (!invoices.length || invoices.length > 100) return;
        const action = state === 3 ? 'ارسال مجدد' : 'ارسال';
        if (!window.confirm(`${action} ${number.format(invoices.length)} صورتحساب انتخاب‌شده انجام شود؟\nاین عملیات در حالت آزمایشی شبیه‌سازی می‌شود.`)) return;
        retrying = true; updateSelection();
        try {
            // Preview only: move the same invoice, never call the send endpoint.
            const pending = sampleInvoices(2);
            for (const invoice of invoices) {
                source.splice(source.indexOf(invoice), 1);
                invoice.Caption = `${action} انجام شد؛ در انتظار استعلام`;
                invoice.ReferenceNumber = `DEMO-${state === 3 ? 'RETRY' : 'SEND'}-${invoice.RealInvoiceNumber}`;
                pending.push(invoice);
            }
            selected.clear();
            demoCounts[state] = source.length; demoCounts[2] = pending.length;
            await load();
            byId('dashboardDetailsStatus').textContent = `${action} ${number.format(invoices.length)} صورتحساب شبیه‌سازی شد؛ به «در انتظار استعلام» منتقل شدند.`;
        } finally { retrying = false; updateSelection(); }
    }
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
            let data;
            if (root.dataset.demo === 'true') {
                data = { Total: Object.values(demoCounts).reduce((a, b) => a + b, 0), Unsent: demoCounts[1], Pending: demoCounts[2], Accepted: demoCounts[4], Failed: demoCounts[3], Other: 0, CheckedAt: new Date().toISOString() };
            } else {
                const response = await fetch(root.dataset.url, { credentials: 'same-origin', cache: 'no-store' });
                data = await response.json();
                if (!response.ok) throw new Error(data.Message || 'آمار دریافت نشد.');
            }
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
                const row = document.createElement('li'), swatch = document.createElement('i'), name = document.createElement(groupStates[key] ? 'button' : 'span'), value = document.createElement('strong');
                if (groupStates[key]) { name.type = 'button'; name.dataset.dashboardState = groupStates[key]; name.setAttribute('aria-controls', 'dashboardDetails'); }
                swatch.style.background = color; swatch.setAttribute('aria-hidden', 'true'); name.textContent = label;
                value.textContent = `${number.format(data[key])} (${new Intl.NumberFormat('fa-IR', { maximumFractionDigits: 1 }).format(percent)}٪)`;
                row.append(swatch, name, value); legend.append(row);
            }
            byId('dashboardPie').style.background = segments.length ? `conic-gradient(${segments.join(',')})` : '#e5ebf0';
            byId('dashboardPieTotal').textContent = number.format(data.Total);
            byId('dashboardBars').hidden = false; byId('dashboardPieContent').hidden = false;
            const time = new Date(data.CheckedAt).toLocaleString('fa-IR', { timeZone: 'Asia/Tehran' });
            byId('dashboardStatus').textContent = root.dataset.demo === 'true'
                ? `${number.format(data.Total)} صورتحساب نمونه · حالت نمایشی موقت`
                : `${number.format(data.Total)} صورتحساب · آخرین بررسی: ${time} (به وقت تهران)`;
            if (activeState) await showInvoices(activeState);
        } catch (error) {
            byId('dashboardError').textContent = error instanceof SyntaxError ? 'پاسخ سرور معتبر نیست؛ دوباره تلاش کنید.' : error.message;
            byId('dashboardError').hidden = false; byId('dashboardStatus').textContent = 'آمار در دسترس نیست؛ به‌روزرسانی آمار را بزنید.';
        } finally { button.disabled = false; root.setAttribute('aria-busy', 'false'); }
    }
    document.querySelectorAll('.sidebar-menu li.active').forEach(item => item.classList.remove('active'));
    byId('menu-newtax-dashboard')?.classList.add('active');
    byId('dashboardRefresh').addEventListener('click', load);
    root.querySelectorAll('.dashboard-bar-row').forEach(item => {
        const status = groupStates[item.querySelector('[data-bar]')?.dataset.bar];
        if (!status) return;
        item.dataset.dashboardState = status; item.setAttribute('role', 'button'); item.tabIndex = 0;
        item.setAttribute('aria-controls', 'dashboardDetails');
        item.addEventListener('keydown', event => {
            if (event.key === 'Enter' || event.key === ' ') { event.preventDefault(); showInvoices(status); }
        });
    });
    root.addEventListener('click', event => {
        const target = event.target.closest('[data-dashboard-state]');
        if (target) showInvoices(Number(target.dataset.dashboardState));
    });
    root.addEventListener('change', event => {
        const box = event.target.closest('[data-dashboard-select]');
        if (!box || retrying || !canSend()) return;
        const row = detailRows[Number(box.dataset.dashboardSelect)];
        if (!row) return;
        if (box.checked && selected.size < 100) selected.add(row.RealInvoiceNumber);
        else if (!box.checked) selected.delete(row.RealInvoiceNumber);
        else byId('dashboardDetailsStatus').textContent = 'در هر نوبت حداکثر ۱۰۰ صورتحساب انتخاب کنید.';
        updateSelection();
    });
    byId('dashboardSelectPage').addEventListener('change', event => {
        if (retrying || !canSend() || !detailTable) return;
        const page = detailTable.rows({ page: 'current', search: 'applied' }).data().toArray();
        for (const row of page) {
            if (event.target.checked && selected.size < 100) selected.add(row.RealInvoiceNumber);
            else if (!event.target.checked) selected.delete(row.RealInvoiceNumber);
        }
        updateSelection();
    });
    byId('dashboardSendSelected').addEventListener('click', sendSelected);
    byId('dashboardSelectAll').addEventListener('click', () => {
        if (retrying || !canSend() || !detailTable) return;
        if (selected.size) { selected.clear(); updateSelection(); return; }
        const matches = detailTable.rows({ search: 'applied' }).data().toArray();
        matches.slice(0, 100).forEach(row => selected.add(row.RealInvoiceNumber));
        updateSelection();
        byId('dashboardDetailsStatus').textContent = matches.length > 100
            ? '۱۰۰ صورتحساب اول نتایج انتخاب شد؛ موارد بیشتر را در نوبت بعدی ارسال کنید.'
            : `${number.format(selected.size)} صورتحساب از نتایج انتخاب شد.`;
    });
    byId('dashboardDetailsClose').addEventListener('click', () => {
        ++detailRequest; const previous = activeState; activeState = null; markState(null);
        selected.clear(); updateSelection();
        byId('dashboardDetails').hidden = true; byId('dashboardDetails').setAttribute('aria-busy', 'false');
        root.querySelector(`.dashboard-metric[data-dashboard-state="${previous}"]`)?.focus();
    });
    load();
})();
