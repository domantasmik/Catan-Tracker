using Microsoft.EntityFrameworkCore;
using Catan.models;
namespace Catan.data;

public class CatanDbContext : DbContext
{
    public DbSet<User> Users {get;set;}
    public DbSet<Team> Teams {get;set;}
    public CatanDbContext(DbContextOptions<CatanDbContext> options) : base(options)
    {
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Team>()
            .HasMany(t => t.Users)
            .WithMany(u => u.Teams);
    }
}