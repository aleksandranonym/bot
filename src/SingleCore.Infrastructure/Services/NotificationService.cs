namespace SingleCore.Infrastructure.Services;

using SingleCore.Core.Interfaces;
using SingleCore.Core.Models;

public sealed class NotificationService : INotificationService
{
    private readonly List<NotificationEntry> _notifications = new();

    public IReadOnlyList<NotificationEntry> Notifications => _notifications.AsReadOnly();

    public void Push(string message, bool isError = false)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        _notifications.Add(new NotificationEntry
        {
            Message = message,
            CreatedUtc = DateTime.UtcNow,
            IsError = isError
        });

        if (_notifications.Count > 8)
        {
            _notifications.RemoveAt(0);
        }
    }

    public void Clear()
    {
        _notifications.Clear();
    }
}
