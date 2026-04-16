namespace crud_net.Shared.Errors;

public sealed record ApiErrorResponse(
    string Code,
    string Message,
    Dictionary<string, string[]>? Errors = null);