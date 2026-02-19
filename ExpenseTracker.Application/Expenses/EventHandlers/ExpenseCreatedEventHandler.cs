using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Expenses.EventHandlers;

public class ExpenseCreatedEventHandler : INotificationHandler<ExpenseCreatedEvent>
{
    private readonly ILogger<ExpenseCreatedEventHandler> _logger;
    private readonly INotificationService _notificationService;

    public ExpenseCreatedEventHandler(ILogger<ExpenseCreatedEventHandler> logger, INotificationService notificationService)
    {
        _logger = logger;
        _notificationService = notificationService;
    }

    public async Task Handle(ExpenseCreatedEvent notification, CancellationToken cancellationToken)
    {
        var expense = notification.Expense;
        _logger.LogInformation("📢 DOMAIN EVENT: New expense Created! Amount: {Amount} {Currency}", 
            notification.Expense.Price.Amount, 
            notification.Expense.Price.Currency);
        
        // if (notification.Expense.IsRecurring)
        // {
            var message = expense.IsRecurring 
                ? $"System opłacił Twój rachunek: {expense.Name} ({expense.Price.Amount} {expense.Price.Currency})" 
                : $"Dodano nowy wydatek: {expense.Name}";
            
            await _notificationService.SendToUserAsync(
                expense.UserId.ToString(), 
                message, 
                cancellationToken);
        // }
    }
}