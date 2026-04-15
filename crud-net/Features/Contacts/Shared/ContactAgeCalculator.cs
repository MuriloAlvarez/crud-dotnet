namespace crud_net.Features.Contacts;

public static class ContactAgeCalculator
{
    public static int CalculateAge(DateOnly dateOfBirth, DateOnly currentDate)
    {
        if (dateOfBirth > currentDate)
        {
            return 0;
        }

        var age = currentDate.Year - dateOfBirth.Year;
        if (dateOfBirth.AddYears(age) > currentDate)
        {
            age--;
        }

        return Math.Max(age, 0);
    }

    public static bool IsAdult(DateOnly dateOfBirth, DateOnly currentDate) => CalculateAge(dateOfBirth, currentDate) >= ContactRules.AdultAge;
}