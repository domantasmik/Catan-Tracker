using Catan.models;
namespace Catan.repositories;
public interface ICatanRepository
{
    Task<User?> GetUser(string username);
    Task<bool> UserExists(string username);
    Task AddUser(User user);
    Task<List<Team>> GetUserTeams(int userId);
    Task<Team?> GetTeam(int teamId);
    Task AddUserToTeam(int userId, int teamId);
    Task AddTeam(Team team);
    Task UpdateLastLogin(User user);
}