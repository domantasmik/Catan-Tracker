using Microsoft.AspNetCore.Mvc;
using Catan.game;
using Microsoft.AspNetCore.Authorization;
using Catan.constants;
using Catan.exceptions;

using Catan.DTO;
namespace Catan.controllers;
[Authorize]
[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly WsHandler _wsHandler;
    private readonly IServiceScopeFactory _scopeFactory;
    public GameController(WsHandler wsHandler, IServiceScopeFactory scopeFactory)
    {
        _wsHandler = wsHandler;
        _scopeFactory = scopeFactory;
    }
    
    [HttpPost("create")]
    public ActionResult<object> CreateServer([FromBody] CreateGameRequestDto request)
    {
        if(_wsHandler.GetActiveSession(request.TeamId) !=null) throw new ConflictException(ErrorCode.GameExists);
        string sessionId = Guid.NewGuid().ToString();
        var session = new GameSession(sessionId,request.TeamId,request.Settings,_scopeFactory);
        _wsHandler.AddSession(sessionId,session);

        return Ok(new {gameId = sessionId});
    }
    [HttpGet("active")]
    public ActionResult<object> GetActiveSession([FromQuery] int teamId)
    {
        string? sessionId = _wsHandler.GetActiveSession(teamId);
        if(sessionId == null) throw new NotFoundException(ErrorCode.GameNotStarted);
        return Ok(new {gameId = sessionId, active = true});
    }
}