using crud_net.Infrastructure.Persistence.Repositories;

namespace crud_net.Features.Contacts;

public static class ActivateContactEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:guid}/activate", ActivateAsync)
            .WithName("ActivateContact")
            .Produces<ContactResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> ActivateAsync(
        Guid id,
        IContactRepository repository,
        IAppClock clock,
        CancellationToken cancellationToken)
    {
        var contact = await repository.GetByIdAsync(id, cancellationToken);
        if (contact is null)
        {
            return Results.NotFound();
        }

        if (!ContactAgeCalculator.IsAdult(contact.DateOfBirth, clock.Today))
        {
            return Results.ValidationProblem(new Dictionary<string, string[]>
            {
                [nameof(Contact.DateOfBirth)] = ["O contato deverá ser maior de idade."]
            });
        }

        contact.Activate(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return Results.Ok(contact.ToResponse(clock.Today));
    }
}