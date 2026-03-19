using Catan.data; 
using Catan.models;
using Microsoft.EntityFrameworkCore;
namespace Catan.repositories;
public class CatanRepository : ICatanRepository
{
    private readonly CatanDbContext _dbContext;
    public CatanRepository(CatanDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<User?> GetUser(string username)
    {
        return await _dbContext.Users.FirstOrDefaultAsync(u => u.Username == username);
    }
    public async Task<bool> UserExists(string username)
    {
        return await _dbContext.Users.AnyAsync(u => u.Username == username);
    }
    public async Task AddUser(User user)
    {
        await _dbContext.Users.AddAsync(user);
        await _dbContext.SaveChangesAsync();
    }
    public async Task<List<Team>> GetUserTeams(int userId)
    {
        // Ar cia tikrai reikia dvieju DB calls del tokio dalyko? hint: ne
        // Return nemanau, kad cia reikia List<Team>, labiau IEnumereable<Team> ir jeigu reikes konvertuosi i list

        // Isivaizduok, kad tu cia buildini query ir kai parasai ToListAsync() forcini .NET issiusti requesta i duomenu baze, nes itercija vyksta.
        // Tau ToListAsync() vis tiek reikia, bet IEnumerable uzdraus modifikuoti duomenis, o leisi modifikuoti tik tada kai reikes.
        var teamIds = await _dbContext.TeamMembers.Where(tm => tm.UserId == userId).Select(tm => tm.TeamId).ToListAsync();
        var teams = await _dbContext.Teams.Where(t => teamIds.Contains(t.Id)).ToListAsync();
        return teams;
    }
    public async Task<Team?> GetTeam(int teamId)
    {
        return await _dbContext.Teams.FirstOrDefaultAsync(t => t.Id == teamId);
    }
    public async Task AddUserToTeam(int userId, int teamId)
    {
        await _dbContext.TeamMembers.AddAsync(new TeamMember(userId,teamId));
        await _dbContext.SaveChangesAsync();
    }
    public async Task AddTeam(Team team)
    {
        await _dbContext.Teams.AddAsync(team);
        await _dbContext.SaveChangesAsync();
    }
    public async Task UpdateLastLogin(User user)
    {
        // Ne taip supratai repozitorijos principa. Repozitorija negali taip literaliai keisti duomenu.
        // Ka turejau omenyje, kad repo skaito arba raso i duomenu baze per queries, o ne literaliai keicia objekta.
        // Jei rasytum testus pamatytum kodel cia yra labai blogai.
        // O testus rasysi visiem minimaliems dalykams visada, kai dirbsi.
        user.LastLogin = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
}