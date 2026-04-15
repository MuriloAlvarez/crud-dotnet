namespace crud_net.Features.Contacts;

public static class ContactsModule
{
    public static IEndpointRouteBuilder MapContactsEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/contacts")
            .WithTags("Contacts");

        CreateContactEndpoint.Map(group);
        ListActiveContactsEndpoint.Map(group);
        GetActiveContactByIdEndpoint.Map(group);
        UpdateActiveContactEndpoint.Map(group);
        ActivateContactEndpoint.Map(group);
        DeactivateContactEndpoint.Map(group);
        DeleteContactEndpoint.Map(group);

        return app;
    }
}