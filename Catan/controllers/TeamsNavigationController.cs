using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Catan.models;
using Catan.services;
using Catan.repositories;
using Catan.exceptions;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Authorization;
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
    public async Task<ActionResult<List<Team>>> GetTeams([FromQuery] int userId)
    {
        var teams = await _repository.GetUserTeams(userId);
        return Ok(new {teams = teams});
    }
    public record JoinRequest(int userId, int teamId);
    [HttpPost("join")]
    public async Task<ActionResult<Team>> JoinTeam([FromBody] JoinRequest request)
    {
        var team = await _repository.GetTeam(request.teamId);

        if(team == null) throw new NotFoundException("Team does not exist");

        await _repository.AddUserToTeam(request.userId, team.Id);
        
        return Ok(new {team = team});
    }
    public record CreateRequest(int userId, string name);
    [HttpPost("create")]
    public async Task<ActionResult<Team>> CreateTeam([FromBody] CreateRequest request)
    {
        var team = new Team(request.name);
        await _repository.AddTeam(team);
        await _repository.AddUserToTeam(request.userId,team.Id);
        
        return Ok(new {team = team});
    }
}