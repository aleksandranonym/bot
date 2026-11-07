namespace SingleCore.Core.Interfaces;

using SingleCore.Core.Models;

public interface INotificationService
{
    IReadOnlyList<NotificationEntry> Notifications { get; }
    void Push(string message, bool isError = false);
    void Clear();
}
