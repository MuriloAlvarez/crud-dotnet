using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Features.Contacts.Mappings;
using crud_net.Features.Contacts.Repositories;

namespace crud_net.Features.Contacts.UseCases;

public sealed class ListActiveContactsUseCase
{
    private readonly IContactRepository _repository;
    private readonly IAppClock _clock;

    public ListActiveContactsUseCase(IContactRepository repository, IAppClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<List<ContactListItemResponseDto>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var contacts = await _repository.ListActiveAsync(cancellationToken);

        return contacts
            .Select(contact => contact.ToListItemResponseDto(_clock.Today))
            .ToList();
    }
}