using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Microsoft.Extensions.Options;
using Rota.Application.Interfaces;
using Rota.Application.Settings;

namespace Rota.Application.Services;

public sealed class EmailService : IEmailService
{
    private readonly EmailSettings _cfg;
    public EmailService(IOptions<EmailSettings> cfg) => _cfg = cfg.Value;

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress(_cfg.FromName, _cfg.FromAddress));
        msg.To  .Add(MailboxAddress.Parse(to));
        msg.Subject = subject;
        msg.Body    = new BodyBuilder { HtmlBody = htmlBody }.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_cfg.SmtpHost, _cfg.SmtpPort,
                                _cfg.UseSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None, ct);

        await smtp.AuthenticateAsync(_cfg.UserName, _cfg.Password, ct);
        await smtp.SendAsync(msg, ct);
        await smtp.DisconnectAsync(true, ct);
    }
}
