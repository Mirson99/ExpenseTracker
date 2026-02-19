using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Infrastructure.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace ExpenseTracker.Infrastructure.Services;

public class SignalRNotificationService : INotificationService
{
    private readonly IHubContext<NotificationsHub, INotificationsClient> _hubContext;

    public SignalRNotificationService(IHubContext<NotificationsHub, INotificationsClient> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task SendToUserAsync(string userId, string message, CancellationToken cancellationToken = default)
    {
        await _hubContext.Clients.User(userId)
            .ReceiveNotification(message);
    }
}