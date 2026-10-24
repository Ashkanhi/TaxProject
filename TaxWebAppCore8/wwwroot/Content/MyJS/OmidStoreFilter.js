(() => {
    'use strict';
    window.OmidStoreFilter = {
        init(select) {
            const $select = $(select);
            const field = select.closest('.omid-store-field');
            if (!field) return;
            if ($select.hasClass('select2-hidden-accessible')) $select.select2('destroy');
            $select.find('option[value="-1"]').remove(); // Empty selection already means all stores.
            $select.select2({
                dir: 'rtl', width: '100%', placeholder: 'همهٔ فروشگاه‌ها',
                allowClear: true, closeOnSelect: false,
                dropdownParent: $(field),
                language: { noResults: () => 'فروشگاهی یافت نشد', searching: () => 'در حال جستجو…' }
            });
            const hint = field.querySelector('#omidStoreHint');
            const clear = field.querySelector('.omid-store-clear');
            const update = () => {
                const count = ($select.val() || []).length;
                hint.textContent = count ? `${count.toLocaleString('fa-IR')} فروشگاه انتخاب شده` : 'همهٔ فروشگاه‌ها';
                clear.disabled = count === 0;
                clear.hidden = count === 0;
                $select.next('.select2').find('input, textarea').attr({ 'aria-label': 'جستجوی نام فروشگاه', placeholder: count ? 'افزودن فروشگاه…' : 'همهٔ فروشگاه‌ها' });
            };
            $select.off('change.omidStore').on('change.omidStore', update);
            $(clear).off('click.omidStore').on('click.omidStore', () => $select.val([]).trigger('change'));
            update();
        }
    };
})();
