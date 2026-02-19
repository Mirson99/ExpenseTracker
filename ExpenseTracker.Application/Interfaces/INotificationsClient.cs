namespace ExpenseTracker.Application.Interfaces;

public interface INotificationsClient
{
    Task ReceiveNotification(string message);
}