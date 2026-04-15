using crud_net.Infrastructure.Persistence.Repositories;

namespace crud_net.Features.Contacts;

public static class DeleteContactEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapDelete("/{id:guid}", DeleteAsync)
            .WithName("DeleteContact")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> DeleteAsync(
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

        contact.Delete(clock.UtcNow);
        await repository.SaveChangesAsync(cancellationToken);

        return Results.NoContent();
    }
}