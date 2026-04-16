using crud_net.Shared.Errors;

namespace crud_net.Features.Contacts.Domain.Errors;

public sealed class InvalidIdException : AppException
{
    public InvalidIdException(string id)
        : base(AppErrorCodes.InvalidId, $"The identifier '{id}' is invalid.", StatusCodes.Status400BadRequest)
    {
    }
}