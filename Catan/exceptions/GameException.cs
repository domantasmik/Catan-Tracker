namespace Catan.exceptions;
public class GameException : Exception
{
    public GameException(string message) : base(message) { }
}