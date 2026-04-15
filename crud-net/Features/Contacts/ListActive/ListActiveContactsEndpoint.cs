using crud_net.Infrastructure.Persistence.Repositories;

namespace crud_net.Features.Contacts;

public static class ListActiveContactsEndpoint
{
    public static void Map(RouteGroupBuilder group)
    {
        group.MapGet("", ListAsync)
            .WithName("ListActiveContacts")
            .Produces<List<ContactListItemResponse>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListAsync(
        IContactRepository repository,
        IAppClock clock,
        CancellationToken cancellationToken)
    {
        var contacts = await repository.ListActiveAsync(cancellationToken);
        var response = contacts
            .Select(contact => contact.ToListItemResponse(clock.Today))
            .ToList();

        return Results.Ok(response);
    }
}