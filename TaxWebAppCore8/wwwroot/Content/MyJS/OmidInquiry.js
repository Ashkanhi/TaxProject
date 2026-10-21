(function () {
    "use strict";

    const page = document.getElementById("omidInquiryPage");
    if (!page) return;

    const fromDate = document.getElementById("txtFromDate");
    const toDate = document.getElementById("txtToDate");
    const stateFilter = document.getElementById("cmdStat");
    const storesFilter = document.getElementById("cmbStores");
    const searchButton = document.getElementById("btnSearchInq");
    const submitButton = document.getElementById("omidInquirySubmit");
    const exportButton = document.getElementById("btnExportExcelInq");
    const checkButton = document.getElementById("btnCheckConnection1");
    const selectAll = document.getElementById("omidInquirySelectAll");
    const rows = document.getElementById("omidInquiryRows");
    const status = document.getElementById("omidInquiryStatus");
    const results = document.getElementById("omidInquiryResults");

    let invoices = [];
    let visibleInvoices = [];
    let busy = false;
    const selectedIds = new Set();
    const inquiryNotes = new Map();

    function setStatus(message, error) {
        status.className = error ? "alert alert-danger" : "alert alert-info";
        status.textContent = message;
    }

    function stateCaption(value) {
        const captions = {
            Pending: "در انتظار تعیین وضعیت",
            Unknown: "وضعیت نامشخص",
            Accepted: "تأییدشده",
            Failed: "ارسال‌شده با خطا"
        };
        return captions[value] || value || "—";
    }

    function money(value) {
        if (value === null || value === undefined) return "—";
        return new Intl.NumberFormat("fa-IR").format(value);
    }

    function addTextCell(tr, value) {
        const cell = document.createElement("td");
        cell.textContent = value === null || value === undefined || value === ""
            ? "—" : String(value);
        tr.appendChild(cell);
        return cell;
    }

    function showSavedResponse(invoice) {
        results.replaceChildren();
        const heading = document.createElement("div");
        heading.textContent = "آخرین پاسخ ذخیره‌شدهٔ امید برای فاکتور " +
            (invoice.InvoiceNumber || invoice.InvoiceId) +
            (invoice.LastInquiryUtc
                ? " — " + new Date(invoice.LastInquiryUtc).toLocaleString("fa-IR")
                : "");
        const pre = document.createElement("pre");
        pre.style.whiteSpace = "pre-wrap";
        pre.style.direction = "ltr";
        pre.style.textAlign = "left";
        pre.textContent = invoice.LastInquiryResponseJson || "پاسخی ذخیره نشده است.";
        results.append(heading, pre);
    }

    function selectedInvoices() {
        return visibleInvoices.filter(function (invoice) {
            return selectedIds.has(invoice.InvoiceId);
        });
    }

    function updateButtons() {
        const count = selectedInvoices().length;
        submitButton.disabled = busy || count === 0;
        exportButton.disabled = busy || visibleInvoices.length === 0;
        searchButton.disabled = busy;
        submitButton.innerHTML = '<i class="fa fa-bolt" aria-hidden="true"></i> ' +
            "استعلام صورتحساب" + (count ? " (" + count + ")" : "");
        selectAll.checked = visibleInvoices.length > 0 &&
            count === visibleInvoices.length;
    }

    function renderRows() {
        rows.replaceChildren();

        if (visibleInvoices.length === 0) {
            const tr = document.createElement("tr");
            const td = addTextCell(tr, "فاکتوری مطابق جستجو پیدا نشد.");
            td.colSpan = 13;
            rows.appendChild(tr);
            updateButtons();
            return;
        }

        visibleInvoices.forEach(function (invoice, index) {
            const tr = document.createElement("tr");
            const selectionCell = document.createElement("td");
            const checkbox = document.createElement("input");
            checkbox.type = "checkbox";
            checkbox.checked = selectedIds.has(invoice.InvoiceId);
            checkbox.setAttribute("aria-label", "انتخاب فاکتور " +
                (invoice.InvoiceNumber || invoice.InvoiceId));
            checkbox.addEventListener("change", function () {
                if (checkbox.checked) selectedIds.add(invoice.InvoiceId);
                else selectedIds.delete(invoice.InvoiceId);
                updateButtons();
            });
            selectionCell.appendChild(checkbox);
            tr.appendChild(selectionCell);

            addTextCell(tr, index + 1);
            addTextCell(tr, invoice.InvoiceId);
            addTextCell(tr, invoice.StoreName);
            addTextCell(tr, invoice.InvoiceNumber);
            addTextCell(tr, stateCaption(invoice.State));
            addTextCell(tr, invoice.LastError ||
                (invoice.State === "Pending" ? "نتیجهٔ نهایی هنوز مشخص نیست." : ""));
            addTextCell(tr, invoice.InvoiceDate);
            addTextCell(tr, invoice.CustomerName);
            addTextCell(tr, "رکورد محلی امید");
            const inquiryCell = addTextCell(tr, inquiryNotes.get(invoice.InvoiceId) ||
                (invoice.LastInquiryUtc
                    ? "آخرین پاسخ در " + new Date(invoice.LastInquiryUtc).toLocaleString("fa-IR")
                    : "هنوز استعلام نشده است."));
            if (invoice.LastInquiryResponseJson) {
                const responseButton = document.createElement("button");
                responseButton.type = "button";
                responseButton.className = "btn btn-xs btn-default";
                responseButton.style.marginRight = "6px";
                responseButton.textContent = "مشاهدهٔ پاسخ آخر";
                responseButton.addEventListener("click", function () {
                    showSavedResponse(invoice);
                });
                inquiryCell.appendChild(responseButton);
            }
            addTextCell(tr, money(invoice.NetPrice));
            addTextCell(tr, money(invoice.TotalTax));

            rows.appendChild(tr);
        });

        updateButtons();
    }

    function applyLocalFilters() {
        const wantedState = stateFilter.value;
        const wantedStores = new Set(Array.from(storesFilter.selectedOptions)
            .map(function (option) { return option.value; })
            .filter(Boolean));

        visibleInvoices = invoices.filter(function (invoice) {
            if (wantedState && invoice.State !== wantedState) return false;
            if (wantedStores.size > 0 &&
                !wantedStores.has(String(invoice.StoreId))) return false;
            return true;
        });

        selectedIds.clear();
        renderRows();
        setStatus(visibleInvoices.length + " فاکتور قابل‌استعلام نمایش داده شد.");
    }

    async function loadStores() {
        try {
            const response = await fetch(page.dataset.storesUrl, {
                headers: { Accept: "application/json" }
            });
            const data = await response.json();
            if (!response.ok || data.success !== true ||
                !Array.isArray(data.data)) return;

            storesFilter.replaceChildren();
            for (const store of data.data) {
                const option = document.createElement("option");
                option.value = store.StoreID;
                option.textContent = store.Name || String(store.StoreID);
                storesFilter.appendChild(option);
            }
        } catch (error) {
            console.error("خواندن فروشگاه‌های امید ناموفق بود.", error);
        }
    }

    async function loadInvoices() {
        if (busy) return;

        const start = fromDate.value.trim();
        const end = toDate.value.trim();
        if (!start || !end) {
            invoices = [];
            visibleInvoices = [];
            selectedIds.clear();
            renderRows();
            setStatus("هر دو تاریخ شروع و پایان را وارد کنید.", true);
            return;
        }

        const url = new URL(page.dataset.listUrl, window.location.origin);
        if (start && end) {
            url.searchParams.set("fromDate", start);
            url.searchParams.set("toDate", end);
        }

        busy = true;
        updateButtons();
        setStatus("در حال خواندن فاکتورهای قابل‌استعلام...");

        try {
            const response = await fetch(url.toString(), {
                headers: { Accept: "application/json" }
            });
            const data = await response.json();
            if (!response.ok || data.success !== true ||
                !Array.isArray(data.data)) {
                throw new Error(data.message || "خواندن فهرست ناموفق بود.");
            }

            invoices = data.data;
            inquiryNotes.clear();
            applyLocalFilters();
        } catch (error) {
            invoices = [];
            visibleInvoices = [];
            selectedIds.clear();
            renderRows();
            setStatus(error.message || "خطا در خواندن فاکتورها.", true);
        } finally {
            busy = false;
            renderRows();
        }
    }

    function showResponse(title, value, description) {
        const box = document.createElement("div");
        box.className = "well well-sm";
        const heading = document.createElement("strong");
        heading.textContent = title;
        box.appendChild(heading);

        const note = document.createElement("p");
        note.textContent = description;
        box.appendChild(note);

        const pre = document.createElement("pre");
        pre.textContent = typeof value === "string"
            ? value : JSON.stringify(value, null, 2);
        box.appendChild(pre);
        results.appendChild(box);
    }

    async function inquireSelected(selected) {
        if (busy) return;
        selected = selected || selectedInvoices();
        if (!selected.length) return;

        busy = true;
        updateButtons();
        results.replaceChildren();

        try {
            for (let start = 0; start < selected.length; start += 100) {
                const batch = selected.slice(start, start + 100);
                const number = Math.floor(start / 100) + 1;
                setStatus("در حال پیگیری دستهٔ " + number + " از امید؛ " +
                    "در صورت خالی بودن نتیجه، استعلام هر ۳۰ ثانیه تکرار می‌شود " +
                    "(حداکثر ۲۰ تلاش). لطفاً پنجره را نبندید.");

                const response = await fetch(page.dataset.inquiryUrl, {
                    method: "POST",
                    headers: {
                        "Content-Type": "application/json",
                        Accept: "application/json"
                    },
                    body: JSON.stringify(batch.map(function (item) {
                        return item.InvoiceId;
                    }))
                });
                const text = await response.text();
                let data;
                try { data = JSON.parse(text); } catch { data = text; }
                const attempts = Number(
                    response.headers.get("X-Omid-Inquiry-Attempts") || 1);
                const hasFinalResult =
                    response.headers.get("X-Omid-Inquiry-Final") === "true";
                const savedState =
                    response.headers.get("X-Omid-Inquiry-State");
                const savedMessageHeader =
                    response.headers.get("X-Omid-Inquiry-Message");
                let savedMessage = "";
                try {
                    savedMessage = savedMessageHeader
                        ? decodeURIComponent(savedMessageHeader) : "";
                } catch { savedMessage = ""; }

                const failed = !response.ok ||
                    (data && typeof data === "object" &&
                     (data.success === false || data.data?.success === false));
                const noFinalResult = !failed && !hasFinalResult;
                const message = failed
                    ? "استعلام ناموفق بود؛ پاسخ و لاگ امید را بررسی کنید."
                    : noFinalResult
                        ? "پس از " + attempts + " تلاش، نتیجهٔ نهایی هنوز آماده نیست."
                        : "نتیجه پس از " + attempts +
                          " تلاش دریافت شد؛ جزئیات را در JSON بررسی کنید.";

                showResponse("دستهٔ " + number + " — فاکتورهای " +
                    batch.map(function (item) {
                        return item.InvoiceNumber || item.InvoiceId;
                    }).join("، "), data, savedMessage || message);

                // سرور برای هر فاکتور وضعیت واقعی ذخیره‌شده را برمی‌گرداند؛ تکی و گروهی یکسان‌اند.
                const savedItems = Array.isArray(data?.savedResults) ? data.savedResults : [];
                for (const invoice of batch) {
                    const saved = savedItems.find(function (item) {
                        return String(item.invoiceId).toLowerCase() === String(invoice.InvoiceId).toLowerCase();
                    });
                    const current = invoices.find(function (item) { return item.InvoiceId === invoice.InvoiceId; });
                    if (current && saved && !saved.stale) {
                        current.State = saved.state;
                        current.LastError = saved.message || null;
                        current.LastInquiryResponseJson = saved.responseJson;
                        current.LastInquiryUtc = new Date().toISOString();
                    }
                    inquiryNotes.set(invoice.InvoiceId, saved?.message ||
                        (saved?.state === "Accepted" ? "صورتحساب تأیید شد." : savedMessage || message));
                }

                if (failed)
                    throw new Error(data?.message || message);
            }

            setStatus("پاسخ استعلام " + selected.length +
                " فاکتور دریافت شد؛ جزئیات پایین صفحه است.");
        } catch (error) {
            setStatus(error.message || "استعلام ناموفق بود.", true);
        } finally {
            busy = false;
            renderRows();
            updateButtons();
        }
    }

    function csvCell(value) {
        let text = value == null ? "" : String(value);
        // جلوگیری از تفسیر متن‌های دیتابیس به‌عنوان فرمول در Excel.
        if (/^[=+\-@]/.test(text)) text = "'" + text;
        return '"' + text.replace(/"/g, '""') + '"';
    }

    function exportForExcel() {
        if (!visibleInvoices.length) return;
        const headers = ["شماره فاکتور", "تاریخ ثبت", "شعبه", "مشتری",
            "وضعیت امید", "شناسه پیگیری", "مبلغ خالص", "مالیات"];
        const lines = [headers.map(csvCell).join(",")];
        for (const item of visibleInvoices) {
            lines.push([item.InvoiceNumber, item.InvoiceDate, item.StoreName,
                item.CustomerName, stateCaption(item.State), item.TrackingId,
                item.NetPrice, item.TotalTax].map(csvCell).join(","));
        }
        const blob = new Blob(["\uFEFF" + lines.join("\r\n")],
            { type: "text/csv;charset=utf-8" });
        const url = URL.createObjectURL(blob);
        const link = document.createElement("a");
        link.href = url;
        link.download = "OmidInquiry.csv";
        document.body.appendChild(link);
        link.click();
        link.remove();
        setTimeout(function () { URL.revokeObjectURL(url); }, 1000);
    }

    async function checkConnection() {
        checkButton.disabled = true;
        setStatus("در حال بررسی ارتباط با امید...");
        try {
            const response = await fetch(page.dataset.checkUrl, {
                headers: { Accept: "application/json" }
            });
            const data = await response.json();
            setStatus(data.message || "پاسخ تست ارتباط دریافت شد.",
                !response.ok || data.success !== true);
        } catch (error) {
            setStatus(error.message || "تست ارتباط ناموفق بود.", true);
        } finally {
            checkButton.disabled = false;
        }
    }

    selectAll.addEventListener("change", function () {
        for (const invoice of visibleInvoices) {
            if (selectAll.checked) selectedIds.add(invoice.InvoiceId);
            else selectedIds.delete(invoice.InvoiceId);
        }
        renderRows();
    });
    document.getElementById("frmSearch").addEventListener("submit", function (e) {
        e.preventDefault();
        loadInvoices();
    });
    searchButton.addEventListener("click", loadInvoices);
    submitButton.addEventListener("click", function () { inquireSelected(); });
    exportButton.addEventListener("click", exportForExcel);
    checkButton.addEventListener("click", checkConnection);
    stateFilter.addEventListener("change", applyLocalFilters);
    storesFilter.addEventListener("change", applyLocalFilters);

    loadStores();
})();
