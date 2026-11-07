namespace SingleCore.Core.Models;

public sealed class NotificationEntry
{
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;
    public bool IsError { get; set; }
}
