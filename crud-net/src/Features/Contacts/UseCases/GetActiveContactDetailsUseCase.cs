using crud_net.Features.Contacts.Domain.Errors;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Features.Contacts.Mappings;
using crud_net.Features.Contacts.Repositories;

namespace crud_net.Features.Contacts.UseCases;

public sealed class GetActiveContactDetailsUseCase
{
    private readonly IContactRepository _repository;
    private readonly IAppClock _clock;

    public GetActiveContactDetailsUseCase(IContactRepository repository, IAppClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<ContactResponseDto> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await _repository.GetActiveByIdAsync(id, cancellationToken)
            ?? throw new ContactNotFoundException(id);

        return contact.ToResponseDto(_clock.Today);
    }
}