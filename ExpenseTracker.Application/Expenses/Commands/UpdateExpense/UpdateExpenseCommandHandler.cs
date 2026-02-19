using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Expenses.Commands.UpdateExpense;

public class UpdateExpenseCommandHandler : IRequestHandler<UpdateExpenseCommand>
{
    private readonly IAppDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public UpdateExpenseCommandHandler(
        IAppDbContext context,
        ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task Handle(UpdateExpenseCommand command, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
                          .FirstOrDefaultAsync(e => e.Id == command.Id, cancellationToken)
                      ?? throw new NotFoundException($"Expense with id {command.Id} not found");
        
        if (expense.UserId != _currentUser.UserId)
            throw new ForbiddenAccessException("User does not have permission to update expense");

        var currencyToUse = command.Currency ?? expense.Price.Currency;
        var newPrice = Money.From(command.Amount, currencyToUse);
        
        expense.Update(command.Name, newPrice, command.Date, command.CategoryId, command.Description
        );

        await _context.SaveChangesAsync(cancellationToken);
    }
}