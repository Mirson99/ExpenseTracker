using System.Linq.Expressions;
using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Expenses.Queries;

public class GetUserExpensesQueryHandler: IRequestHandler<GetUserExpensesQuery, PagedList<ExpenseResponse>>
{
    private readonly IAppDbContext  _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserExpensesQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
    
    public async Task<PagedList<ExpenseResponse>> Handle(GetUserExpensesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<Expense> expenseQuery = _context.Expenses.Where(e => e.UserId == _currentUserService.UserId);
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            expenseQuery = expenseQuery.Where(p => p.Name.Contains(request.SearchTerm));
        }

        if (request.CategoryId.HasValue)
        {
            expenseQuery = expenseQuery.Where(e => e.CategoryId == request.CategoryId);
        }

        if (request.SortOrder?.ToLower() == "asc")
        {
            expenseQuery = expenseQuery.OrderBy(GetSortProperty(request)).ThenBy(e => e.CreatedAt);
        }
        else
        {
            expenseQuery = expenseQuery.OrderByDescending(GetSortProperty(request)).ThenByDescending(e => e.CreatedAt);
        }

        var expensesResponseQuery = expenseQuery
            .Select(p => new ExpenseResponse()
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Currency = p.Price.Currency,
                Amount = p.Price.Amount,
                Date = p.Date,
                CategoryName = p.Category.Name,
                CategoryId = p.CategoryId,
                IsRecurring = p.IsRecurring,
            });

        var expenses = await PagedList<ExpenseResponse>.CreateAsync(
            expensesResponseQuery,
            request.Page,
            request.PageSize);

        return expenses;
    }

    private static Expression<Func<Expense, object>> GetSortProperty(GetUserExpensesQuery request) =>
        request.SortColumn?.ToLower() switch
        {
            "name" => expense => expense.Name,
            "amount" => expense => expense.Price.Amount,
            "currency" => expense => expense.Price.Currency,
            "date" => expense => expense.Date,
            _ => expense => expense.Date
        };

}