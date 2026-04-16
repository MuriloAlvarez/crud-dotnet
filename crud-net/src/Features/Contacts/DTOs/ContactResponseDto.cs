using crud_net.Features.Contacts.Domain.Services;

namespace crud_net.Features.Contacts.DTOs;

public sealed record ContactResponseDto(
    Guid Id,
    string Name,
    DateOnly DateOfBirth,
    int Age,
    Gender Gender,
    bool IsActive,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    DateTime? DeactivatedAtUtc);