namespace ExpenseTracker.Application.Interfaces;

public interface INotificationService
{
    Task SendToUserAsync(string userId, string message, CancellationToken cancellationToken = default); 
}