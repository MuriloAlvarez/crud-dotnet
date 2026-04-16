using System.Net;
using System.Net.Http.Json;
using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.DTOs;
using crud_net.Shared.Errors;
using crud_net.Tests.Common;
using Microsoft.AspNetCore.Mvc.Testing;

namespace crud_net.Tests.Integration;

public sealed class ContactsEndpointsIntegrationTests
{
    [Fact]
    public async Task PostContacts_WhenPayloadIsValid_ReturnsCreated()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var request = new CreateContactInputDto("Maria Silva", new DateOnly(1990, 1, 10), Gender.Female);

        var response = await client.PostAsJsonAsync("/api/contacts", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ContactResponseDto>();
        Assert.NotNull(body);
        Assert.Equal(request.Name, body!.Name);
        Assert.True(body.IsActive);
    }

    [Fact]
    public async Task GetContacts_ReturnsOnlyActiveContacts()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var activeContact = await CreateContactAsync(client, "Contato Ativo", new DateOnly(1992, 8, 11), Gender.Female);
        var inactiveContact = await CreateContactAsync(client, "Contato Inativo", new DateOnly(1991, 3, 20), Gender.Male);

        using var deactivateRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{inactiveContact.Id}/deactivate");
        var deactivateResponse = await client.SendAsync(deactivateRequest);
        Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);

        var response = await client.GetAsync("/api/contacts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var contacts = await response.Content.ReadFromJsonAsync<List<ContactListItemResponseDto>>();
        Assert.NotNull(contacts);
        Assert.Contains(contacts!, contact => contact.Id == activeContact.Id);
        Assert.DoesNotContain(contacts!, contact => contact.Id == inactiveContact.Id);
    }

    [Fact]
    public async Task GetContactsById_WhenContactIsActive_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Detalhe", new DateOnly(1988, 2, 20), Gender.Male);

        var response = await client.GetAsync($"/api/contacts/{created.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ContactResponseDto>();
        Assert.NotNull(body);
        Assert.Equal(created.Id, body!.Id);
    }

    [Fact]
    public async Task PutContactsById_WhenContactIsActive_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Atualizar", new DateOnly(1987, 5, 14), Gender.Female);
        var request = new UpdateActiveContactInputDto("Nome Atualizado", new DateOnly(1987, 5, 14), Gender.Other);

        var response = await client.PutAsJsonAsync($"/api/contacts/{created.Id}", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ContactResponseDto>();
        Assert.NotNull(body);
        Assert.Equal(request.Name, body!.Name);
        Assert.Equal(request.Gender, body.Gender);
    }

    [Fact]
    public async Task PatchDeactivate_WhenContactIsActive_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Para Desativar", new DateOnly(1989, 9, 9), Gender.Male);

        using var request = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{created.Id}/deactivate");
        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<ContactResponseDto>();
        Assert.NotNull(body);
        Assert.False(body!.IsActive);
    }

    [Fact]
    public async Task PatchActivate_WhenContactIsInactiveAdult_ReturnsOk()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Para Ativar", new DateOnly(1990, 4, 15), Gender.Female);

        using (var deactivateRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{created.Id}/deactivate"))
        {
            var deactivateResponse = await client.SendAsync(deactivateRequest);
            Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        }

        using var activateRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{created.Id}/activate");
        var activateResponse = await client.SendAsync(activateRequest);

        Assert.Equal(HttpStatusCode.OK, activateResponse.StatusCode);

        var body = await activateResponse.Content.ReadFromJsonAsync<ContactResponseDto>();
        Assert.NotNull(body);
        Assert.True(body!.IsActive);
    }

    [Fact]
    public async Task DeleteContactsById_WhenContactExists_ReturnsNoContent()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Para Excluir", new DateOnly(1989, 6, 30), Gender.Other);

        var response = await client.DeleteAsync($"/api/contacts/{created.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task GetContactsById_WhenIdIsInvalid_ReturnsBadRequestWithInvalidIdCode()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync("/api/contacts/id-invalido");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(AppErrorCodes.InvalidId, error!.Code);
    }

    [Fact]
    public async Task PostContacts_WhenContactIsUnderage_ReturnsBadRequestWithValidationCode()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var request = new CreateContactInputDto("Joao Junior", TestWebApplicationFactory.FixedToday.AddYears(-17), Gender.Male);

        var response = await client.PostAsJsonAsync("/api/contacts", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(AppErrorCodes.ContactValidationError, error!.Code);
        Assert.NotNull(error.Errors);
        Assert.True(error.Errors!.ContainsKey(nameof(CreateContactInputDto.DateOfBirth)));
    }

    [Fact]
    public async Task PutContactsById_WhenContactIsInactive_ReturnsNotFoundWithCode()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var created = await CreateContactAsync(client, "Contato Inativo", new DateOnly(1990, 2, 2), Gender.Female);

        using (var deactivateRequest = new HttpRequestMessage(HttpMethod.Patch, $"/api/contacts/{created.Id}/deactivate"))
        {
            var deactivateResponse = await client.SendAsync(deactivateRequest);
            Assert.Equal(HttpStatusCode.OK, deactivateResponse.StatusCode);
        }

        var updateRequest = new UpdateActiveContactInputDto("Nome Atualizado", new DateOnly(1989, 10, 10), Gender.Other);
        var updateResponse = await client.PutAsJsonAsync($"/api/contacts/{created.Id}", updateRequest);

        Assert.Equal(HttpStatusCode.NotFound, updateResponse.StatusCode);

        var error = await updateResponse.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(AppErrorCodes.ContactNotFound, error!.Code);
    }

    [Fact]
    public async Task GetContactsById_WhenContactDoesNotExist_ReturnsNotFoundWithCode()
    {
        using var factory = new TestWebApplicationFactory();
        using var client = CreateClient(factory);

        var response = await client.GetAsync($"/api/contacts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        Assert.NotNull(error);
        Assert.Equal(AppErrorCodes.ContactNotFound, error!.Code);
    }

    private static HttpClient CreateClient(TestWebApplicationFactory factory)
    {
        return factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost")
        });
    }

    private static async Task<ContactResponseDto> CreateContactAsync(
        HttpClient client,
        string name,
        DateOnly dateOfBirth,
        Gender gender)
    {
        var request = new CreateContactInputDto(name, dateOfBirth, gender);
        var response = await client.PostAsJsonAsync("/api/contacts", request);

        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<ContactResponseDto>();
        Assert.NotNull(body);

        return body!;
    }
}
