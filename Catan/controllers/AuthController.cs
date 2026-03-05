using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Catan.models;
using Catan.data;
using Catan.services;
namespace Catan.controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly CatanDbContext _dbContext;
    private PasswordHasher _hasher;
    public AuthController(CatanDbContext dbContext, PasswordHasher hasher)
    {
        _dbContext = dbContext;
        _hasher = hasher;
    }
    public record AuthRequest(string Username, string Password);
    [HttpPost("login")] 
    public ActionResult<User> Login([FromBody] AuthRequest request){
        try {
        User? user = _dbContext.Users.FirstOrDefault(u => u.Username == request.Username);
        if (user == null || !_hasher.Verify(request.Password, user.PasswordHash)) return Unauthorized(new { error = "Invalid username or password" });

        user.LastLogin = DateTime.UtcNow;
        _dbContext.SaveChanges();

        return Ok(new { userId = user.Id });
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }
    [HttpPost("register")]
    public ActionResult<User> Register([FromBody] AuthRequest request)
    {
        try{
            string passwordHash = _hasher.Hash(request.Password);
            if(_dbContext.Users.Any(u => u.Username == request.Username)) throw new Exception("username already exists");
            User user = new User(request.Username, passwordHash);
            _dbContext.Users.Add(user);
            _dbContext.SaveChanges();
            return Ok( new {userId = user.Id});
        }
        catch (Exception ex) {
            return StatusCode(500, new { error = ex.Message });
        }
    }
}