using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Catan.models;
[Table("game_players")]
public class GamePlayer
{
    [Column("game_id")]
    public int GameId{get;set;}
    [Column("user_id")] 
    public int UserId{get;set;}
    [Column("user_dc")]
    public int PlayerDC{get;set;}
    [Column("user_points")]
    public int PlayerPoints{get;set;}
    [Column("user_reputation")]
    public int PlayerReputation{get;set;}
    public GamePlayer(){}
    public GamePlayer(int gameId, int userId, int playerDc, int playerPoints, int playerReputation)
    {
        GameId = gameId;
        UserId = userId;
        PlayerDC = playerDc;
        PlayerPoints = playerPoints;
        PlayerReputation = playerReputation;
    }
}