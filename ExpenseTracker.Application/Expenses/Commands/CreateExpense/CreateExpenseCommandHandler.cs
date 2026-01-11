using System.ComponentModel.DataAnnotations;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Expenses.Commands;

public class CreateExpenseCommandHandler: IRequestHandler<CreateExpenseCommand, Guid>
{
    private readonly IAppDbContext  _dbContext;
    private readonly ICurrentUserService _currentUser;
    public CreateExpenseCommandHandler(IAppDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }
    
    public async Task<Guid> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
    {
        if (!_dbContext.Categories.Any(c => c.Id == request.CategoryId))
        {
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
        return expense.Id;
    }
}