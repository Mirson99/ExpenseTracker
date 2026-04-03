using ExpenseTracker.Domain.Events;
using MassTransit;
using MediatR;

namespace ExpenseTracker.Application.Receipts.EventHandlers;

public class ReceiptProcessingStartedEventHandler(IPublishEndpoint publishEndpoint) 
    : INotificationHandler<ReceiptProcessingStartedEvent>
{
    public async Task Handle(ReceiptProcessingStartedEvent notification, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(notification.Expense.StorageKey))
        {
            throw new InvalidOperationException("Empty storage key");
        }
        var integrationEvent = new ReceiptUploadedEvent(
            notification.Expense.Id, 
            notification.Expense.StorageKey);
        await publishEndpoint.Publish(integrationEvent, cancellationToken);
    }
}