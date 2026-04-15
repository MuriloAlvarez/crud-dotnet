using crud_net.Infrastructure.Persistence.Repositories;

namespace crud_net.Features.Contacts;

public static class UpdateActiveContactEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPut("/{id:guid}", UpdateAsync)
            .WithName("UpdateActiveContact")
            .Produces<ContactResponse>(StatusCodes.Status200OK)
            .ProducesValidationProblem()
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> UpdateAsync(
        Guid id,
        UpdateContactRequest request,
        IContactRepository repository,
        IAppClock clock,
        CancellationToken cancellationToken)
    {
        var errors = ContactValidation.ValidateUpdate(request, clock.Today);
        if (errors.Count > 0)
        {
            return Results.ValidationProblem(errors);
        }

        var contact = await repository.GetActiveByIdAsync(id, cancellationToken);
        if (contact is null)
        {
            return Results.NotFound();
        }

        contact.Update(request.Name, request.DateOfBirth, request.Gender, clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return Results.Ok(contact.ToResponse(clock.Today));
    }
}