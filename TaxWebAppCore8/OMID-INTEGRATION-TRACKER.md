# ترکر اجرای یکپارچه‌سازی Omid

## 1. نحوه استفاده

این فایل منبع وضعیت اجرایی Roadmap است. معماری، فازها و Gateها در [OMID-INTEGRATION-ROADMAP.md](./OMID-INTEGRATION-ROADMAP.md) تعریف شده‌اند؛ این Tracker فقط کار قابل اندازه‌گیری، وضعیت، dependency، خروجی و evidence را ثبت می‌کند.

### وضعیت‌های مجاز

| وضعیت | تعریف |
|---|---|
| `NOT_STARTED` | هنوز شروع نشده است. |
| `READY` | dependencyها و ورودی‌ها کامل و قابل اجرا است. |
| `IN_PROGRESS` | Agent اکنون مجاز و مشغول اجرای آن است. |
| `BLOCKED` | ورودی، تصمیم، credential یا محیط لازم موجود نیست. |
| `DONE` | معیار پذیرش و evidence ثبت شده است. |
| `DEFERRED` | با تصمیم ثبت‌شده به زمان دیگری منتقل شده است. |
| `NOT_APPLICABLE` | با دلیل و تایید از scope خارج شده است. |

### قواعد به‌روزرسانی

- در هر زمان حداکثر یک Task اصلی `IN_PROGRESS` باشد.
- checkbox به‌تنهایی evidence نیست.
- هر `DONE` باید حداقل یک evidence قابل بازبینی داشته باشد.
- `BLOCKED` باید سؤال یا ورودی دقیق موردنیاز را ثبت کند.
- تاریخ‌ها با قالب `YYYY-MM-DD` ثبت شوند.
- secret، token، password، API key و payload مشتری در این فایل ثبت نشوند.

## 2. Snapshot فعلی

| فیلد | مقدار |
|---|---|
| تاریخ آخرین به‌روزرسانی | `2026-09-01` |
| فاز جاری | `Phase 0 - Contract Freeze` با شروع محدود Configuration |
| وضعیت کلی | `CONFIG_VALIDATOR_VERIFIED / LOGIN_BLOCKED_INVALID_AES_KEY` |
| branch | `main` |
| checkout | dirty پیش از شروع Omid |
| Task فعال | ندارد؛ کار امن بعدی بدون پاسخ vendor یا AES Key صحیح آماده نیست |
| blocker اصلی | AES Key ارائه‌شده Base64 معتبر نیست؛ تناقض body/signature و response schema نیز باز است |
| سطح اثبات فعلی | `BUILD_AND_15_VALIDATION_SCENARIOS_VERIFIED` |

### Baseline محافظت‌شده

- `BI/BMain.cs` پیش از Omid دارای تغییرات کاربر است.
- فایل‌های RimaCalculate، Controller، SignalR و JavaScript پیش از Omid دارای تغییرات کاربر هستند.
- `Resources/CustomerID/CustomerInformation.xml` staged و حساس است.
- Agent نباید هیچ‌یک از این تغییرات را reset، checkout، overwrite یا ناخواسته stage کند.

## 3. Decision Log

| ID | تصمیم | وضعیت | نیاز/مالک | نتیجه و evidence |
|---|---|---|---|---|
| `DEC-001` | نام canonical در XML | `ACCEPTED` | کاربر/فنی | مقدار canonical: `Omid`؛ نمونه XML ثبت شد |
| `DEC-002` | شکل body رمزنگاری‌شده | `BLOCKED` | Omid/vendor | wrapper JSON یا encrypted خام |
| `DEC-003` | ورودی دقیق HMAC | `BLOCKED` | Omid/vendor | JSON نهایی یا encrypted خام |
| `DEC-004` | casing فیلد tax ID | `BLOCKED` | Omid/vendor | `taxid` یا `taxId` |
| `DEC-005` | نوع timestamp فاکتور | `BLOCKED` | Omid/vendor | Unix ms یا قالب دیگر |
| `DEC-006` | request/tracking correlation | `BLOCKED` | Omid/vendor | رابطه requestId، trackingId و inquiry ID |
| `DEC-007` | login response/token expiry | `BLOCKED` | نمونه پاسخ sandbox | schema typed لازم است |
| `DEC-008` | send response و partial success | `BLOCKED` | نمونه پاسخ sandbox | schema typed لازم است |
| `DEC-009` | inquiry status taxonomy | `BLOCKED` | نمونه پاسخ sandbox | فهرست statusها لازم است |
| `DEC-010` | batch/rate/concurrency | `BLOCKED` | Omid/vendor | اعداد Rima قابل کپی نیستند |
| `DEC-011` | clock tolerance | `BLOCKED` | Omid/vendor | tolerance timestamp لازم است |
| `DEC-012` | idempotency/retry | `BLOCKED` | Omid/vendor | رفتار timeout و duplicate لازم است |
| `DEC-013` | نسبت Excelهای Agent/Sales با schema API | `BLOCKED` | Omid/vendor | مشخص شود کدام فیلدها ورودی پنل‌اند و کدام فیلدها باید توسط API consumer تولید شوند |
| `DEC-014` | قواعد اصلاح/ابطال/برگشت/طلا | `BLOCKED` | Omid/vendor | `ins`, `irtaxid`, `tcpbs` و تعریف قطعی `BBC/SBC` لازم است |

## 4. فهرست Taskها

### Phase 0 - Contract Freeze و baseline

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-000` | ثبت inventory کد نمونه Omid | `DONE` | - | Config، Crypto، Client، Payload و Program بررسی شده‌اند | `D:\4Ashkan\OmidTaxClient\OmidTaxClient`؛ تحلیل 2026-08-24 |
| `OMID-001` | بررسی PDF API | `DONE` | - | endpointها، headerها، AES و HMAC استخراج شده‌اند | `مستندات API.pdf`، 12 صفحه؛ تحلیل 2026-08-24 |
| `OMID-002` | بررسی Excel field model | `DONE` | - | مدل ۵۴ فیلدی و requiredness بررسی شده‌اند | `Format Agent Model (1)!A1:F55`: 14 اجباری، 21 شرطی، 19 اختیاری؛ تحلیل 2026-09-01 |
| `OMID-003` | ثبت baseline جدید Git پیش از اولین تغییر کد | `DONE` | - | branch، status و فایل‌های conflict-prone ثبت شود | `main...origin/main` و dirty baseline در 2026-09-01 ثبت شد |
| `OMID-004` | دریافت نسخه/تاریخ رسمی آخرین قرارداد | `BLOCKED` | vendor | نسخه معتبر و supersede وضعیت PDF مشخص شود | PDF بسته جدید hash-identical، `ویرایش 01` و جدول تایید بدون امضا/تاریخ است |
| `OMID-005` | حل تصمیم‌های `DEC-002` تا `DEC-006` | `BLOCKED` | vendor | body، signature، casing، timestamp و correlation قطعی | PDF صفحات 2-3 wrapper JSON ولی صفحات 10-11 raw encrypted را امضا/ارسال می‌کنند |
| `OMID-006` | دریافت response نمونه redacted برای login/send/inquiry | `BLOCKED` | sandbox/vendor | fixture تاییدشده برای موفق و خطا | Program فقط مسیرهای JObject را حدس می‌زند؛ fixture واقعی وجود ندارد |
| `OMID-007` | دریافت محدودیت batch/rate/time/retry | `BLOCKED` | vendor | `DEC-010` تا `DEC-012` بسته شوند | pending |
| `OMID-008` | تعریف status map اولیه | `NOT_STARTED` | `OMID-006` | تمام statusها به success/pending/failed/unknown نگاشت شوند | pending |
| `OMID-009` | تعیین داده تست و endpoint sandbox | `BLOCKED` | environment owner | هیچ داده واقعی مشتری استفاده نشود | Base URL و credential پنل تست دریافت شد؛ AES Key معتبر و داده فاکتور تست هنوز لازم است |
| `OMID-010` | رفع ابهام schema فایل‌های Agent/Sales و قواعد ارجاعی | `BLOCKED` | vendor | `DEC-013/014` بسته شوند | `sales_template` فاقد envelope API است؛ `tcpbs` و تعریف `BBC/SBC` متناقض‌اند |
| `GATE-0` | تایید Contract Freeze | `BLOCKED` | `OMID-004..010` | تمام تصمیم‌های امنیتی و parser قطعی باشند | `OMID-VENDOR-CONTRACT-QUESTIONS.md` برای اخذ پاسخ رسمی آماده شد |

### Phase 1 - Configuration و secret safety

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-100` | تعیین نام propertyهای Omid | `DONE` | مجوز صریح کاربر | نام‌های صریح بدون repurpose مبهم | `OMID_API_URL`, credential و AES propertyهای مستقل در `DTOServerInfo` |
| `OMID-101` | افزودن config model | `DONE` | `OMID-100` | build و deserialize موفق | XML با `XmlSerializer` سازگار؛ build صفر error در 2026-09-01 |
| `OMID-102` | افزودن validation config | `DONE` | `OMID-101` | empty/URI/Base64/key length/IV length پوشش داده شود | `BI/Omid/OmidConfigurationValidator.cs`؛ 15/15 سناریو پاس و config فعلی فقط با `AesKeyInvalidBase64` رد شد |
| `OMID-103` | تهیه نمونه XML redacted | `DONE` | `OMID-100` | بدون secret واقعی و قابل استفاده برای استقرار | `Resources/CustomerID/CustomerInformation.Omid.example.xml` |
| `OMID-104` | تعریف masking/logging policy | `NOT_STARTED` | `GATE-0` | token/header/signature/payload افشا نشوند | pending |
| `OMID-105` | secret scan baseline | `NOT_STARTED` | `OMID-103` | تغییرات Omid فاقد credential باشند | pending |
| `GATE-1` | تایید config و secret safety | `NOT_STARTED` | `OMID-100..105` | startup config behavior و secret scan پاس | pending |

### Phase 2 - Provider skeleton و BMain routing

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-200` | تعریف surface `OmidTaxService` | `NOT_STARTED` | `GATE-1` | تمام عملیات façade قابل compile باشند | pending |
| `OMID-201` | ایجاد instance lifecycle سازگار | `NOT_STARTED` | `OMID-200` | context صحیح و بدون per-operation static state | pending |
| `OMID-202` | افزودن field و constructor branch در BMain | `NOT_STARTED` | `OMID-201` | `ServiceType=Omid` provider صحیح را بسازد | pending |
| `OMID-203` | افزودن routing تمام عملیات BMain | `NOT_STARTED` | `OMID-202` | هیچ عملیات Omid به Tiss نیفتد | pending |
| `OMID-204` | explicit Tis و unknown fail-fast | `NOT_STARTED` | `OMID-202` | typo/empty مقدار configuration error بدهد | pending |
| `OMID-205` | characterization test providerهای قدیمی | `NOT_STARTED` | `OMID-203` | Tax/Rima/RimaCalculate/Tis routing حفظ شود | pending |
| `GATE-2` | تایید provider selection | `NOT_STARTED` | `OMID-200..205` | build و routing tests پاس | pending |

### Phase 3 - Crypto، HTTP و Authentication

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-300` | پیاده‌سازی AES helper | `NOT_STARTED` | `GATE-0` | تطبیق با test vector تاییدشده | pending |
| `OMID-301` | پیاده‌سازی HMAC helper | `NOT_STARTED` | `GATE-0` | تطبیق با test vector تاییدشده | pending |
| `OMID-302` | serialization canonical | `NOT_STARTED` | `DEC-002,DEC-003` | byte/string امضاشده قابل اثبات باشد | pending |
| `OMID-303` | URI builder | `NOT_STARTED` | `GATE-1` | base/relative path امن و تست‌شده | pending |
| `OMID-304` | request builder header/nonce/timestamp | `NOT_STARTED` | `OMID-300..303` | headerها دقیق و nonce یکتا | pending |
| `OMID-305` | typed login DTO/parser | `NOT_STARTED` | `OMID-006` | token/expiry/error بدون JObject حدسی | pending |
| `OMID-306` | token cache و concurrent login gate | `NOT_STARTED` | `OMID-305` | login storm رخ ندهد | pending |
| `OMID-307` | 401 re-auth policy | `NOT_STARTED` | `OMID-306,DEC-012` | retry محدود و قابل audit | pending |
| `OMID-308` | timeout/cancellation/error classification | `NOT_STARTED` | `OMID-304` | خطاهای config/network/auth/server جدا شوند | pending |
| `OMID-309` | `Init` هم‌زمان‌امن | `NOT_STARTED` | `OMID-300..306` | فقط پس از موفقیت initialized شود و failure قابل retry باشد | pending |
| `OMID-310` | `CheckConnectionAsync` | `NOT_STARTED` | `OMID-305..309` | login/request سبک sandbox تایید شود | pending |
| `GATE-3` | تایید client و login | `NOT_STARTED` | `OMID-300..310` | crypto fixtures و login sandbox پاس | pending |

### Phase 4 - DTO و Invoice Mapping

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-400` | تعریف send envelope DTO | `NOT_STARTED` | `GATE-0` | batchId/fiscalId/invoices typed | pending |
| `OMID-401` | تعریف header/body/payment DTO | `NOT_STARTED` | `OMID-001,OMID-002` | field/type/casing قطعی | pending |
| `OMID-402` | تحلیل reuse از RimaCalculate mapper | `NOT_STARTED` | `GATE-0` | فهرست reuse و divergence ثبت شود | pending |
| `OMID-403` | mapper header | `NOT_STARTED` | `OMID-401,402` | taxid/date/buyer/seller/totals صحیح | pending |
| `OMID-404` | mapper body | `NOT_STARTED` | `OMID-401,402` | کالا، مبلغ، تخفیف و مالیات صحیح | pending |
| `OMID-405` | mapper payments | `NOT_STARTED` | `OMID-401,402` | null و نوع عددی صحیح | pending |
| `OMID-406` | requestId/batchId correlation model | `NOT_STARTED` | `DEC-006` | lookup صریح invoice↔request↔tracking | pending |
| `OMID-407` | validation پیش از ارسال | `NOT_STARTED` | `OMID-403..405` | required fieldها و sstid کنترل شوند | pending |
| `OMID-408` | snapshot fixtures الگوهای فاکتور | `NOT_STARTED` | `OMID-403..407` | سناریوهای Roadmap پوشش داده شوند | pending |
| `GATE-4` | تایید payload | `NOT_STARTED` | `OMID-400..408` | fixtureها با قرارداد تاییدشده برابر باشند | pending |

### Phase 5 - Send Pipeline

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-500` | eligibility و duplicate guard | `NOT_STARTED` | `GATE-4` | pending/success دوباره ارسال نشوند | pending |
| `OMID-501` | batch builder | `NOT_STARTED` | `DEC-010,OMID-408` | count/bytes مطابق Omid | pending |
| `OMID-502` | concurrency limiter | `NOT_STARTED` | `DEC-010` | اعداد مستقل و قابل تنظیم | pending |
| `OMID-503` | typed send response parser | `NOT_STARTED` | `OMID-006` | success/partial/error پوشش داده شود | pending |
| `OMID-504` | correlation پاسخ با invoice | `NOT_STARTED` | `OMID-406,503` | بدون جست‌وجوی string در JSON | pending |
| `OMID-505` | persistence tracking/pending | `NOT_STARTED` | `OMID-504` | reference و state در DB صحیح | pending |
| `OMID-506` | partial batch semantics | `NOT_STARTED` | `OMID-503..505` | ResultModel شمارش صحیح بدهد | pending |
| `OMID-507` | ambiguous timeout recovery | `NOT_STARTED` | `DEC-012` | duplicate ایجاد نشود؛ inquiry قابل بازیابی باشد | pending |
| `OMID-508` | ارسال تکی از wrapper گروهی | `NOT_STARTED` | `OMID-500..507` | یک invoice همان pipeline را طی کند | pending |
| `OMID-509` | ارسال گروهی sandbox | `NOT_STARTED` | `OMID-500..508,GATE-3` | send واقعی تست و DB evidence | pending |
| `GATE-5` | تایید send pipeline | `NOT_STARTED` | `OMID-500..509` | single/batch/partial/duplicate تایید شوند | pending |

### Phase 6 - Inquiry و Status Persistence

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-600` | typed inquiry DTO/parser | `NOT_STARTED` | `OMID-006` | schema پاسخ کامل | pending |
| `OMID-601` | status mapper مرکزی | `NOT_STARTED` | `OMID-008,600` | terminal/pending/unknown صریح | pending |
| `OMID-602` | inquiry تکی با invoice ID محلی | `NOT_STARTED` | `OMID-505,600` | tracking lookup صحیح | pending |
| `OMID-603` | inquiry گروهی list | `NOT_STARTED` | `OMID-600..602` | pendingها batch inquiry شوند | pending |
| `OMID-604` | success persistence | `NOT_STARTED` | `OMID-601` | state=4 و log صحیح | pending |
| `OMID-605` | failure/error persistence | `NOT_STARTED` | `OMID-601` | state=3 و جزئیات خطا | pending |
| `OMID-606` | pending/unknown persistence | `NOT_STARTED` | `OMID-601` | success کاذب تولید نشود | pending |
| `OMID-607` | unfinished recovery | `NOT_STARTED` | `OMID-603..606` | pendingها قابل تعیین تکلیف باشند | pending |
| `OMID-608` | date range inquiry | `NOT_STARTED` | `OMID-603..606` | بازه محلی → references → inquiry | pending |
| `OMID-609` | GetFullInvoice | `NOT_STARTED` | قرارداد endpoint | پاسخ typed یا نتیجه controlled | pending |
| `OMID-610` | inquiry sandbox lifecycle | `NOT_STARTED` | `OMID-600..609,GATE-5` | pending→terminal ثبت شود | pending |
| `GATE-6` | تایید inquiry pipeline | `NOT_STARTED` | `OMID-600..610` | تمام status classها تایید شوند | pending |

### Phase 7 - Progress، UI Contract و Errors

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-700` | operation-local progress state | `NOT_STARTED` | `GATE-5` | هیچ static counter مشترک نباشد | pending |
| `OMID-701` | SignalR group progress در send | `NOT_STARTED` | `OMID-700` | فقط operationId مربوط پیام بگیرد | pending |
| `OMID-702` | SignalR group progress در inquiry | `NOT_STARTED` | `GATE-6,OMID-700` | شمارش دقیق و پایان صریح | pending |
| `OMID-703` | Persian safe error mapping | `NOT_STARTED` | `OMID-308,601` | پیام کاربر امن؛ log فنی redacted | pending |
| `OMID-704` | Controller contract regression | `NOT_STARTED` | `OMID-701..703` | endpoint/JSON موجود نشکند | pending |
| `OMID-705` | JS/UI smoke test | `NOT_STARTED` | `OMID-704` | بدون F5 و بدون overlap پیام | pending |
| `OMID-706` | two-user/two-tab test | `NOT_STARTED` | `OMID-705` | اختلاط پیام صفر | pending |
| `GATE-7` | تایید UX عملیاتی | `NOT_STARTED` | `OMID-700..706` | progress و error واقعی مرورگر تایید | pending |

### Phase 8 - Test، Regression و Security

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-800` | crypto unit tests | `NOT_STARTED` | `GATE-3` | valid/invalid vectors پاس | pending |
| `OMID-801` | DTO contract tests | `NOT_STARTED` | `GATE-6` | تمام fixtureها parse شوند | pending |
| `OMID-802` | mapper snapshot tests | `NOT_STARTED` | `GATE-4` | تمام الگوهای ضروری پوشش | pending |
| `OMID-803` | routing regression tests | `NOT_STARTED` | `GATE-2` | تمام ServiceTypeها | pending |
| `OMID-804` | send/inquiry integration tests | `NOT_STARTED` | `GATE-6` | happy/error/partial/timeout | pending |
| `OMID-805` | build verification | `NOT_STARTED` | implementation complete | صفر error | pending |
| `OMID-806` | JS syntax و browser regression | `NOT_STARTED` | `GATE-7` | خطای syntax/console بحرانی صفر | pending |
| `OMID-807` | secret scan | `NOT_STARTED` | implementation complete | secret/token جدید صفر | pending |
| `OMID-808` | `git diff --check` و scope audit | `NOT_STARTED` | implementation complete | whitespace error صفر؛ فایل خارج scope صفر | pending |
| `OMID-809` | provider regression review | `NOT_STARTED` | `OMID-803,805` | رفتار قبلی حفظ شده باشد | pending |
| `GATE-8` | تایید test readiness | `NOT_STARTED` | `OMID-800..809` | evidence کامل build/test/security | pending |

### Phase 9 - Publish، Sandbox E2E و Rollback

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-900` | publish به مسیر جدا | `NOT_STARTED` | `GATE-8` | output بدون overwrite مقصد | pending |
| `OMID-901` | audit publish content | `NOT_STARTED` | `OMID-900` | binaries/resources/config کامل | pending |
| `OMID-902` | inject امن config sandbox | `BLOCKED` | environment owner | secret خارج Git | pending |
| `OMID-903` | startup و CheckConnection sandbox | `NOT_STARTED` | `OMID-901,902` | runtime path تایید | pending |
| `OMID-904` | browser send E2E | `NOT_STARTED` | `OMID-903` | UI→API→DB→UI | pending |
| `OMID-905` | browser inquiry E2E | `NOT_STARTED` | `OMID-904` | pending→terminal | pending |
| `OMID-906` | Excel/report verification | `NOT_STARTED` | `OMID-905` | status/source/error صحیح | pending |
| `OMID-907` | rollback rehearsal | `NOT_STARTED` | `OMID-900` | بازگشت publish/config موفق | pending |
| `GATE-9` | تایید sandbox delivery | `NOT_STARTED` | `OMID-900..907` | E2E و rollback تایید | pending |

### Phase 10 - Customer Acceptance و Closure

| ID | Task | وضعیت | وابستگی | خروجی/معیار پذیرش | Evidence |
|---|---|---|---|---|---|
| `OMID-1000` | change window و owner | `BLOCKED` | customer | زمان و مسئول go/rollback | pending |
| `OMID-1001` | backup publish/config | `NOT_STARTED` | `OMID-1000,GATE-9` | backup قابل بازیابی | pending |
| `OMID-1002` | فعال‌سازی runtime Omid | `NOT_STARTED` | `OMID-1001` | ServiceType runtime صحیح و restart | pending |
| `OMID-1003` | canary send/inquiry | `NOT_STARTED` | `OMID-1002` | چرخه کامل تاییدشده | pending |
| `OMID-1004` | DB/log/UI reconciliation | `NOT_STARTED` | `OMID-1003` | tracking/status/error یکسان | pending |
| `OMID-1005` | post-release monitoring | `NOT_STARTED` | `OMID-1003` | latency/error/pending/duplicate پایش | pending |
| `OMID-1006` | go یا rollback decision | `NOT_STARTED` | `OMID-1004,1005` | تصمیم owner ثبت شود | pending |
| `OMID-1007` | runbook و closure report | `NOT_STARTED` | `OMID-1006` | محدودیت‌ها و evidence نهایی | pending |
| `GATE-10` | تایید تکمیل Integration | `NOT_STARTED` | `OMID-1000..1007` | معیار تکمیل Roadmap برقرار | pending |

## 5. Evidence Log

هر اجرای Agent یک ردیف جدید اضافه کند؛ ردیف قبلی overwrite نشود.

| تاریخ | Task | اقدام | نتیجه | Evidence/Command | محدودیت |
|---|---|---|---|---|---|
| `2026-08-24` | `OMID-000` | بررسی source نمونه | انجام شد | `Config.cs`, `CryptoHelper.cs`, `OmidTaxClient.cs`, `Payloads.cs`, `Program.cs` | build جدید و live API اجرا نشد |
| `2026-08-24` | `OMID-001` | استخراج کامل متن PDF | انجام شد | 12 صفحه API document | تناقض فصل امنیت و نمونه client باز است |
| `2026-08-24` | `OMID-002` | inspect فایل Excel | انجام شد | `Table_Struncture_Omid_Motamed!A1:E55` | فقط ساختار field؛ response schema ندارد |
| `2026-08-25` | Planning | ایجاد Roadmap و Tracker | انجام شد | `OMID-INTEGRATION-ROADMAP.md`, `OMID-INTEGRATION-TRACKER.md` | هیچ کد Omid پیاده نشده است |
| `2026-09-01` | `OMID-003` | ثبت branch و dirty baseline پیش از تغییر Omid | انجام شد | `git status --short --branch`؛ branch=`main` | تغییرات قبلی کاربر دست‌نخورده باقی ماند |
| `2026-09-01` | `OMID-100..103` | افزودن propertyهای مستقل، runtime config و نمونه redacted | بخشی انجام شد | `DTOServerInfo.cs`، XML runtime، XML example | `ServiceType` عملیاتی همچنان `Tax` است؛ secretها در Tracker ثبت نشدند |
| `2026-09-01` | `OMID-101` | build پروژه | پاس | `dotnet build --no-restore --disable-build-servers`؛ صفر error، 995 warning | warningها عمدتاً legacy/package هستند و جداگانه triage نشده‌اند |
| `2026-09-01` | `OMID-102` | اعتبارسنجی config دریافتی | مسدود | XML/HTTPS/GUID/IV بررسی شد؛ AES Key decode نشد | login و هر درخواست API تا دریافت کلید معتبر اجرا نشد |
| `2026-09-01` | Discovery refresh | بررسی کامل `newFiles` | انجام شد | source/PDF hash-identical؛ Excel requiredness و sales fixture تحلیل شد | قرارداد transport/response همچنان متناقض است |
| `2026-09-01` | Contract Freeze | تهیه پرسش‌های رسمی vendor | انجام شد | `OMID-VENDOR-CONTRACT-QUESTIONS.md` با 32 سؤال و قالب evidence | پاسخ vendor هنوز دریافت نشده است |
| `2026-09-01` | `OMID-102` | پیاده‌سازی validator مستقل config | انجام شد | URI امن، requiredها، Base64، AES key length و IV length | هنوز به `BMain` متصل نشده و provider فعال `Tax` باقی مانده است |
| `2026-09-01` | `OMID-102` | اجرای سناریوهای validator روی assembly واقعی | پاس | 15/15 سناریو؛ شامل AES-128/192/256 و required fields؛ build صفر error | XML فعلی به‌صورت کنترل‌شده با `AesKeyInvalidBase64` رد شد؛ secret چاپ نشد |
| `2026-09-01` | Secret check | اسکن code/docs/example در scope Omid | پاس | 5 فایل، صفر تطبیق با credentialهای runtime | XML عملیاتی به درخواست کاربر secret-bearing و عمداً خارج از این scan است |

## 6. Blocker Register

| ID | شرح | اثر | اقدام لازم | وضعیت |
|---|---|---|---|---|
| `BLK-001` | body/signature در PDF متناقض است | login و تمام APIها ممکن است signature invalid شوند | تایید vendor یا request موفق sandbox | باز |
| `BLK-002` | response schema رسمی کامل نیست | parser و persistence قابل اتکا نیست | نمونه redacted login/send/inquiry | باز |
| `BLK-003` | requestId/trackingId مبهم است | احتمال اتصال پاسخ به invoice اشتباه | تعریف correlation رسمی | باز |
| `BLK-004` | batch/rate/idempotency نامعلوم است | duplicate یا overload | محدودیت رسمی Omid | باز |
| `BLK-005` | credential پنل تست دریافت شده ولی AES Key آن Base64 معتبر نیست و داده تست فاکتور مشخص نشده است | login و E2E قابل اتکا نیست | AES Key صحیح و بعداً payload تست غیرمشتری ارائه شود | باز |
| `BLK-006` | checkout اصلی dirty است | خطر overwrite یا stage ناخواسته | baseline و stage فقط فایل‌های scope | باز و قابل مدیریت |
| `BLK-007` | فایل runtime تنظیمات از قبل staged و حاوی داده حساس است | احتمال commit ناخواسته credential | این فایل stage نشود و پیش از هر commit وضعیت index جداگانه بازبینی شود | باز و قابل مدیریت |

## 7. Change Log سند

| نسخه | تاریخ | تغییر |
|---|---|---|
| `0.1` | `2026-08-25` | ایجاد Roadmap اجرایی و Tracker اولیه براساس تحلیل BMain، providerها و قرارداد Omid |
| `0.2` | `2026-09-01` | ثبت baseline، config اختصاصی و نمونه redacted؛ ثبت blocker کلید AES نامعتبر |
| `0.3` | `2026-09-01` | ثبت تحلیل newFiles، پرسش‌های vendor و تکمیل validator مستقل با 15 سناریوی پاس |
