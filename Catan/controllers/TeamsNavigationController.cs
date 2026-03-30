using Microsoft.AspNetCore.Mvc;
using Catan.models;
using Catan.repositories;
using Catan.exceptions;
using Microsoft.AspNetCore.Authorization;
using Catan.DTO;
using Catan.constants;
namespace Catan.controllers;
[Authorize]
[ApiController]
[Route("teams")]
public class TeamsNavigationController : ControllerBase
{
    private readonly ICatanRepository _repository;
    public TeamsNavigationController(ICatanRepository repository)
    {
        _repository = repository;        
    }
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<List<Team>>>> GetTeams([FromQuery] int userId)
    {
        var teams = await _repository.GetUserTeams(userId);
        return Ok(new {teams = teams});
    }
    
    [HttpPost("join")]
    public async Task<ActionResult<Team>> JoinTeam([FromBody] JoinRequestDto request)
    {
        var team = await _repository.GetTeam(request.TeamId);

        if(team == null) throw new NotFoundException(ErrorCode.TeamDoesntExist);

        await _repository.AddUserToTeam(request.UserId, team.Id);
        
        return Ok(new {team = team});
    }
    
    [HttpPost("create")]
    public async Task<ActionResult<Team>> CreateTeam([FromBody] CreateTeamRequestDto request)
    {
        var team = new Team(request.Name);
        await _repository.AddTeam(team);
        await _repository.AddUserToTeam(request.UserId,team.Id);
        
        return Ok(new {team = team});
    }
    [HttpGet("games")]
    public async Task<ActionResult<IEnumerable<Game>>> GetGames([FromQuery] int teamId)
    {
        var games = await _repository.GetGames(teamId);
        var gameList = new List<GameListDto>();
        foreach(var game in games)
        {
            gameList.Add(new GameListDto(game.Date, game.Name, game.Id));
        }
        var response = new GameHistoryDto(games.Count(), gameList);
        return Ok(new {games = response});
    }
    [HttpGet("games/{id:int}")]
    public async Task<ActionResult<Game>> GetGameById([FromRoute] int id)
    {
        var game = await _repository.GetGameById(id);
        var gameDto = new GameDto(game);
        return Ok(new{game = gameDto});
    }
    [HttpPatch("games/{id:int}/name")]
    public async Task<ActionResult<string>> RenameGame([FromRoute] int id, [FromBody] RenameRequestDto request)
    {
        string gameName = await _repository.RenameGame(id,request.Name);
        return Ok(new{name = gameName});
    }
}