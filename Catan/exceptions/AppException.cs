namespace Catan.exceptions;
public class AppException : Exception
{
    public int HttpCode;
    public AppException(int code, string message) : base(message)
    {
        HttpCode = code;
    }
}