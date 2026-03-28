using Catan.models;
namespace Catan.game;
public class GameState
{
    private readonly List<Player> _players;
    private readonly GameSettings _settings;
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
            }
        }
        return null;
    }
    public void AddPlayer(Player player)
    {
        if(!PlayerExists(player))
        {
            foreach(var pl in _players)
            {
                pl.Relations[player.Id] = 0;
                player.Relations[pl.Id] = 0;
            }
            _players.Add(player);
        }
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
    public int GetReputation(Player player)
    {
        int reputation = _players.Where(pl => pl.Id != player.Id).Select(pl => pl.Relations[player.Id]).Sum();
        reputation/=_players.Count-1;
        return reputation;
    }
    public Player GetWinner()
    {
        return _players.OrderByDescending(player => player.Resources["Point"]).First();
    }
    public bool GameFinished()
    {
        return GetWinner().Resources["Point"] >= _settings.PointsToWin;
    }
}