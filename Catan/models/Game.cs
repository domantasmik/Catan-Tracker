using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Catan.models;
[Table("games")]
public class Game
{
    [Key]
    [Column("id")]
    public int Id{get;set;}
    [Column("date")]
    public DateTime Date{get;set;}
    [Column("turn_count")]
    public int TurnCount{get;set;}
    [Column("winner_id")]
    public int WinnerId{get;set;}
    [Column("photo_url")]
    public string? PhotoUrl{get;set;} = string.Empty;
    [Column("name")]
    public string Name {get;set;} = string.Empty;
    [Column("player_count")]
    public int PlayerCount{get;set;}
    [Column("team_id")]
    public int TeamId{get;set;}
    [Column("finished")]
    public bool Finished{get;set;}
    public List<GamePlayer> GamePlayers {get;set;} = new List<GamePlayer>();

    public Game(){}
}