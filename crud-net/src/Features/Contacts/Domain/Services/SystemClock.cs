namespace crud_net.Features.Contacts.Domain.Services;

public sealed class SystemClock : IAppClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(UtcNow);
}