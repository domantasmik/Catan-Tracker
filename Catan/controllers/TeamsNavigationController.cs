using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Catan.models;
using Catan.data;
using Catan.services;
namespace Catan.controllers;

[ApiController]
[Route("teams")]
public class TeamsNavigationController : ControllerBase
{
    private readonly CatanDbContext _dbContext;
    public TeamsNavigationController(CatanDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    [HttpGet("mine")]
    public ActionResult<List<Team>> GetTeams([FromQuery] int userId)
    {
        try
        {
            var teamIds = _dbContext.TeamMembers.Where(tm => tm.UserId == userId).Select(tm => tm.TeamId).ToList();
            var teams = _dbContext.Teams.Where(t => teamIds.Contains(t.Id)).ToList();
            return Ok(new {teams = teams});
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
       
    }
    public record JoinRequest(int userId, int teamId);
    [HttpPost("join")]
    public ActionResult<Team> JoinTeam([FromBody] JoinRequest request)
    {
        try{
            var team = _dbContext.Teams.FirstOrDefault(t => t.Id == request.teamId);

            if(team == null) throw new Exception("Team does not exist");

            var teamMember = new TeamMember(request.userId,team.Id);
            _dbContext.TeamMembers.Add(teamMember);
            _dbContext.SaveChanges();
            return Ok(new {team = team});
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    public record CreateRequest(int userId, string name);
    [HttpPost("create")]
    public ActionResult<Team> CreateTeam([FromBody] CreateRequest request)
    {
        try{
            var team = new Team(request.name);
            _dbContext.Teams.Add(team);
            _dbContext.SaveChanges();

            var teamMember = new TeamMember(request.userId,team.Id);
            _dbContext.TeamMembers.Add(teamMember);
            _dbContext.SaveChanges();
            return Ok(new {team = team});
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    //public ActionResult<List<Game>> GetGames([FromQuery] int teamId)

}