# نقشه راه یکپارچه‌سازی واسط مودیان Omid

## 1. هدف سند

این سند مرجع معماری، ترتیب اجرا، کنترل ریسک و معیارهای پذیرش افزودن واسط مودیان `Omid` به `TaxWebAppCore8` است. Agent باید اجرای کار را از روی این Roadmap و وضعیت روزانه را از روی [OMID-INTEGRATION-TRACKER.md](./OMID-INTEGRATION-TRACKER.md) دنبال کند.

هدف نهایی این است که با تنظیم زیر در نسخه runtime فایل `CustomerInformation.xml`، تمام عملیات وابسته به واسط مودیان به پیاده‌سازی اختصاصی Omid هدایت شوند، بدون اینکه رفتار `Tax`، `Rima`، `RimaCalculate` یا `Tis` تغییر ناخواسته کند:

```xml
<ServiceType>Omid</ServiceType>
```

این سند مجوز اجرای خودکار همه فازها نیست. هر فاز فقط پس از عبور از Gate فاز قبلی و ثبت وضعیت در Tracker شروع می‌شود.

## 2. وضعیت مبنا در تاریخ 2026-08-25

- تحلیل اولیه معماری `BMain` و providerهای موجود انجام شده است.
- کد نمونه، PDF و Excel ارائه‌شده برای Omid بررسی شده‌اند.
- پیاده‌سازی Omid در پروژه اصلی هنوز شروع نشده است.
- قراردادهای login، send و inquiry در مستند Omid چند ابهام مهم دارند و هنوز live-verified نیستند.
- checkout فعلی از قبل دارای تغییرات staged، modified و untracked است.
- `BMain.cs` از قبل تغییر دارد؛ هر ویرایش آینده باید روی همان نسخه ادغام شود و تغییرات موجود کاربر حفظ شوند.
- `Resources/CustomerID/CustomerInformation.xml` دارای تنظیمات حساس واقعی‌نما و در وضعیت staged است؛ هیچ secret نباید در Roadmap، Tracker، log، diff یا commit بازنشر شود.

### سطوح اثبات

در Tracker فقط از این سطوح استفاده شود:

| سطح | معنی |
|---|---|
| `PLANNED` | فقط در Roadmap تعریف شده است. |
| `CODED` | کد نوشته شده ولی build/test کامل نشده است. |
| `BUILD_VERIFIED` | build یا آزمون واحد مشخص پاس شده است. |
| `CONTRACT_VERIFIED` | payload و parser با نمونه پاسخ تاییدشده یا sandbox تطبیق داده شده‌اند. |
| `SANDBOX_VERIFIED` | login/send/inquiry واقعی در محیط غیرعملیاتی موفق بوده است. |
| `CUSTOMER_VERIFIED` | مسیر واقعی مرورگر، دیتابیس و سرویس مشتری تایید شده است. |

موفقیت build به‌تنهایی معادل آمادگی تحویل یا موفقیت API نیست.

## 3. مرز مسئولیت‌ها

### در محدوده

- افزودن `Omid` به انتخاب صریح `ServiceType`.
- پیاده‌سازی lifecycle و initialization اختصاصی Omid.
- پیاده‌سازی AES، HMAC، timestamp، nonce و authentication مطابق قرارداد تاییدشده.
- ساخت payload استاندارد صورتحساب Omid.
- ارسال تکی و گروهی.
- نگاشت پاسخ ارسال به invoice محلی.
- استعلام تکی و گروهی و نگاشت وضعیت‌ها.
- ثبت tracking/reference، وضعیت، log و خطا در ساختار موجود دیتابیس.
- حفظ قرارداد `ResultModel`، UI، Controller و SignalR.
- بررسی اتصال.
- پشتیبانی از `GetFullInvoice` فقط در صورتی که قرارداد پاسخ تایید شود.
- build، unit test، contract test، sandbox test، deployment و rollback.

### خارج از محدوده مگر با تایید جداگانه

- بازنویسی کامل معماری تمام providerها به DI یا factory جدید.
- تغییر محاسبات مالیاتی مشترک بدون نیاز اثبات‌شده Omid.
- تغییر schema دیتابیس بدون gap analysis و migration/rollback تاییدشده.
- تغییر ظاهر صفحات ارسال و استعلام.
- تغییر قرارداد providerهای `Tax`، `Rima`، `RimaCalculate` و `Tis`.
- commit یا push کردن secret، XML عملیاتی، appsettings عملیاتی یا artifactهای IDE.
- ارسال فاکتور واقعی مشتری در فازهای توسعه و sandbox.

## 4. منابع مرجع

### پروژه اصلی

- `BI/BMain.cs`: façade و routing عملیات providerها.
- `BI/RimaServiceCalculateMethod.cs`: نزدیک‌ترین مرجع برای mapping، batch، inquiry، persistence و progress.
- `BI/TaxService.cs`: مرجع initialization واقعی SDK و status persistence.
- `BI/RimaService.cs` و `BI/TissService.cs`: مراجع سازگاری قرارداد provider.
- `Models/SharedDTO/DTOServerInfo.cs`: مدل تنظیمات XML.
- `Models/SharedDTO/DTOResultModel.cs`: قرارداد خروجی کنترلر و UI.
- `Models/DBContextMedels/Interface/IContext.cs`: قرارداد دسترسی دیتابیس.
- `Controllers/SendInvoiceController.cs`: مصرف‌کننده façade.
- `BI/MessagingService/RequestsHub.cs`: تفکیک عملیات SignalR.
- `wwwroot/Content/MyJS/TaxOperationStatus.js`: قرارداد progress سمت کاربر.
- `Program.cs`: تعیین مسیر runtime منابع و startup.

### مرجع Omid

- `D:\4Ashkan\OmidTaxClient\OmidTaxClient\مستندات API.pdf`
- `D:\4Ashkan\OmidTaxClient\OmidTaxClient\Format Agent Model (2).xlsx`
- `D:\4Ashkan\OmidTaxClient\OmidTaxClient\OmidTaxClient.cs`
- `D:\4Ashkan\OmidTaxClient\OmidTaxClient\CryptoHelper.cs`
- `D:\4Ashkan\OmidTaxClient\OmidTaxClient\Payloads.cs`
- `D:\4Ashkan\OmidTaxClient\OmidTaxClient\Program.cs`

کد console نمونه مرجع رفتاری است، نه library آماده production. پروژه نمونه `net9.0` و `Exe` است و نباید مستقیماً به وب‌اپ `net8.0` reference شود.

## 5. قرارداد معماری هدف

```text
BMain
  └── OmidTaxService
        ├── OmidApiClient
        │     ├── OmidAuthenticationManager
        │     ├── OmidCryptoService
        │     └── HttpClient
        ├── OmidInvoiceMapper
        ├── OmidResponseMapper
        ├── IContext
        └── SignalR operation progress
```

### مسئولیت BMain

- خواندن `ServiceType`.
- ساخت provider انتخاب‌شده.
- هدایت تمام عملیات عمومی به provider انتخاب‌شده.
- رد صریح مقدار ناشناخته؛ مقدار ناشناخته نباید به‌صورت ضمنی به Tiss برود.
- عدم ورود منطق AES، login، payload یا parsing Omid به `BMain`.

### مسئولیت OmidTaxService

- ارائه surface سازگار با عملیات مورد استفاده `BMain`.
- هماهنگ‌کردن mapping، send، inquiry، persistence و progress.
- تبدیل مدل‌ها و statusهای Omid به قرارداد داخلی پروژه.
- جلوگیری از نشت DTOهای Omid به Controller و JavaScript تا حد امکان.

### مسئولیت OmidApiClient

- ساخت URI معتبر.
- serialization فشرده و deterministic مطابق قرارداد امضا.
- AES-CBC-PKCS7 و Base64.
- تولید nonce یکتا و timestamp UTC.
- HMAC-SHA256.
- افزودن headerهای امنیتی و Bearer token.
- timeout، cancellation و مدیریت پاسخ HTTP.
- retry محدود و فقط برای خطاهای امن؛ ارسال صورتحساب نباید کورکورانه retry شود.

### مسئولیت OmidInvoiceMapper

- دریافت مدل فاکتور از `IContext`.
- استفاده مجدد کنترل‌شده از قواعد معتبر mapping موجود.
- تولید header، body و payments با نام و نوع دقیق فیلدها.
- تولید `batchId` و `requestId` قابل audit.
- نگهداری correlation بین invoice ID محلی، tax ID، request ID و tracking ID.

## 6. Surface عملیاتی Omid

پیاده‌سازی نهایی باید حداقل این عملیات را پوشش دهد:

| عملیات عمومی | رفتار Omid | سطح الزام |
|---|---|---|
| `Init` | اعتبارسنجی config، URI و material رمزنگاری؛ initialization هم‌زمان‌امن | الزامی |
| `CheckConnectionAsync` | login یا درخواست سبک authenticated و تفسیر صحیح پاسخ | الزامی |
| `GetAllInvoice` | wrapper خواندن محلی از `_context` | الزامی برای سازگاری |
| `SendGroupInvoicesForGroupingAsync` | pipeline ارسال تکی/گروهی Omid | الزامی |
| `GetAllInvoiceByInqFromServer` | فهرست محلی + inquiry گروهی + persistence | الزامی |
| `GetInvoiceStatusByRefrenceAsync` | invoice محلی → tracking/request ID → inquiry | الزامی |
| `GetNotFinishedLastRefrenceIdAsync` | بازیابی pendingها و inquiry مجدد | الزامی قبل از تحویل |
| `GetInvoiceStatusByDateRange` | انتخاب محلی بازه و inquiry referenceها، مگر endpoint رسمی تایید شود | الزامی قبل از تحویل |
| `GetInvoiceByID` | `GetFullInvoice` یا نتیجه کنترل‌شده در صورت عدم پشتیبانی | مشروط به قرارداد |

`SendInvoiceAsync` در `BMain` باید مانند providerهای فعلی، ارسال تکی را به لیست تک‌عضوی و pipeline گروهی Omid تبدیل کند.

## 7. تصمیم‌های مسدودکننده قرارداد

هیچ کدنویسی transport یا parser عملیاتی نباید پیش از تعیین این موارد به‌عنوان «قرارداد قطعی» تلقی شود:

| شناسه | تصمیم موردنیاز |
|---|---|
| `DEC-001` | نام canonical مقدار XML: پیشنهاد `Omid`. |
| `DEC-002` | آیا body ارسالی `{"data":"..."}` است یا رشته encrypted خام؟ |
| `DEC-003` | آیا HMAC روی JSON نهایی wrapper محاسبه می‌شود یا encrypted payload خام؟ |
| `DEC-004` | casing قطعی fieldها، مخصوصاً `taxid` در برابر `taxId`. |
| `DEC-005` | نوع قطعی `indatim/indati2m`: Unix milliseconds یا مقدار دیگر. |
| `DEC-006` | رابطه `requestId` ارسالی با `trackingId` پاسخ و `requestIds` inquiry. |
| `DEC-007` | schema کامل پاسخ login و محل token/expiry. |
| `DEC-008` | schema کامل send برای موفقیت کامل، جزئی و رد کامل. |
| `DEC-009` | schema کامل inquiry و فهرست statusهای terminal/pending/failed. |
| `DEC-010` | محدودیت batch، payload bytes، rate limit و concurrency. |
| `DEC-011` | tolerance زمانی timestamp و سیاست clock synchronization. |
| `DEC-012` | idempotency و رفتار retry پس از timeout یا قطع ارتباط. |
| `DEC-013` | رابطه `Format Agent Model` و `sales_template` با schema واقعی API و مالک تولید فیلدهای محاسباتی. |
| `DEC-014` | قواعد قطعی اصلی/اصلاح/ابطال/برگشت و الگوی طلا، شامل `ins`، `irtaxid` و `tcpbs`. |

خروجی موردقبول این مرحله یکی از این دو مورد است:

- مستند رسمی/Swagger جدید که تناقض‌ها را رفع کند؛ یا
- نمونه request/response تاییدشده از sandbox برای login، send و inquiry.

## 8. فازهای اجرا

### فاز 0 - تثبیت قرارداد و خط مبنا

#### کارها

- ثبت baseline جدید Git پیش از هر تغییر.
- ثبت hash یا نسخه مستند PDF، Excel و source نمونه.
- حل `DEC-001` تا `DEC-012`.
- دریافت credential غیرعملیاتی و endpoint sandbox از مسیر امن.
- تعیین فاکتور تست غیرمشتری و نتیجه موردانتظار.
- ثبت payload و responseهای نمونه به‌صورت redacted fixture.
- تعیین نگاشت وضعیت Omid به وضعیت داخلی.
- دریافت پاسخ مکتوب به `OMID-VENDOR-CONTRACT-QUESTIONS.md` و بستن `DEC-013/014`.

#### Gate 0

- هیچ تناقض باز در body/signature وجود ندارد.
- parser بر اساس نمونه پاسخ واقعی یا تاییدشده طراحی می‌شود.
- secret در repository قرار نگرفته است.
- Tracker تمام تصمیم‌ها و owner آنها را ثبت کرده است.

### فاز 1 - قرارداد تنظیمات و امنیت secrets

#### کارها

- افزودن propertyهای صریح Omid به `DTOServerInfo` یا تعریف config اختصاصی تاییدشده.
- تعیین نام elementهای XML بدون repurpose مبهم فیلدهای certificate فعلی.
- اعتبارسنجی required/empty/Base64/URI.
- تولید نمونه config بدون secret واقعی.
- تعریف masking برای log و خطا.
- بررسی copy شدن XML به output و publish بدون افشای ناخواسته.

#### Gate 1

- startup با config معتبر و نامعتبر رفتار قابل پیش‌بینی دارد.
- مقدار ناشناخته `ServiceType` fail-fast می‌شود.
- هیچ secret در Git diff یا log دیده نمی‌شود.

### فاز 2 - اسکلت provider و routing

#### کارها

- ایجاد `OmidTaxService` با surface موردنیاز.
- ایجاد singleton/instance سازگار با معماری فعلی، بدون state اشتراکی per-operation.
- افزودن `_omidService` به `BMain`.
- افزودن branch صریح `Omid` در constructor و تمام عملیات façade.
- تبدیل fallback نهایی به branch صریح `Tis` و خطای configuration برای مقدار ناشناخته.
- حفظ رفتار providerهای موجود با characterization test.

#### Gate 2

- `ServiceType=Omid` فقط Omid را انتخاب می‌کند.
- `Tis/Tax/Rima/RimaCalculate` همان provider قبلی را انتخاب می‌کنند.
- مقدار typo یا خالی به Tiss منحرف نمی‌شود.

### فاز 3 - Crypto، HTTP و authentication

#### کارها

- پیاده‌سازی `OmidCryptoService` با test vector ثابت.
- پیاده‌سازی serialization دقیق مورد استفاده signature.
- تولید nonce یکتا برای هر درخواست.
- تولید timestamp UTC ثانیه‌ای.
- پیاده‌سازی `OmidApiClient` روی runtime سازگار با `net8.0`.
- استفاده از client قابل reuse و timeout مشخص.
- پیاده‌سازی login typed.
- cache token در حافظه؛ عدم ذخیره token در فایل متنی.
- مدیریت expiry و re-login هم‌زمان‌امن.
- retry یک‌باره authentication پس از 401، بدون retry کور ارسال.
- تولید خطای فارسی امن و log فنی redacted.

#### Gate 3

- AES و HMAC با fixture تاییدشده برابرند.
- headerها و body دقیقاً با قرارداد تاییدشده منطبق‌اند.
- login sandbox موفق و خطاهای signature/timestamp/auth قابل تشخیص‌اند.

### فاز 4 - DTO و mapping صورتحساب

#### کارها

- تعریف DTOهای typed برای login، send، inquiry و error.
- تعریف envelope شامل `batchId`، `fiscalId` و invoiceها.
- استخراج/استفاده مجدد کنترل‌شده از mapping معتبر RimaCalculate.
- تطبیق field-by-field با PDF و Excel.
- تعیین null omission در برابر ارسال null/zero.
- اعتبارسنجی `sstid` و فیلدهای اجباری.
- اعتبارسنجی tax ID، `inno`، `irtaxid`، timestamp و انواع عددی.
- ثبت correlation map بدون جست‌وجوی رشته‌ای در JSON.
- ساخت snapshot/contract fixture برای الگوهای منتخب صورتحساب.

#### سناریوهای اجباری mapping

- صورتحساب نوع 1 عادی.
- صورتحساب نوع 2 یا الگوی مرتبط موجود در داده مشتری.
- فاکتور دارای تخفیف.
- خریدار حقیقی و حقوقی.
- فاکتور بدون اطلاعات پرداخت و با اطلاعات پرداخت.
- اصلاحی/ابطالی/برگشت از فروش در صورت پشتیبانی Omid.
- فاکتور دارای `irtaxid` مرجع.
- فاکتور با بیش از یک ردیف.
- مقادیر null و zero در فیلدهای اختیاری.

#### Gate 4

- payload fixtureها با قرارداد Omid و محاسبات داخلی reconcile شده‌اند.
- هیچ field با casing، type یا واحد زمانی مبهم باقی نمانده است.
- mapping existing providerها تغییر نکرده است.

### فاز 5 - ارسال تکی و گروهی

#### کارها

- پیش‌بررسی وضعیت قابل ارسال در دیتابیس.
- آماده‌سازی payload هر فاکتور و ثبت failure محلی قابل اقدام.
- batch بندی مطابق محدودیت تاییدشده Omid.
- محدودسازی concurrency مستقل از اعداد RimaCalculate.
- ارسال و parse پاسخ typed.
- تطبیق پاسخ با invoice از طریق correlation صریح.
- پشتیبانی از موفقیت جزئی batch.
- ذخیره tracking/reference و وضعیت pending.
- جلوگیری از duplicate send در retry یا double-click.
- مدیریت timeout مبهم بدون ثبت اشتباه موفقیت/شکست قطعی.
- تولید `ResultModel` دقیق برای موفقیت کامل، جزئی و شکست کامل.

#### Gate 5

- ارسال یک و چند فاکتور در sandbox موفق است.
- هر پاسخ دقیقاً به invoice صحیح متصل می‌شود.
- pending/reference در دیتابیس صحیح ذخیره می‌شود.
- duplicate و partial response پوشش تست دارند.

### فاز 6 - inquiry و نگاشت وضعیت

#### کارها

- پیاده‌سازی inquiry typed با `requestIds` تاییدشده.
- پیاده‌سازی inquiry تکی از invoice ID محلی.
- پیاده‌سازی inquiry گروهی برای فهرست صفحه.
- پیاده‌سازی pending recovery.
- پیاده‌سازی بازه تاریخ از روی داده محلی و referenceها.
- تعریف status map مرکزی Omid → وضعیت داخلی.
- ثبت success، pending، validation failure و unknown.
- ثبت error code/message/details به‌صورت قابل audit.
- عدم تبدیل response missing یا unknown به success.
- پیاده‌سازی `GetFullInvoice` در صورت تایید contract.

#### Gate 6

- success/pending/failed/unknown با fixture و sandbox تایید شده‌اند.
- استعلام مجدد idempotent است.
- نتیجه تازه سرویس از وضعیت صرفاً محلی در UI قابل تشخیص است.

### فاز 7 - progress، Controller و سازگاری UI

#### کارها

- پذیرش `operationId` در ارسال و inquiry Omid.
- ارسال progress فقط به SignalR group همان عملیات.
- شمارش `processed/total/succeeded/failed` بدون state static مشترک.
- پایان صریح عملیات در success، partial failure و exception.
- حفظ endpointها و JavaScript موجود مگر نیاز اثبات‌شده.
- نمایش پیام فارسی امن و نگهداری جزئیات فنی در log.
- بررسی exportهای Excel و صفحات ارسال/استعلام با Omid.

#### Gate 7

- دو کاربر/دو تب پیام یکدیگر را نمی‌بینند.
- progress برای نتیجه موفق و ناموفق دقیقاً به total می‌رسد.
- UI بدون F5 از عملیات خارج می‌شود.

### فاز 8 - آزمون و hardening

#### آزمون‌های واحد

- AES valid/invalid key/IV.
- HMAC و serialization deterministic.
- URI validation.
- token cache/expiry/concurrent login.
- DTO parsing برای تمام response fixtureها.
- status mapping.
- payload mapping برای سناریوهای فاکتور.
- routing تمام `ServiceType`ها.

#### آزمون‌های integration کنترل‌شده

- login موفق و ناموفق.
- send تک و گروهی.
- partial batch.
- inquiry pending → success.
- inquiry validation failure.
- 401، 403، 429، 5xx، timeout و malformed JSON.
- nonce تکراری و اختلاف ساعت.
- قطع ارتباط پس از ارسال و قبل از دریافت پاسخ.

#### regression

- build پروژه با تنظیمات فعلی.
- smoke test برای `Tax`، `Rima`، `RimaCalculate` و `Tis` در حد امکان.
- عدم تغییر route، JSON response و UI contract موجود.
- `git diff --check` و secret scan.

#### Gate 8

- صفر خطای build.
- آزمون‌های contract و regression مرتبط پاس شده‌اند.
- هیچ secret یا token در artifactها نیست.
- محدودیت‌های تاییدنشده صریحاً در Tracker باقی مانده‌اند.

### فاز 9 - publish، sandbox acceptance و rollback rehearsal

#### کارها

- publish به مسیر جداگانه و غیرعملیاتی.
- بررسی وجود DLLها، config و Resources در output.
- قرار دادن secret فقط از کانال امن محیط مقصد.
- اجرای startup و `CheckConnection`.
- اجرای send/inquiry با داده تست.
- بررسی log، دیتابیس، SignalR و مرورگر.
- تمرین rollback به publish قبلی و `ServiceType` قبلی.

#### Gate 9

- کل مسیر browser → controller → BMain → Omid → database → UI در sandbox تایید شده است.
- rollback آزمایش شده و زمان/مراحل آن ثبت شده است.

### فاز 10 - تحویل محدود مشتری و پایش

#### کارها

- backup از publish و config مقصد.
- ثبت tenant/customer و window تغییر.
- فعال‌سازی `ServiceType=Omid` در runtime XML مقصد.
- restart کنترل‌شده.
- `CheckConnection` و ارسال canary غیرحساس/تاییدشده.
- inquiry تا وضعیت نهایی.
- تطبیق UI، log و رکوردهای دیتابیس.
- پایش خطا، latency، pending و duplicate.
- تصمیم go/rollback با owner مشخص.

#### Gate 10

- حداقل یک چرخه کامل واقعی با تایید کاربر کسب‌وکار ثبت شده است.
- هیچ regression بحرانی در providerهای دیگر مشاهده نشده است.
- runbook و وضعیت نهایی Tracker به‌روز است.

## 9. نگاشت وضعیت پیشنهادی داخلی

نام دقیق statusهای Omid باید در فاز 0 تایید شود، اما قرارداد داخلی باید این چهار دسته را حفظ کند:

| دسته Omid | رفتار داخلی |
|---|---|
| accepted/queued/processing | `fSendState = 2` و پیام «در حال بررسی» |
| final success | `fSendState = 4` و ثبت موفق |
| validation/final failure | `fSendState = 3` و ثبت جزئیات خطا |
| unknown/missing/transport ambiguous | موفق محسوب نشود؛ وضعیت قابل بازیابی و inquiry مجدد حفظ شود |

status ناشناخته نباید به‌صورت default به failed نهایی یا success تبدیل شود؛ ابتدا باید raw status به‌شکل redacted ثبت و برای بررسی نگه داشته شود.

## 10. اصول امنیتی اجباری

- secret واقعی در source، Markdown، fixture، screenshot یا Git ثبت نشود.
- username/password نمونه console به پروژه اصلی منتقل نشود.
- token در `token.txt` یا فایل قابل دسترس برنامه ذخیره نشود.
- headerهای `Authorization`، `X-Api-Key` و signature در log کامل ثبت نشوند.
- payload خام فاکتور فقط در logger تاییدشده و مطابق سیاست داده مشتری ثبت شود.
- encrypted payload نیز داده حساس تلقی شود.
- خطای کاربر فارسی و امن باشد؛ exception فنی در logger ثبت شود.
- callback غیرفعال‌کردن اعتبارسنجی TLS از providerهای قدیمی کپی نشود.
- هیچ retry خودکار send بدون idempotency تاییدشده انجام نشود.

## 11. Definition of Ready برای هر Task

یک Task فقط وقتی `READY` می‌شود که:

- dependencyهای آن `DONE` باشند.
- ورودی و خروجی مشخص داشته باشد.
- فایل‌های مجاز برای تغییر مشخص باشند.
- معیار پذیرش قابل آزمون داشته باشد.
- نیاز به credential یا محیط خارجی تعیین شده باشد.
- ریسک secret، دیتابیس و API بررسی شده باشد.

## 12. Definition of Done برای هر Task

یک Task فقط وقتی `DONE` می‌شود که:

- تغییر موردنظر کامل باشد، نه صرفاً scaffold.
- build/test متناسب پاس شده باشد.
- evidence شامل command، نتیجه و فایل/خط مرتبط در Tracker ثبت شده باشد.
- secret scan و `git diff --check` در صورت تغییر کد پاس شده باشد.
- regression مرتبط بررسی شده باشد.
- محدودیت runtime صریح ثبت شده باشد.
- Tracker همان turn به‌روز شده باشد.

## 13. پروتکل کار Agent

Agent در ابتدای هر نوبت Omid باید:

1. این Roadmap و Tracker را کامل بخواند.
2. `git status --short` را ثبت کند.
3. تغییرات موجود کاربر را از تغییرات Omid تفکیک کند.
4. فقط یک Task یا یک مجموعه کوچک هم‌وابسته با وضعیت `READY` انتخاب کند.
5. پیش از ویرایش، status آن را `IN_PROGRESS` کند.
6. فقط فایل‌های اعلام‌شده در scope Task را تغییر دهد.
7. validation متناسب را اجرا کند.
8. evidence و نتیجه را در Tracker ثبت کند.
9. در صورت blocker، Task را `BLOCKED` و نیاز دقیق را ثبت کند؛ کد حدسی production ننویسد.
10. بدون درخواست صریح commit یا push نکند.

### ممنوعیت‌های Agent

- پاک‌کردن یا overwrite تغییرات قبلی checkout.
- تغییر XML عملیاتی برای تست بدون تایید.
- نمایش secret در خروجی ابزار یا گزارش.
- اعلام «تکمیل» فقط بر اساس build.
- استفاده از response schema حدسی به‌عنوان قرارداد قطعی.
- اجرای ارسال واقعی مشتری بدون مجوز روشن.
- کپی‌کردن concurrency، TLS bypass یا retry provider دیگر بدون تطبیق Omid.

## 14. معیار تکمیل کل Integration

Integration فقط وقتی کامل است که:

- `ServiceType=Omid` در runtime مقصد provider صحیح را انتخاب کند.
- initialization، login، send و inquiry واقعی کار کنند.
- ارسال تکی و گروهی correlation صحیح داشته باشند.
- success/pending/failed در دیتابیس و UI صحیح باشند.
- progress بین کاربران مخلوط نشود.
- خطاهای امنیتی/شبکه‌ای پیام امن و log قابل اقدام داشته باشند.
- providerهای موجود regression نداشته باشند.
- sandbox و یک acceptance محدود مشتری ثبت شده باشد.
- rollback تمرین و مستند شده باشد.
- تمام Taskهای mandatory در Tracker `DONE` و تمام استثناها دارای تایید باشند.
