using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Catan.models;
[Table("users")]
public class User
{
    [Key]
    [Column("id")]
    public int Id{get;set;}
    [Column("username")]
    public string Username{get;set;} = string.Empty;
    [Column("password_hash")]
    public string PasswordHash{get;set;} = string.Empty;
    [Column("registered_at")]
    public DateTime RegisteredAt {get;set;}
    [Column("last_login")]
    public DateTime LastLogin {get;set;}
    public List<Team> Teams {get;set;} = new List<Team>();
    public User(){}
    public User(string username,string passwordhash)
    {
        Username = username;
        PasswordHash = passwordhash;
        RegisteredAt = DateTime.UtcNow;
        LastLogin = DateTime.UtcNow;
    }
}