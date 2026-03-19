using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Catan.models;
using Microsoft.IdentityModel.Tokens;
namespace Catan.services;
public class JwtHandler
{
    private string _key;
    private string _issuer;
    public JwtHandler(IConfiguration config)
    {
        _key = config["Jwt:Key"]!;
        _issuer = config["Jwt:Issuer"]!;
    }
    public string GenerateJwtString(User user)
    {
        List<Claim> claims = new List<Claim>();
        claims.Add(new Claim("userId", user.Id.ToString()));
        claims.Add(new Claim("username", user.Username));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor{
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(24),
            SigningCredentials = credentials,
            Issuer = _issuer
        };
        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);
        string tokenString = handler.WriteToken(token);
        return tokenString;
    }
    public Player ValidateJwt(string token)
    {
        SecurityToken validatedToken;
        var handler = new JwtSecurityTokenHandler();
        ClaimsPrincipal claims = handler.ValidateToken(token,new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = _issuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key))
        },out validatedToken);
        if(!int.TryParse(claims.FindFirst("userId")?.Value,out int userId) ||  claims.FindFirst("username")?.Value == null) throw new Exception("failed parsing token");

        Player? player = new Player(userId, claims.FindFirst("username")?.Value!);
        return player;
    }
}