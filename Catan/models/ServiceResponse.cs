namespace Catan.models;
public class ServiceResponse
{
    public object? Broadcast{get;set;}
    public object? Private{get;set;}
    public Player? Player{get;set;}
    public static ServiceResponse Error(string message)
{
    return new ServiceResponse
    {
        Private = new { error = message }
    };
}
}