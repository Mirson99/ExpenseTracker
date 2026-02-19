using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Expenses.Commands.ProcessRecurringExpenses;
using ExpenseTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Expenses.Commands.DeleteRecurringExpense;

public class DeleteRecurringExpenseCommandHandler : IRequestHandler<DeleteRecurringExpenseCommand>
{
    private readonly IAppDbContext _context;
    private readonly ILogger<DeleteRecurringExpenseCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public DeleteRecurringExpenseCommandHandler(IAppDbContext context, ILogger<DeleteRecurringExpenseCommandHandler> logger, ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    
    public async Task Handle(DeleteRecurringExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _context.RecurringExpenses.FirstOrDefaultAsync(r => r.Id == request.Id && r.IsActive && r.UserId == _currentUserService.UserId, cancellationToken);
        
        if (expense is null)
        {
            _logger.LogWarning("Recurring Expense {ExpenseId} does not exist, user id: {UserId}", request.Id, _currentUserService.UserId);
            throw new NotFoundException("Expense not found");
        }

        expense.MakeRecurringExpenseInActive();
        await _context.SaveChangesAsync(cancellationToken);
    } 
}