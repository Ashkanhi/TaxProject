using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI;
using TaxWebAppCore8.BI.MessagingService;
using TaxWebAppCore8.BI.Omid;
using TaxWebAppCore8.Models.DBContextMedels;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;

var builder = WebApplication.CreateBuilder(args);

// تعیین مسیر فایل‌های موردنیاز سرویس‌های فعلی.
BMain.ServerResourcesPath = Path.Combine(
    AppContext.BaseDirectory,
    "Resources");

// راه‌اندازی منطق فعلی برنامه و بررسی اشیای موردنیاز دیتابیس.
BMain bmain = new BMain();
DatabaseStartupInitializer.EnsureRequiredObjects(BMain.SrvInfo.DBConn);

// ثبت کنترلرها و Viewها و فعال‌سازی کامپایل Razor در زمان اجرا.
// نام خصوصیات در پاسخ JSON بدون تغییر باقی می‌ماند.
builder.Services
    .AddControllersWithViews()
    .AddRazorRuntimeCompilation()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// ثبت SignalR برای ارتباط لحظه‌ای با مرورگر.
builder.Services.AddSignalR();

// فراهم‌کردن دسترسی سرویس‌ها به درخواست HTTP جاری.
builder.Services.AddHttpContextAccessor();

// خواندن تنظیمات ایمیل و ثبت سرویس ارسال ایمیل.
builder.Services.Configure<SmtpSettings>(
    builder.Configuration.GetSection("SmtpSettings"));

builder.Services.AddTransient<IEmailSender, EmailSender>();

// خواندن بخش Omid از تنظیمات و اتصال آن به کلاس OmidConfig.
builder.Services.Configure<OmidConfig>(
    builder.Configuration.GetSection(OmidConfig.SectionName));

// درخواست‌های امید را بدون پروکسیِ سیستم ارسال می‌کنیم.
// این تنظیم فقط روی OmidApiClient اثر دارد.
builder.Services
    .AddHttpClient<OmidApiClient>()
    .ConfigurePrimaryHttpMessageHandler(() => new System.Net.Http.HttpClientHandler
    {
        UseProxy = false
    });

// ثبت کارهای زمان‌بندی‌شده برنامه.
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    // اجرای کار ارسال ایمیل، هر دوشنبه ساعت ۱۲ ظهر.
    var emailJobKey = new JobKey("MonthlyEmailJob");

    q.AddJob<MonthlyEmailJob>(opts =>
        opts.WithIdentity(emailJobKey));

    q.AddTrigger(opts => opts
        .ForJob(emailJobKey)
        .WithIdentity("WeeklyEmailTrigger")
        .WithCronSchedule("0 0 12 ? * MON *"));

    // اجرای کار روزانه در ساعت ۶ صبح.
    // این ساعت مطابق عبارت Cron موجود در کد قبلی است.
    var nightlyJobKey = new JobKey("NightlyJob");

    q.AddJob<NightlyJob>(opts =>
        opts.WithIdentity(nightlyJobKey));

    q.AddTrigger(opts => opts
        .ForJob(nightlyJobKey)
        .WithIdentity("NightlyJob-trigger")
        .WithCronSchedule("0 0 6 * * ?"));
});

// اجرای Quartz همراه برنامه.
// هنگام توقف برنامه، منتظر پایان کارهای در حال اجرا می‌ماند.
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true;
});

// ثبت Session با زمان انقضای ۳۰ دقیقه عدم فعالیت.
// این مقدار، همان مقدار مؤثر ثبت دوم در کد قبلی است.
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// ساخت برنامه پس از تکمیل ثبت سرویس‌ها.
var app = builder.Build();

// تنظیم مدیریت خطا و HSTS در محیط عملیاتی.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// هدایت درخواست‌های HTTP به HTTPS و ارائه فایل‌های ثابت.
app.UseHttpsRedirection();
app.UseStaticFiles();

// تعیین مسیر درخواست قبل از اجرای احراز هویت و مجوزها.
app.UseRouting();

// اجرای احراز هویت و بررسی مجوز دسترسی.
app.UseAuthentication();
app.UseAuthorization();

// فعال‌سازی Session پیش از اجرای کنترلرها.
app.UseSession();

// نگاشت کنترلرهایی که مسیر را با Attribute تعریف کرده‌اند.
app.MapControllers();

// مسیر پیش‌فرض صفحات برنامه.
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/");

// مسیر ارتباط SignalR.
app.MapHub<RequestsHub>("/RequestsHub");

// قراردادن ابزار ارسال پیام SignalR در اختیار منطق فعلی برنامه.
BMain._hubContext =
    app.Services.GetRequiredService<IHubContext<RequestsHub>>();

// شروع اجرای برنامه.
app.Run();