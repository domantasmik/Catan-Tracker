namespace Catan.models;
public class Player
{
    public int Id{get;set;}
    public string Name{get;set;}
    public Dictionary<string, int> Resources {get;set;} = new() {
    { "DevCard", 0 },
    { "Point", 0 }
    };
    public Dictionary<int,int> Relations {get;set;} = new();

    public Player(int id, string name)
    {
        Id = id;
        Name = name;
    }
}