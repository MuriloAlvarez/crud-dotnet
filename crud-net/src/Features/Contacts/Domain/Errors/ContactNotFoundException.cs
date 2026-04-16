using crud_net.Shared.Errors;

namespace crud_net.Features.Contacts.Domain.Errors;

public sealed class ContactNotFoundException : AppException
{
    public ContactNotFoundException(Guid id)
        : base(AppErrorCodes.ContactNotFound, $"Contact '{id}' was not found.", StatusCodes.Status404NotFound)
    {
    }
}