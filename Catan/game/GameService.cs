using Catan.models;
using System.Text.Json;
namespace Catan.game;

public class GameService
{
    private GameState _state;
    private GameSettings _settings;
    private bool _gameStarted;
    private System.Timers.Timer _timer;
    public event Action<ServiceResponse> OnTimerExpired;
    public GameService(GameState state,GameSettings settings)
    {
        _state = state;
        _settings = settings;
        _gameStarted = false;
    }
    public ServiceResponse Auth(JsonElement payload, Player joinedPlayer)
    {
        ServiceResponse response = new ServiceResponse();
        Player player;

        if (_state.PlayerExists(joinedPlayer)) player = _state.GetPlayerById(joinedPlayer.Id);
        else
        {
            if (_gameStarted) return ServiceResponse.Error("game already started");
            if (_state.EnoughPlayers()) return ServiceResponse.Error("lobby full");

            player = JsonSerializer.Deserialize<Player>(payload);
            _state.AddPlayer(player);
        }

        response.Player = player;

        response.Private = new {
            settings = new { 
                _settings.TimerOn, 
                _settings.TurnDuration 
        }};
        response.Broadcast = new { 
            Type = "playerJoined", 
            Payload = JsonSerializer.Serialize(_state.GetPlayers())
        };
        return response;
    }
    public ServiceResponse StartGame()
    {
        _gameStarted = true;
        ServiceResponse response = new ServiceResponse();
        var currentPlayer = _state.GetCurrentPlayer();

        response.Broadcast = new {
            Type = "nextTurn", 
            Payload = new 
            {
                Turn = _state.GetTurn(), 
                CurrentPlayer = currentPlayer
            }
        };

        if(_settings.TimerOn) StartTimer();
        return response;
    }
    public ServiceResponse NextTurn(Player? connectionPlayer,bool timerExpired)
    {
        ServiceResponse response = new ServiceResponse();

        if(!timerExpired && _state.GetCurrentPlayer().Id != connectionPlayer.Id) 
        {
            return ServiceResponse.Error("not your turn");
        }

        if(_settings.TimerOn) StopTimer();

        _state.NextTurn();
        _state.IncrementCurrentPlayerIndex();

        if(_settings.TimerOn) StartTimer();

        response.Broadcast = new {
            Type = "nextTurn", 
            Payload = new 
            {
                Turn = _state.GetTurn(), 
                CurrentPlayer = _state.GetCurrentPlayer()
            }
        };
        return response;
    }
    private void StopTimer()
    {
        _timer?.Stop();
        _timer?.Dispose();
    }
    private void StartTimer(){
        _timer = new System.Timers.Timer(_settings.TurnDuration);
        _timer.Elapsed += (s, e) => ForceEndTurn();
        _timer.AutoReset = false;
        _timer.Start();
    }
    private void ForceEndTurn()
    {
        var response = NextTurn(null,true); // compute what happens when turn ends
        OnTimerExpired?.Invoke(response); // fire the event, passing response to all subscribers
    }
}

