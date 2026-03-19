using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Catan.models;
using Catan.data;
using Catan.services;
using Catan.game;
using Microsoft.AspNetCore.Authorization;
using Catan.exceptions;
namespace Catan.controllers;
[Authorize]
[ApiController]
[Route("game")]
public class GameController : ControllerBase
{
    private readonly WsHandler _wsHandler;
    // dbContext imeti, bet nenaudoji
    public GameController(CatanDbContext dbContext,WsHandler wsHandler)
    {
        _wsHandler = wsHandler;
    }
    public record CreateGameRequest(int userId, int teamId, GameSettings settings);
    [HttpPost("create")]
    // Tu naudoji Object klase, o ne kintamaji. Visos klases prasideda is didziosios raides. Cia turi naudoji 'object'
    public ActionResult<Object> CreateServer([FromBody] CreateGameRequest request)
    {
        // Susikurk konstatu failiuka ir susirasyk ten, o ne tiesiog strings metyk visur
        if(_wsHandler.GetActiveSession(request.teamId) !=null) throw new ConflictException("Game already exists for this team");
        // Siaip as nekonvetuociau i string cia, kai siusi per endpointa .NET man atrodo automatiskai konvertuos kai reikes.
        string sessionId = Guid.NewGuid().ToString();
        
        var session = new GameSession(sessionId,request.teamId,request.settings);
        _wsHandler.AddSession(sessionId,session);

        return Ok(new {gameId = sessionId});
    }
    [HttpGet("active")]
    public ActionResult<Object> GetActiveSession([FromQuery] int teamId)
    {
        string? sessionId = _wsHandler.GetActiveSession(teamId);
        if(sessionId == null) throw new NotFoundException("No game started");
        return Ok(new {gameId = sessionId, active = true});
    }
}