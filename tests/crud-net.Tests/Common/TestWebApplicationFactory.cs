using crud_net.Features.Contacts;
using crud_net.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace crud_net.Tests.Common;

public sealed class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public static readonly DateTime FixedUtcNow = new(2026, 4, 15, 12, 0, 0, DateTimeKind.Utc);
    public static readonly DateOnly FixedToday = DateOnly.FromDateTime(FixedUtcNow);

    private readonly string _databaseName = $"CrudNetTests-{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();
            services.RemoveAll<IAppClock>();

            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            services.AddSingleton<IAppClock>(new FixedClock(FixedUtcNow));
        });
    }

    private sealed class FixedClock : IAppClock
    {
        public FixedClock(DateTime utcNow)
        {
            UtcNow = utcNow;
            Today = DateOnly.FromDateTime(utcNow);
        }

        public DateTime UtcNow { get; }

        public DateOnly Today { get; }
    }
}
