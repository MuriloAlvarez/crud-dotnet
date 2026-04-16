using crud_net.Features.Contacts.Domain.Entities;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Features.Contacts.Mappings;
using crud_net.Features.Contacts.Repositories;

namespace crud_net.Features.Contacts.UseCases;

public sealed class CreateContactUseCase
{
    private readonly IContactRepository _repository;
    private readonly IAppClock _clock;

    public CreateContactUseCase(IContactRepository repository, IAppClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<ContactResponseDto> ExecuteAsync(CreateContactInputDto input, CancellationToken cancellationToken)
    {
        var contact = Contact.Create(input.Name, input.DateOfBirth, input.Gender, _clock.UtcNow);

        await _repository.AddAsync(contact, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return contact.ToResponseDto(_clock.Today);
    }
}