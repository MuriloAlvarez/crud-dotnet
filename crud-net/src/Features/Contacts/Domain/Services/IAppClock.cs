namespace crud_net.Features.Contacts.Domain.Services;

public interface IAppClock
{
    DateTime UtcNow { get; }

    DateOnly Today { get; }
}