using System.ComponentModel.DataAnnotations;

namespace EmailSender.Records
{
    public sealed class SendEmailRequest
    {
        [Required, EmailAddress]
        public string To { get; init; } = string.Empty;

        [Required, MinLength(1)]
        public string ToName { get; init; } = string.Empty;

        [Required, StringLength(100)]
        public string Subject { get; init; } = string.Empty;

        [Required, MinLength(1)]
        public string Body { get; init; } = string.Empty;

        public SendEmailRequest()
        {
        }

        public SendEmailRequest(string To, string ToName, string Subject, string Body)
        {
            this.To = To;
            this.ToName = ToName;
            this.Subject = Subject;
            this.Body = Body;
        }
    }
}
