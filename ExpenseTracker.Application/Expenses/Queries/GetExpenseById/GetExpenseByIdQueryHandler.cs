using ExpenseTracker.Application.Common.Exceptions;
using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Application.Expenses.Queries.GetExpenseById;

public class GetExpenseByIdQueryHandler: IRequestHandler<GetExpenseByIdQuery, ExpenseResponse>
{
    private readonly IAppDbContext  _context;
    private readonly ICurrentUserService _currentUserService;

    public GetExpenseByIdQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
    
    public async Task<ExpenseResponse> Handle(GetExpenseByIdQuery request, CancellationToken cancellationToken)
    {
        var expense = await _context.Expenses.Include(e => e.Category)
                          .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken)
                      ?? throw new NotFoundException($"Expense with id {request.Id} not found");
        
        if (expense.UserId != _currentUserService.UserId)
            throw new ForbiddenAccessException("User does not have permission to update expense");


        return new ExpenseResponse()
        {
            Name = expense.Name,
            Description = expense.Description ?? "",
            Currency = expense.Price.Currency,
            Amount = expense.Price.Amount,
            Date = expense.Date,
            CategoryName = expense.Category.Name,
        };
    }
}