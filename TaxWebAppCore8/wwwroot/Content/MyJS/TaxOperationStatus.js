(function (window, $) {
    'use strict';

    var currentOperationId = null;
    var currentConnection = null;
    var closeTimer = null;

    function ensurePanel() {
        if ($('#taxOperationStatus').length) return;

        var panel = [
            '<div id="taxOperationStatus" class="alert alert-info" role="status" aria-live="polite" style="display:none; clear:both; margin:12px 0; direction:rtl;">',
            '  <button type="button" class="close" id="taxOperationStatusClose" aria-label="بستن"><span aria-hidden="true">&times;</span></button>',
            '  <div style="font-weight:bold" id="taxOperationStatusTitle"></div>',
            '  <div id="taxOperationStatusText" style="margin-top:6px; white-space:pre-line"></div>',
            '  <div id="taxOperationStatusCounts" style="margin-top:8px"></div>',
            '  <div class="progress" id="taxOperationStatusProgressWrap" style="margin:8px 0 0; height:18px; display:none">',
            '    <div id="taxOperationStatusProgress" class="progress-bar progress-bar-striped active" role="progressbar" style="min-width:2em; width:0%">۰٪</div>',
            '  </div>',
            '</div>'
        ].join('');

        var waiting = $('#waiting');
        if (waiting.length) waiting.after(panel);
        else $('#butensDiv').after(panel);

        $('#taxOperationStatusClose').on('click', function () {
            hide();
        });
    }

    function toPersianNumber(value) {
        return String(value == null ? 0 : value).replace(/\d/g, function (digit) {
            return '۰۱۲۳۴۵۶۷۸۹'[digit];
        });
    }

    function normalize(payload) {
        if (payload == null) return {};
        if (typeof payload === 'string') return { message: payload };
        return {
            operationId: payload.operationId || payload.OperationId,
            message: payload.message || payload.Message || '',
            processed: payload.processed != null ? payload.processed : payload.Processed,
            total: payload.total != null ? payload.total : payload.Total,
            succeeded: payload.succeeded != null ? payload.succeeded : payload.Succeeded,
            failed: payload.failed != null ? payload.failed : payload.Failed,
            completed: payload.completed != null ? payload.completed : payload.Completed
        };
    }

    function localizeErrorMessage(message) {
        var text = String(message || '').trim();
        if (!text) return 'عملیات انجام نشد. جزئیات فنی در گزارش سیستم ثبت شد.';

        if (/invalid uri|hostname could not be parsed|uri format/i.test(text))
            return 'آدرس سرویس ریما در تنظیمات معتبر نیست. لطفاً تنظیم API_URL بررسی شود.';
        if (/timed?\s*out|timeout|task was canceled/i.test(text))
            return 'زمان انتظار برای پاسخ سرویس به پایان رسید. لطفاً دوباره تلاش کنید.';
        if (/network|connection|actively refused|name or service not known|no such host/i.test(text))
            return 'ارتباط با سرویس برقرار نشد. لطفاً اتصال شبکه و دسترسی سرویس بررسی شود.';
        if (/unauthorized|forbidden|\b401\b|\b403\b/i.test(text))
            return 'مجوز ارتباط با سرویس معتبر نیست. لطفاً اطلاعات ورود سرویس بررسی شود.';

        // پیام فارسی بک‌اند بدون تغییر نمایش داده می‌شود؛ متن فنی انگلیسی به کاربر نهایی نشان داده نمی‌شود.
        if (/[\u0600-\u06FF]/.test(text)) return text;
        return 'عملیات به دلیل خطای فنی انجام نشد. جزئیات در گزارش سیستم ثبت شد.';
    }

    function show(title, payload, type) {
        ensurePanel();
        if (closeTimer) {
            clearTimeout(closeTimer);
            closeTimer = null;
        }

        var data = normalize(payload);
        // وقتی یک عملیات ردیابی‌شده فعال است، پیام‌های قدیمی/پس‌زمینه نباید
        // وضعیت همان عملیات را روی صفحه بازنویسی کنند.
        if (currentOperationId && data.operationId !== currentOperationId) return;
        if (!currentOperationId && data.operationId) return;
        if (type === 'error') data.message = localizeErrorMessage(data.message);

        var panel = $('#taxOperationStatus');
        panel.removeClass('alert-info alert-success alert-danger alert-warning')
            .addClass(type === 'success' ? 'alert-success' : type === 'error' ? 'alert-danger' : type === 'warning' ? 'alert-warning' : 'alert-info');
        $('#taxOperationStatusTitle').text(title || 'وضعیت عملیات');
        $('#taxOperationStatusText').text(data.message || 'در حال انجام عملیات...');

        var hasCounts = data.total != null && data.total >= 0;
        if (hasCounts) {
            var processed = Math.max(0, data.processed || 0);
            var total = Math.max(0, data.total || 0);
            var percent = total > 0 ? Math.min(100, Math.round(processed * 100 / total)) : 0;
            var counts = 'پردازش‌شده: ' + toPersianNumber(processed) + ' از ' + toPersianNumber(total);
            if (data.succeeded != null) counts += ' | موفق: ' + toPersianNumber(data.succeeded);
            if (data.failed != null) counts += ' | ناموفق: ' + toPersianNumber(data.failed);
            counts += ' | باقی‌مانده: ' + toPersianNumber(Math.max(0, total - processed));
            $('#taxOperationStatusCounts').text(counts).show();
            $('#taxOperationStatusProgressWrap').show();
            $('#taxOperationStatusProgress').css('width', percent + '%').text(toPersianNumber(percent) + '٪');
        } else {
            $('#taxOperationStatusCounts').hide().empty();
            $('#taxOperationStatusProgressWrap').hide();
        }

        panel.stop(true, true).show();

        // پیام‌های قدیمی که operationId ندارند نیز نباید تا F5 روی صفحه بمانند.
        if (!currentOperationId && !data.operationId && type !== 'error') {
            closeTimer = setTimeout(hide, 8000);
        }
    }

    function newId() {
        if (window.crypto && window.crypto.randomUUID) return window.crypto.randomUUID();
        return 'op-' + Date.now() + '-' + Math.random().toString(16).slice(2);
    }

    function waitForConnection(connection) {
        if (connection.state === signalR.HubConnectionState.Connected) return Promise.resolve();
        if (connection.state === signalR.HubConnectionState.Disconnected) return connection.start();

        return new Promise(function (resolve, reject) {
            var attempts = 0;
            var timer = setInterval(function () {
                attempts++;
                if (connection.state === signalR.HubConnectionState.Connected) {
                    clearInterval(timer);
                    resolve();
                } else if (connection.state === signalR.HubConnectionState.Disconnected || attempts >= 200) {
                    clearInterval(timer);
                    reject(new Error('اتصال وضعیت زنده برقرار نشد.'));
                }
            }, 50);
        });
    }

    async function begin(connection, title, total) {
        await leaveCurrent();
        currentConnection = connection || null;
        currentOperationId = newId();

        if (currentConnection) {
            try {
                await waitForConnection(currentConnection);
                await currentConnection.invoke('JoinOperation', currentOperationId);
            } catch (error) {
                currentOperationId = null;
                currentConnection = null;
                throw error;
            }
        }

        show(title, { operationId: currentOperationId, message: 'در حال آماده‌سازی...', processed: 0, total: total || 0 }, 'info');
        return currentOperationId;
    }

    async function leaveCurrent() {
        var operationId = currentOperationId;
        var connection = currentConnection;
        currentOperationId = null;
        currentConnection = null;
        if (operationId && connection && connection.state === signalR.HubConnectionState.Connected) {
            try { await connection.invoke('LeaveOperation', operationId); } catch (e) { console.log('LeaveOperation:', e); }
        }
    }

    function complete(title, payload) {
        show(title, payload, 'success');
        leaveCurrent();
        closeTimer = setTimeout(hide, 8000);
    }

    function fail(title, payload) {
        show(title, payload, 'error');
        leaveCurrent();
        closeTimer = setTimeout(hide, 15000);
    }

    function hide() {
        if (closeTimer) clearTimeout(closeTimer);
        closeTimer = null;
        $('#taxOperationStatus').stop(true, true).fadeOut(150);
    }

    window.TaxOperationStatus = {
        begin: begin,
        update: function (title, payload) {
            var data = normalize(payload);
            if (data.completed === true) {
                if (Number(data.failed || 0) > 0) fail(title + ' با خطا پایان یافت', data);
                else complete(title + ' تکمیل شد', data);
                return;
            }
            show(title, data, 'info');
        },
        complete: complete,
        fail: fail,
        hide: hide,
        currentId: function () { return currentOperationId; },
        localizeError: localizeErrorMessage
    };
})(window, jQuery);
