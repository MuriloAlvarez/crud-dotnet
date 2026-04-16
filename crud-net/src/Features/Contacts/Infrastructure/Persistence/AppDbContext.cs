using crud_net.Features.Contacts.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace crud_net.Features.Contacts.Infrastructure.Persistence;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Contact> Contacts => Set<Contact>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        modelBuilder.Entity<Contact>().HasQueryFilter(contact => !contact.IsDeleted);
    }
}