namespace EmailSender.Infra;

using MailKit;

public class MailKitSender
{
    private readonly IConfiguration _configuration;

    MailKitSender(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public void SendEmail()
    {
        var _host = _configuration["Email:Host"];
        var _user = _configuration["Email:User"];
        var _pass = _configuration["Email:Pass"];
        var _fromName = _configuration["Email:FromName"];
        var _fromEmail = _configuration["Email:FromEmail"];
        var port = int.TryParse(_configuration["Email:Port"], out var p) ? p : 587;

        
    }
}