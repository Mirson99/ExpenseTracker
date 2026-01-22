using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
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
        
        var expense = new Expense
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            Amount = request.Amount,
            Date = request.Date,
            CategoryId = request.CategoryId,
            UserId = _currentUser.UserId,
            Currency = request.Currency ?? "PLN",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        await _dbContext.Expenses.AddAsync(expense, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("User {UserId} created expense {ExpenseId}", _currentUser.UserId, expense.Id);
        return expense.Id;
    }
}