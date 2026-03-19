using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Fleck;
using Catan.models;
using Catan.DTO;
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
    public void SendResponse(ServiceResponse? response, IWebSocketConnection? ws)
    {
        if(response == null) throw new Exception("response is null");

        if(response.Private != null && ws != null) ws.Send(JsonSerializer.Serialize(response.Private));
        if(response.Broadcast != null)
        {
            foreach(var connection in _connections.Keys)
                {   
                    connection.Send(JsonSerializer.Serialize(response.Broadcast));
                }
        }
    }
    public void Auth(Player player, IWebSocketConnection ws)
    {
        var response = _service.Auth(player);
        _connections[ws] = response.Player;
        SendResponse(response,ws);
    }
    public void Execute(ClientMessage data, IWebSocketConnection ws)
    {
        ServiceResponse? response = new ServiceResponse();
        switch (data.Type)
        {
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
            case "resourceUpdate":
                {
                    response = _service.UpdateResource(_connections[ws], data.Payload);
                    break;
                }
            case "relationUpdate":
                {
                    RelationUpdateResponse responses = _service.UpdateRelations(_connections[ws], data.Payload);
                    response = responses.Incoming;

                    var outgoingWs = _connections.FirstOrDefault(x => x.Value == responses.Outgoing.Player).Key;
                    SendResponse(responses.Outgoing, outgoingWs);

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