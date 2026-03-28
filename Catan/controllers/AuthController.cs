using Microsoft.AspNetCore.Mvc;
using Catan.models;
using Catan.services;
using Catan.repositories;
using Catan.exceptions;
using Catan.DTO;
using Catan.constants;
namespace Catan.controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly ICatanRepository _repository;
    private readonly JwtHandler _jwtHandler;
    public AuthController(ICatanRepository repository, JwtHandler jwtHandler)
    {
        _repository = repository;
        _jwtHandler = jwtHandler;
    }
    
    [HttpPost("login")]
    public async Task<ActionResult<User>> Login([FromBody] AuthRequestDto request){
        User? user = await _repository.GetUser(request.Username);

        if (user == null || !PasswordHasher.Verify(request.Password, user.PasswordHash)) throw new UnauthorizedException(ErrorCode.InvalidUsernameOrPassword);
        await _repository.UpdateLastLogin(user);

        return Ok(new {userId = user.Id, token = _jwtHandler.GenerateJwtString(user)});
    }
    [HttpPost("register")]
    public async Task<ActionResult<User>> Register([FromBody] AuthRequestDto request)
    {
        if(await _repository.UserExists(request.Username)) throw new ConflictException(ErrorCode.UsernameExists);

        string passwordHash = PasswordHasher.Hash(request.Password);
        var user = new User(request.Username, passwordHash);
        await _repository.AddUser(user);

        return Ok(new {userId = user.Id, token = _jwtHandler.GenerateJwtString(user)});
    }
}