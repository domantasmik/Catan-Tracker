namespace Catan.exceptions;
public class ConflictException : AppException
{
    public ConflictException(string message) : base(409,message){}
}