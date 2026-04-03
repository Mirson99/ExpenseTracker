using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MassTransit;
using MediatR;

namespace ExpenseTracker.Application.Receipts.Commands.ProcessReceipt;

public class ProcessReceiptCommandHandler(
    IAppDbContext dbContext,
    IPublishEndpoint publishEndpoint,
    ICurrentUserService currentUserService) : IRequestHandler<ProcessReceiptCommand, Guid>
{
    public async Task<Guid> Handle(ProcessReceiptCommand request, CancellationToken cancellationToken)
    {
        var expense = Expense.CreateFromReceipt(currentUserService.UserId, request.StorageKey);
        dbContext.Expenses.Add(expense);
        await dbContext.SaveChangesAsync(cancellationToken);
        return expense.Id;
    }
}