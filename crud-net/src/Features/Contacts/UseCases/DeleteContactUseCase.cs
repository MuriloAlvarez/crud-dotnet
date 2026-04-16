using crud_net.Features.Contacts.Domain.Errors;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.Repositories;

namespace crud_net.Features.Contacts.UseCases;

public sealed class DeleteContactUseCase
{
    private readonly IContactRepository _repository;
    private readonly IAppClock _clock;

    public DeleteContactUseCase(IContactRepository repository, IAppClock clock)
    {
        _repository = repository;
        _clock = clock;
    }

    public async Task ExecuteAsync(Guid id, CancellationToken cancellationToken)
    {
        var contact = await _repository.GetByIdAsync(id, cancellationToken)
            ?? throw new ContactNotFoundException(id);

        contact.Delete(_clock.UtcNow);
        await _repository.SaveChangesAsync(cancellationToken);
    }
}