using WhiteSoft.Models;

namespace WhiteSoft.Services
{
    public class LogService : ILogService
    {
        private readonly ApplicationDbContext _logContext;

        public LogService(ApplicationDbContext context)
        {
            _logContext = context;
        }

        public async Task LogAsync(string level, string message, string? userName)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Central Europe Standard Time"); // CET/CEST
            var logEntry = new LogEntry
            {
                Timestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone),
                Level = level,
                Message = message,
                UserName = userName ?? "Anonymous",
                CreatedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone),
            };

            _logContext.LogEntries.Add(logEntry);
            await _logContext.SaveChangesAsync();
        }
    }
}