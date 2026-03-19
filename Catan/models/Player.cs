// Isirayk koki code style cop ir kazka kas koda formatuos, common issues parodys ir paaiskins.
// Rekomenduoju SonarQube ir formatteri koki. Toliau problemu, kurias parodys SonarQube nekomentuosiu.

public class Player
{
    public int Id{get;set;}
    public string Name{get;set;}
    public Dictionary<string, int> Resources = new() {
    { "DevCard", 0 },
    { "Point", 0 }
    };
    public Dictionary<int,int> Relations = new();

    public Player(int id, string name)
    {
        Id = id;
        Name = name;
    }
}