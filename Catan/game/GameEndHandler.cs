using Catan.models;
using Catan.repositories;
namespace Catan.game;
public class GameEndHandler
{
    private readonly IServiceScopeFactory _scopeFactory;
    public GameEndHandler(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }
    public static Game RecordGame(GameState state, int teamId)
    {
        var game = new Game
        {
            Date = DateTime.UtcNow,
            TurnCount = state.GetTurn(),
            WinnerId = state.GetWinner().Id,
            PhotoUrl = null,
            Name = "Zaidimas",
            PlayerCount = state.GetPlayers().Count,
            TeamId = teamId,
            Finished = state.GameFinished()
        };
        foreach(var player in state.GetPlayers())
        {
            GamePlayer gp = new(game.Id, player.Id,player.Resources["DevCard"], player.Resources["Point"],state.GetReputation(player));
            game.Players.Add(gp);
        }
        return game;
    }
    public async Task SaveToDb(Game game)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<ICatanRepository>();
        await repository.SaveGame(game);
    }
}