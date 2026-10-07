using Microsoft.Extensions.Logging;
using ClsTools;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

using TaxCollectData.Library.Models;
using TaxCollectData.Library.Properties;
using TaxServiceAndApp.Models;
using TaxServiceAndApp.Models.SharedDTO;
using TaxServiceAndApp.Models.TaxDTO;
using TaxServiceAndApp.Models.TISSDTO;
using Microsoft.AspNetCore.SignalR;
using TaxWebAppCore8.BI.MessagingService;
using Microsoft.EntityFrameworkCore;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;
using TaxWebAppCore8.Models.SharedDTO;
using Org.BouncyCastle.Crypto;
using System.Reflection;
using TaxWebAppCore8.BI.Omid;

namespace TaxServiceAndApp.BI
{
    /// <summary>
    /// نقطهٔ مرکزی انتخاب سرویس مالیاتی.
    /// کنترلرها BMain را صدا می‌زنند و BMain بر اساس ServiceType
    /// درخواست را به سرویس مناسب هدایت می‌کند.
    /// </summary>
    public class BMain
    {
        // نمونهٔ سرویس‌های موجود پروژه.
        [DataMember] public static TissServise _tissServise;
        [DataMember] public static TaxService _taxService;
        [DataMember] public static RimaService _rimaService;
        [DataMember] public static RimaServiceCalculateMethod _rimaCalcService;

        // سرویس جدید امید؛ فعلاً فاکتور را می‌خواند و درخواست را می‌سازد.
        [DataMember] public static OmidTaxService _omidService;

        public static IHubContext<RequestsHub> _hubContext;

        // تنظیمات مشتری از CustomerInformation.xml خوانده می‌شوند.
        public static DTOServerInfo SrvInfo;

        // مسیر Resources در Program.cs تنظیم می‌شود.
        public static string ServerResourcesPath;

        // واسط دسترسی به دیتابیس.
        public static IContext _Context;

        /// <summary>
        /// ServiceType را بدون حساسیت به بزرگی حروف و فاصله‌ها بررسی می‌کند.
        /// </summary>
        private static bool IsService(string name)
        {
            return string.Equals(
                SrvInfo?.ServiceType?.Trim(),
                name,
                StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// برای عملیات امید که هنوز آماده نیستند خطای مشخص می‌سازد.
        /// </summary>
        private static InvalidOperationException OmidNotReady(
            string operation)
        {
            return new InvalidOperationException(
                $"عملیات «{operation}» برای سرویس امید هنوز پیاده‌سازی نشده است.");
        }

        /// <summary>
        /// هنگام شروع برنامه، تنظیمات و دیتابیس را می‌خواند
        /// و سرویس مشخص‌شده در ServiceType را می‌سازد.
        /// </summary>
        public BMain()
        {
            SrvInfo = ClsTools.ClsTools.DeSerializeObject<DTOServerInfo>(
                ServerResourcesPath + "/CustomerID/CustomerInformation.xml");

            var databaseType = SrvInfo.DataBaseType ?? "baran";

            _Context = DBBridge.getInstance(
                databaseType.ToLowerInvariant());

            if (IsService("Tax"))
                _taxService = TaxService.getInstance(_Context);
            else if (IsService("Rima"))
                _rimaService = RimaService.getInstance(_Context);
            else if (IsService("RimaCalculate"))
                _rimaCalcService =
                    RimaServiceCalculateMethod.getInstance(_Context);
            else if (IsService("Omid"))
                _omidService = new OmidTaxService(_Context, SrvInfo);
            else if (IsService("Tis"))
                _tissServise = TissServise.getInstance(_Context);
            else
                throw new InvalidOperationException(
                    $"ServiceType نامعتبر است: " +
                    $"'{SrvInfo.ServiceType ?? "(خالی)"}'");
        }

        /// <summary>
        /// فهرست فاکتورهای پنل را برمی‌گرداند.
        /// امید فعلاً این فهرست را از دیتابیس محلی می‌خواند.
        /// </summary>
        public static async Task<List<GeneralInvoiceHeaderViewModel>>
            GetAllInvoiceAsync(SearchParameters search)
        {
            await InitAsync();

            if (IsService("Tax"))
                return await _taxService.GetAllInvoice(search);

            if (IsService("Rima"))
                return await _rimaService.GetAllInvoice(search);

            if (IsService("RimaCalculate"))
                return await _rimaCalcService.GetAllInvoice(search);

            if (IsService("Omid"))
                return await _Context.GetAllInvoice(search);

            if (IsService("Tis"))
                return await _tissServise.GetAllInvoice(search);

            throw new InvalidOperationException(
                "ServiceType نامعتبر است.");
        }

        /// <summary>
        /// فهرست را همراه با استعلام وضعیت از سرویس بیرونی می‌گیرد.
        /// استعلام امید هنوز آماده نیست.
        /// </summary>
        public static async Task<List<GeneralInvoiceHeaderViewModel>>
            GetAllInvoiceWithInqFromServerAsync(
                SearchParameters search,
                string? operationId = null)
        {
            await InitAsync();

            if (IsService("Tax"))
                return await _taxService
                    .GetAllInvoiceByInqFromServer(search);

            if (IsService("Rima"))
                return await _rimaService
                    .GetAllInvoiceByInqFromServer(search);

            if (IsService("RimaCalculate"))
                return await _rimaCalcService
                    .GetAllInvoiceByInqFromServer(
                        search,
                        operationId);

            if (IsService("Omid"))
                throw OmidNotReady(
                    "فهرست همراه با استعلام");

            if (IsService("Tis"))
                return await _tissServise
                    .GetAllInvoiceByInqFromServer(search);

            throw new InvalidOperationException(
                "ServiceType نامعتبر است.");
        }

        /// <summary>
        /// جزئیات یک فاکتور را از دیتابیس محلی می‌خواند.
        /// </summary>
        public static async Task<GeneralInvoiceViewModel>
            GetInvoiceAsync(string invoiceID)
        {
            return await _Context.GetInvoice(invoiceID);
        }

        /// <summary>
        /// اطلاعات مالیاتی ردیف‌های فاکتور را از دیتابیس می‌خواند.
        /// </summary>
        public static List<DTO_UV_TissInfoForShow>
            GetAllTissInfoItems(
                Guid invoiceID,
                int storeId,
                int workstaionID)
        {
            return _Context.GetAllTissInfoItemsFromDetailInfo(
                invoiceID,
                storeId,
                workstaionID);
        }

        /// <summary>
        /// وضعیت فاکتورها را بر اساس بازهٔ تاریخ استعلام می‌کند.
        /// </summary>
        public static async Task<List<InquiryResultModel>>
            GetInvoiceStatusByDateRangeAsync(
                string fromdate,
                string todate)
        {
            await InitAsync();

            if (IsService("Tax"))
                return await _taxService
                    .GetInvoiceStatusByDateRange(
                        fromdate,
                        todate);

            if (IsService("Rima"))
                return await _rimaService
                    .GetInvoiceStatusByDateRange(
                        fromdate,
                        todate);

            if (IsService("RimaCalculate"))
                return await _rimaCalcService
                    .GetInvoiceStatusByDateRange(
                        fromdate,
                        todate);

            if (IsService("Omid"))
                throw OmidNotReady(
                    "استعلام وضعیت بر اساس بازهٔ تاریخ");

            if (IsService("Tis"))
                return await _tissServise
                    .GetInvoiceStatusByDateRange(
                        fromdate,
                        todate);

            throw new InvalidOperationException(
                "ServiceType نامعتبر است.");
        }

        /// <summary>
        /// وضعیت یک فاکتور را استعلام می‌کند.
        /// </summary>
        public static async Task<ResultModel>
            GetInvoiceStatusByRefrenceAsync(string invoiceID)
        {
            await InitAsync();

            if (IsService("Tax"))
                return await _taxService
                    .GetInvoiceStatusByRefrenceAsync(invoiceID);

            if (IsService("Rima"))
                return await _rimaService
                    .GetInvoiceStatusByRefrenceAsync(invoiceID);

            if (IsService("RimaCalculate"))
                return await _rimaCalcService
                    .GetInvoiceStatusByRefrenceAsync(invoiceID);

            if (IsService("Omid"))
                throw OmidNotReady(
                    "استعلام وضعیت فاکتور");

            if (IsService("Tis"))
                return await _tissServise
                    .GetInvoiceStatusByRefrenceAsync(invoiceID);

            throw new InvalidOperationException(
                "ServiceType نامعتبر است.");
        }

        /// <summary>
        /// درخواست‌های قبلی بدون وضعیت نهایی را پیگیری می‌کند.
        /// </summary>
        public static async Task
            GetNotFinishedLastRefrenceIdAsync()
        {
            await InitAsync();

            if (IsService("Tax"))
                await _taxService
                    .GetNotFinishedLastRefrenceIdAsync();
            else if (IsService("Rima"))
                await _rimaService
                    .GetNotFinishedLastRefrenceIdAsync();
            else if (IsService("RimaCalculate"))
                await _rimaCalcService
                    .GetNotFinishedLastRefrenceIdAsync();
            else if (IsService("Omid"))
                throw OmidNotReady(
                    "پیگیری درخواست‌های ناتمام");
            else if (IsService("Tis"))
                await _tissServise
                    .GetNotFinishedLastRefrenceIdAsync();
            else
                throw new InvalidOperationException(
                    "ServiceType نامعتبر است.");
        }

        /// <summary>
        /// سرویس انتخاب‌شده را پیش از عملیات آماده می‌کند.
        /// امید فعلاً Init عملیاتی ندارد.
        /// </summary>
        public static async Task InitAsync()
        {
            if (IsService("Omid"))
                return;

            try
            {
                if (IsService("Tax"))
                    await _taxService.Init();
                else if (IsService("Rima"))
                    await _rimaService.Init();
                else if (IsService("RimaCalculate"))
                    await _rimaCalcService.Init();
                else if (IsService("Tis"))
                    await _tissServise.Init();
                else
                    throw new InvalidOperationException(
                        "ServiceType نامعتبر است.");
            }
            catch (Exception ex)
            {
                ClsTools.Logger.NPSLogger.GetLogger(
                    ClsTools.Logger.ModuleName.ArghamCompany_TaxGoverment)
                    .LogError(
                        $"BaranDB:" +
                        $"{MethodBase.GetCurrentMethod()!.DeclaringType!.Name}::" +
                        " InitAsync ::" +
                        ex.Message +
                        "::" +
                        ex.InnerException?.Message,
                        ex);
            }
        }

        /// <summary>
        /// ارسال گروهی فاکتورها از پنل.
        /// ارسال امید هنوز آماده نیست و صریحاً متوقف می‌شود.
        /// </summary>
        public static async Task<ResultModel>
            SendGroupInvoicesAsync(
                List<DTOInvoiceLimitParameter> ids,
                string? operationId = null)
        {
            await InitAsync();

            if (IsService("Tax"))
                return await _taxService
                    .SendGroupInvoicesForGroupingAsync(ids);

            if (IsService("Rima"))
                return await _rimaService
                    .SendGroupInvoicesForGroupingAsync(ids);

            if (IsService("RimaCalculate"))
                return await _rimaCalcService
                    .SendGroupInvoicesForGroupingAsync(
                        ids,
                        operationId);

            if (IsService("Omid"))
                throw OmidNotReady(
                    "ارسال گروهی فاکتورها");

            if (IsService("Tis"))
                return await _tissServise
                    .SendGroupInvoicesForGroupingAsync(ids);

            throw new InvalidOperationException(
                "ServiceType نامعتبر است.");
        }

        /// <summary>
        /// ارسال یک فاکتور از پنل.
        /// مانند سرویس‌های فعلی، فاکتور به لیست تک‌عضوی تبدیل می‌شود.
        /// </summary>
        public static async Task<ResultModel>
            SendInvoiceAsync(
                DTOInvoiceLimitParameter invoice,
                bool isCancel = false)
        {
            await InitAsync();

            var invoices = new List<DTOInvoiceLimitParameter>
            {
                invoice
            };

            if (IsService("Tax"))
                return await _taxService
                    .SendGroupInvoicesForGroupingAsync(invoices);

            if (IsService("Rima"))
                return await _rimaService
                    .SendGroupInvoicesForGroupingAsync(invoices);

            if (IsService("RimaCalculate"))
                return await _rimaCalcService
                    .SendGroupInvoicesForGroupingAsync(invoices);

            if (IsService("Omid"))
                throw OmidNotReady(
                    "ارسال فاکتور");

            if (IsService("Tis"))
                return await _tissServise
                    .SendGroupInvoicesForGroupingAsync(invoices);

            throw new InvalidOperationException(
                "ServiceType نامعتبر است.");
        }

        /// <summary>
        /// پس از یک تأخیر، وضعیت یک عملیات را استعلام می‌کند.
        /// این رفتار از کد قبلی پروژه حفظ شده است.
        /// </summary>
        public static async Task ScheduleOperationAsync(
            string operationKey,
            int milisecond = -1)
        {
            if (milisecond == -1)
                await Task.Delay(
                    TimeSpan.FromMinutes(100));
            else
                await Task.Delay(1800000);

            PerformOperation(operationKey);
        }

        /// <summary>
        /// استعلام وضعیت عملیات زمان‌بندی‌شده.
        /// </summary>
        static async void PerformOperation(string operations)
        {
            await GetInvoiceStatusByRefrenceAsync(operations);
        }

        /// <summary>
        /// اتصال به سرویس انتخاب‌شده را بررسی می‌کند.
        /// مسیر عمومی بررسی اتصال امید هنوز وصل نشده است.
        /// </summary>
        public static async Task<bool> CheckConnectionAsync()
        {
            await InitAsync();

            if (IsService("Tax"))
                return await _taxService.CheckConnectionAsync();

            if (IsService("Rima"))
                return await _rimaService.CheckConnectionAsync();

            if (IsService("RimaCalculate"))
                return await _rimaCalcService
                    .CheckConnectionAsync();

            if (IsService("Omid"))
                throw OmidNotReady(
                    "بررسی اتصال");

            if (IsService("Tis"))
                return await _tissServise
                    .CheckConnectionAsync();

            throw new InvalidOperationException(
                "ServiceType نامعتبر است.");
        }

        /// <summary>
        /// فهرست فروشگاه‌ها را از دیتابیس محلی می‌گیرد.
        /// </summary>
        public static List<DTOStore> GetStore()
        {
            return _Context.GetStore();
        }

        /// <summary>
        /// انواع خطاهای ثبت‌شده در دیتابیس را می‌خواند.
        /// </summary>
        public static List<tTisTaxErrorType> GetErrorType()
        {
            using (var npsContext = new NPSGeneralContext())
            {
                npsContext.Database.Initialize(force: true);

                return npsContext.tTisTaxErrorTypes.ToList();
            }
        }

        /// <summary>
        /// اطلاعات فاکتور را از سرویس انتخاب‌شده درخواست می‌کند.
        /// این عملیات برای امید هنوز تکمیل نشده است.
        /// </summary>
        public static async Task<ResultModel>
            GetInvoiceByIDAsync(string invoiceId)
        {
            await InitAsync();

            if (IsService("Tax"))
                return await _taxService
                    .GetInvoiceByID(invoiceId);

            if (IsService("Rima"))
                return await _rimaService
                    .GetInvoiceByID(invoiceId);

            if (IsService("RimaCalculate"))
                return await _rimaCalcService
                    .GetInvoiceByID(invoiceId);

            if (IsService("Omid"))
                throw OmidNotReady(
                    "دریافت فاکتور از سرویس امید");

            if (IsService("Tis"))
                return await _tissServise
                    .GetInvoiceByID(invoiceId);

            throw new InvalidOperationException(
                "ServiceType نامعتبر است.");
        }
    }
}