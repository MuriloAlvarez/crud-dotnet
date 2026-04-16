using crud_net.Features.Contacts.Domain.Services;
using crud_net.Features.Contacts.Infrastructure.Persistence;
using crud_net.Features.Contacts.Infrastructure.Persistence.Repositories;
using crud_net.Features.Contacts.Repositories;
using crud_net.Features.Contacts.UseCases;
using crud_net.Features.Contacts.Validations;
using crud_net.Shared.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "Contacts API", Version = "v1" });
});

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "Connection string 'DefaultConnection' is not configured. Set it in appsettings.json or in the environment variable 'ConnectionStrings__DefaultConnection'.");
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddScoped<IContactRepository, ContactRepository>();
builder.Services.AddScoped<IContactInputValidator, ContactInputValidator>();
builder.Services.AddScoped<CreateContactUseCase>();
builder.Services.AddScoped<ListActiveContactsUseCase>();
builder.Services.AddScoped<GetActiveContactDetailsUseCase>();
builder.Services.AddScoped<UpdateActiveContactUseCase>();
builder.Services.AddScoped<ActivateContactUseCase>();
builder.Services.AddScoped<DeactivateContactUseCase>();
builder.Services.AddScoped<DeleteContactUseCase>();
builder.Services.AddSingleton<IAppClock, SystemClock>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseGlobalExceptionHandling(app.Environment);

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { message = "Contacts API is running." }));

app.MapControllers();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
