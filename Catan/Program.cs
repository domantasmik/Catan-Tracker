using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Catan.data;
using Catan.services;
using Catan;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];


builder.Services.AddDbContext<CatanDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<PasswordHasher>();

var wsHandler = new WsHandler();
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

app.MapControllers();
app.UseCors("AllowAll");
app.Run();



