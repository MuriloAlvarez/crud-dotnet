using crud_net.Features.Contacts.Domain.Errors;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Features.Contacts.Mappings;
using crud_net.Features.Contacts.Repositories;

namespace crud_net.Features.Contacts.UseCases;

public sealed class ActivateContactUseCase
{
    private readonly IContactRepository _repository;
    private readonly IAppClock _clock;

    public ActivateContactUseCase(IContactRepository repository, IAppClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task<ContactResponseDto> ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ContactNotFoundException(id);

        if (!ContactAgeCalculator.IsAdult(contact.DateOfBirth, _clock.Today))
        {
            throw new ContactValidationException(new Dictionary<string, string[]>
            {
                [nameof(CreateContactInputDto.DateOfBirth)] = ["Contact must be an adult."]
            });
        }

        contact.Activate(_clock.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);

        return contact.ToResponseDto(_clock.Today);
    }
}