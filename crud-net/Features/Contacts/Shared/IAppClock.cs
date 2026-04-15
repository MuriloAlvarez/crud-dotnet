namespace crud_net.Features.Contacts;

public interface IAppClock
{
    DateTime UtcNow { get; }
    DateOnly Today { get; }
}