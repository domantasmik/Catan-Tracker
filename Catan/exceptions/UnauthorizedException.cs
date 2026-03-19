namespace Catan.exceptions;
public class UnauthorizedException : AppException
{
    public UnauthorizedException(string message) : base(401,message){}
}