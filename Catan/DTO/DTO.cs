using Catan.models;
namespace Catan.DTO;
public record JoinRequestDto(int UserId, int TeamId);
public record AuthRequestDto(string Username, string Password);
public record CreateTeamRequestDto(int UserId, string Name);
public record CreateGameRequestDto(int UserId, int TeamId, GameSettings Settings);
public record GameHistoryDto(int GameCount, IEnumerable<DateTime> Dates, IEnumerable<string> Names, IEnumerable<int> Ids);
public record UpdateRelationsRequestDto(int PlayerId, int NewValue);
public record UpdateResourceRequestDto(string Resource, int Amount);