namespace WorldState.Domain.Commands;

public record CommandResult(
    bool IsSuccess,
    string? ErrorCode = null,
    string? ErrorMessage = null
)
{
    public static CommandResult Success() => new(true);
    public static CommandResult Failure(string errorCode, string errorMessage) =>
        new(false, errorCode, errorMessage);
}

public interface ICommand
{
    CommandResult Execute(Models.GameSession session);
}
