using crud_net.Features.Contacts.Domain.Entities;
using crud_net.Features.Contacts.Domain.Services;

namespace crud_net.Tests.Domain;

public sealed class ContactEntityTests
{
    [Fact]
    public void Create_WhenValidInput_SetsInitialState()
    {
        var now = new DateTime(2026, 4, 16, 12, 0, 0, DateTimeKind.Utc);

        var contact = Contact.Create("  Maria Silva  ", new DateOnly(1990, 1, 10), Gender.Female, now);

        Assert.NotEqual(Guid.Empty, contact.Id);
        Assert.Equal("Maria Silva", contact.Name);
        Assert.True(contact.IsActive);
        Assert.False(contact.IsDeleted);
        Assert.Equal(now, contact.CreatedAtUtc);
        Assert.Equal(now, contact.UpdatedAtUtc);
    }

    [Fact]
    public void Update_WhenNotDeleted_UpdatesFields()
    {
        var now = new DateTime(2026, 4, 16, 12, 0, 0, DateTimeKind.Utc);
        var contact = Contact.Create("Maria", new DateOnly(1990, 1, 10), Gender.Female, now);
        var updateTime = now.AddHours(1);

        contact.Update("  Maria Atualizada ", new DateOnly(1989, 5, 5), Gender.Other, updateTime);

        Assert.Equal("Maria Atualizada", contact.Name);
        Assert.Equal(new DateOnly(1989, 5, 5), contact.DateOfBirth);
        Assert.Equal(Gender.Other, contact.Gender);
        Assert.Equal(updateTime, contact.UpdatedAtUtc);
    }

    [Fact]
    public void DeactivateAndActivate_WhenNotDeleted_TogglesActiveFlag()
    {
        var now = new DateTime(2026, 4, 16, 12, 0, 0, DateTimeKind.Utc);
        var contact = Contact.Create("Maria", new DateOnly(1990, 1, 10), Gender.Female, now);

        var deactivateTime = now.AddHours(1);
        contact.Deactivate(deactivateTime);

        Assert.False(contact.IsActive);
        Assert.Equal(deactivateTime, contact.DeactivatedAtUtc);

        var activateTime = now.AddHours(2);
        contact.Activate(activateTime);

        Assert.True(contact.IsActive);
        Assert.Null(contact.DeactivatedAtUtc);
    }

    [Fact]
    public void Delete_WhenCalled_MarksContactAsDeleted()
    {
        var now = new DateTime(2026, 4, 16, 12, 0, 0, DateTimeKind.Utc);
        var contact = Contact.Create("Maria", new DateOnly(1990, 1, 10), Gender.Female, now);

        var deleteTime = now.AddHours(1);
        contact.Delete(deleteTime);

        Assert.False(contact.IsActive);
        Assert.True(contact.IsDeleted);
        Assert.Equal(deleteTime, contact.DeletedAtUtc);
    }

    [Fact]
    public void Update_WhenContactWasDeleted_ThrowsInvalidOperationException()
    {
        var now = new DateTime(2026, 4, 16, 12, 0, 0, DateTimeKind.Utc);
        var contact = Contact.Create("Maria", new DateOnly(1990, 1, 10), Gender.Female, now);
        contact.Delete(now.AddHours(1));

        var action = () => contact.Update("Maria 2", new DateOnly(1991, 1, 1), Gender.Female, now.AddHours(2));

        Assert.Throws<InvalidOperationException>(action);
    }
}
