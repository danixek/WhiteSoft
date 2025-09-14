namespace WhiteSoft.Services
{
    public interface ILogService
    {
        Task LogAsync(string level, string message, string? userName);
    }
}
