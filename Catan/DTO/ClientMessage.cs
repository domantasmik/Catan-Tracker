using System.Text.Json;
namespace Catan.DTO;
public class ClientMessage
{
    public string SessionId{ get; set; } = String.Empty;
    public string Type{ get; set; } = String.Empty;
    public JsonElement Payload{ get; set; }

}