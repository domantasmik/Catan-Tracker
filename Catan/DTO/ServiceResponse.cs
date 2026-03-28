using Catan.models;
namespace Catan.DTO;
public class ServiceResponse
{
    public ResponseDto? Broadcast{get;set;}
    public ResponseDto? Private{get;set;}
    public Player? Player{get;set;}
    public class ResponseDto
    {
        public required string Type {get;set;}
        public required object Payload {get;set;}
    }
    public static ServiceResponse Error(string errorCode)
    {
        return new ServiceResponse
        {
            Private = new ResponseDto{ Type = "Error", Payload = new {ErrorCode = errorCode} }
        };
    }
}