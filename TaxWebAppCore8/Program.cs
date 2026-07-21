using Microsoft.AspNetCore.SignalR;
using TaxServiceAndApp.BI;
using TaxWebAppCore8.BI.MessagingService;
using Quartz;
using Quartz.Impl;
using Quartz.Spi;
using TaxWebAppCore8.BI;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using TaxCollectData.Library.Dto;
using TaxWebAppCore8.Models.SharedDTO.BaranDB;
using System.Text.Json;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
var builder = WebApplication.CreateBuilder(args);

BMain.ServerResourcesPath = Path.Combine(AppContext.BaseDirectory, "Resources");

BMain bmain = new BMain();

// Add services to the container.
builder.Services.AddControllersWithViews().AddRazorRuntimeCompilation();


// Add required services
builder.Services.AddSingleton<IWebHostEnvironment>(builder.Environment); // For IWebHostEnvironment

// Middleware configuration...
builder.Services.AddSignalR();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Retain original property names
    });





builder.Services.Configure<SmtpSettings>(builder.Configuration.GetSection("SmtpSettings"));
builder.Services.AddTransient<IEmailSender, EmailSender>();
builder.Services.AddHttpContextAccessor();
// Add Quartz
builder.Services.AddQuartz(q =>
{
    q.UseMicrosoftDependencyInjectionJobFactory();

    var jobKey = new JobKey("MonthlyEmailJob");
    q.AddJob<MonthlyEmailJob>(opts => opts.WithIdentity(jobKey));

    q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("WeeklyEmailTrigger")
        .WithCronSchedule("0 0 12 ? * MON *"));  // Runs every Monday at noon
});

builder.Services.AddQuartz(q =>
{
    var jobKey = new JobKey("NightlyJob");
q.AddJob<NightlyJob>(opts => opts.WithIdentity(jobKey));

q.AddTrigger(opts => opts
        .ForJob(jobKey)
        .WithIdentity("NightlyJob-trigger")
        .WithCronSchedule("0 0 6 * * ?")); // Cron expression for 5 PM every day
                                            
});
//builder.Services.AddQuartz(q =>
//{
//    var jobKey = new JobKey("NightlyJob");
//    q.AddJob<NightlyJob>(opts => opts.WithIdentity(jobKey));

//    // Create a trigger to run the job one minute after application starts
//    q.AddTrigger(opts => opts
//        .ForJob(jobKey)
//        .WithIdentity("OneMinuteTrigger")
//        .StartAt(DateTimeOffset.UtcNow.AddMinutes(1)) // Start in one minute
//        .WithSimpleSchedule(x => x.WithInterval(TimeSpan.FromMinutes(1)).WithRepeatCount(0)) // Run once
//    );
//});
builder.Services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// In Program.cs or Startup.cs

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
});

// In Configure section:



var app = builder.Build();
app.UseAuthentication();
app.MapControllers();
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();
app.UseSession();
app.MapDefaultControllerRoute();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/");

app.MapHub<RequestsHub>("/RequestsHub");

BMain._hubContext = app.Services.GetRequiredService<IHubContext<RequestsHub>>();


app.Run();
