using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Configuration;

namespace UserService.Services;

public class EmailService
{
    private readonly IConfiguration _cfg;
    public EmailService(IConfiguration cfg) => _cfg = cfg;

    public async Task SendAsync(string to, string subject, string htmlBody)
    {
        var msg = new MimeMessage();
        msg.From.Add(MailboxAddress.Parse(_cfg["Email:From"] ?? "no-reply@example.com"));
        msg.To.Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var host = _cfg["Email:SmtpHost"] ?? "localhost";
        var port = int.Parse(_cfg["Email:SmtpPort"] ?? "1025");
        var user = _cfg["Email:SmtpUser"];
        var pass = _cfg["Email:SmtpPass"];
        var useSsl = bool.Parse(_cfg["Email:SmtpUseSSL"] ?? "false");

        await client.ConnectAsync(host, port, useSsl);
        if (!string.IsNullOrEmpty(user))
            await client.AuthenticateAsync(user, pass);
        await client.SendAsync(msg);
        await client.DisconnectAsync(true);
    }

    public Task SendConfirmationEmail(string to, string confirmationLink) =>
        SendAsync(to, "Подтверждение аккаунта", $"<p>Подтвердите: <a href=\"{confirmationLink}\">{confirmationLink}</a></p>");

    public Task SendResetPasswordEmail(string to, string resetLink) =>
        SendAsync(to, "Сброс пароля", $"<p>Сброс пароля: <a href=\"{resetLink}\">{resetLink}</a></p>");
}
