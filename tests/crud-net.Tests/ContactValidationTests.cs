using crud_net.Features.Contacts;

namespace crud_net.Tests;

public sealed class ContactValidationTests
{
	[Fact]
	public void ValidateCreate_WhenDateOfBirthIsInFuture_ReturnsError()
	{
		var request = new CreateContactRequest("Maria", new DateOnly(2026, 4, 16), Gender.Female);

		var errors = ContactValidation.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Contains(nameof(CreateContactRequest.DateOfBirth), errors.Keys);
		Assert.Contains(errors[nameof(CreateContactRequest.DateOfBirth)], message => message.Contains("maior que a data de hoje", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void ValidateCreate_WhenContactIsUnderage_ReturnsError()
	{
		var request = new CreateContactRequest("Maria", new DateOnly(2010, 4, 15), Gender.Female);

		var errors = ContactValidation.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Contains(errors[nameof(CreateContactRequest.DateOfBirth)], message => message.Contains("maior de idade", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void ValidateCreate_WhenGenderIsNotSpecified_ReturnsError()
	{
		var request = new CreateContactRequest("Maria", new DateOnly(1990, 4, 15), Gender.NotSpecified);

		var errors = ContactValidation.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Contains(nameof(CreateContactRequest.Gender), errors.Keys);
	}

	[Fact]
	public void ValidateCreate_WhenRequestIsValid_ReturnsNoErrors()
	{
		var request = new CreateContactRequest("Maria Silva", new DateOnly(1990, 4, 15), Gender.Female);

		var errors = ContactValidation.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Empty(errors);
	}

	[Fact]
	public void ValidateCreate_WhenNameExceedsMaxLength_ReturnsError()
	{
		var longName = new string('A', ContactRules.MaxNameLength + 1);
		var request = new CreateContactRequest(longName, new DateOnly(1990, 4, 15), Gender.Female);

		var errors = ContactValidation.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Contains(nameof(CreateContactRequest.Name), errors.Keys);
		Assert.Contains(errors[nameof(CreateContactRequest.Name)], message => message.Contains("no máximo", StringComparison.OrdinalIgnoreCase));
	}
}