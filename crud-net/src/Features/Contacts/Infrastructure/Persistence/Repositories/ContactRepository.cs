using crud_net.Features.Contacts.Domain.Entities;
using crud_net.Features.Contacts.Repositories;
using Microsoft.EntityFrameworkCore;

namespace crud_net.Features.Contacts.Infrastructure.Persistence.Repositories;

public sealed class ContactRepository : IContactRepository
{
    private readonly AppDbContext _dbContext;

    public ContactRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(Contact contact, CancellationToken cancellationToken)
    {
        await _dbContext.Contacts.AddAsync(contact, cancellationToken);
    }

    public Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Contacts.FirstOrDefaultAsync(contact => contact.Id == id, cancellationToken);
    }

    public Task<Contact?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return _dbContext.Contacts.FirstOrDefaultAsync(contact => contact.Id == id && contact.IsActive, cancellationToken);
    }

    public Task<List<Contact>> ListActiveAsync(CancellationToken cancellationToken)
    {
        return _dbContext.Contacts
            .Where(contact => contact.IsActive)
            .OrderBy(contact => contact.Name)
            .ToListAsync(cancellationToken);
    }

    public Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}