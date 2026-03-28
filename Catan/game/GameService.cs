using Catan.models;
using Catan.DTO;
using System.Text.Json;
using Catan.constants;
namespace Catan.game;

// TODO: race condition — timer and player can both call NextTurn concurrently, needs a lock
public class GameService
{
    private readonly GameState _state;
    private readonly int _teamId;
    private readonly GameEndHandler _endHandler;
    private readonly GameSettings _settings;
    private bool _gameStarted;
    private System.Timers.Timer? _timer;
    public event Action<ServiceResponse>? OnTimerExpired;
    public GameService(GameState state,GameEndHandler endHandler,GameSettings settings, int teamId)
    {
        _state = state;
        _settings = settings;
        _endHandler = endHandler;
        _gameStarted = false;
        _teamId = teamId;
    }
    public ServiceResponse Auth(Player joinedPlayer)
    {
        ServiceResponse response = new();

        Player player;

        if (_state.PlayerExists(joinedPlayer)) player = _state.GetPlayerById(joinedPlayer.Id)!;
        else
        {
            if (_gameStarted) return ServiceResponse.Error(ErrorCode.GameStarted);
            if (_state.EnoughPlayers()) return ServiceResponse.Error(ErrorCode.LobbyFull);
            
            player = joinedPlayer;
            _state.AddPlayer(player);
        }
        response.Player = player;

        if (_gameStarted)
        {
            response.Private = new ServiceResponse.ResponseDto
            {
                Type = WsMessageType.AuthAck, 
                Payload = new 
                {
                    GameStarted = true,
                    Turn = _state.GetTurn(), 
                    CurrentPlayer = _state.GetCurrentPlayer(),
                    Settings = _settings
                } 
            };
        }
        else
        {
            response.Private = new ServiceResponse.ResponseDto {
                Type = WsMessageType.AuthAck,
                Payload = new
                {
                    GameStarted = false,
                    Settings = _settings
                }
            };
        }
        
        response.Broadcast = new ServiceResponse.ResponseDto{ 
            Type = WsMessageType.PlayerJoined, 
            Payload = new {
                Players = _state.GetPlayers()
            }
        };
        return response;
    }
    public ServiceResponse StartGame()
    {
        _gameStarted = true;
        ServiceResponse response = new();
        var currentPlayer = _state.GetCurrentPlayer();

        response.Broadcast = new ServiceResponse.ResponseDto {
            Type = WsMessageType.NextTurn, 
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
        ServiceResponse response = new();

        if(!timerExpired && (connectionPlayer == null || _state.GetCurrentPlayer().Id != connectionPlayer.Id)) 
        {
            return ServiceResponse.Error(ErrorCode.NotYourTurn);
        }

        if(_settings.TimerOn) StopTimer();

        _state.NextTurn();
        _state.IncrementCurrentPlayerIndex();

        if(_settings.TimerOn) StartTimer();

        response.Broadcast = new ServiceResponse.ResponseDto{
            Type = WsMessageType.NextTurn, 
            Payload = new 
            {
                Turn = _state.GetTurn(), 
                CurrentPlayer = _state.GetCurrentPlayer()
            }
        };
        return response;
    }
    public ServiceResponse UpdateResource(Player player,JsonElement payload)
    {
        ServiceResponse response = new();

        UpdateResourceRequestDto? request = JsonSerializer.Deserialize<UpdateResourceRequestDto>(payload);
        if(request == null) return ServiceResponse.Error(ErrorCode.FailedToDeserialize(nameof(UpdateResourceRequestDto)));

        if(player.Resources[request.Resource] + request.Amount<0) return ServiceResponse.Error(ErrorCode.InvalidResourceAmount(request.Resource));
        player.Resources[request.Resource] += request.Amount;

        response.Broadcast = new ServiceResponse.ResponseDto{
            Type = WsMessageType.ResourceUpdate,
            Payload = new {
                Player = player,
                Resource = request.Resource,
                Count = player.Resources[request.Resource]
            }
        };
        return response;
    }
    public (ServiceResponse, ServiceResponse?) UpdateRelations(Player incomingPlayer, JsonElement payload)
    {
        ServiceResponse incomingResponse = new();
        ServiceResponse? outgoingResponse = new();

        var request = JsonSerializer.Deserialize<UpdateRelationsRequestDto>(payload);
        if(request == null) 
        {
            incomingResponse = ServiceResponse.Error(ErrorCode.FailedToDeserialize(nameof(UpdateRelationsRequestDto)));
            outgoingResponse = null;
            return (incomingResponse,outgoingResponse);
        }
        Player? outgoingPlayer = _state.GetPlayerById(request.PlayerId);
        if(outgoingPlayer == null)
        {
            incomingResponse = ServiceResponse.Error(ErrorCode.PlayerNotFound);
            outgoingResponse = null;
            return (incomingResponse,outgoingResponse);    
        }

        incomingPlayer.Relations[outgoingPlayer.Id] = request.NewValue;
        incomingResponse.Private = new ServiceResponse.ResponseDto
        {
            Type = WsMessageType.RelationUpdateAck,
            Payload = new
            {
                PlayerId = outgoingPlayer.Id,
                NewValue = request.NewValue
            }
        };
        outgoingResponse.Player = outgoingPlayer;
        outgoingResponse.Private = new ServiceResponse.ResponseDto
        {
            Type = WsMessageType.RelationUpdate,
            Payload = new
            {
                PlayerId = incomingPlayer.Id,
                NewValue = request.NewValue
            }
        };
        return (incomingResponse,outgoingResponse);
        
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
    public async Task<ServiceResponse> EndGame()
    {
        ServiceResponse response = new();

        var game = GameEndHandler.RecordGame(_state, _teamId);
        await _endHandler.SaveToDb(game);

        response.Broadcast = new ServiceResponse.ResponseDto{
            Type = WsMessageType.EndGame,
            Payload =  new {
                Game = game,
            }
        };
        return response;
    }
}

