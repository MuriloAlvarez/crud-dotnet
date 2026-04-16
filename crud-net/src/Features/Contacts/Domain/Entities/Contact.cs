using crud_net.Features.Contacts.Domain.Services;

namespace crud_net.Features.Contacts.Domain.Entities;

public sealed class Contact
{
    private Contact()
    {
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateOnly DateOfBirth { get; private set; }

    public Gender Gender { get; private set; }

    public bool IsActive { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime? UpdatedAtUtc { get; private set; }

    public DateTime? DeactivatedAtUtc { get; private set; }

    public DateTime? DeletedAtUtc { get; private set; }

    public static Contact Create(string name, DateOnly dateOfBirth, Gender gender, DateTime utcNow)
    {
        return new Contact
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            DateOfBirth = dateOfBirth,
            Gender = gender,
            IsActive = true,
            IsDeleted = false,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };
    }

    public void Update(string name, DateOnly dateOfBirth, Gender gender, DateTime utcNow)
    {
        EnsureNotDeleted();
        Name = name.Trim();
        DateOfBirth = dateOfBirth;
        Gender = gender;
        UpdatedAtUtc = utcNow;
    }

    public void Activate(DateTime utcNow)
    {
        EnsureNotDeleted();
        IsActive = true;
        DeactivatedAtUtc = null;
        UpdatedAtUtc = utcNow;
    }

    public void Deactivate(DateTime utcNow)
    {
        EnsureNotDeleted();
        IsActive = false;
        DeactivatedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public void Delete(DateTime utcNow)
    {
        if (IsDeleted)
        {
            return;
        }

        IsActive = false;
        IsDeleted = true;
        DeletedAtUtc = utcNow;
        UpdatedAtUtc = utcNow;
    }

    public int GetAge(DateOnly currentDate) => ContactAgeCalculator.CalculateAge(DateOfBirth, currentDate);

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
        {
            throw new InvalidOperationException("Contact has been deleted.");
        }
    }
}