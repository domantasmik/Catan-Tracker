using System.Text.Json;
namespace Catan.DTO;
// Gal butu verta padaryt visus DTO's immutable (tinka visiems dto sitam folderyje.)
// {get; init; }
// Parasyk explicitly, kad cia Dto -> ClientMessageDto
public class ClientMessage
{
    public string SessionId{ get; set; } = String.Empty;
    public string Type{ get; set; } = String.Empty;
    public JsonElement Payload{ get; set; }

}