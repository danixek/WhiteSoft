namespace WhiteSoft.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string Level { get; set; } = string.Empty; // e.g. Info, Warning, Error
        public string Message { get; set; } = string.Empty;
        public required string UserName { get; set; }
        public DateTime CreatedAt { get; set; }
    }

}
