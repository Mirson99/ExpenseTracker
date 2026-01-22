using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Expenses.Commands.DeleteExpense;

public class DeleteExpenseCommandHandler: IRequestHandler<DeleteExpenseCommand>
{
    private readonly IAppDbContext  _context;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteExpenseCommandHandler> _logger;


    public DeleteExpenseCommandHandler(IAppDbContext context, ICurrentUserService currentUserService, ILogger<DeleteExpenseCommandHandler> logger)
    {
        _context = context;
        _currentUserService = currentUserService;
        _logger = logger;
    }
    
    public async Task Handle(DeleteExpenseCommand request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses
            .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);
        if (expense == null)
        {
            _logger.LogWarning("Expense {ExpenseId} does not exist, user id: {UserId}", request.Id, _currentUserService.UserId);
            throw new NotFoundException($"Expense with id {request.Id} not found");
        }

        if (expense.UserId != _currentUserService.UserId)
        {
            _logger.LogWarning("User with id: {UserId} does not have permission to update expense with id {ExpenseId}", _currentUserService.UserId, expense.Id);
            throw new ForbiddenAccessException("User does not have permission to update expense");   
        }
        
        _context.Expenses.Remove(expense);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Expense {ExpenseId} deleted successfully by User {UserId}", request.Id, _currentUserService.UserId);
    }
}