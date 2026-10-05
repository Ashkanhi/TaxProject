# پرسش‌های رسمی قرارداد فنی Omid

## هدف

این سند برای بستن ابهام‌های مسدودکننده یکپارچه‌سازی `Omid` تهیه شده است. پاسخ‌ها باید به یک نسخه مشخص از قرارداد، Swagger/OpenAPI یا نمونه request/response تاییدشده پنل تست ارجاع دهند. هیچ credential، token یا داده واقعی مشتری نباید در پاسخ درج شود.

## شواهد بررسی‌شده

- سند ۱۲ صفحه‌ای `مستندات API.pdf` با عنوان `ویرایش 01`.
- سورس نمونه `Config.cs`, `CryptoHelper.cs`, `OmidTaxClient.cs`, `Payloads.cs`, `Program.cs`.
- `Format Agent Model (1).xlsx` با ۵۴ فیلد و وضعیت اجباری/اختیاری.
- `sales_template(3).xlsx` با ۳۵ ستون ورودی و چهار ردیف نمونه.
- توضیحات اصلاح، برگشت از فروش، طلا و قواعد Excel.

## A. نسخه و محیط

1. آخرین نسخه معتبر قرارداد API چیست؟ لطفاً شماره نسخه، تاریخ انتشار و وضعیت supersede شدن `ویرایش 01` را اعلام کنید.
2. آیا `https://service.omid.tax` محیط تست، عملیاتی یا مشترک است؟ در صورت تفکیک، Base URL هر محیط را اعلام کنید.
3. آیا Swagger/OpenAPI یا Postman Collection نسخه‌بندی‌شده و مورد تایید وجود دارد؟

## B. رمزنگاری و امضا

4. بدنه نهایی POST دقیقاً کدام است؟
   - `{"data":"Base64EncryptedData"}`
   - یا رشته Base64 رمزنگاری‌شده به‌صورت raw body
5. ورودی دقیق HMAC کدام است؟
   - JSON فشرده نهایی wrapper + timestamp + nonce
   - یا encrypted Base64 + timestamp + nonce
6. encoding تمام اجزای HMAC، ترتیب الحاق و حساسیت نسبت به whitespace را با یک test vector کامل و بدون secret واقعی اعلام کنید.
7. فرمت قطعی nonce چیست؟ GUID با خط تیره، GUID با فرمت `N` یا هر رشته یکتای دیگر؟ مدت نگهداری nonce در سمت سرور چقدر است؟
8. tolerance مجاز `X-Timestamp` چند ثانیه است و پاسخ خطای اختلاف ساعت چه HTTP status/error code دارد؟
9. طول byte مجاز AES Key و IV را تایید و یک pair تست معتبر Base64 ارائه کنید. مقدار فعلی دریافت‌شده برای AES Key قابل Base64 decode نیست.

## C. Login و Token

10. casing قطعی فیلد login کدام است: `userName` یا `username`؟
11. نمونه redacted پاسخ موفق و ناموفق login را ارائه کنید؛ محل token، نوع token، expiry، refresh و سیاست 401 مشخص باشد.
12. آیا token به tenant/fiscalId/API key خاصی مقید است و concurrent login محدودیت دارد؟

## D. ارسال صورتحساب

13. JSON Schema قطعی `SendInvoice` را ارائه کنید و مشخص کنید `batchId`، `requestId`، `fiscalId` و `taxid` کدام‌یک اجباری‌اند.
14. casing قطعی شناسه مالیاتی `taxid` است یا `taxId`؟
15. نوع و واحد `indatim` و `indati2m` چیست: Unix milliseconds، Unix seconds، تاریخ میلادی یا تاریخ شمسی رشته‌ای؟
16. نوع قطعی `inno` چیست: string یا number؟ آیا مقدار hexadecimal پذیرفته/الزامی است؟
17. آیا فایل `Format Agent Model` و `sales_template` فقط قالب import پنل‌اند یا schema ورودی API؟ فیلدهای محاسباتی حذف‌شده را چه طرفی تولید می‌کند؟
18. نمونه redacted پاسخ‌های موفق کامل، موفق جزئی، رد کامل و خطای validation را ارائه کنید.
19. رابطه `requestId` ارسالی، `trackingId` پاسخ، `requestIds` استعلام و شناسه محلی صورتحساب را دقیقاً تعریف کنید.

## E. محدودیت، retry و idempotency

20. سقف تعداد invoice در batch، حداکثر اندازه payload، rate limit و concurrency مجاز چیست؟
21. timeout پیشنهادی endpointهای login/send/inquiry چقدر است؟
22. پس از timeout یا قطع اتصال در SendInvoice، روش تشخیص ثبت‌شدن یا نشدن درخواست چیست؟
23. آیا `batchId` یا `requestId` نقش idempotency key دارد؟ مدت نگهداری و رفتار duplicate چیست؟
24. HTTP statusها، error codeها و `Retry-After` رسمی را ارائه کنید.

## F. Inquiry و وضعیت‌ها

25. نمونه redacted پاسخ inquiry برای تمام وضعیت‌های pending، success، failed و unknown را ارائه کنید.
26. فهرست status/codeهای terminal و non-terminal را اعلام کنید.
27. endpointهای `GetFullInvoice` و `GetStuffByIDOrName` چه response schema، pagination و محدودیتی دارند؟

## G. اصلاح، برگشت، ابطال و طلا

28. مقدار قطعی `ins` برای اصلی، اصلاحی، ابطال و برگشت از فروش چیست؟
29. قواعد `irtaxid`، امکان چند اصلاح متوالی و محدودیت تغییر فیلدها را تایید کنید.
30. برای برگشت از فروش آیا فقط کاهش `am` مجاز است یا مبلغ/تخفیف/مالیات نیز قواعد خاص دارند؟
31. برای الگوی طلا، وضعیت اجباری بودن `consfee`, `spro`, `bros`, `tcpbs` و فرمول کنترل آنها را اعلام کنید؛ `tcpbs` در مدل ۵۴ فیلدی موجود نیست.
32. تعریف قطعی `BBC` و `SBC` را تایید کنید؛ توضیحات تحویلی در نام خریدار/فروشنده تناقض دارد.

## قالب evidence مورد انتظار

برای هر endpoint حداقل این موارد تحویل شود:

- request plaintext قبل از AES با داده ساختگی؛
- encrypted body نهایی؛
- timestamp و nonce نمونه؛
- رشته دقیق ورودی HMAC و signature مورد انتظار؛
- HTTP method/path/headerها؛
- response موفق redacted؛
- response خطای redacted؛
- شماره نسخه قرارداد مرتبط.

