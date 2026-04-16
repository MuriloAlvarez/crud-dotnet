using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;

namespace crud_net.Features.Contacts.Validations;

public sealed class ContactInputValidator : IContactInputValidator
{
    public Dictionary<string, string[]> ValidateCreate(CreateContactInputDto input, DateOnly currentDate)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        ValidateName(input.Name, errors);
        ValidateDateOfBirth(input.DateOfBirth, currentDate, errors);
        ValidateGender(input.Gender, errors);
        ValidateAdult(input.DateOfBirth, currentDate, errors);

        return ToDictionary(errors);
    }

    public Dictionary<string, string[]> ValidateUpdate(UpdateActiveContactInputDto input, DateOnly currentDate)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        ValidateName(input.Name, errors);
        ValidateDateOfBirth(input.DateOfBirth, currentDate, errors);
        ValidateGender(input.Gender, errors);
        ValidateAdult(input.DateOfBirth, currentDate, errors);

        return ToDictionary(errors);
    }

    private static void ValidateName(string name, Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddError(errors, nameof(CreateContactInputDto.Name), "Contact name is required.");
            return;
        }

        var trimmedName = name.Trim();
        if (trimmedName.Length < ContactRules.MinNameLength)
        {
            AddError(errors, nameof(CreateContactInputDto.Name), $"Contact name must have at least {ContactRules.MinNameLength} characters.");
        }

        if (trimmedName.Length > ContactRules.MaxNameLength)
        {
            AddError(errors, nameof(CreateContactInputDto.Name), $"Contact name must have at most {ContactRules.MaxNameLength} characters.");
        }
    }

    private static void ValidateDateOfBirth(DateOnly dateOfBirth, DateOnly currentDate, Dictionary<string, List<string>> errors)
    {
        if (dateOfBirth > currentDate)
        {
            AddError(errors, nameof(CreateContactInputDto.DateOfBirth), "Date of birth cannot be in the future.");
            return;
        }

        var age = ContactAgeCalculator.CalculateAge(dateOfBirth, currentDate);
        if (age <= 0)
        {
            AddError(errors, nameof(CreateContactInputDto.DateOfBirth), "Contact age cannot be zero.");
        }
    }

    private static void ValidateAdult(DateOnly dateOfBirth, DateOnly currentDate, Dictionary<string, List<string>> errors)
    {
        if (!ContactAgeCalculator.IsAdult(dateOfBirth, currentDate))
        {
            AddError(errors, nameof(CreateContactInputDto.DateOfBirth), "Contact must be an adult.");
        }
    }

    private static void ValidateGender(Gender gender, Dictionary<string, List<string>> errors)
    {
        if (gender == Gender.NotSpecified || !Enum.IsDefined(typeof(Gender), gender))
        {
            AddError(errors, nameof(CreateContactInputDto.Gender), "Gender value is invalid.");
        }
    }

    private static void AddError(Dictionary<string, List<string>> errors, string key, string message)
    {
        if (!errors.TryGetValue(key, out var messages))
        {
            messages = [];
            errors[key] = messages;
        }

        messages.Add(message);
    }

    private static Dictionary<string, string[]> ToDictionary(Dictionary<string, List<string>> errors)
    {
        return errors.ToDictionary(pair => pair.Key, pair => pair.Value.ToArray(), StringComparer.OrdinalIgnoreCase);
    }
}