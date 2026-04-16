using crud_net.Features.Contacts.Domain.Errors;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Features.Contacts.Mappings;
using crud_net.Features.Contacts.Repositories;

namespace crud_net.Features.Contacts.UseCases;

public sealed class UpdateActiveContactUseCase
{
    private readonly IContactRepository _repository;
    private readonly IAppClock _clock;

    public UpdateActiveContactUseCase(IContactRepository repository, IAppClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<ContactResponseDto> ExecuteAsync(Guid id, UpdateActiveContactInputDto input, CancellationToken cancellationToken)
    {
        var contact = await _repository.GetActiveByIdAsync(id, cancellationToken)
            ?? throw new ContactNotFoundException(id);

        contact.Update(input.Name, input.DateOfBirth, input.Gender, _clock.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);

        return contact.ToResponseDto(_clock.Today);
    }
}