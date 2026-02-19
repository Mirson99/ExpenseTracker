using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using ExpenseTracker.Domain.Enums;
using ExpenseTracker.Domain.ValueObjects;
using MediatR;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Application.Expenses.Commands;

public class CreateExpenseCommandHandler: IRequestHandler<CreateExpenseCommand, Guid>
{
    private readonly IAppDbContext  _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly ILogger<CreateExpenseCommandHandler> _logger;
    public CreateExpenseCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUser, ILogger<CreateExpenseCommandHandler> logger)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _logger = logger;
    }
    
    public async Task<Guid> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating expense: {ExpenseName} for user with id: {UserId}", request.Name, _currentUser.UserId);
        if (!_dbContext.Categories.Any(c => c.Id == request.CategoryId))
        {
            _logger.LogWarning("Category with id: {CategoryId} does not exist, user id: {UserId}", request.CategoryId,  _currentUser.UserId);
            throw new ValidationException("Category does not exist");
        }
        
        var expense = Expense.Create(request.Name, request.Currency, request.Amount, request.Date, request.CategoryId, request.IsRecurring, _currentUser.UserId, request.Description );
        await _dbContext.Expenses.AddAsync(expense, cancellationToken);
        
        if (request.IsRecurring && request.Frequency.HasValue)
        {
            var recurringExpense = RecurringExpense.Create(request.Name, request.Currency, request.Amount, request.Date, request.CategoryId, _currentUser.UserId, request.Description, request.Frequency);
            await _dbContext.RecurringExpenses.AddAsync(recurringExpense);
            
            _logger.LogInformation("Created recurring expense pattern for {ExpenseName} for User {UserId}. Next run: {NextRun}", 
                request.Name, _currentUser.UserId, recurringExpense.NextExecutionDate);
        }
        
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} created expense {ExpenseId}", _currentUser.UserId, expense.Id);
        return expense.Id;
    }
}