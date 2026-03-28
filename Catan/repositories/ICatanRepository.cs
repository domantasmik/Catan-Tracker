using Catan.models;
namespace Catan.repositories;
public interface ICatanRepository
{
    Task<User?> GetUser(string username);
    Task<bool> UserExists(string username);
    Task AddUser(User user);
    Task<IEnumerable<Team>> GetUserTeams(int userId);
    Task<Team?> GetTeam(int teamId);
    Task AddUserToTeam(int userId, int teamId);
    Task AddTeam(Team team);
    Task UpdateLastLogin(User user);
    Task SaveGame(Game game);
    Task AddPlayerToGame(GamePlayer gp);
    Task<IEnumerable<Game>> GetGames(int teamId);
    Task<Game?> GetGameById(int id);
}