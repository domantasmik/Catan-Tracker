namespace Catan.exceptions;
// Cia jau SonarQube rodys ir su AI issiaiskinsi kas negerai. Turetu visuose exceptionuose buti taip.
public class AppException : Exception
{
    public int HttpCode;
    public AppException(int code, string message) : base(message)
    {
        HttpCode = code;
    }
}