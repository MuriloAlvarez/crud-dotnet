using crud_net.Features.Contacts.Domain.Errors;

namespace crud_net.Shared.Errors;

public static class GlobalExceptionHandlingExtensions
{
    public static void UseGlobalExceptionHandling(this IApplicationBuilder app, IHostEnvironment environment)
    {
        app.UseExceptionHandler(errorApp =>
        {
            errorApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
                var exception = exceptionFeature?.Error;

                var (statusCode, payload) = MapException(exception, environment.IsDevelopment());
                context.Response.StatusCode = statusCode;
                context.Response.ContentType = "application/json";

                await context.Response.WriteAsJsonAsync(payload);
            });
        });
    }

    private static (int StatusCode, ApiErrorResponse Payload) MapException(Exception? exception, bool isDevelopment)
    {
        if (exception is ContactValidationException validationException)
        {
            return (
                validationException.StatusCode,
                new ApiErrorResponse(validationException.Code, validationException.Message, validationException.Errors));
        }

        if (exception is AppException appException)
        {
            return (
                appException.StatusCode,
                new ApiErrorResponse(appException.Code, appException.Message));
        }

        var message = isDevelopment && exception is not null
            ? exception.Message
            : "An unexpected error occurred.";

        return (
            StatusCodes.Status500InternalServerError,
            new ApiErrorResponse(AppErrorCodes.InternalServerError, message));
    }
}