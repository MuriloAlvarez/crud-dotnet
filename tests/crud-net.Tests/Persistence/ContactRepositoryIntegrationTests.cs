using crud_net.Features.Contacts.Domain.Entities;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.Infrastructure.Persistence;
using crud_net.Features.Contacts.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace crud_net.Tests.Persistence;

public sealed class ContactRepositoryIntegrationTests
{
    [Fact]
    public async Task AddAndGetActiveById_ReturnsEntity()
    {
        await using var dbContext = CreateDbContext($"CrudNetRepositoryTests-{Guid.NewGuid():N}");
        var repository = new ContactRepository(dbContext);

        var contact = Contact.Create("Maria Silva", new DateOnly(1990, 1, 10), Gender.Female, DateTime.UtcNow);

        await repository.AddAsync(contact, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var found = await repository.GetActiveByIdAsync(contact.Id, CancellationToken.None);

        Assert.NotNull(found);
        Assert.Equal(contact.Id, found!.Id);
        Assert.Equal(contact.Name, found.Name);
        Assert.True(found.IsActive);
    }

    [Fact]
    public async Task ListActive_ExcludesDeactivatedContacts()
    {
        await using var dbContext = CreateDbContext($"CrudNetRepositoryTests-{Guid.NewGuid():N}");
        var repository = new ContactRepository(dbContext);

        var activeContact = Contact.Create("Ana Clara", new DateOnly(1992, 4, 4), Gender.Female, DateTime.UtcNow);
        var deactivatedContact = Contact.Create("Bruno Lima", new DateOnly(1988, 7, 7), Gender.Male, DateTime.UtcNow);
        deactivatedContact.Deactivate(DateTime.UtcNow);

        await repository.AddAsync(activeContact, CancellationToken.None);
        await repository.AddAsync(deactivatedContact, CancellationToken.None);
        await repository.SaveChangesAsync(CancellationToken.None);

        var contacts = await repository.ListActiveAsync(CancellationToken.None);

        Assert.Single(contacts);
        Assert.Equal(activeContact.Id, contacts[0].Id);
        Assert.DoesNotContain(contacts, contact => contact.Id == deactivatedContact.Id);
    }

    [Fact]
    public async Task GetById_DoesNotReturnSoftDeletedContact()
    {
        var databaseName = $"CrudNetRepositoryTests-{Guid.NewGuid():N}";
        Guid contactId;

        await using (var writeContext = CreateDbContext(databaseName))
        {
            var writeRepository = new ContactRepository(writeContext);
            var contact = Contact.Create("Carlos Souza", new DateOnly(1985, 2, 2), Gender.Male, DateTime.UtcNow);
            contactId = contact.Id;

            await writeRepository.AddAsync(contact, CancellationToken.None);
            await writeRepository.SaveChangesAsync(CancellationToken.None);

            contact.Delete(DateTime.UtcNow);
            await writeRepository.SaveChangesAsync(CancellationToken.None);
        }

        await using var readContext = CreateDbContext(databaseName);
        var readRepository = new ContactRepository(readContext);

        var found = await readRepository.GetByIdAsync(contactId, CancellationToken.None);

        Assert.Null(found);
    }

    private static AppDbContext CreateDbContext(string databaseName)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName)
            .Options;

        return new AppDbContext(options);
    }
}
