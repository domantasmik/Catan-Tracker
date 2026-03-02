using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Catan.models;
using Catan.data;
using Catan.services;
namespace Catan.controllers;
[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly CatanDbContext _dbContext;
    private WebSocketServer _server;
    public GameController(CatanDbContext dbContext, WebSocketServer server)
    {
        _dbContext = dbContext;
        _server = server;
    }
    public ActionResult<Session> CreateServer(int userId, int teamId, Settings settings)
    {
        var session = new GameSession(_server,_dbContext);
        session.RunSession();

        return Ok
        //session = new GameSession(server)
        //session.RunSession()
        //return session id,connection
    }
}