namespace EmailSender.Infra;

using EmailSender.Records;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
public class MailKitSender
{
    private readonly IConfiguration _configuration;

    MailKitSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public async Task SendEmailAsync(SendEmailRequest sendEmailRequest,CancellationToken ct = default)
    {
        var _host = _configuration["Email:Host"];
        var _user = _configuration["Email:User"];
        var _pass = _configuration["Email:Pass"];
        var _fromName = _configuration["Email:FromName"];
        var _fromEmail = _configuration["Email:FromEmail"];
        var port = int.TryParse(_configuration["Email:Port"], out var p) ? p : 587;

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(_fromName, _fromEmail));
        message.To.Add(new MailboxAddress(sendEmailRequest.ToName, sendEmailRequest.To));
        message.Subject = sendEmailRequest.Subject;
        message.Body = new TextPart("plain") { Text = sendEmailRequest.Body };

        using var client = new SmtpClient();
        client.ConnectAsync(_host, port, SecureSocketOptions.StartTls, ct);
        client.AuthenticateAsync(_user, _pass,ct);
        client.SendAsync(message,ct);
        client.DisconnectAsync(true,ct);
    }


    private void setUp()
    {
        
    }



}