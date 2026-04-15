using System.Net;
using System.Net.Http.Json;
using crud_net.Features.Contacts;
using crud_net.Infrastructure.Persistence;
using crud_net.Tests.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace crud_net.Tests.Integration;

public sealed class ContactsEndpointsIntegrationTests
{
    [Fact]
    public async Task PostContacts_WhenPayloadIsValid_ReturnsCreatedWithCalculatedAge()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var request = new CreateContactRequest("Maria Silva", new DateOnly(1990, 1, 10), Gender.Female);

        var response = await client.PostAsJsonAsync("/api/contacts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ContactResponse>();
        Assert.NotNull(body);

        var expectedAge = ContactAgeCalculator.CalculateAge(request.DateOfBirth, TestWebApplicationFactory.FixedToday);
        Assert.Equal(expectedAge, body!.Age);
        Assert.Equal(request.Name, body.Name);
        Assert.Equal(request.DateOfBirth, body.DateOfBirth);
        Assert.True(body.IsActive);
    }

    [Fact]
    public async Task PostContacts_WhenContactIsUnderage_ReturnsBadRequestValidationProblem()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var request = new CreateContactRequest("Joao Junior", TestWebApplicationFactory.FixedToday.AddYears(-17), Gender.Male);

        var response = await client.PostAsJsonAsync("/api/contacts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey(nameof(CreateContactRequest.DateOfBirth)));
    }

    [Fact]
    public async Task PatchDeactivateThenList_DoesNotReturnDeactivatedContact()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var deactivated = await CreateContactAsync(client, "Contato Desativado", new DateOnly(1991, 3, 20), Gender.Male);
        var active = await CreateContactAsync(client, "Contato Ativo", new DateOnly(1992, 8, 11), Gender.Female);

        using var patchRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{deactivated.Id}/deactivate");
        var patchResponse = await client.SendAsync(patchRequest);

        Assert.Equal(HttpStatusCode.OK, patchResponse.StatusCode);

        var listResponse = await client.GetAsync("/api/contacts");
        Assert.Equal(HttpStatusCode.OK, listResponse.StatusCode);

        var contacts = await listResponse.Content.ReadFromJsonAsync<List<ContactListItemResponse>>();
        Assert.NotNull(contacts);

        Assert.DoesNotContain(contacts!, contact => contact.Id == deactivated.Id);
        Assert.Contains(contacts!, contact => contact.Id == active.Id);
    }

    [Fact]
    public async Task DeleteThenGetById_ReturnsNotFound()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Para Excluir", new DateOnly(1989, 6, 30), Gender.Other);

        var deleteResponse = await client.DeleteAsync($"/api/contacts/{created.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/contacts/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task GetById_WhenContactIsDeactivated_ReturnsNotFound()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Inativo", new DateOnly(1990, 2, 2), Gender.Male);

        using var deactivateRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{created.Id}/deactivate");
        var deactivateResponse = await client.SendAsync(deactivateRequest);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/contacts/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Put_WhenContactIsDeactivated_ReturnsNotFound()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Inativo", new DateOnly(1990, 2, 2), Gender.Female);

        using var deactivateRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{created.Id}/deactivate");
        var deactivateResponse = await client.SendAsync(deactivateRequest);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var updateRequest = new UpdateContactRequest("Nome Atualizado", new DateOnly(1989, 10, 10), Gender.Other);
        var updateResponse = await client.PutAsJsonAsync($"/api/contacts/{created.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);
    }

    [Fact]
    public async Task Activate_WhenUnderageContactSeeded_ReturnsBadRequest()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var underageContact = await SeedContactAsync(
            factory,
            "Contato Menor",
            TestWebApplicationFactory.FixedToday.AddYears(-17),
            Gender.Male,
            isActive: false);

        using var activateRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{underageContact.Id}/activate");
        var activateResponse = await client.SendAsync(activateRequest);

        Assert.Equal(HttpStatusCode.BadRequest, activateResponse.StatusCode);

        var problem = await activateResponse.Content.ReadFromJsonAsync<HttpValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.True(problem!.Errors.ContainsKey(nameof(Contact.DateOfBirth)));
    }

    private static HttpClient CreateClient(TestWebApplicationFactory factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static async Task<ContactResponse> CreateContactAsync(
        HttpClient client,
        string name,
        DateOnly dateOfBirth,
        Gender gender)
    {
        var request = new CreateContactRequest(name, dateOfBirth, gender);
        var response = await client.PostAsJsonAsync("/api/contacts", request);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ContactResponse>();
        Assert.NotNull(body);

        return body!;
    }

    private static async Task<Contact> SeedContactAsync(
        TestWebApplicationFactory factory,
        string name,
        DateOnly dateOfBirth,
        Gender gender,
        bool isActive)
    {
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var contact = Contact.Create(name, dateOfBirth, gender, TestWebApplicationFactory.FixedUtcNow);
        if (!isActive)
        {
            contact.Deactivate(TestWebApplicationFactory.FixedUtcNow);
        }

        dbContext.Contacts.Add(contact);
        await dbContext.SaveChangesAsync();

        return contact;
    }
}
