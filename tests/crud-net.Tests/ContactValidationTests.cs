using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Features.Contacts.Validations;

namespace crud_net.Tests;

public sealed class ContactValidationTests
{
	private readonly ContactInputValidator _validator = new();

	[Fact]
	public void ValidateCreate_WhenDateOfBirthIsInFuture_ReturnsError()
	{
		var request = new CreateContactInputDto("Maria", new DateOnly(2026, 4, 16), Gender.Female);

		var errors = _validator.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Contains(nameof(CreateContactInputDto.DateOfBirth), errors.Keys);
		Assert.Contains(errors[nameof(CreateContactInputDto.DateOfBirth)], message => message.Contains("future", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void ValidateCreate_WhenContactIsUnderage_ReturnsError()
	{
		var request = new CreateContactInputDto("Maria", new DateOnly(2010, 4, 15), Gender.Female);

		var errors = _validator.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Contains(errors[nameof(CreateContactInputDto.DateOfBirth)], message => message.Contains("adult", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void ValidateCreate_WhenGenderIsNotSpecified_ReturnsError()
	{
		var request = new CreateContactInputDto("Maria", new DateOnly(1990, 4, 15), Gender.NotSpecified);

		var errors = _validator.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Contains(nameof(CreateContactInputDto.Gender), errors.Keys);
	}

	[Fact]
	public void ValidateCreate_WhenRequestIsValid_ReturnsNoErrors()
	{
		var request = new CreateContactInputDto("Maria Silva", new DateOnly(1990, 4, 15), Gender.Female);

		var errors = _validator.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Empty(errors);
	}

	[Fact]
	public void ValidateCreate_WhenNameExceedsMaxLength_ReturnsError()
	{
		var longName = new string('A', ContactRules.MaxNameLength + 1);
		var request = new CreateContactInputDto(longName, new DateOnly(1990, 4, 15), Gender.Female);

		var errors = _validator.ValidateCreate(request, new DateOnly(2026, 4, 15));

		Assert.Contains(nameof(CreateContactInputDto.Name), errors.Keys);
		Assert.Contains(errors[nameof(CreateContactInputDto.Name)], message => message.Contains("at most", StringComparison.OrdinalIgnoreCase));
	}
}