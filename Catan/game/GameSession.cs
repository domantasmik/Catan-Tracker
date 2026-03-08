using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Fleck;
using Catan.models;
using Catan.data;
namespace Catan.game;
public class GameSession
{
    public readonly string SessionId;
    public readonly int TeamId;
    private Dictionary<IWebSocketConnection,Player> _connections;
    private GameService _service;
    private GameState _state;

    public GameSession(string sessionId,int teamId, GameSettings settings)
    {
        TeamId = teamId;
        SessionId = sessionId;

        _connections = new Dictionary<IWebSocketConnection, Player>();
        _state = new GameState(settings);
        _service = new GameService(_state,settings);

        _service.OnTimerExpired += response => SendResponse(response, null);
    }
    public void SendResponse(ServiceResponse response, IWebSocketConnection? ws)
    {
        if(response.Private != null && ws != null) ws.Send(JsonSerializer.Serialize(response.Private));
        if(response.Broadcast != null)
        {
            foreach(var connection in _connections.Keys)
                {   
                    connection.Send(JsonSerializer.Serialize(response.Broadcast));
                }
        }
    }
    public void Execute(ClientMessage data, IWebSocketConnection ws)
    {
        ServiceResponse response = new ServiceResponse();
        switch (data.Type)
        {
            case "auth":
                {
                    _connections.TryGetValue(ws, out Player? player);
                    response = _service.Auth(data.Payload, player);
                    if(response.Player == null) {
                        response = ServiceResponse.Error("auth failed");
                        break;
                    }
                    _connections[ws] = response.Player;
                    break;
                }
            case "startGame":
                {   
                    response = _service.StartGame();
                    break;
                }
            case "nextTurn":
                {
                    response = _service.NextTurn(_connections[ws],false);
                    break;
                }
            case "updateResource":
                {
                    response = _service.UpdateResource(_connections[ws], data.Payload);
                    break;
                }
        }
        SendResponse(response,ws);
    }
    public bool ConnectionExists(IWebSocketConnection ws)
    {
        return _connections.ContainsKey(ws);
    }
    public int GetConnectionCount()
    {
        return _connections.Count;
    }
    public Player RemoveConnection(IWebSocketConnection ws)
    {
        Player player = _connections[ws];
        _connections.Remove(ws);
        return player;
    }
}