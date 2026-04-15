namespace crud_net.Features.Contacts;

public sealed record CreateContactRequest(string Name, DateOnly DateOfBirth, Gender Gender);

public sealed record UpdateContactRequest(string Name, DateOnly DateOfBirth, Gender Gender);

public sealed record ContactResponse(
    Guid Id,
    string Name,
    DateOnly DateOfBirth,
    int Age,
    Gender Gender,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? DeactivatedAtUtc);

public sealed record ContactListItemResponse(
    Guid Id,
    string Name,
    DateOnly DateOfBirth,
    int Age,
    Gender Gender);