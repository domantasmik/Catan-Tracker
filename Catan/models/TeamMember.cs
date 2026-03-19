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

    // Man asmeniskai labiau patinka tureti explicit konstruktorius, o ne primary. Cia priklauso nuo code style.
    // Turetu rodyti quick fix: use primary constructor.
    public TeamMember(int userId, int teamId)
    {
        UserId = userId;
        TeamId = teamId;
    }
}