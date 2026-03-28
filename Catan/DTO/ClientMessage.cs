using System.Text.Json;
namespace Catan.DTO;
public record ClientMessage
{
    public string SessionId{ get; set; } = String.Empty;
    public string Type{ get; set; } = String.Empty;
    public JsonElement Payload{ get; set; }

}