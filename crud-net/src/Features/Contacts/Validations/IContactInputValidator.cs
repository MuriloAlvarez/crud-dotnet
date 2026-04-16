using crud_net.Features.Contacts.DTOs;

namespace crud_net.Features.Contacts.Validations;

public interface IContactInputValidator
{
    Dictionary<string, string[]> ValidateCreate(CreateContactInputDto input, DateOnly currentDate);

    Dictionary<string, string[]> ValidateUpdate(UpdateActiveContactInputDto input, DateOnly currentDate);
}