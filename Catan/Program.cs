using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Catan.data;
using Catan.services;
using Catan.game;
using Catan.repositories;
using Catan.middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"] 
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured");
string jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("Jwt:Key is not configured");
string jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? throw new InvalidOperationException("Jwt:Issuer is not configured");

builder.Services.AddDbContext<CatanDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<ICatanRepository,CatanRepository>();
builder.Services.AddScoped<PasswordHasher>();
builder.Services.AddScoped<JwtHandler>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

var wsHandler = new WsHandler(new JwtHandler(builder.Configuration));
wsHandler.Start();

builder.Services.AddSingleton<WsHandler>(wsHandler);

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
        

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("AllowAll");
app.MapControllers();


app.Run();



