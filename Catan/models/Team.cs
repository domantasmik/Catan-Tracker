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
    public List<User> Users{get;set;} = [];
    public Team(){}
    public Team(string name)
    {
        Name = name;
    }
}