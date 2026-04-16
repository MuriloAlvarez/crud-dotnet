using crud_net.Features.Contacts.Domain.Errors;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Features.Contacts.UseCases;
using crud_net.Features.Contacts.Validations;
using crud_net.Shared.Errors;
using Microsoft.AspNetCore.Mvc;

namespace crud_net.Features.Contacts.Controllers;

[ApiController]
[Route("api/contacts")]
public sealed class ContactController : ControllerBase
{
    private readonly IContactInputValidator _inputValidator;
    private readonly IAppClock _clock;
    private readonly CreateContactUseCase _createContactUseCase;
    private readonly ListActiveContactsUseCase _listActiveContactsUseCase;
    private readonly GetActiveContactDetailsUseCase _getActiveContactDetailsUseCase;
    private readonly UpdateActiveContactUseCase _updateActiveContactUseCase;
    private readonly ActivateContactUseCase _activateContactUseCase;
    private readonly DeactivateContactUseCase _deactivateContactUseCase;
    private readonly DeleteContactUseCase _deleteContactUseCase;

    public ContactController(
        IContactInputValidator inputValidator,
        IAppClock clock,
        CreateContactUseCase createContactUseCase,
        ListActiveContactsUseCase listActiveContactsUseCase,
        GetActiveContactDetailsUseCase getActiveContactDetailsUseCase,
        UpdateActiveContactUseCase updateActiveContactUseCase,
        ActivateContactUseCase activateContactUseCase,
        DeactivateContactUseCase deactivateContactUseCase,
        DeleteContactUseCase deleteContactUseCase)
    {
        _inputValidator = inputValidator;
        _clock = clock;
        _createContactUseCase = createContactUseCase;
        _listActiveContactsUseCase = listActiveContactsUseCase;
        _getActiveContactDetailsUseCase = getActiveContactDetailsUseCase;
        _updateActiveContactUseCase = updateActiveContactUseCase;
        _activateContactUseCase = activateContactUseCase;
        _deactivateContactUseCase = deactivateContactUseCase;
        _deleteContactUseCase = deleteContactUseCase;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ContactResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ContactResponseDto>> CreateAsync(
        [FromBody] CreateContactInputDto input,
        CancellationToken cancellationToken)
    {
        var errors = _inputValidator.ValidateCreate(input, _clock.Today);
        if (errors.Count > 0)
        {
            throw new ContactValidationException(errors);
        }

        var created = await _createContactUseCase.ExecuteAsync(input, cancellationToken);
        return Created($"/api/contacts/{created.Id}", created);
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<ContactListItemResponseDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<ContactListItemResponseDto>>> ListActiveAsync(CancellationToken cancellationToken)
    {
        var contacts = await _listActiveContactsUseCase.ExecuteAsync(cancellationToken);
        return Ok(contacts);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ContactResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponseDto>> GetActiveByIdAsync(string id, CancellationToken cancellationToken)
    {
        var contactId = ParseIdOrThrow(id);
        var contact = await _getActiveContactDetailsUseCase.ExecuteAsync(contactId, cancellationToken);
        return Ok(contact);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ContactResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponseDto>> UpdateActiveAsync(
        string id,
        [FromBody] UpdateActiveContactInputDto input,
        CancellationToken cancellationToken)
    {
        var contactId = ParseIdOrThrow(id);

        var errors = _inputValidator.ValidateUpdate(input, _clock.Today);
        if (errors.Count > 0)
        {
            throw new ContactValidationException(errors);
        }

        var updated = await _updateActiveContactUseCase.ExecuteAsync(contactId, input, cancellationToken);
        return Ok(updated);
    }

    [HttpPatch("{id}/activate")]
    [ProducesResponseType(typeof(ContactResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponseDto>> ActivateAsync(string id, CancellationToken cancellationToken)
    {
        var contactId = ParseIdOrThrow(id);
        var activated = await _activateContactUseCase.ExecuteAsync(contactId, cancellationToken);
        return Ok(activated);
    }

    [HttpPatch("{id}/deactivate")]
    [ProducesResponseType(typeof(ContactResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ContactResponseDto>> DeactivateAsync(string id, CancellationToken cancellationToken)
    {
        var contactId = ParseIdOrThrow(id);
        var deactivated = await _deactivateContactUseCase.ExecuteAsync(contactId, cancellationToken);
        return Ok(deactivated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiErrorResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteAsync(string id, CancellationToken cancellationToken)
    {
        var contactId = ParseIdOrThrow(id);
        await _deleteContactUseCase.ExecuteAsync(contactId, cancellationToken);
        return NoContent();
    }

    private static Guid ParseIdOrThrow(string id)
    {
        if (!Guid.TryParse(id, out var parsedId))
        {
            throw new InvalidIdException(id);
        }

        return parsedId;
    }
}