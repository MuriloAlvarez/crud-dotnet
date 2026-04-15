namespace crud_net.Features.Contacts;

public static class ContactMappingExtensions
{
    public static ContactResponse ToResponse(this Contact contact, DateOnly currentDate)
    {
        return new ContactResponse(
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

    public static ContactListItemResponse ToListItemResponse(this Contact contact, DateOnly currentDate)
    {
        return new ContactListItemResponse(
            contact.Id,
            contact.Name,
            contact.DateOfBirth,
            contact.GetAge(currentDate),
            contact.Gender);
    }
}