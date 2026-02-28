using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Catan.data;
using Catan.services;

var builder = WebApplication.CreateBuilder(args);
string connectionString = builder.Configuration["ConnectionStrings:DefaultConnection"];
builder.Services.AddDbContext<CatanDbContext>(options => options.UseNpgsql(connectionString));
builder.Services.AddScoped<PasswordHasher>();

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



