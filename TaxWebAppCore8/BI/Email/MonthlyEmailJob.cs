using Quartz;
using System.Threading.Tasks;
using TaxServiceAndApp.BI;

public class MonthlyEmailJob : IJob
{
    private readonly IEmailSender _emailSender;

    public MonthlyEmailJob(IEmailSender emailSender)
    {
        _emailSender = emailSender;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        // Replace with the recipient's email and your content
        string toEmail = "office@arghamnegargroup.com";
        string subject = "گزارش وضعیت مودیان " + BMain.SrvInfo.CustomerName;
        string message = "گزارش وضعیت فاکتورهای ارسالی(" + BMain.SrvInfo.CustomerName + ") تا تاریخ ::" + MD.PersianDateTime.PersianDateTime.Now;
        message += "\r\n";
        message += "\r\n";

        await _emailSender.SendEmailAsync(toEmail, subject, message);
    }
}
