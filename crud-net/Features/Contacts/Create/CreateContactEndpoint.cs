using crud_net.Infrastructure.Persistence.Repositories;

namespace crud_net.Features.Contacts;

public static class CreateContactEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPost("", CreateAsync)
            .WithName("CreateContact")
            .Produces<ContactResponse>(StatusCodes.Status201Created)
            .ProducesValidationProblem();
    }

    private static async Task<IResult> CreateAsync(
        CreateContactRequest request,
        IContactRepository repository,
        IAppClock clock,
        CancellationToken cancellationToken)
    {
        var errors = ContactValidation.ValidateCreate(request, clock.Today);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var contact = Contact.Create(request.Name, request.DateOfBirth, request.Gender, clock.UtcNow);
        await repository.AddAsync(contact, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return Results.Created($"/api/contacts/{contact.Id}", contact.ToResponse(clock.Today));
    }
}