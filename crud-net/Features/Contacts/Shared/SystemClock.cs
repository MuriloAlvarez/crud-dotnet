namespace crud_net.Features.Contacts;

public sealed class SystemClock : IAppClock
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateOnly Today => DateOnly.FromDateTime(DateTime.UtcNow);
}