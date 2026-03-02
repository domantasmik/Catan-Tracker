public class GameSession
{
    private readonly CatanDbContext _dbContext;
    private WebSocketServer _server;

    private Queue _connections;
    private record Connection(Player player, IWebSocketConnection connection);

    private int teamId;
    public GameSession(WebSocketServer server,CatanDbContext dbContext)
    {
        _server = server;
        _dbContext = dbContext;
    }
    public ___ RunSession()
    {
        _server.Start(ws =>
        {
            ws.OnOpen = () =>
            {
                //send auth request 
            };
            ws.OnMessage = message =>
            {
                var data = JsonSerializer.Deserialize<GameAction>(message);
    
                switch (data.Type) {
                    case "auth": AuthConnection(data.UserId, ws);
                };
            };
        });
    }
    private AuthConnection(int userId, IWebSocketConnection connection)
    {
        //dbcontext get User 
        //add that user to connections
    }
}