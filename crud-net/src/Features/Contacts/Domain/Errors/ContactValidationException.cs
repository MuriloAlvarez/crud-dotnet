using crud_net.Shared.Errors;

namespace crud_net.Features.Contacts.Domain.Errors;

public sealed class ContactValidationException : AppException
{
    public ContactValidationException(Dictionary<string, string[]> errors)
        : base(AppErrorCodes.ContactValidationError, "Contact validation failed.", StatusCodes.Status400BadRequest)
    {
        Errors = errors;
    }

    public Dictionary<string, string[]> Errors { get; }
}