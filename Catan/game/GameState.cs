using Catan.models;
namespace Catan.game;
public class GameState
{
    private List<Player> _players;
    private GameSettings _settings;
    private int _currentPlayerIndex;
    private int _turnCount;
    public GameState(GameSettings settings)
    {
        _players = new List<Player>();
        _settings = settings;
        _currentPlayerIndex = 0;
        _turnCount = 0;
    }
    public bool PlayerExists(Player player)
    {
        bool exists = false;

        if(player == null) return exists;

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
    public Player? GetPlayerById(int id)
    {
        foreach(var pl in _players)
        {
            if(pl.Id == id)
            {
                return pl;
            };
        }
        return null;
    }
    public void AddPlayer(Player player)
    {
        if(!PlayerExists(player)) _players.Add(player);
    }
    public List<Player> GetPlayers()
    {
        return _players;
    }
    public bool EnoughPlayers()
    {
        return _players.Count == _settings.PlayerCount;
    }
    public Player GetCurrentPlayer()
    {
        return _players[_currentPlayerIndex];
    }
    public void IncrementCurrentPlayerIndex()
    {
        if(_currentPlayerIndex == _players.Count-1) _currentPlayerIndex = 0;
        else _currentPlayerIndex++;
    }
    public void NextTurn()
    {
        _turnCount++;
    }
    public int GetTurn()
    {
        return _turnCount;
    }
}