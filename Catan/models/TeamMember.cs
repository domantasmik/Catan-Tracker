using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Catan.models;
[Table("team_members")]
public class TeamMember
{
    [Column("user_id")]
    public int UserId { get; set; }
    [Column("team_id")]
    public int TeamId { get; set; }
    public TeamMember(int userId, int teamId)
    {
        UserId = userId;
        TeamId = teamId;
    }
}