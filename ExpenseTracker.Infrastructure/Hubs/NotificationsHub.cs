using ExpenseTracker.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace ExpenseTracker.Infrastructure.Hubs;

[Authorize]
public class NotificationsHub : Hub<INotificationsClient>
{
    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
    }
}