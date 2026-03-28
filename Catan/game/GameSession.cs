using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Fleck;
using Catan.models;
using Catan.DTO;
using Catan.constants;
namespace Catan.game;
public class GameSession
{
    public readonly string SessionId;
    public readonly int TeamId;
    private readonly Dictionary<IWebSocketConnection,Player> _connections;
    private readonly GameService _service;

    public GameSession(string sessionId,int teamId, GameSettings settings, IServiceScopeFactory scopeFactory)
    {
        TeamId = teamId;
        SessionId = sessionId;

        _connections = new Dictionary<IWebSocketConnection, Player>();
        _service = new GameService(new GameState(settings),new GameEndHandler(scopeFactory),settings,teamId);

        _service.OnTimerExpired += response => SendResponse(response, null);
    }
    public bool ConnectionExists(IWebSocketConnection ws)
    {
        return _connections.ContainsKey(ws);
    }
    public int GetConnectionCount()
    {
        return _connections.Count;
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
    public ServiceResponse Auth(Player player, IWebSocketConnection ws)
    {
        var response = _service.Auth(player);

        if(response.Player == null) return ServiceResponse.Error(ErrorCode.AuthFailed);

        _connections[ws] = response.Player;
        return response;
    }
    public ServiceResponse PlayerDisconnected(IWebSocketConnection ws)
    {
        Player player = _connections[ws];
        _connections.Remove(ws);
        ServiceResponse response = new()
        {
        Broadcast = new  ServiceResponse.ResponseDto{
            Type = WsMessageType.PlayerDisconnected, 
            Payload = new {
                Id = player.Id, 
                Name = player.Name
                }
        }};
        return response;
    }
    public async Task<ServiceResponse> Execute(ClientMessage data, IWebSocketConnection ws)
    {
        ServiceResponse response;
        switch (data.Type)
        {
            case WsMessageType.StartGame:
                {   
                    response = _service.StartGame();
                    break;
                }
            case WsMessageType.NextTurn:
                {
                    response = _service.NextTurn(_connections[ws],false);
                    break;
                }
            case WsMessageType.ResourceUpdate:
                {
                    response = _service.UpdateResource(_connections[ws], data.Payload);
                    break;
                }
            case WsMessageType.RelationUpdate:
                {
                    var(incomingResponse, outgoingResponse) = _service.UpdateRelations(_connections[ws], data.Payload);
                    if(outgoingResponse == null)
                    {
                        response = ServiceResponse.Error(ErrorCode.NullResponse);
                        break;
                    }
                    response = incomingResponse;

                    var outgoingWs = _connections.FirstOrDefault(x => x.Value == outgoingResponse.Player).Key;
                    SendResponse(outgoingResponse, outgoingWs);

                    break;
                }
            case WsMessageType.EndGame:
                {
                    response = await _service.EndGame();
                    break;
                }
            default:
                {
                    response = ServiceResponse.Error(ErrorCode.InvalidMessageType);
                    break;
                }
        }
        return response;
    }
}