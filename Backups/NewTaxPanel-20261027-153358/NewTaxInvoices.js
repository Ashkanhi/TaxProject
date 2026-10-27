(() => {
    'use strict';
    const root = document.querySelector('[data-newtax-invoices]');
    if (!root) return;
    const inquiry = root.dataset.mode === 'inquiry';
    const form = root.querySelector('form');
    const byId = id => document.getElementById(id);
    const message = byId('newtaxStatusMessage');
    const selected = new Set();
    const number = new Intl.NumberFormat('fa-IR');
    let rows = [], table, busy = false;
    const text = value => String(value ?? '');
    const escape = value => text(value).replace(/[&<>"']/g, char => ({ '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' }[char]));
    const reference = row => row.fTaxHistoryReferenceNumber || row.taxReferenceNumber || row.ReferenceNumber || '';
    const state = row => Number(row.fSendState || 1);
    const eligible = row => inquiry ? !!reference(row) : [1, 3].includes(state(row));
    const statusLabel = row => ({ 1: 'ارسال نشده', 2: 'در انتظار استعلام', 3: 'ارسال‌شده با خطا', 4: 'ثبت موفق' }[state(row)] || 'نامشخص');
    const token = () => form.querySelector('[name="__RequestVerificationToken"]').value;
    function say(value, kind = 'info') {
        message.textContent = value;
        message.className = `newtax-operation-status is-${kind}`;
    }
    function updateSelection() {
        byId('newtaxSelectionCount').textContent = `${number.format(selected.size)} انتخاب‌شده`;
        byId('newtaxSubmit').disabled = busy || !selected.size;
        byId('newtaxSelectAll').disabled = busy || !rows.some(eligible);
        byId('newtaxSelectAll').innerHTML = `<i class="fa fa-check-square-o" aria-hidden="true"></i> ${selected.size ? 'پاک کردن انتخاب‌ها' : 'انتخاب همه نتایج'}`;
        byId('newtaxExport').disabled = busy || !rows.length;
        if (!table) return;
        const page = table.rows({ page: 'current', search: 'applied' }).data().toArray().filter(eligible);
        const count = page.filter(row => selected.has(row.InvoiceId)).length;
        const header = byId('newtaxSelectPage');
        header.disabled = busy || !page.length;
        header.checked = page.length > 0 && count === page.length;
        header.indeterminate = count > 0 && count < page.length;
        root.querySelectorAll('[data-invoice-select]').forEach(box => {
            box.checked = selected.has(box.dataset.invoiceSelect);
            box.closest('tr').classList.toggle('newtax-selected', box.checked);
            box.disabled = busy || box.dataset.eligible !== 'true';
        });
    }
    function setBusy(value) {
        busy = value;
        root.setAttribute('aria-busy', String(value));
        form.querySelectorAll('button, input:not([type="hidden"]), select').forEach(input => { input.disabled = value; });
        byId('newtaxTableSearch').disabled = value;
        updateSelection();
    }
    async function request(url, body, json = false) {
        const response = await fetch(url, {
            method: 'POST', credentials: 'same-origin',
            headers: json ? { 'Content-Type': 'application/json', RequestVerificationToken: token() } : {},
            body: json ? JSON.stringify(body) : body
        });
        if (!(response.headers.get('content-type') || '').includes('application/json'))
            throw new Error('پاسخ سرور دریافت نشد؛ صفحه را تازه کنید و دوباره تلاش کنید.');
        const result = await response.json();
        if (!response.ok) throw new Error(result.Message || 'عملیات تکمیل نشد.');
        return result;
    }
    function initTable() {
        byId('newtaxInvoices').querySelector('tbody').replaceChildren();
        const safeText = $.fn.dataTable.render.text();
        const value = name => ({ data: name, defaultContent: '', render: safeText });
        table = $('#newtaxInvoices').DataTable({
            data: [], pageLength: 25, lengthMenu: [10, 25, 50, 100], order: [],
            dom: 'lrtip', autoWidth: false,
            language: { emptyTable: 'صورتحسابی برای این فیلترها پیدا نشد.', zeroRecords: 'نتیجه‌ای با این عبارت پیدا نشد.',
                info: 'نمایش _START_ تا _END_ از _TOTAL_ صورتحساب', infoEmpty: 'بدون نتیجه', infoFiltered: '(از _MAX_ صورتحساب)',
                lengthMenu: 'نمایش _MENU_ ردیف', paginate: { first: 'اول', last: 'آخر', next: 'بعدی', previous: 'قبلی' } },
            columns: [
                { data: null, orderable: false, render: (_, type, row) => type !== 'display' ? '' :
                    `<input type="checkbox" data-invoice-select="${escape(row.InvoiceId)}" data-eligible="${eligible(row)}" aria-label="انتخاب فاکتور ${escape(row.RealInvoiceNumber)}" ${eligible(row) ? '' : 'disabled'} />` },
                { data: null, render: (_, type, row, meta) => number.format(meta.row + 1) },
                value('StoreName'), value('RealInvoiceNumber'),
                { data: null, render: (_, type, row) => type !== 'display' ? statusLabel(row) : `<span class="newtax-badge state-${state(row)}">${statusLabel(row)}</span>` },
                value('fListOfErrorCaption'), value('PersianCreateDate'), value('CustomerName'),
                { data: 'NetPrice', render: (value, type) => type === 'display' ? number.format(Number(value) || 0) : value },
                { data: 'TotalTax', render: (value, type) => type === 'display' ? number.format(Number(value) || 0) : value },
                { data: null, render: (_, type, row) => type === 'display' ? `<bdi class="newtax-reference">${escape(reference(row)) || '—'}</bdi>` : reference(row) }
            ], drawCallback: updateSelection
        });
    }
    async function load() {
        if (busy || !form.reportValidity()) return;
        const filter = new FormData(form);
        setBusy(true); say('در حال دریافت صورتحساب‌ها…');
        try {
            const result = await request(root.dataset.listUrl, filter);
            if (!Array.isArray(result.Rows)) throw new Error('فهرست صورتحساب‌ها معتبر نیست.');
            rows = result.Rows;
            selected.clear();
            if (!table) initTable();
            table.clear().rows.add(rows).draw();
            byId('newtaxResultCount').textContent = `${number.format(rows.length)} صورتحساب`;
            say(rows.length ? `${number.format(rows.length)} صورتحساب دریافت شد؛ موارد موردنظر را انتخاب کنید.` : 'صورتحسابی در این بازه و وضعیت پیدا نشد.');
        } catch (error) {
            // Clear stale data so it cannot be submitted under a different set of filters.
            rows = []; selected.clear(); if (table) table.clear().draw();
            byId('newtaxResultCount').textContent = '۰ صورتحساب';
            say(error.message, 'error');
        } finally { setBusy(false); }
    }
    function results(items) {
        const list = byId('newtaxResultItems'); list.replaceChildren();
        for (const item of items) {
            const row = rows.find(row => text(row.InvoiceId).toLowerCase() === text(item.InvoiceID).toLowerCase());
            const entry = document.createElement('li');
            entry.className = !item.Success || item.Status === 'FAILED' ? 'is-error' :
                ['PENDING', 'IN_PROGRESS', 'Sent'].includes(item.Status) ? 'is-pending' : 'is-success';
            const label = document.createElement('strong');
            label.textContent = `فاکتور ${row?.RealInvoiceNumber || item.InvoiceID || ''}`;
            const description = document.createElement('span'); description.textContent = item.Message || 'نتیجه دریافت شد.';
            entry.append(label, description); list.append(entry);
        }
        byId('newtaxResults').hidden = false;
    }
    async function perform() {
        if (busy || !selected.size) return;
        const chosen = rows.filter(row => selected.has(row.InvoiceId) && eligible(row));
        const payload = inquiry ? chosen.map(row => row.InvoiceId) : chosen.map(row => ({
            InvoiceID: row.InvoiceId, StoreID: row.StoreID, BookerWorkstationID: row.BookerWorkstationID || 0
        }));
        if (!payload.length || payload.length > 100) { say('بین ۱ تا ۱۰۰ صورتحساب را انتخاب کنید.', 'error'); return; }
        $('#newtaxConfirm').modal('hide');
        setBusy(true); say(inquiry ? 'در حال استعلام صورتحساب‌های انتخاب‌شده…' : 'در حال ارسال صورتحساب‌ها؛ تا دریافت نتیجه صبر کنید…');
        let finalMessage, kind = 'info';
        try {
            const response = await request(root.dataset.operationUrl, payload, true);
            const items = inquiry ? response.Rows : response.Data;
            if (!Array.isArray(items)) throw new Error('جزئیات نتیجهٔ عملیات دریافت نشد.');
            results(items);
            const failed = items.filter(item => !item.Success).length;
            const rejected = items.filter(item => item.Status === 'FAILED').length;
            finalMessage = inquiry ? `استعلام ${number.format(items.length)} صورتحساب انجام شد؛ ${number.format(rejected)} مورد ردشده و ${number.format(failed)} مورد بدون پاسخ معتبر.` : response.Message;
            kind = failed || rejected || response.Success === false ? 'error' : 'success';
            selected.clear();
        } catch (error) {
            finalMessage = inquiry ? error.message : `${error.message} پیش از ارسال مجدد، وضعیت صورتحساب‌ها را استعلام کنید.`;
            kind = 'error';
        } finally { setBusy(false); }
        await load();
        say(finalMessage || 'عملیات پایان یافت.', kind);
    }
    form.addEventListener('submit', event => { event.preventDefault(); load(); });
    byId('newtaxTableSearch').addEventListener('input', event => table?.search(event.target.value).draw());
    root.addEventListener('change', event => {
        const box = event.target.closest('[data-invoice-select]');
        if (!box) return;
        if (box.checked && selected.size >= 100) { box.checked = false; say('در هر نوبت حداکثر ۱۰۰ صورتحساب انتخاب کنید.', 'error'); return; }
        if (box.checked) selected.add(box.dataset.invoiceSelect); else selected.delete(box.dataset.invoiceSelect);
        updateSelection();
    });
    function selectRows(candidates, checked) {
        for (const row of candidates.filter(eligible)) {
            if (checked && selected.size < 100) selected.add(row.InvoiceId); else if (!checked) selected.delete(row.InvoiceId);
        }
        updateSelection();
    }
    byId('newtaxSelectPage').addEventListener('change', event => selectRows(table.rows({ page: 'current', search: 'applied' }).data().toArray(), event.target.checked));
    byId('newtaxSelectAll').addEventListener('click', () => {
        if (selected.size) { selected.clear(); updateSelection(); return; }
        const filtered = table.rows({ search: 'applied' }).data().toArray().filter(eligible);
        selectRows(filtered, true);
        if (filtered.length > 100) say('۱۰۰ صورتحساب اول انتخاب شد؛ موارد بیشتر را در نوبت بعدی انتخاب کنید.');
    });
    byId('newtaxSubmit').addEventListener('click', () => {
        if (inquiry) { perform(); return; }
        byId('newtaxConfirmText').textContent = `${number.format(selected.size)} صورتحساب انتخاب‌شده به سامانه مودیان ارسال شود؟`;
        $('#newtaxConfirm').modal('show');
    });
    byId('newtaxConfirmSend').addEventListener('click', perform);
    byId('newtaxCheck').addEventListener('click', async () => {
        if (busy) return;
        const body = new FormData(); body.append('__RequestVerificationToken', token());
        setBusy(true); say('در حال بررسی ارتباط…');
        try { const result = await request(root.dataset.checkUrl, body); say(result.Message, result.Success ? 'success' : 'error'); }
        catch (error) { say(error.message, 'error'); }
        finally { setBusy(false); }
    });
    byId('newtaxExport').addEventListener('click', async () => {
        if (busy) return;
        const body = new FormData(form);
        setBusy(true); say('در حال آماده‌سازی خروجی اکسل…');
        try {
            const response = await fetch(root.dataset.exportUrl, { method: 'POST', body, credentials: 'same-origin' });
            if (!response.ok || !(response.headers.get('content-type') || '').includes('spreadsheetml')) throw new Error('خروجی اکسل دریافت نشد.');
            const url = URL.createObjectURL(await response.blob());
            const link = document.createElement('a'); link.href = url; link.download = 'NewTax-Invoices.xlsx'; link.click();
            setTimeout(() => URL.revokeObjectURL(url), 1000); say('فایل اکسل آماده شد.', 'success');
        } catch (error) { say(error.message, 'error'); }
        finally { setBusy(false); }
    });
    async function stores() {
        const select = byId('newtaxStores');
        try {
            const response = await fetch(root.dataset.storesUrl, { credentials: 'same-origin', cache: 'no-store' });
            const data = await response.json();
            if (!response.ok || !Array.isArray(data.Rows)) throw new Error();
            data.Rows.forEach(store => { const option = document.createElement('option'); option.value = store.StoreID; option.textContent = store.Name; select.append(option); });
            $(select).select2({ dir: 'rtl', width: '100%', placeholder: 'همهٔ فروشگاه‌ها', closeOnSelect: false, dropdownParent: $(select.closest('.newtax-store-field')),
                language: { noResults: () => 'فروشگاهی پیدا نشد' } });
            $(select).on('change.newtax', () => {
                const count = ( $(select).val() || [] ).length;
                byId('newtaxStoreHint').textContent = count ? `${number.format(count)} فروشگاه انتخاب‌شده` : '';
                byId('newtaxStoreHint').hidden = count === 0;
                byId('newtaxClearStores').hidden = count === 0;
            });
        } catch { byId('newtaxStoresStatus').textContent = 'فهرست فروشگاه‌ها دریافت نشد؛ صفحه را تازه کنید.'; }
    }
    byId('newtaxClearStores').addEventListener('click', () => $('#newtaxStores').val([]).trigger('change'));
    document.querySelectorAll('.sidebar-menu li.active').forEach(item => item.classList.remove('active'));
    byId(inquiry ? 'menu-inquiry' : 'menu-invoice')?.classList.add('active');
    stores();
})();
