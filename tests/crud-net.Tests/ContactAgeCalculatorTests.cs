using crud_net.Features.Contacts;

namespace crud_net.Tests;

public sealed class ContactAgeCalculatorTests
{
	[Fact]
	public void CalculateAge_WhenBirthdayIsToday_ReturnsZero()
	{
		var today = new DateOnly(2026, 4, 15);
		var age = ContactAgeCalculator.CalculateAge(today, today);

		Assert.Equal(0, age);
	}

	[Fact]
	public void CalculateAge_WhenBirthdayHasPassed_ReturnsExpectedAge()
	{
		var currentDate = new DateOnly(2026, 4, 15);
		var dateOfBirth = new DateOnly(2000, 4, 15);

		var age = ContactAgeCalculator.CalculateAge(dateOfBirth, currentDate);

		Assert.Equal(26, age);
	}

	[Fact]
	public void IsAdult_WhenContactIsEighteen_ReturnsTrue()
	{
		var currentDate = new DateOnly(2026, 4, 15);
		var dateOfBirth = new DateOnly(2008, 4, 15);

		var isAdult = ContactAgeCalculator.IsAdult(dateOfBirth, currentDate);

		Assert.True(isAdult);
	}
}