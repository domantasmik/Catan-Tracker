using System.Text.Json;

public class ActionData
{
    public string SessionId{ get; set; }
    public string Type{ get; set; }
    public JsonElement Payload{ get; set; }

}