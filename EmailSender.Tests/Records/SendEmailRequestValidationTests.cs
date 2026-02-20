using System.ComponentModel.DataAnnotations;
using EmailSender.Records;

namespace EmailSender.Tests.Records;

public class SendEmailRequestValidationTests
{
    [Fact]
    public void Should_BeValid_WhenAllFieldsAreCorrect()
    {
        var request = new SendEmailRequest(
            To: "recipient@example.com",
            ToName: "Recipient",
            Subject: "Subject",
            Body: "Email body");

        var (isValid, errors) = Validate(request);

        Assert.True(isValid);
        Assert.Empty(errors);
    }

    [Fact]
    public void Should_BeInvalid_WhenEmailIsMalformed()
    {
        var request = new SendEmailRequest(
            To: "invalid-email",
            ToName: "Recipient",
            Subject: "Subject",
            Body: "Email body");

        var (isValid, errors) = Validate(request);

        Assert.False(isValid);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SendEmailRequest.To)));
    }

    [Fact]
    public void Should_BeInvalid_WhenToNameIsEmpty()
    {
        var request = new SendEmailRequest(
            To: "recipient@example.com",
            ToName: string.Empty,
            Subject: "Subject",
            Body: "Email body");

        var (isValid, errors) = Validate(request);

        Assert.False(isValid);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SendEmailRequest.ToName)));
    }

    [Fact]
    public void Should_BeInvalid_WhenBodyIsEmpty()
    {
        var request = new SendEmailRequest(
            To: "recipient@example.com",
            ToName: "Recipient",
            Subject: "Subject",
            Body: string.Empty);

        var (isValid, errors) = Validate(request);

        Assert.False(isValid);
        Assert.Contains(errors, e => e.MemberNames.Contains(nameof(SendEmailRequest.Body)));
    }

    private static (bool IsValid, IReadOnlyList<ValidationResult> Errors) Validate(SendEmailRequest request)
    {
        var context = new ValidationContext(request);
        var errors = new List<ValidationResult>();
        var isValid = Validator.TryValidateObject(request, context, errors, validateAllProperties: true);
        return (isValid, errors);
    }
}
