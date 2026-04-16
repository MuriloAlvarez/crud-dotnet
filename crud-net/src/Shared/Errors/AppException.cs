namespace crud_net.Shared.Errors;

public abstract class AppException : Exception
{
    protected AppException(string code, string message, int statusCode) : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public string Code { get; }

    public int StatusCode { get; }
}