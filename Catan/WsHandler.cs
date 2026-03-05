using Fleck;
using System.Text.Json;
namespace Catan;
public class WsHandler {
    public WsHandler(){}
    private Dictionary<string, GameSession> _sessions = new();
    public void AddSession(string sessionId, GameSession session)
    {
        _sessions.Add(sessionId, session);
    }
    public string GetActiveSession(int teamId)
    {
        foreach(GameSession session in _sessions.Values)
        {
            if(session.TeamId == teamId) return session.SessionId;
        }
        return null;
    }
    public void Start() {
        var server = new WebSocketServer("ws://0.0.0.0:8181");
        server.Start(ws => {
            ws.OnOpen = () => {
                ws.Send("auth");//auth request
            };
            ws.OnMessage = message => {
                try
                {
                    var data = JsonSerializer.Deserialize<ActionData>(message);

                    if (data?.SessionId == null) throw new Exception( "SessionId required");
                    if (!_sessions.ContainsKey(data.SessionId)) throw new Exception("Session not found");
                    
                    _sessions[data.SessionId].Execute(data, ws);
                }
                catch (Exception ex)
                {
                    ws.Send(JsonSerializer.Serialize(new { error = ex.Message }));
                }
            };
            ws.OnClose = () =>
            {
                foreach(var session in _sessions)
                {
                    if(session.Value.ConnectionExists(ws)) 
                    {
                        var player = session.Value.RemoveConnection(ws);
                        session.Value.SendToAll( new{
                            Type = "playerDisconnected", 
                            Payload = new {
                                Id = player.Id, 
                                Name = player.Name
                                }
                        });
                    }
                    if(session.Value.GetConnectionCount() == 0)
                    {
                        //session.Value.LogGame()
                        _sessions.Remove(session.Key);
                        Console.WriteLine("nieko nebeliko, pisu nx sessiona");
                    }
                }
            };
        });
    }
}