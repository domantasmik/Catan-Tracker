using Fleck;
using System.Text.Json;
using Catan.models;
using Catan.DTO;
using Catan.services;
using Catan.exceptions;
using Catan.constants;
namespace Catan.game;
public class WsHandler {
    private readonly JwtHandler _jwtHandler;
    public WsHandler(JwtHandler jwtHandler)
    {
        _jwtHandler = jwtHandler;
    }
    private readonly Dictionary<string, GameSession> _sessions = new();
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
    public async Task Start() {
        var server = new WebSocketServer("ws://0.0.0.0:8181");
        server.Start(ws => {
            ws.OnOpen = () => HandleJoin(ws);
            ws.OnMessage = async message => await RouteMessage(ws,message);
            ws.OnClose = () => HandleDisconnect(ws);
        });
    }
    private void HandleJoin(IWebSocketConnection ws)
    {
        try{
            var path = ws.ConnectionInfo.Path;
            string[] tokenAndId = path.Split("?token=")[1].Split("&sessionId=");
            var (token, sessionId) = (tokenAndId[0], tokenAndId[1]);

            Player player = _jwtHandler.ValidateJwt(token) ?? throw new GameException(ErrorCode.JwtValidationFailed);

            var response = _sessions[sessionId].Auth(player,ws);
            _sessions[sessionId].SendResponse(response,ws);
        }
        catch (Exception e)
        {
            var response = ServiceResponse.Error(e.Message);
            ws.Send(JsonSerializer.Serialize(response));
            ws.Close();
        }
    }
    private async Task RouteMessage(IWebSocketConnection ws, string message)
    {
        try{
            ClientMessage data;
            data = JsonSerializer.Deserialize<ClientMessage>(message) ?? throw new GameException(ErrorCode.FailedToDeserialize(nameof(ClientMessage)));

            var response = await _sessions[data.SessionId].Execute(data, ws);
            _sessions[data.SessionId].SendResponse(response,ws);
        }
        catch (Exception e)
        {
            var response = ServiceResponse.Error(e.Message);
            await ws.Send(JsonSerializer.Serialize(response));
        }
    }
    private void HandleDisconnect(IWebSocketConnection ws)
    {
        List<string> keys = new List<string>();
        foreach(var session in _sessions)
        {
            if(session.Value.ConnectionExists(ws)) session.Value.SendResponse(session.Value.PlayerDisconnected(ws),ws);
            if(session.Value.GetConnectionCount() == 0) keys.Add(session.Key);
        }
        foreach(string key in keys)
        {
            _sessions.Remove(key);
        }
    }
}