using crud_net.Features.Contacts.Domain.Entities;
using crud_net.Features.Contacts.DTOs;

namespace crud_net.Features.Contacts.Mappings;

public static class ContactMappingExtensions
{
    public static ContactResponseDto ToResponseDto(this Contact contact, DateOnly currentDate)
    {
        return new ContactResponseDto(
            contact.Id,
            contact.Name,
            contact.DateOfBirth,
            contact.GetAge(currentDate),
            contact.Gender,
            contact.IsActive,
            contact.CreatedAtUtc,
            contact.UpdatedAtUtc,
            contact.DeactivatedAtUtc);
    }

    public static ContactListItemResponseDto ToListItemResponseDto(this Contact contact, DateOnly currentDate)
    {
        return new ContactListItemResponseDto(
            contact.Id,
            contact.Name,
            contact.DateOfBirth,
            contact.GetAge(currentDate),
            contact.Gender);
    }
}