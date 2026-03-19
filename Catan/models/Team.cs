using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Catan.models;
[Table("teams")]
public class Team
{
    [Key]
    [Column("id")]
    public int Id {get;set;}
    [Column("name")]
    public string Name {get;set;} = string.Empty;
    // Beveik visada klausyk SonarQube ir formatteriu, jie geriau zino. Turetu tau parodyti problema su net List<User>();
    // pavyzdys TeamMember.cs kada nebutinai klausyti reikia.
    public List<User> Users{get;set;} = new List<User>();
    public Team(){}
    public Team(string name)
    {
        Name = name;
    }
}