using System.ComponentModel.DataAnnotations;

namespace EmailSender.Records
{
    public record SendEmailRequest(
        [property: Required,EmailAddress] string To,
        [property:Required, StringLength(100)] string Subject,
        [property:Required, MinLength(1)] string Body
    );
}