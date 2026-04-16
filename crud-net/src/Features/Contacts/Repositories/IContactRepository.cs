using crud_net.Features.Contacts.Domain.Entities;

namespace crud_net.Features.Contacts.Repositories;

public interface IContactRepository
{
    Task AddAsync(Contact contact, CancellationToken cancellationToken);

    Task<Contact?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Contact?> GetActiveByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<Contact>> ListActiveAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);
}