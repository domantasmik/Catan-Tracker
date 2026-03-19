using Catan.models;
using Catan.DTO;
using System.Text.Json;
namespace Catan.game;

// Cia krc gali buti problemu su race conditions, bet cia bsk advanced labai ir sunku pastebeti
// Patarciau pasidometi, bet as pats buciau cia byby dejes 

/* CIA GEMINI KOMENTARAS
Gija A (Timeris): Įeina į NextTurn(null, true). Parametras timerExpired yra true.

Gija A: Kadangi timerExpired == true, ji peršoka tavo if patikrinimą (kur tikrinamas žaidėjo ID).

Gija B (Vartotojas): Įeina į NextTurn(p1, false). Ji prieina if patikrinimą.

Gija A: Vykdo _state.NextTurn(). Dabar aktyvus žaidėjas pasikeičia iš P1 į P2.

Gija B: Dabar vykdo savo if patikrinimą: _state.GetCurrentPlayer().Id != connectionPlayer.Id.

    GetCurrentPlayer().Id dabar grąžina P2 (nes Gija A jį ką tik pakeitė!).

    connectionPlayer.Id yra P1.

    Pataikei! P2 != P1 yra True.

Gija B: Kadangi sąlyga teisinga, ji nieko negrąžina ir eina toliau vykdyti kodo.

Gija A: Pabaigia savo darbą.

Gija B: Dar kartą iškviečia _state.NextTurn(). Dabar aktyvus žaidėjas pasikeičia iš P2 į P3.
*/

// Toliau sitam folderyje failu as nenoriu review, nes cia tsg daxuja kodo ir sudetingos logikos xd realybeje todel ir rekomenduojami mazi PR
// Nes kai susijusi logika ir dar jos daug ir sudetingos, labai sunku reviewint
public class GameService
{
    private GameState _state;
    private GameSettings _settings;
    private bool _gameStarted;
    private System.Timers.Timer? _timer;
    public event Action<ServiceResponse>? OnTimerExpired;
    public GameService(GameState state,GameSettings settings)
    {
        _state = state;
        _settings = settings;
        _gameStarted = false;
    }
    public ServiceResponse Auth(Player joinedPlayer)
    {
        ServiceResponse response = new ServiceResponse();

        Player? player;

        if (_state.PlayerExists(joinedPlayer)) player = _state.GetPlayerById(joinedPlayer.Id);
        else
        {
            if (_gameStarted) return ServiceResponse.Error("game already started");
            if (_state.EnoughPlayers()) return ServiceResponse.Error("lobby full");
            
            player = joinedPlayer;
            _state.AddPlayer(player);
        }
        response.Player = player;

        if (_gameStarted)
        {
            response.Private = new {
                settings = new { 
                    _settings.TimerOn, 
                    _settings.TurnDuration 
                },
                Type = "nextTurn", 
                Payload = new 
                {
                    Turn = _state.GetTurn(), 
                    CurrentPlayer = _state.GetCurrentPlayer()
                }   
            };
        }
        else
        {
            response.Private = new {
                settings = new { _settings.TimerOn, _settings.TurnDuration }
            };
        }
        
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

        if(!timerExpired && (connectionPlayer == null || _state.GetCurrentPlayer().Id != connectionPlayer.Id)) 
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
    private record UpdateResourceRequest(string Resource, int Amount);
    public ServiceResponse UpdateResource(Player player,JsonElement payload)
    {
        ServiceResponse response = new ServiceResponse();

        UpdateResourceRequest? request = JsonSerializer.Deserialize<UpdateResourceRequest>(payload);
        if(request == null) return ServiceResponse.Error("failed to parse the request");

        if(player.Resources[request.Resource] == 0 && request.Amount<0) return ServiceResponse.Error("cant have less than 0 "+request.Resource);
        player.Resources[request.Resource] += request.Amount;

        response.Broadcast = new
        {
            Player = player,
            Resource = request.Resource,
            Count = player.Resources[request.Resource]
        };
        return response;
    }
    private record UpdateRelationsRequest(Player Player, int NewValue);
    public RelationUpdateResponse UpdateRelations(Player incomingPlayer, JsonElement payload)
    {
        RelationUpdateResponse response = new RelationUpdateResponse();
        ServiceResponse? incomingResponse = new ServiceResponse();
        ServiceResponse? outgoingResponse = new ServiceResponse();

        var request = JsonSerializer.Deserialize<UpdateRelationsRequest>(payload);
        if(request == null) 
        {
            response.Incoming = ServiceResponse.Error("failed to parse the request");
            response.Outgoing = null;
            return response;
        }
        Player? outgoingPlayer = _state.GetPlayerById(request.Player.Id);
        if(outgoingPlayer == null)
        {
            response.Incoming = ServiceResponse.Error("outgoing player is null");
            response.Outgoing = null;
            return response;
        }

        incomingPlayer.Relations[outgoingPlayer.Id] = request.NewValue;
        incomingResponse.Private = new
        {
            Type = "relationUpdateAck",
            Payload = new
            {
                Player = outgoingPlayer,
                NewValue = request.NewValue
            }
        };
        outgoingResponse.Player = outgoingPlayer;
        outgoingResponse.Private = new
        {
            Player = incomingPlayer,
            NewValue = request.NewValue
        };
        response.Incoming = incomingResponse;
        response.Outgoing = outgoingResponse;
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

