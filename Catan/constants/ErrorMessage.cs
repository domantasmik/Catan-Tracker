namespace Catan.constants;
public static class ErrorCode
{
    public const string InvalidUsernameOrPassword = "INVALID_USERNAME_OR_PASSWORD";
    public const string UsernameExists = "USERNAME_ALREADY_EXISTS";
    public const string GameExists = "GAME_ALREADY_EXISTS";
    public const string GameNotStarted = "GAME_NOT_STARTED";
    public const string TeamDoesntExist= "TEAM_DOESNT_EXIST";
    public const string GameStarted = "GAME_ALREADY_STARTED";
    public const string LobbyFull = "LOBBY_FULL";
    public const string NotYourTurn = "NOT_YOUR_TURN";
    public static string FailedToDeserialize(string subject) {return "FAILED_TO_DESERIALIZE_" + subject.ToUpper();}
    public static string InvalidResourceAmount(string subject) {return "INVALID_RESOURCE_REQUEST_"+subject.ToUpper();}
    public const string AuthFailed = "AUTH_FAILED";
    public const string PlayerNotFound = "PLAYER_NOT_FOUND";
    public const string NullResponse = "NULL_RESPONSE";
    public const string InvalidMessageType = "INVALID_MESSAGE_TYPE";
    public const string JwtValidationFailed = "JWT_VALIDATION_FAILED";
}