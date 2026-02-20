namespace EmailSender.Infra;

using EmailSender.Records;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using System.Text;

public class MailKitSender
{
    private readonly IConfiguration _configuration;

    public MailKitSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendEmailAsync(SendEmailRequest sendEmailRequest, CancellationToken ct = default)
    {
        var host = _configuration["Email:Host"] ?? throw new InvalidOperationException("Email:Host is not configured.");
        var user = _configuration["Email:User"] ?? throw new InvalidOperationException("Email:User is not configured.");
        var rawPass = _configuration["Email:Pass"] ?? throw new InvalidOperationException("Email:Pass is not configured.");
        var pass = NormalizeAppPassword(rawPass);
        var fromName = _configuration["Email:FromName"] ?? throw new InvalidOperationException("Email:FromName is not configured.");
        var fromEmail = _configuration["Email:FromEmail"] ?? throw new InvalidOperationException("Email:FromEmail is not configured.");
        var port = int.TryParse(_configuration["Email:Port"], out var p) ? p : 587;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromEmail));
        message.To.Add(new MailboxAddress(sendEmailRequest.ToName, sendEmailRequest.To));
        message.Subject = sendEmailRequest.Subject;
        message.Body = new TextPart("plain") { Text = sendEmailRequest.Body };

        using var client = new SmtpClient();
        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(user, pass, ct);
        await client.SendAsync(message, ct);
        await client.DisconnectAsync(true, ct);
    }

    private static string NormalizeAppPassword(string value)
    {
        // Google displays app passwords in groups; accept with or without spaces/quotes.
        var trimmed = value.Trim().Trim('"', '\'');
        var sanitized = new StringBuilder(trimmed.Length);

        foreach (var ch in trimmed)
        {
            if (!char.IsWhiteSpace(ch))
            {
                sanitized.Append(ch);
            }
        }

        return sanitized.ToString();
    }
}
