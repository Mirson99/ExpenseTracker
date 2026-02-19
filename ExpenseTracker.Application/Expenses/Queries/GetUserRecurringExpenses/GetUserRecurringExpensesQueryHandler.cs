using System.Linq.Expressions;
using ExpenseTracker.Application.Common.Models;
using ExpenseTracker.Application.Interfaces;
using ExpenseTracker.Domain.Entities;
using MediatR;

namespace ExpenseTracker.Application.Expenses.Queries.GetUserRecurringExpenses;

public class GetUserRecurringExpensesQueryHandler:  IRequestHandler<GetUserRecurringExpensesQuery, PagedList<RecurringExpenseResponse>>
{
    private readonly IAppDbContext  _context;
    private readonly ICurrentUserService _currentUserService;

    public GetUserRecurringExpensesQueryHandler(IAppDbContext context, ICurrentUserService currentUserService)
    {
        _context = context;
        _currentUserService = currentUserService;
    }
    
    public async Task<PagedList<RecurringExpenseResponse>> Handle(GetUserRecurringExpensesQuery request, CancellationToken cancellationToken)
    {
        IQueryable<RecurringExpense> expenseQuery =
            _context.RecurringExpenses.Where(e => e.UserId == _currentUserService.UserId && e.IsActive);
        
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            expenseQuery = expenseQuery.Where(p => p.Name.Contains(request.SearchTerm));
        }
        
        if (request.SortOrder?.ToLower() == "asc")
        {
            expenseQuery = expenseQuery.OrderBy(GetSortProperty(request)).ThenBy(e => e.NextExecutionDate);
        }
        else
        {
            expenseQuery = expenseQuery.OrderByDescending(GetSortProperty(request)).ThenByDescending(e => e.NextExecutionDate);
        }

        if (request.CategoryId.HasValue)
        {
            expenseQuery = expenseQuery.Where(e => e.CategoryId == request.CategoryId);
        }
        
        var expensesResponseQuery = expenseQuery
            .Select(p => new RecurringExpenseResponse()
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                Currency = p.Price.Currency,
                Amount = p.Price.Amount,
                CategoryName = p.Category.Name,
                CategoryId = p.CategoryId,
                NextPaymentDate = p.NextExecutionDate,
                Frequency = p.Frequency.ToString()
            });

        var expenses = await PagedList<RecurringExpenseResponse>.CreateAsync(
            expensesResponseQuery,
            request.Page,
            request.PageSize);

        return expenses;
    }

    private static Expression<Func<RecurringExpense, object>> GetSortProperty(GetUserRecurringExpensesQuery request) =>
        request.SortColumn?.ToLower() switch
        {
            "name" => expense => expense.Name,
            "amount" => expense => expense.Price.Amount,
            "currency" => expense => expense.Price.Currency,
            _ => expense => expense.NextExecutionDate
        };
}