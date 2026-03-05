using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Fleck;
using Catan.models;
using Catan.data;
namespace Catan; //change later
public class GameSession
{
    public readonly string SessionId;
    public readonly int TeamId;
    private List<Player> _players;
    private Dictionary<Player,IWebSocketConnection> _connections;
    private GameSettings _settings;
    private int _currentPlayerIndex;
    private int _turnCount;

    public GameSession(string sessionId,int teamId, GameSettings settings)
    {
        TeamId = teamId;
        SessionId = sessionId;
        _connections = new Dictionary<Player, IWebSocketConnection>();
        _players = new List<Player>();
        _settings = settings;
    }
    public void SendToAll(object message)
    {
        foreach(var connection in _connections.Values)
            {   
                connection.Send(JsonSerializer.Serialize(message));
            }
    }
    public void Execute(ActionData data, IWebSocketConnection ws)
    {
        switch (data.Type)
        {
            case "auth":
                {
                    var player = JsonSerializer.Deserialize<Player>(data.Payload);
                    if(!PlayerExists(player)){
                    _players.Add(player);
                    }
                    _connections[player] = ws;
                    SendToAll(new {Type = "playerJoined", Payload = JsonSerializer.Serialize(_players)});
                    break;
                }
            case "startGame":
                {   
                    if(_players.Count != _settings.PlayerCount) {
                        ws.Send(JsonSerializer.Serialize(new {error = "Cant start game, not enough players"}));
                        break;
                    }
                    _currentPlayerIndex = 0;
                    _turnCount = 0;
                    var currentPlayer = _players[_currentPlayerIndex];
                    SendToAll(new {Type = "nextTurn", Payload = new {Turn = _turnCount, CurrentPlayer = currentPlayer}});
                    break;
                }
            case "nextTurn":
                {
                    Player player = _connections.FirstOrDefault(connection => connection.Value == ws).Key;
                    if(_players[_currentPlayerIndex].Id != player.Id)
                    {
                        ws.Send(JsonSerializer.Serialize(new{error = "not your turn!"}));
                        break;
                    }
                    if(_currentPlayerIndex == _players.Count-1) _currentPlayerIndex = 0;
                    else _currentPlayerIndex++;
                    _turnCount++;
                    var currentPlayer = _players[_currentPlayerIndex];
                    SendToAll(new {Type = "nextTurn", Payload = new {Turn = _turnCount, CurrentPlayer = currentPlayer}});
                    break;
                }
        }
    }
    public bool ConnectionExists(IWebSocketConnection ws)
    {
        return _connections.ContainsValue(ws);
    }
    public int GetConnectionCount()
    {
        return _connections.Count;
    }
    public Player RemoveConnection(IWebSocketConnection ws)
    {
        Player key = _connections.FirstOrDefault(connection => connection.Value == ws).Key;
        _connections.Remove(key);
        return key;
    }
    private bool PlayerExists(Player player)
    {
        bool exists = false;
        foreach(var pl in _players)
        {
            if(pl.Id == player.Id)
            {
                exists = true;
                break;
            }
        }
        return exists;
    }
}