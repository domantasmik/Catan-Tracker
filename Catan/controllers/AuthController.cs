using Microsoft.AspNetCore.Mvc;
using Catan.models;
using Catan.services;
using Catan.repositories;
using Catan.exceptions;
namespace Catan.controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ICatanRepository _repository;
    private readonly PasswordHasher _hasher;
    private readonly JwtHandler _jwtHandler;
    public AuthController(ICatanRepository repository, PasswordHasher hasher,JwtHandler jwtHandler)
    {
        _repository = repository;
        _hasher = hasher;
        _jwtHandler = jwtHandler;
    }
    public record AuthRequest(string Username, string Password);
    [HttpPost("login")] 
    public async Task<ActionResult<User>> Login([FromBody] AuthRequest request){
        User? user = await _repository.GetUser(request.Username);

        if (user == null || !_hasher.Verify(request.Password, user.PasswordHash)) throw new UnauthorizedException("Invalid username or password" );
        await _repository.UpdateLastLogin(user);

        return Ok(new {userId = user.Id, token = _jwtHandler.GenerateJwtString(user)});
    }
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody] AuthRequest request)
    {
        if(await _repository.UserExists(request.Username)) throw new ConflictException("username already exists");

        string passwordHash = _hasher.Hash(request.Password);
        var user = new User(request.Username, passwordHash);
        await _repository.AddUser(user);

        return Ok(new {userId = user.Id, token = _jwtHandler.GenerateJwtString(user)});
    }
}