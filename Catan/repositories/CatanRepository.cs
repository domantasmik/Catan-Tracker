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
    public async Task<IEnumerable<Team>> GetUserTeams(int userId)
    {
        var teams = await _dbContext.TeamMembers.Include(tm => tm.Team).Where(tm => tm.UserId == userId).Select(tm => tm.Team).ToListAsync();
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
user.LastLogin = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
    }
    public async Task SaveGame(Game game)
    {
        await _dbContext.Games.AddAsync(game);
        await _dbContext.SaveChangesAsync();
    }
    public async Task AddPlayerToGame(GamePlayer gp)
    {
        await _dbContext.GamePlayers.AddAsync(gp);
        await _dbContext.SaveChangesAsync();
    }
    public async Task<IEnumerable<Game>> GetGames(int teamId)
    {
        return await _dbContext.Games.Where(g => g.TeamId == teamId).ToListAsync();
    }
    public async Task<Game?> GetGameById(int id)
    {
        return await _dbContext.Games.Include(g => g.GamePlayers).ThenInclude(gp => gp.User).Where(g => g.Id == id).FirstOrDefaultAsync();
    }
}