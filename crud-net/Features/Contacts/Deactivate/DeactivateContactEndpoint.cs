using crud_net.Infrastructure.Persistence.Repositories;

namespace crud_net.Features.Contacts;

public static class DeactivateContactEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapPatch("/{id:guid}/deactivate", DeactivateAsync)
            .WithName("DeactivateContact")
            .Produces<ContactResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> DeactivateAsync(
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

        contact.Deactivate(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return Results.Ok(contact.ToResponse(clock.Today));
    }
}