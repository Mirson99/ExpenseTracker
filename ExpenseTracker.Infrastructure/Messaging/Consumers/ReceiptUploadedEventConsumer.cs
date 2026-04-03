using ExpenseTracker.Application.Receipts;
using ExpenseTracker.Application.Receipts.Commands.AnalyzeReceipt;
using MassTransit;
using MediatR;

namespace ExpenseTracker.Infrastructure.Messaging.Consumers;

public class ReceiptUploadedEventConsumer(ISender sender) : IConsumer<ReceiptUploadedEvent>
{
    public async Task Consume(ConsumeContext<ReceiptUploadedEvent> context)
    {
        var command = new AnalyzeReceiptCommand(
            context.Message.ExpenseId, 
            context.Message.StorageKey
        );
        
        await sender.Send(command, context.CancellationToken);
    }
}