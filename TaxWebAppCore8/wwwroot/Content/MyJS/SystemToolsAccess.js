(() => {
    'use strict';
    const input = document.getElementById('systemToolsPassword');
    const toggle = document.getElementById('accessToggle');
    const caps = document.getElementById('accessCapsLock');
    const submit = document.getElementById('accessSubmit');
    const caption = submit.querySelector('span');
    const originalCaption = caption.textContent;
    toggle.addEventListener('click', () => {
        const visible = input.type === 'password';
        input.type = visible ? 'text' : 'password';
        toggle.setAttribute('aria-pressed', String(visible));
        toggle.setAttribute('aria-label', visible ? 'پنهان کردن رمز عبور' : 'نمایش رمز عبور');
        toggle.querySelector('i').className = visible ? 'fa fa-eye-slash' : 'fa fa-eye';
        input.focus();
    });
    for (const event of ['keydown', 'keyup']) input.addEventListener(event, e => {
        // Synthetic keyboard events may not implement the KeyboardEvent API.
        caps.hidden = typeof e.getModifierState !== 'function' || !e.getModifierState('CapsLock');
    });
    input.addEventListener('blur', () => { caps.hidden = true; });
    document.getElementById('accessForm').addEventListener('submit', () => {
        submit.disabled = true;
        caption.textContent = 'در حال بررسی رمز…';
        submit.setAttribute('aria-busy', 'true');
    });
    window.addEventListener('pageshow', () => {
        submit.disabled = input.disabled;
        caption.textContent = originalCaption;
        submit.removeAttribute('aria-busy');
    });
})();
