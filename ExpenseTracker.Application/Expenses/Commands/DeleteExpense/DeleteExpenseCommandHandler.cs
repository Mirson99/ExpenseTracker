using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Expenses.Commands.DeleteExpense;

public class DeleteExpenseCommandHandler: IRequestHandler<DeleteExpenseCommand>
{
    private readonly IAppDbContext  _context;
    private readonly ICurrentUserService _currentUserService;

    public DeleteExpenseCommandHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
    
    public async Task Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
                          .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                      ?? throw new NotFoundException($"Expense with id {request.Id} not found");
        
        if (expense.UserId != _currentUserService.UserId)
            throw new ForbiddenAccessException("User does not have permission to update expense");
        
        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync(cancellationToken);
    }
}