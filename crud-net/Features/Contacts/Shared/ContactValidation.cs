namespace crud_net.Features.Contacts;

public static class ContactValidation
{
    public static Dictionary<string, string[]> ValidateCreate(CreateContactRequest request, DateOnly currentDate)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        ValidateName(request.Name, errors);
        ValidateDateOfBirth(request.DateOfBirth, currentDate, errors);
        ValidateGender(request.Gender, errors);
        ValidateAdult(request.DateOfBirth, currentDate, errors);

        return ToDictionary(errors);
    }

    public static Dictionary<string, string[]> ValidateUpdate(UpdateContactRequest request, DateOnly currentDate)
    {
        var errors = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        ValidateName(request.Name, errors);
        ValidateDateOfBirth(request.DateOfBirth, currentDate, errors);
        ValidateGender(request.Gender, errors);
        ValidateAdult(request.DateOfBirth, currentDate, errors);

        return ToDictionary(errors);
    }

    private static void ValidateName(string name, Dictionary<string, List<string>> errors)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            AddError(errors, nameof(CreateContactRequest.Name), "Nome do contato é obrigatório.");
            return;
        }

        if (name.Trim().Length < 3)
        {
            AddError(errors, nameof(CreateContactRequest.Name), "Nome do contato deve ter ao menos 3 caracteres.");
        }
    }

    private static void ValidateDateOfBirth(DateOnly dateOfBirth, DateOnly currentDate, Dictionary<string, List<string>> errors)
    {
        if (dateOfBirth > currentDate)
        {
            AddError(errors, nameof(CreateContactRequest.DateOfBirth), "Data de nascimento não pode ser maior que a data de hoje.");
            return;
        }

        var age = ContactAgeCalculator.CalculateAge(dateOfBirth, currentDate);
        if (age <= 0)
        {
            AddError(errors, nameof(CreateContactRequest.DateOfBirth), "A idade não pode ser igual a 0.");
        }
    }

    private static void ValidateAdult(DateOnly dateOfBirth, DateOnly currentDate, Dictionary<string, List<string>> errors)
    {
        if (!ContactAgeCalculator.IsAdult(dateOfBirth, currentDate))
        {
            AddError(errors, nameof(CreateContactRequest.DateOfBirth), "O contato deverá ser maior de idade.");
        }
    }

    private static void ValidateGender(Gender gender, Dictionary<string, List<string>> errors)
    {
        if (gender == Gender.NotSpecified || !Enum.IsDefined(typeof(Gender), gender))
        {
            AddError(errors, nameof(CreateContactRequest.Gender), "Sexo informado é inválido.");
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