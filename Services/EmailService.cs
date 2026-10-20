
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Booking_thanhnt.Services;

public class EmailSettings
{
    public string SmtpServer { get; set; } = "smtp.gmail.com";
    public int Port { get; set; } = 587;
    public string SenderName { get; set; } = "Barber Booking";
    public string SenderEmail { get; set; } = "";
    public string Username { get; set; } = "";
    public string Password { get; set; } = "";
}

public interface IEmailService
{
    Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody);
}

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;

    public EmailService(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendEmailAsync(
        string toEmail,
        string subject,
        string htmlBody)
    {
        var message = new MimeMessage();

        message.From.Add(new MailboxAddress(
            _settings.SenderName,
            _settings.SenderEmail));

        message.To.Add(MailboxAddress.Parse(toEmail));
        message.Subject = subject;
        message.Body = new BodyBuilder
        {
            HtmlBody = htmlBody
        }.ToMessageBody();

        using var client = new SmtpClient();

        await client.ConnectAsync(
            _settings.SmtpServer,
            _settings.Port,
            SecureSocketOptions.StartTls);

        try
        {
            await client.AuthenticateAsync(
                _settings.Username,
                _settings.Password);

            await client.SendAsync(message);
        }
        finally
        {
            await client.DisconnectAsync(true);
        }
    }
}
