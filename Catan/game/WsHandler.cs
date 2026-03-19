using Fleck;
using System.Text.Json;
using Catan.models;
using Catan.DTO;
using Catan.services;
namespace Catan.game;
public class WsHandler {
    private readonly JwtHandler _jwtHandler;
    public WsHandler(JwtHandler jwtHandler)
    {
        _jwtHandler = jwtHandler;
    }
    private Dictionary<string, GameSession> _sessions = new();
    public void AddSession(string sessionId, GameSession session)
    {
        _sessions.Add(sessionId, session);
    }
    public string? GetActiveSession(int teamId)
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
                var path = ws.ConnectionInfo.Path;
                var tokenAndId = path.Split("?token=").LastOrDefault();
                string[] parts = tokenAndId!.Split("&sessionId=");
                try{
                    Player player = _jwtHandler.ValidateJwt(parts[0]);
                    _sessions[parts[1]].Auth(player,ws);
                }
                catch(Exception ex)
                {
                    
                }
            };
            ws.OnMessage = message => {
                try
                {
                    var data = JsonSerializer.Deserialize<ClientMessage>(message);

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
                        ServiceResponse response = new ServiceResponse();
                        response.Broadcast = new{
                            Type = "playerDisconnected", 
                            Payload = new {
                                Id = player.Id, 
                                Name = player.Name
                                }
                        };
                        session.Value.SendResponse(response,ws);
                    }
                    if(session.Value.GetConnectionCount() == 0)
                    {
                        //session.Value.LogGame()
                        _sessions.Remove(session.Key);
                    }
                }
            };
        });
    }
}