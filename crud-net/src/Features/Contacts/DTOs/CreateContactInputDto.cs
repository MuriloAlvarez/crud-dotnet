using crud_net.Features.Contacts.Domain.Services;

namespace crud_net.Features.Contacts.DTOs;

public sealed record CreateContactInputDto(string Name, DateOnly DateOfBirth, Gender Gender);