using crud_net.Features.Contacts.Domain.Entities;
using crud_net.Features.Contacts.Domain.Errors;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Features.Contacts.Repositories;
using crud_net.Features.Contacts.UseCases;
using Moq;

namespace crud_net.Tests.UseCases;

public sealed class ContactUseCasesTests
{
    private static readonly DateTime FixedUtcNow = new(2026, 4, 15, 12, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly FixedToday = DateOnly.FromDateTime(FixedUtcNow);

    [Fact]
    public async Task CreateContactUseCase_WhenInputIsValid_PersistsAndReturnsResponse()
    {
        var repository = new Mock<IContactRepository>(MockBehavior.Strict);
        var clock = new FakeClock(FixedUtcNow);
        Contact? addedContact = null;

        repository
            .Setup(current => current.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()))
            .Callback<Contact, CancellationToken>((contact, _) => addedContact = contact)
            .Returns(Task.CompletedTask);
        repository
            .Setup(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new CreateContactUseCase(repository.Object, clock);
        var input = new CreateContactInputDto("Maria", new DateOnly(1990, 1, 10), Gender.Female);

        var result = await useCase.ExecuteAsync(input, CancellationToken.None);

        Assert.NotNull(addedContact);
        Assert.Equal(addedContact!.Id, result.Id);
        Assert.Equal("Maria", result.Name);
        repository.Verify(current => current.AddAsync(It.IsAny<Contact>(), It.IsAny<CancellationToken>()), Times.Once);
        repository.Verify(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ListActiveContactsUseCase_WhenRepositoryReturnsContacts_MapsOutput()
    {
        var repository = new Mock<IContactRepository>(MockBehavior.Strict);
        var clock = new FakeClock(FixedUtcNow);
        var contact = Contact.Create("Ana", new DateOnly(1992, 4, 4), Gender.Female, FixedUtcNow);

        repository
            .Setup(current => current.ListActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync([contact]);

        var useCase = new ListActiveContactsUseCase(repository.Object, clock);

        var result = await useCase.ExecuteAsync(CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(contact.Id, result[0].Id);
        Assert.Equal(contact.GetAge(FixedToday), result[0].Age);
    }

    [Fact]
    public async Task GetActiveContactDetailsUseCase_WhenContactDoesNotExist_ThrowsNotFound()
    {
        var repository = new Mock<IContactRepository>(MockBehavior.Strict);
        var clock = new FakeClock(FixedUtcNow);
        var contactId = Guid.NewGuid();

        repository
            .Setup(current => current.GetActiveByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Contact?)null);

        var useCase = new GetActiveContactDetailsUseCase(repository.Object, clock);

        await Assert.ThrowsAsync<ContactNotFoundException>(() => useCase.ExecuteAsync(contactId, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateActiveContactUseCase_WhenContactIsActive_UpdatesAndSaves()
    {
        var repository = new Mock<IContactRepository>(MockBehavior.Strict);
        var clock = new FakeClock(FixedUtcNow);
        var contactId = Guid.NewGuid();
        var contact = Contact.Create("Bruno", new DateOnly(1990, 3, 3), Gender.Male, FixedUtcNow);

        repository
            .Setup(current => current.GetActiveByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        repository
            .Setup(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new UpdateActiveContactUseCase(repository.Object, clock);
        var input = new UpdateActiveContactInputDto("Bruno Atualizado", new DateOnly(1989, 9, 9), Gender.Other);

        var result = await useCase.ExecuteAsync(contactId, input, CancellationToken.None);

        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Gender, result.Gender);
        repository.Verify(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ActivateContactUseCase_WhenContactIsUnderage_ThrowsValidation()
    {
        var repository = new Mock<IContactRepository>(MockBehavior.Strict);
        var clock = new FakeClock(FixedUtcNow);
        var contactId = Guid.NewGuid();
        var underage = Contact.Create("Joao", FixedToday.AddYears(-17), Gender.Male, FixedUtcNow);

        repository
            .Setup(current => current.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(underage);

        var useCase = new ActivateContactUseCase(repository.Object, clock);

        var exception = await Assert.ThrowsAsync<ContactValidationException>(() => useCase.ExecuteAsync(contactId, CancellationToken.None));

        Assert.True(exception.Errors.ContainsKey(nameof(CreateContactInputDto.DateOfBirth)));
        repository.Verify(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ActivateContactUseCase_WhenContactIsInactiveAdult_ActivatesAndSaves()
    {
        var repository = new Mock<IContactRepository>(MockBehavior.Strict);
        var clock = new FakeClock(FixedUtcNow);
        var contactId = Guid.NewGuid();
        var contact = Contact.Create("Carlos", new DateOnly(1990, 1, 1), Gender.Male, FixedUtcNow);
        contact.Deactivate(FixedUtcNow);

        repository
            .Setup(current => current.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        repository
            .Setup(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new ActivateContactUseCase(repository.Object, clock);

        var result = await useCase.ExecuteAsync(contactId, CancellationToken.None);

        Assert.True(contact.IsActive);
        Assert.True(result.IsActive);
        repository.Verify(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeactivateContactUseCase_WhenContactExists_DeactivatesAndSaves()
    {
        var repository = new Mock<IContactRepository>(MockBehavior.Strict);
        var clock = new FakeClock(FixedUtcNow);
        var contactId = Guid.NewGuid();
        var contact = Contact.Create("Marina", new DateOnly(1991, 7, 7), Gender.Female, FixedUtcNow);

        repository
            .Setup(current => current.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        repository
            .Setup(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new DeactivateContactUseCase(repository.Object, clock);

        var result = await useCase.ExecuteAsync(contactId, CancellationToken.None);

        Assert.False(contact.IsActive);
        Assert.False(result.IsActive);
        repository.Verify(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteContactUseCase_WhenContactExists_MarksAsDeletedAndSaves()
    {
        var repository = new Mock<IContactRepository>(MockBehavior.Strict);
        var clock = new FakeClock(FixedUtcNow);
        var contactId = Guid.NewGuid();
        var contact = Contact.Create("Pedro", new DateOnly(1992, 8, 8), Gender.Male, FixedUtcNow);

        repository
            .Setup(current => current.GetByIdAsync(contactId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(contact);
        repository
            .Setup(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var useCase = new DeleteContactUseCase(repository.Object, clock);

        await useCase.ExecuteAsync(contactId, CancellationToken.None);

        Assert.True(contact.IsDeleted);
        Assert.False(contact.IsActive);
        repository.Verify(current => current.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private sealed class FakeClock : IAppClock
    {
        public FakeClock(DateTime utcNow)
        {
            UtcNow = utcNow;
            Today = DateOnly.FromDateTime(utcNow);
        }

        public DateTime UtcNow { get; }

        public DateOnly Today { get; }
    }
}
