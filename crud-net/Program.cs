using crud_net.Features.Contacts;
using crud_net.Infrastructure.Persistence;
using crud_net.Infrastructure.Persistence.Repositories;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

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
builder.Services.AddSingleton<IAppClock, SystemClock>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var detail = app.Environment.IsDevelopment()
            ? exceptionFeature?.Error.Message
            : "An unexpected error occurred.";

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        await Results.Problem(
            title: "Unexpected error",
            detail: detail,
            statusCode: StatusCodes.Status500InternalServerError)
            .ExecuteAsync(context);
    });
});

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new { message = "Contacts API is running." }));

app.MapContactsEndpoints();

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();

public partial class Program { }
