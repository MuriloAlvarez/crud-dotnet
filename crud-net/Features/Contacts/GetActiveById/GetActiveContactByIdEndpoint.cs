using crud_net.Infrastructure.Persistence.Repositories;

namespace crud_net.Features.Contacts;

public static class GetActiveContactByIdEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("/{id:guid}", GetByIdAsync)
            .WithName("GetActiveContactById")
            .Produces<ContactResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetByIdAsync(
        Guid id,
        IContactRepository repository,
        IAppClock clock,
        CancellationToken cancellationToken)
    {
        var contact = await repository.GetActiveByIdAsync(id, cancellationToken);
        if (contact is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(contact.ToResponse(clock.Today));
    }
}