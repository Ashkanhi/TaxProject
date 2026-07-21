using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;

public class SmtpSettings
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool EnableSSL { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
}

public interface IEmailSender
{
    Task SendEmailAsync(string toEmail, string subject, string message);
}

public class EmailSender : IEmailSender
{
    private readonly SmtpSettings _smtpSettings;

    public EmailSender(IOptions<SmtpSettings> smtpSettings)
    {
        _smtpSettings = smtpSettings.Value;
    }
     public EmailSender(SmtpSettings smtpSettings)
    {
        _smtpSettings = smtpSettings;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string message)
    {
        var emailMessage = new MimeMessage();
        emailMessage.From.Add(new MailboxAddress("Argham1", _smtpSettings.Username));
        emailMessage.To.Add(new MailboxAddress("", toEmail));
        emailMessage.Subject = subject;
        emailMessage.Body = new TextPart("plain") { Text = message };

        using (var client = new SmtpClient())
        {
            await client.ConnectAsync(_smtpSettings.Host, _smtpSettings.Port, _smtpSettings.EnableSSL);
            await client.AuthenticateAsync(_smtpSettings.Username, _smtpSettings.Password);
            await client.SendAsync(emailMessage);
            await client.DisconnectAsync(true);
        }
    }
}
