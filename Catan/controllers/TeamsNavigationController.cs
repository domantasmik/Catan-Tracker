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
    private readonly IConfiguration _configuration;
    public TeamsNavigationController(ICatanRepository repository, IConfiguration configuration)
    {
        _repository = repository;   
        _configuration = configuration;     
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
    [HttpPost("games/{id}/photo")]
    public async Task<IActionResult> UploadPhoto(int id, IFormFile file)
    {
        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(file.FileName).ToLower();
        if (!allowed.Contains(ext)) return BadRequest("Invalid file type");

        var fileName = $"{Guid.NewGuid()}{ext}";
        var uploadPath = _configuration["Storage:UploadPath"];
        var path = Path.Combine(uploadPath, fileName);

        using var stream = System.IO.File.Create(path);
        await file.CopyToAsync(stream);

        await _repository.UpdatePhotoUrl(id, $"/images/{fileName}");
        return Ok($"/images/{fileName}");
    }
    [HttpPatch("games/{id:int}/sheep")]
    public async Task<ActionResult<string>> RenameSheep([FromRoute] int id, [FromBody] RenameRequestDto request)
    {
        string sheepName = await _repository.RenameSheep(id,request.Name);
        return Ok(new{name = sheepName});
    }
}