using Catan.models;
namespace Catan.DTO;
public record GameDto
{
    public int Id { get; init; }
    public DateTime Date { get; init; }
    public int TurnCount { get; init; }
    public int WinnerId { get; init; }
    public string? PhotoUrl { get; init; }
    public string Name { get; init; }
    public int PlayerCount { get; init; }
    public int TeamId { get; init; }
    public bool Finished { get; init; }
    public string SheepName{get;init;}
    public IEnumerable<GamePlayerDto> GamePlayers { get; init; }

    public GameDto(Game game)
    {
        Id = game.Id;
        Date = game.Date;
        TurnCount = game.TurnCount;
        WinnerId = game.WinnerId;
        PhotoUrl = game.PhotoUrl;
        Name = game.Name;
        PlayerCount = game.PlayerCount;
        TeamId = game.TeamId;
        Finished = game.Finished;
        SheepName = game.SheepName;
        GamePlayers = game.GamePlayers.Select(gp =>
            new GamePlayerDto(gp.UserId, gp.PlayerPoints, gp.PlayerReputation, gp.User.Username, gp.PlayerColor));
    }

    public record GamePlayerDto(int Id, int Points, int Reputation, string Name, string Color);
}