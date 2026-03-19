namespace Catan.exceptions;
public class NotFoundException : AppException
{
    public NotFoundException(string message) : base(404,message){}
}